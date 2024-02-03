namespace ark.bible.analysis
{
    public class FileHelper
    {
        public static void Create()
        {

        }
        public static master_data? GetMasterData()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<master_data>(System.IO.File.ReadAllText(System.IO.Path.Combine("Data", "master_data.json")));
        }
    }
}
