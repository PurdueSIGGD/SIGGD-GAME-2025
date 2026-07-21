namespace SIGGD.Save
{
    /// <summary>
    /// Which lifecycle a save module participates in.
    /// </summary>
    /// <remarks>
    /// The three pipelines have different save/load rules:
    /// <list type="bullet">
    ///   <item><description><see cref="Settings"/> — loaded once at boot, saved when the user changes a setting.</description></item>
    ///   <item><description><see cref="Gameplay"/> — loaded on every scene enter, saved on scene exit (forced), on checkpoint, and on app quit (gated by GameStateManager).</description></item>
    ///   <item><description><see cref="Screenshot"/> — one-shot artifacts, saved only on explicit "save game" requests; never auto-loaded.</description></item>
    /// </list>
    /// </remarks>
    public enum SaveScope
    {
        Settings,
        Gameplay,
        Screenshot,
    }
}
