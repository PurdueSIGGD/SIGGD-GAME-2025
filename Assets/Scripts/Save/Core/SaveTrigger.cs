namespace SIGGD.Save
{
    /// <summary>
    /// The reason a save was requested. Determines whether the pursuit gate applies
    /// and which pipelines run.
    /// </summary>
    public enum SaveTrigger
    {
        /// <summary>OnApplicationQuit / OnApplicationPause. Gated by GameStateManager.</summary>
        ApplicationExit,

        /// <summary>Player is leaving the current scene. Forced — bypasses GameStateManager so the outgoing scene's data is on disk before the fade begins.</summary>
        SceneExit,

        /// <summary>Checkpoint hit. Gated by GameStateManager.</summary>
        Checkpoint,

        /// <summary>User-initiated (escape menu, settings menu). Gated by GameStateManager.</summary>
        Manual,

        /// <summary>Explicit "save game" that also refreshes the screenshot. Gated by GameStateManager.</summary>
        ManualWithScreenshot,
    }
}
