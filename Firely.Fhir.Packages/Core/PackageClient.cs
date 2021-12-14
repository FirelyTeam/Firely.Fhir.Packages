#nullable enable

using Hl7.Fhir.Specification;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public class PackageClient : IPackageServer, IDisposable
    {
        public static PackageClient Create(string source, bool npm = false, bool insecure = false)
        {
            var urlprovider = npm ? (IPackageUrlProvider)new NodePackageUrlProvider(source) : new FhirPackageUrlProvider(source);
            var httpClient = insecure ? Testing.GetInsecureClient() : new HttpClient();

            return new PackageClient(urlprovider, httpClient);

        }

        public static PackageClient Create()
        {
            var provider = PackageUrlProviders.Simplifier;
            return new PackageClient(provider);
        }

        public PackageClient(IPackageUrlProvider urlProvider, HttpClient? client = null)
        {
            this._urlProvider = urlProvider;
            this._httpClient = client ?? new HttpClient();
        }

        private readonly IPackageUrlProvider _urlProvider;
        private readonly HttpClient _httpClient;

        public async ValueTask<string?> DownloadListingRawAsync(string? pkgname)
        {
            if (pkgname is null)
            {
                return null;
            }

            var url = _urlProvider.GetPackageListingUrl(pkgname);
            try
            {
                var response = await _httpClient.GetAsync(url);
                return response.StatusCode == HttpStatusCode.OK
                    ? await response.Content.ReadAsStringAsync()
                    : response.StatusCode == HttpStatusCode.NotFound ? null : null;
            }
            catch
            {
                return null;
            }
        }

        public async ValueTask<PackageListing?> DownloadListingAsync(string pkgname)
        {
            var body = await DownloadListingRawAsync(pkgname);
            return body is null ? null : Parser.Deserialize<PackageListing>(body);
        }

        public async ValueTask<List<PackageCatalogEntry>> CatalogPackagesAsync(
            string? pkgname = null,
            string? canonical = null,
            string? fhirversion = null,
            bool preview = false)
        {
            var parameters = new NameValueCollection();
            parameters.AddWhenValued("name", pkgname);
            parameters.AddWhenValued("canonical", canonical);
            parameters.AddWhenValued("fhirversion", fhirversion);
            parameters.AddWhenValued("prerelease", preview ? "true" : "false");
            string query = parameters.ToQueryString();

            string url = $"{_urlProvider.Root}/catalog?{query}";

            try
            {
                var body = await _httpClient.GetStringAsync(url);
                var result = Parser.Deserialize<List<PackageCatalogEntry>>(body);
                return result ?? new List<PackageCatalogEntry>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null ?? new List<PackageCatalogEntry>();
            }
        }

        internal async ValueTask<byte[]> DownloadPackage(PackageReference reference)
        {
            string url = _urlProvider.GetPackageUrl(reference);
            return await _httpClient.GetByteArrayAsync(url);
        }

        public async ValueTask<HttpResponseMessage> Publish(PackageReference reference, FhirRelease release, byte[] buffer)
        {
            string url = _urlProvider.GetPublishUrl(release, reference, PublishMode.Any);
            var content = new ByteArrayContent(buffer);
            var response = await _httpClient.PostAsync(url, content);

            return response;
        }

        #region IDisposable

        private bool _disposed;

        void IDisposable.Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // [WMR 20181102] HttpClient will dispose internal HttpClientHandler/WebRequestHandler
                    _httpClient?.Dispose();
                }

                // release any unmanaged objects
                // set the object references to null

                _disposed = true;
            }
        }

        #endregion

        public override string? ToString() => _urlProvider.ToString();

        public async Task<Versions?> GetVersions(string name)
        {
            var listing = await DownloadListingAsync(name);
            return listing is null ? new Versions() : listing.ToVersions();
        }

        public async Task<byte[]> GetPackage(PackageReference reference)
        {
            return await DownloadPackage(reference);
        }
    }
}

#nullable restore