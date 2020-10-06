public static class MusicTitleDataStore
{
    public static readonly string[] musicTitles = {
        // Music Titles
        "shining_star",     // 0
        "floating_blue",    // 1
        "haruno_otozure",   // 2
        "burning_heart",    // 3
    };

    public static readonly bool[,] musicLevels = {
        //  Easy , Normal , Hard , Demon
        { false , false , false , false },  // shining_star
        { false , false , true , false },   // floating_blue
        { false , false , true , false },   // haruno_otozure
        { false , false , true , false },   // burning_heart
    };
}