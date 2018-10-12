using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hl7.Fhir.Packages
{

    public class PackageClient
    {
        public PackageClient(IPackageUrlProvider urlProvider, Action<string> report, bool insecure = false)
        {
            this.urlProvider = urlProvider;
            this.report = report;

            this.httpClient = insecure ? Testing.GetInsecureClient() : new HttpClient();
        }

        public static PackageClient Create(string source, bool npm, bool insecure = false)
        {
            var urlprovider = npm ? (IPackageUrlProvider)new NodePackageUrlProvider(source) : new FhirPackageUrlProvider(source);

            return new PackageClient(urlprovider, Console.WriteLine, insecure);

        }

        IPackageUrlProvider urlProvider;
        Action<string> report;
        HttpClient httpClient;

        private void Report(string message)
        {
            report?.Invoke(message);
        }

        public async Task<string> DownloadListingRawAsync(PackageReference reference)
        {
            if (reference.Version != null && reference.Version.StartsWith("git"))
            {
                Report($"Git not implemented. Skipping download of: {reference.Version}");
                return null;
            }

            var url = urlProvider.GetPackageListingUrl(reference);
            var response = await httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                return null;
            }
            
        }

        public async Task<string> DownloadListingRawAsync(string pkgname, string version = null)
        {
            var reference = new PackageReference(pkgname, version);
            var body = await DownloadListingRawAsync(reference);
            return body;
        }

        public async Task<PackageListing> DownloadListingAsync(PackageReference reference)
        {
            var body = await DownloadListingRawAsync(reference);
            if (body is null) return null;
            
            return Parser.Deserialize<PackageListing>(body);
        }

        public async Task<PackageListing> DownloadListingAsync(string pkgname, string version = null)
        {
            var reference = new PackageReference(pkgname, version);
            var body = await DownloadListingRawAsync(reference);
            if (body is null) return null;
            return Parser.Deserialize<PackageListing>(body);
        }

        public async Task<IList<string>> FindCanonical(string canonical)
        {
            string url = urlProvider.Root + "/find?canonical=" + canonical;
            var body = await httpClient.GetStringAsync(url);
            var result = Parser.Deserialize<List<string>>(body);
            return result;
        }

        internal async Task<byte[]> DownloadPackage(PackageReference reference)
        {
            string url = urlProvider.GetPackageUrl(reference); 
            return await httpClient.GetByteArrayAsync(url);
    }

        public override string ToString() => urlProvider.ToString();

    }
 
}
