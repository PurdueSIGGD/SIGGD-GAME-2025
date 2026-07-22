namespace SIGGD.Save
{
    /// <summary>
    /// Marker interface for save modules whose <see cref="ISaveModule.Apply"/> can safely be
    /// invoked immediately after deserialize, without waiting for a scene singleton to become
    /// ready. <see cref="SaveManager"/> calls <see cref="ISaveModule.Apply"/> automatically on
    /// modules that also implement this interface.
    /// </summary>
    /// <remarks>
    /// Use for modules whose target lives outside the scene lifecycle (e.g. a
    /// <c>LazySingleton</c> service, or the FMOD runtime). Do NOT implement this on modules
    /// whose <see cref="ISaveModule.Apply"/> depends on a <see cref="UnityEngine.MonoBehaviour"/>
    /// singleton that finishes initialising in its own <c>Start</c> — those must be applied
    /// from the scene singleton itself via <see cref="SaveManager.Apply{T}"/> or
    /// <see cref="SaveManager.WhenGameplayReady"/>.
    /// </remarks>
    public interface IAutoApplyOnLoad
    {
    }
}
