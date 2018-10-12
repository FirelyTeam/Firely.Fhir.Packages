using Newtonsoft.Json;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Hl7.Fhir.Packages
{
    public static class Parser
    {
        public static T Deserialize<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return default;
            }
        }

        public static PackageManifest ReadManifest(string content)
        {
            return JsonConvert.DeserializeObject<PackageManifest>(content);
        }

        public static PackageManifest ReadManifest(byte[] buffer)
        {
            string contents = Encoding.UTF8.GetString(buffer);
            return Parser.ReadManifest(contents);
        }

        public static string WriteManifest(PackageManifest manifest)
        {
            return JsonConvert.SerializeObject(manifest, Formatting.Indented)+"\n";
        }

        //public static string JsonMerge(object thing, string contents)
        //{
        //    var mixin = JObject.FromObject(thing);
        //    var source = JObject.Parse(contents);
        //    var settings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Ignore };
        //    source.Merge(mixin, settings);
        //    return source.ToString();
        //}

        public static string JsonMergeManifest(PackageManifest manifest, string original)
        {
            var jmanifest = JObject.FromObject(manifest);
            var jcontent = JObject.Parse(original);

            var settings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Ignore };
            jcontent.Remove("dependencies");
            jcontent.Merge(jmanifest, settings);
            return jcontent.ToString()+"\n";
        }

        public static AssetsFile ReadAssets(string content)
        {
            return JsonConvert.DeserializeObject<AssetsFile>(content);
        }

        public static string WriteAssets(AssetsFile assets)
        {
            return JsonConvert.SerializeObject(assets, Formatting.Indented)+"\n";
        }

        public static CanonicalReferences ReadReferences(string content)
        {
            return JsonConvert.DeserializeObject<CanonicalReferences>(content);
        } 

        public static string WriteReferences(CanonicalReferences assets)
        {
            return JsonConvert.SerializeObject(assets, Formatting.Indented);
        }

    }


}


