#nullable enable

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
        /// <summary>
        /// Creates a package client
        /// </summary>
        /// <param name="source">The package source the client using</param>
        /// <param name="npm">Whether the source is a NPM package source or not</param>
        /// <param name="insecure">Whether to use an insecure connection</param>
        /// <returns>A newly created package client</returns>
        public static PackageClient Create(string source, bool npm = false, bool insecure = false)
        {
            var urlprovider = npm ? (IPackageUrlProvider)new NodePackageUrlProvider(source) : new FhirPackageUrlProvider(source);
            var httpClient = insecure ? Testing.GetInsecureClient() : new HttpClient();

            return new PackageClient(urlprovider, httpClient);

        }

        /// <summary>
        /// Creates a package client to Simplifier.net
        /// </summary>
        /// <returns>A Simplifier.net package client</returns>
        public static PackageClient Create()
        {
            var provider = PackageUrlProviders.Simplifier;
            return new PackageClient(provider);
        }

        /// <summary>
        /// Creates a package client using a custom <see cref="IPackageUrlProvider"/> and optional custom  <see cref="HttpClient"/>        
        /// </summary>
        /// <param name="urlProvider">Custom <see cref="IPackageUrlProvider"/></param>
        /// <param name="client">Custom <see cref="HttpClient"/></param>
        public PackageClient(IPackageUrlProvider urlProvider, HttpClient? client = null)
        {
            this._urlProvider = urlProvider;
            this._httpClient = client ?? new HttpClient();
        }

        private readonly IPackageUrlProvider _urlProvider;
        private readonly HttpClient _httpClient;


        internal async ValueTask<string?> DownloadListingRawAsync(string? pkgname)
        {
            if (pkgname is null)
            {
                return null;
            }

            var url = _urlProvider.GetPackageListingUrl(pkgname);
            try
            {
                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                return response.StatusCode == HttpStatusCode.OK
                    ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                    : response.StatusCode == HttpStatusCode.NotFound ? null : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Download the package listing
        /// </summary>
        /// <param name="pkgname">Name of the package</param>
        /// <returns>Package listing</returns>
        public async ValueTask<PackageListing?> DownloadListingAsync(string pkgname)
        {
            var body = await DownloadListingRawAsync(pkgname).ConfigureAwait(false);
            return body is null ? null : PackageParser.Deserialize<PackageListing>(body);
        }

        /// <summary>
        /// Get a list of package catalogs, based on optional parameters
        /// </summary>
        /// <param name="pkgname">Name of the package</param>
        /// <param name="canonical">the canonical url of an artifact that is in the package</param>
        /// <param name="fhirversion">the FHIR version of a package</param>
        /// <param name="preview">allow for prelease packages</param>
        /// <returns>A list of package catalogs that conform to the parameters</returns>
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
                var body = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
                var result = PackageParser.Deserialize<List<PackageCatalogEntry>>(body);
                return result ?? new List<PackageCatalogEntry>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null ?? new List<PackageCatalogEntry>();
            }
        }

        private async ValueTask<byte[]> downloadPackage(PackageReference reference)
        {
            string url = _urlProvider.GetPackageUrl(reference);
            return await _httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
        }


        /// <summary>
        /// Publish a package to the package source
        /// </summary>
        /// <param name="reference">PackageReference of the package to be published</param>
        /// <param name="fhirRelease">FHIR Release that is used by the package</param>
        /// <param name="buffer">package content</param>
        /// <returns>Http response whether the package has been succesfully published</returns>
        public async ValueTask<HttpResponseMessage> Publish(PackageReference reference, byte[] buffer)
        {
            string url = _urlProvider.GetPublishUrl(reference, PublishMode.Any);
            var content = new ByteArrayContent(buffer);
            var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);

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

        /// <summary>
        /// Retrieve the different versions of a package
        /// </summary>
        /// <param name="name">Package name</param>
        /// <returns>List is versions</returns>
        public async Task<Versions?> GetVersions(string name)
        {
            var listing = await DownloadListingAsync(name);
            return listing is null ? new Versions() : listing.ToVersions();
        }

        /// <summary>
        /// Download a package from the source
        /// </summary>
        /// <param name="reference">Package reference of the package to be downloaded </param>
        /// <returns>Package content as byte array</returns>
        public async Task<byte[]> GetPackage(PackageReference reference)
        {
            return await downloadPackage(reference);
        }
    }
}

#nullable restore