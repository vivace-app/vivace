public static class MusicTitleDataStore
{
    public static readonly string[] musicTitles = {
        // Music Titles
        "shining_star",     // 0
        "floating_blue",    // pho - Floating Blue (Prototype)
        "haruno_otozure",   // 2
        "burning_heart",    // 3
        "reversed_lonely_line",//pho feat.重音テト - 反転孤独線
        "pale_blue",        // pho feat. 重音テト - 淡色
        "see_you_again",    // pho feat. 重音テト - またね
        "clock_tower",      // 智重 - 時計塔
        "gale",             // 智重 - 疾風
        "looking_the_last", // 智重 - 終点目指して
        "spring_visit",     // 智重 - 春の訪れ
        "sunny_day",        // 智重 - Sunny Day
        "quiet_morning",    // 智重 - Quiet Morning
        "future_vision",    // 智重 - 未来構想図
        "progress",         // 智重 - Progress
        "crystatus",        // 根菜 - CryStatuS
    };

    public static readonly bool[,] musicLevels = {
        //  Easy , Normal , Hard , Demon
        { false , false , false , false },  // shining_star
        { false , false , true , false },   // floating_blue
        { false , false , true , false },   // haruno_otozure
        { false , false , true , false },   // burning_heart
        { false , false , false , false },  // reversed_lonely_line
        { false , false , false , false },  // pale_blue
        { false , false , false , false },  // see_you_again
        { false , false , false , false },  // clock_tower
        { false , false , false , false },  // gale
        { false , false , false , false },  // looking_the_last
        { false , false , false , false },  // spring_visit
        { false , false , false , false },  // sunny_day
        { false , false , false , false },  // quiet_morning
        { false , false , false , false },  // future_vision
        { false , false , false , false },  // progress
        { false , false , false , false },  // crystatus
    };

    public static readonly string[] musicGenres = {
        //Music Genres
        "temp",
        "Hardstyle",
        "Free Style",
        "temp",
        "J-pop",
        "J-pop",
        "J-rock",
        "Free Style",
        "Free Style",
        "Free Style",
        "Free Style",
        "Free Style",
        "Free Style",
        "Free Style",
        "Free Style",
        "Hardcore",
    };
}