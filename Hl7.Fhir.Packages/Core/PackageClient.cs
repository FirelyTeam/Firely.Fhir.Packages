using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{

    public class PackageClient : IDisposable
    {
        public static PackageClient Create(string source, bool npm = false, bool insecure = false)
        {
            var urlprovider = npm ? (IPackageUrlProvider)new NodePackageUrlProvider(source) : new FhirPackageUrlProvider(source);

            return new PackageClient(urlprovider, Console.WriteLine, insecure);

        }

        public static PackageClient Create()
        {
            var source = "https://packages.simplifier.net";
            var urlprovider = new FhirPackageUrlProvider(source);
            return new PackageClient(urlprovider, null);
        }

        public PackageClient(IPackageUrlProvider urlProvider, Action<string> report, bool insecure = false)
        {
            this.urlProvider = urlProvider;
            this.report = report;

            this.httpClient = insecure ? Testing.GetInsecureClient() : new HttpClient();
        }

        readonly IPackageUrlProvider urlProvider;
        readonly Action<string> report;
        readonly HttpClient httpClient;

        private void Report(string message)
        {
            report?.Invoke(message);
        }

        public async ValueTask<string> DownloadListingRawAsync(PackageReference reference)
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

        public async ValueTask<string> DownloadListingRawAsync(string pkgname, string version = null)
        {
            var reference = new PackageReference(pkgname, version);
            var body = await DownloadListingRawAsync(reference);
            return body;
        }

        public async ValueTask<PackageListing> DownloadListingAsync(PackageReference reference)
        {
            var body = await DownloadListingRawAsync(reference);
            if (body is null) return null;

            return Parser.Deserialize<PackageListing>(body);
        }

        public async ValueTask<PackageListing> DownloadListingAsync(string pkgname, string version = null)
        {
            var reference = new PackageReference(pkgname, version);
            var body = await DownloadListingRawAsync(reference);
            if (body is null) return null;
            return Parser.Deserialize<PackageListing>(body);
        }

        public async ValueTask<IList<string>> FindPackageByName(string partial)
        {
            string url = $"{urlProvider.Root}/find?name={partial}";
            try
            {
                var body = await httpClient.GetStringAsync(url);
                var result = Parser.Deserialize<List<string>>(body);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async ValueTask<IList<string>> FindPackageByCanonical(string canonical)
        {
            string url = $"{urlProvider.Root}/find?canonical={canonical}";
            try
            {
                var body = await httpClient.GetStringAsync(url);
                var result = Parser.Deserialize<List<string>>(body);
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        internal async ValueTask<byte[]> DownloadPackage(PackageReference reference)
        {
            string url = urlProvider.GetPackageUrl(reference);
            return await httpClient.GetByteArrayAsync(url);
        }

        #region IDisposable

        bool disposed;

        void IDisposable.Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // [WMR 20181102] HttpClient will dispose internal HttpClientHandler/WebRequestHandler
                    httpClient?.Dispose();
                }

                // release any unmanaged objects
                // set the object references to null

                disposed = true;
            }
        }

        #endregion

        public override string ToString() => urlProvider.ToString();

    }

}
