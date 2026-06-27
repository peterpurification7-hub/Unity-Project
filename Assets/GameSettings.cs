public static class GameSettings
{
    public enum Difficulty { Easy, Normal, Hard }
    public static Difficulty SelectedDifficulty = Difficulty.Normal;

    // NEW: 0 = Start, 1 = Tower 1, 2 = Tower 2, 3 = Tower 3
    public static int LastCheckpointIndex = 0;
}