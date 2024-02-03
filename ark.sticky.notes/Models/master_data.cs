namespace ark.bible.analysis
{
    public class master_data
    {
        public Back_Translsation back_translsation { get; set; }
    }

    public class Back_Translsation
    {
        public string key { get; set; }
        public string folder_path { get; set; }
        public List<verse_compare> data { get; set; }
    }

    public class verse_compare
    {
        public string title { get; set; }
        public string prefix { get; set; }
    }

}