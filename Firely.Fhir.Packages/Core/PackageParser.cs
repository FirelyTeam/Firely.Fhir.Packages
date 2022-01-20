#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Firely.Fhir.Packages
{
    public static class PackageParser
    {
        internal static T? Deserialize<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content, SETTINGS);
            }
            catch
            {
                return default;
            }
        }

        private static readonly JsonSerializerSettings SETTINGS = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        };



        public static PackageManifest? ReadManifest(string content)
        {
            return JsonConvert.DeserializeObject<PackageManifest>(content);
        }

        public static PackageManifest? ReadManifest(byte[] buffer)
        {
            string contents = Encoding.UTF8.GetString(buffer);
            return PackageParser.ReadManifest(contents);
        }

        public static string WriteManifest(PackageManifest manifest)
        {
            return JsonConvert.SerializeObject(manifest, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) + "\n";
            //return JsonConvert.SerializeObject(manifest, Formatting.Indented )+"\n";
        }

        public static string JsonMergeManifest(PackageManifest manifest, string original)
        {
            var jmanifest = JObject.FromObject(manifest, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            var jcontent = JObject.Parse(original);

            var settings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Ignore };
            jcontent.Remove("dependencies");
            jcontent.Merge(jmanifest, settings);
            return jcontent.ToString() + "\n";
        }

        internal static LockFileJson? ReadLockFileJson(string content)
        {
            return JsonConvert.DeserializeObject<LockFileJson>(content);
        }

        internal static string WriteLockFileDto(LockFileJson dto)
        {
            return JsonConvert.SerializeObject(dto, Formatting.Indented) + "\n";
        }

        internal static CanonicalIndex? ReadCanonicalIndex(string content)
        {
            return JsonConvert.DeserializeObject<CanonicalIndex>(content);
        }

        internal static string WriteCanonicalIndex(CanonicalIndex references)
        {
            return JsonConvert.SerializeObject(references, Formatting.Indented);
        }

        /// <summary>
        /// Writes an IndexJson to a string
        /// </summary>
        /// <param name="index">index object to be serialized to json</param>
        /// <returns>string representation of an index.json object</returns>
        internal static string WriteIndexJson(IndexJson index)
        {
            return JsonConvert.SerializeObject(index, Formatting.Indented);
        }

    }


}

#nullable restore
