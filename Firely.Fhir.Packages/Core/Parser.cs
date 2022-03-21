using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System;

namespace Firely.Fhir.Packages
{
    public static class Parser
    {
        public static T Deserialize<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content, settings);
            }
            catch
            {
                return default;
            }
        }

        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        };



        public static PackageManifest ParseManifest(string content)
        {
            return JsonConvert.DeserializeObject<PackageManifest>(content);
        }

        public static PackageManifest ParseManifest(byte[] buffer)
        {
            string contents = Encoding.UTF8.GetString(buffer);
            return Parser.ParseManifest(contents);
        }

        public static string SerializeManifest(PackageManifest manifest)
        {
            return JsonConvert.SerializeObject(manifest, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore } )+"\n";
            //return JsonConvert.SerializeObject(manifest, Formatting.Indented )+"\n";
        }

        public static string JsonMergeManifest(PackageManifest manifest, string original)
        {
            var jmanifest = JObject.FromObject(manifest, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            var jcontent = JObject.Parse(original);

            var settings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Ignore };
            jcontent.Remove("dependencies");
            jcontent.Merge(jmanifest, settings);
            return jcontent.ToString()+"\n";
        }

        public static LockFileJson ParseLockFileJson(string content)
        {
            return JsonConvert.DeserializeObject<LockFileJson>(content);
        }

        public static string SerializeLockFileDto(LockFileJson dto)
        {
            return JsonConvert.SerializeObject(dto, Formatting.Indented)+"\n";
        }

        public static CanonicalIndex ParseCanonicalIndex(string content)
        {
            return JsonConvert.DeserializeObject<CanonicalIndex>(content);
        } 

        public static string SerializeCanonicalIndex(CanonicalIndex references)
        {
            return JsonConvert.SerializeObject(references, Formatting.Indented);
        }

    }


}


