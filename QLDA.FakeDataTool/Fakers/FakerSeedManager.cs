namespace QLDA.FakeDataTool.Fakers;

/// <summary>
/// Global seed manager for deterministic faker generation.
/// </summary>
public static class FakerSeedManager
{
    private static int _currentSeed = 12345;

    /// <summary>
    /// Get current seed value.
    /// </summary>
    public static int CurrentSeed => _currentSeed;

    /// <summary>
    /// Set seed for deterministic generation.
    /// </summary>
    public static void SetSeed(int seed)
    {
        _currentSeed = seed;
    }
}