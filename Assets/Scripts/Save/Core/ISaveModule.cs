namespace SIGGD.Save
{
    /// <summary>
    /// A single unit of persisted state (player, inventory, quests, audio, etc.).
    /// </summary>
    /// <remarks>
    /// Contract:
    /// <list type="number">
    ///   <item><description><see cref="Key"/> is the stable, filesystem-safe identifier and must never change once shipped.</description></item>
    ///   <item><description><see cref="Capture"/> pulls live scene state into the module's in-memory POCO. It is the only method that reads from the scene.</description></item>
    ///   <item><description><see cref="Apply"/> pushes the in-memory POCO onto the live scene. It is the only method that writes to the scene, and must be idempotent.</description></item>
    ///   <item><description><see cref="Serialize"/> / <see cref="Deserialize"/> convert the POCO to/from bytes. They must not touch the scene.</description></item>
    /// </list>
    /// The <see cref="SaveManager"/> guarantees these are called in a valid order; individual
    /// modules should not reach into other singletons on load/save timelines.
    /// </remarks>
    public interface ISaveModule
    {
        /// <summary>Stable identifier used as the filename and override key. Never rename once shipped.</summary>
        string Key { get; }

        /// <summary>Which pipeline this module participates in.</summary>
        SaveScope Scope { get; }

        /// <summary>
        /// Data version stamp. Bump when the serialized layout changes so
        /// <see cref="Deserialize"/> can migrate or reject old files.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// <c>true</c> once <see cref="Deserialize"/> has consumed a real payload from disk;
        /// <c>false</c> on a freshly constructed module and after <see cref="Deserialize"/> is
        /// called with an empty payload (missing file / reset).
        /// </summary>
        /// <remarks>
        /// Modules whose <see cref="Apply"/> would overwrite live scene state with meaningless
        /// defaults (player pose, inventory contents, quest progress, grave, ...) must gate
        /// <see cref="Apply"/> on this flag. Modules whose defaults are safe to push (audio,
        /// input) may ignore it.
        /// </remarks>
        bool IsLoaded { get; }

        /// <summary>
        /// Read live scene state into the module's in-memory data.
        /// Called immediately before <see cref="Serialize"/> when saving.
        /// Must be safe to call even if the required scene singletons are missing —
        /// return silently and let <see cref="SaveManager"/> log the skip.
        /// </summary>
        void Capture();

        /// <summary>
        /// Push the module's in-memory data onto the live scene.
        /// Called after deserialization once the target scene singletons are ready.
        /// Must be idempotent — the manager may call it more than once per scene load.
        /// Implementations that would clobber scene defaults with an unloaded POCO must
        /// early-return when <see cref="IsLoaded"/> is <c>false</c>.
        /// </summary>
        void Apply();

        /// <summary>Convert the module's in-memory POCO to bytes.</summary>
        byte[] Serialize();

        /// <summary>
        /// Populate the module's in-memory POCO from bytes previously produced by
        /// <see cref="Serialize"/> at the given <paramref name="version"/>. Implementations
        /// are responsible for migrating older versions or rebuilding a default POCO,
        /// and for maintaining <see cref="IsLoaded"/>:
        /// <list type="bullet">
        ///   <item><description>Empty or <c>null</c> <paramref name="bytes"/> ⇒ reset POCO and set <see cref="IsLoaded"/> to <c>false</c>.</description></item>
        ///   <item><description>Non-empty payload that deserializes cleanly ⇒ set <see cref="IsLoaded"/> to <c>true</c>.</description></item>
        ///   <item><description>Non-empty payload that fails to deserialize ⇒ reset POCO and set <see cref="IsLoaded"/> to <c>false</c>.</description></item>
        /// </list>
        /// </summary>
        void Deserialize(byte[] bytes, int version);
    }
}
