namespace Tools.PlayStatus
{
    public enum Level
    {
        Easy,
        Normal,
        Hard,
        Master
    }
    public enum Achieve
    {
        AllPerfect = 3,
        FullCombo = 2,
        Clear = 1
    }
    
    public partial class PlayStatusHandler
    {

        private static int _selectedMusic;
        private static Level _selectedLevel = Level.Easy;

        public static int GetSelectedMusic() => _selectedMusic;

        public static void SetSelectedMusic(int num) => _selectedMusic = num;

        public static Level GetSelectedLevel() => _selectedLevel;

        public static void SetSelectedLevel(Level level) => _selectedLevel = level;
    }
}