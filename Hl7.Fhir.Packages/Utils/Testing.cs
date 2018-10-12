using System.Net.Http;

namespace Hl7.Fhir.Packages
{


    public static class Testing
    {
        public static HttpClient GetInsecureClient()
        {
            // for testing without proper certificate
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            var client = new HttpClient(httpClientHandler, true);
            return client;
        }
    }


}
