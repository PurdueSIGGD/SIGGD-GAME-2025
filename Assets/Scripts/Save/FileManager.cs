using System.IO;
using UnityEngine;

public class FileManager : Singleton<FileManager>
{
    private readonly string mainDirectory;

    public static string saveDirectory = "Save";

    private static string defaultExtension = ".json";

    #region Init

    public FileManager()
    {
        mainDirectory = Application.persistentDataPath;

        InitializeDirectoryStructure();
    }

    private void InitializeDirectoryStructure()
    {
        if(!DirectoryExists(saveDirectory)) CreateDirectory(saveDirectory);
    }

    #endregion

    #region File Operations

    public bool FileExists(string relativePath)
    {
        string path = GetFilePath(relativePath);

        return File.Exists(path);
    }

    public string ReadFile(string relativePath)
    {
        string path = GetFilePath(relativePath);

        return File.ReadAllText(path);
    }

    public void WriteFile(string relativePath, string content)
    {
        string path = GetFilePath(relativePath);

        File.WriteAllText(path, content);
    }

    #endregion

    #region Directory Operations

    public bool DirectoryExists(string relativePath)
    {
        string path = GetDirectoryPath(relativePath);

        return Directory.Exists(path);
    }

    public void CreateDirectory(string relativePath)
    {
        string path = GetDirectoryPath(relativePath);

        Directory.CreateDirectory(path);
    }

    #endregion

    #region Helpers

    private string GetFilePath(string relativePath)
    {
        string path = Path.Combine(mainDirectory, relativePath);

        if (!path.Contains('.')) path += defaultExtension;

        return path;
    }

    private string GetDirectoryPath(string relativePath)
    {
        string path = Path.Combine(mainDirectory, relativePath);

        return path;
    }

    #endregion
}
