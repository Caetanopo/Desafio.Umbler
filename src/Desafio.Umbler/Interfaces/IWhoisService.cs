using System.Threading.Tasks;

namespace Desafio.Umbler.Interfaces
{
    public interface IWhoisService
    {
        Task<string> GetWhoisRawDataAsync(string domainName);
        Task<string> GetHostedAtAsync(string ip);
    }
}