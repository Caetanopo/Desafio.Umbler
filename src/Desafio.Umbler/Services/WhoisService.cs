using System.Threading.Tasks;
using Desafio.Umbler.Interfaces;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public class WhoisService : IWhoisService
    {
        public async Task<string> GetWhoisRawDataAsync(string domainName)
        {
            var response = await WhoisClient.QueryAsync(domainName);
            return response?.Raw;
        }

        public async Task<string> GetHostedAtAsync(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return "Desconhecido";
            var response = await WhoisClient.QueryAsync(ip);
            return response?.OrganizationName;
        }
    }
}