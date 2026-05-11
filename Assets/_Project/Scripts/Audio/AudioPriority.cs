namespace UnityFramework.MiniGames.Audio
{
    /// <summary>
    /// Higher numeric value interrupts lower priority narration tiers.
    /// </summary>
    public enum AudioPriority
    {
        Ambient = 0,
        Celebration = 10,
        Correction = 40,
        Tutorial = 60
    }
}
