using System;
using System.IO;
using UnityEngine;

namespace SIGGD.Save
{
    /// <summary>
    /// Low-level filesystem helper for the save pipeline. Nothing else in the game should touch
    /// <c>Application.persistentDataPath</c> for save data — go through this class.
    /// </summary>
    /// <remarks>
    /// <para>Directory layout under <c>Application.persistentDataPath/Data</c>:
    /// <c>Settings/</c> for <see cref="SaveScope.Settings"/> modules and <c>Gameplay/</c> for
    /// <see cref="SaveScope.Gameplay"/> and <see cref="SaveScope.Screenshot"/> modules. The
    /// gameplay directory can be wiped as a unit via <see cref="ClearGameplay"/> to start a
    /// fresh run without touching the player's settings.</para>
    ///
    /// <para>Every file is written with a 4-byte little-endian version header followed by the payload.
    /// Writes are atomic: bytes go to <c>filename.tmp</c>, then <see cref="File.Replace(string,string,string)"/>
    /// swaps the temp file into place with a <c>.bak</c> backup. A crash mid-write leaves the previous
    /// good file untouched.</para>
    /// </remarks>
    public static class SaveFileIO
    {
        private const string RootDir = "Data";
        private const string SettingsDir = "Settings";
        private const string GameplayDir = "Gameplay";

        private const string TempSuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        // Files without an explicit extension get this one appended.
        private const string DefaultExtension = ".dat";

        private static string _root;

        /// <summary>Absolute path to the save-data root. Directories are created on first access.</summary>
        public static string RootPath
        {
            get
            {
                if (_root == null)
                {
                    _root = Path.Combine(Application.persistentDataPath, RootDir);
                    Directory.CreateDirectory(_root);
                    Directory.CreateDirectory(Path.Combine(_root, SettingsDir));
                    Directory.CreateDirectory(Path.Combine(_root, GameplayDir));
                }
                return _root;
            }
        }

        /// <summary>
        /// Compute the absolute path for the file storing <paramref name="key"/> in the given <paramref name="scope"/>.
        /// Adds <c>.dat</c> if <paramref name="key"/> has no extension.
        /// </summary>
        public static string GetPath(SaveScope scope, string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Save key must be non-empty.", nameof(key));

            string subdir = scope == SaveScope.Settings ? SettingsDir : GameplayDir;
            string filename = key.IndexOf('.') >= 0 ? key : key + DefaultExtension;
            return Path.Combine(RootPath, subdir, filename);
        }

        /// <summary>True if a file exists for the given scope + key.</summary>
        public static bool Exists(SaveScope scope, string key) => File.Exists(GetPath(scope, key));

        /// <summary>
        /// Atomically write <paramref name="payload"/> under the given scope + key, tagged with
        /// <paramref name="version"/>. Returns <c>true</c> on success.
        /// </summary>
        public static bool Write(SaveScope scope, string key, int version, byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            string path = GetPath(scope, key);
            string tempPath = path + TempSuffix;
            string backupPath = path + BackupSuffix;

            try
            {
                string parent = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);

                // Write header + payload to the temp file.
                using (FileStream fs = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] header = BitConverter.GetBytes(version);
                    fs.Write(header, 0, header.Length);
                    fs.Write(payload, 0, payload.Length);
                    fs.Flush(true);
                }

                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, backupPath, ignoreMetadataErrors: true);
                }
                else
                {
                    File.Move(tempPath, path);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveFileIO: failed to write {path}: {e}");
                TryDelete(tempPath);
                return false;
            }
        }

        /// <summary>
        /// Read the payload for the given scope + key, returning it plus the version stamp.
        /// Returns <c>false</c> when the file is missing or malformed; in that case
        /// <paramref name="payload"/> is <c>null</c> and <paramref name="version"/> is 0.
        /// </summary>
        public static bool Read(SaveScope scope, string key, out byte[] payload, out int version)
        {
            payload = null;
            version = 0;

            string path = GetPath(scope, key);
            if (!File.Exists(path)) return false;

            try
            {
                byte[] all = File.ReadAllBytes(path);
                if (all.Length < 4)
                {
                    Debug.LogWarning($"SaveFileIO: {path} is too short to contain a version header, ignoring.");
                    return false;
                }

                version = BitConverter.ToInt32(all, 0);
                payload = new byte[all.Length - 4];
                Buffer.BlockCopy(all, 4, payload, 0, payload.Length);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveFileIO: failed to read {path}: {e}");
                return false;
            }
        }

        /// <summary>Delete the file for the given scope + key, if present. No-op otherwise.</summary>
        public static void Delete(SaveScope scope, string key)
        {
            TryDelete(GetPath(scope, key));
        }

        /// <summary>Delete every gameplay-scope file (used when starting a new run).</summary>
        public static void ClearGameplay()
        {
            string dir = Path.Combine(RootPath, GameplayDir);
            if (!Directory.Exists(dir)) return;

            try
            {
                Directory.Delete(dir, recursive: true);
                Directory.CreateDirectory(dir);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveFileIO: failed to clear gameplay directory {dir}: {e}");
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveFileIO: could not delete {path}: {e.Message}");
            }
        }
    }
}
