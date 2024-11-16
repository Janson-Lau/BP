using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BPClassLibrary
{
    public static class JsonUtils
    {
        public static string ObjectToJson(object obj, string path)
        {
            var jsonStr = JsonConvert.SerializeObject(obj,Formatting.Indented);
            File.WriteAllText(path, jsonStr);
            return jsonStr;
        }

        public static T JsonToObject<T>(string path)
        {
            string jsonFromFile = File.ReadAllText(path);
            T obj = JsonConvert.DeserializeObject<T>(jsonFromFile);
            return obj;
        }
    }
}
