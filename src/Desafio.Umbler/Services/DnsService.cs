using System.Linq;
using System.Threading.Tasks;
using Desafio.Umbler.Interfaces;
using DnsClient;

namespace Desafio.Umbler.Services
{
    public class DnsService : IDnsService
    {
        public async Task<(string Ip, int Ttl)> GetIpAndTtlAsync(string domainName)
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domainName, QueryType.ANY);
            var record = result.Answers.ARecords().FirstOrDefault();

            var ip = record?.Address?.ToString();
            var ttl = record?.TimeToLive ?? 0;

            return (ip, ttl);
        }
    }
}