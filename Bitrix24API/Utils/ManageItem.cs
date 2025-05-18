using Bitrix24API.Models;
using System.Text.Json;

namespace Bitrix24API.Utils
{
    public class ManageItem
    {
        private readonly static string DB_PATH = "db.json";
        public static void SaveToFile(BitrixAuthPayload token)
        {
            var json = JsonSerializer.Serialize(token);
            File.WriteAllText(DB_PATH, json);
        }

        public static BitrixAuthPayload ReadFromFile()
        {
            if (!File.Exists(DB_PATH)) return null;

            var json = File.ReadAllText(DB_PATH);
            return JsonSerializer.Deserialize<BitrixAuthPayload>(json);
        }
    }
}
