using System.Threading.Tasks;

namespace Desafio.Umbler.Interfaces
{
    public interface IDnsService
    {
        Task<(string Ip, int Ttl)> GetIpAndTtlAsync(string domainName);
    }
}