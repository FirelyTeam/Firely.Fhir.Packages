using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public interface IPackageServer
    {
        Task<Versions> GetVersionsAsync(string name);
        Task<byte[]> GetPackageAsync(PackageReference reference);
    }
}
