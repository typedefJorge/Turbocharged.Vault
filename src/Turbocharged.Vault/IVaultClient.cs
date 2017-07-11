using System.Collections.Generic;
using System.Threading.Tasks;

namespace Turbocharged.Vault
{
    interface IVaultClient
    {
        Task AuthenticateAsync(IAuthenticationMethod authentication);
        Task<SealStatus> SealStatusAsync();
        Task SealAsync();
        Task<SealStatus> UnsealAsync(string unsealKey);
        Task WriteSecretAsync(string path, IEnumerable<KeyValuePair<string, object>> value);
        Task DeleteSecretAsync(string path);
        Task<Lease> LeaseAsync(string path);
        Task<Lease> RenewAsync(Lease lease);
        Task<List<Mount>> GetMountsAsync();
    }
}
