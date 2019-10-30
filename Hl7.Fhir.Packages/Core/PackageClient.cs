using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

            return new PackageClient(urlprovider, insecure);
            
        }

        public static PackageClient Create()
        {
            var provider = PackageUrlProvider.Localhost;
            return new PackageClient(provider);
        }

        public PackageClient(IPackageUrlProvider urlProvider, bool insecure = false)
        {
            this.urlProvider = urlProvider;
            this.httpClient = insecure ? Testing.GetInsecureClient() : new HttpClient();
        }

        readonly IPackageUrlProvider urlProvider;
        readonly HttpClient httpClient;

        public async ValueTask<string> DownloadListingRawAsync(string pkgname)
        {
           
            var url = urlProvider.GetPackageListingUrl(pkgname);
            try
            {
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async ValueTask<PackageListing> DownloadListingAsync(string pkgname)
        {
            var body = await DownloadListingRawAsync(pkgname);
            if (body is null) return null;
            return Parser.Deserialize<PackageListing>(body);
        }

        public async ValueTask<List<PackageCatalogEntry>> CatalogPackagesAsync(
            string pkgname = null, 
            string canonical = null, 
            string fhirversion = null,
            bool preview = false)
        { 
            var parameters = new NameValueCollection();
            parameters.AddWhenValued("name", pkgname);
            parameters.AddWhenValued("canonical", canonical);
            parameters.AddWhenValued("fhirversion", fhirversion);
            parameters.AddWhenValued("prerelease", preview ? "true" : "false");
            string query = parameters.ToQueryString();

            string url = $"{urlProvider.Root}/catalog?{query}";

            try
            {
                var body = await httpClient.GetStringAsync(url);
                var result = Parser.Deserialize<List<PackageCatalogEntry>>(body);
                return result;
            }
            catch
            {
                return null;
            }
        }

        internal async ValueTask<byte[]> DownloadPackage(PackageReference reference)
        {
            string url = urlProvider.GetPackageUrl(reference);
            return await httpClient.GetByteArrayAsync(url);
        }

        public async ValueTask<HttpStatusCode> Publish(PackageReference reference, byte[] buffer)
        {
            string url = urlProvider.GetPublishUrl(3, reference, PublishMode.Any);
            var content = new ByteArrayContent(buffer);
            var response = await httpClient.PostAsync(url, content);

            return response.StatusCode;
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


    public static class PackageClientExtensions
    {
        public static async ValueTask<string> DownloadListingRawAsync(this PackageClient client, PackageReference reference)
        {
            if (reference.Version != null && reference.Version.StartsWith("git"))
            {
                throw new NotImplementedException("We cannot yet resolve git references");
            }

            return await client.DownloadListingRawAsync(reference.Name);
        }


        public static async ValueTask<IList<string>> FindPackageByName(this PackageClient client, string partial)
        {
            // backwards compatibility
            var result = await client.CatalogPackagesAsync(pkgname: partial);
            return result.Select(c => c.Name).ToList();
        }

        public static async ValueTask<IList<string>> FindPackagesByCanonical(this PackageClient client, string canonical)
        {
            // backwards compatibility
            var result = await client.CatalogPackagesAsync(canonical: canonical);
            return result.Select(c => c.Name).ToList();
        }

        
    }
}
