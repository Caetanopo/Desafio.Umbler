using Desafio.Umbler.DTOs;
using Desafio.Umbler.Interfaces;
using Desafio.Umbler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Desafio.Umbler.Controllers
{
    [Route("api")]
    public class DomainController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly IWhoisService _whoisService;
        private readonly IDnsService _dnsService;

        public DomainController(DatabaseContext db, IWhoisService whoisService, IDnsService dnsService)
        {
            _db = db;
            _whoisService = whoisService;
            _dnsService = dnsService;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
                return BadRequest("O nome do domínio é obrigatório.");

            var domain = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

            if (domain == null || DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)
            {
                domain ??= new Domain(); 

                var (ip, ttl) = await _dnsService.GetIpAndTtlAsync(domainName);
                var whoisRaw = await _whoisService.GetWhoisRawDataAsync(domainName);
                var hostedAt = await _whoisService.GetHostedAtAsync(ip);

                domain.Name = domainName;
                domain.Ip = ip;
                domain.UpdatedAt = DateTime.Now;
                domain.WhoIs = whoisRaw;
                domain.Ttl = ttl;
                domain.HostedAt = hostedAt;

                if (domain.Id == 0)
                    _db.Domains.Add(domain);

                await _db.SaveChangesAsync();
            }

            var responseDTO = new DomainResponseDTO
            {
                Name = domain.Name,
                Ip = domain.Ip,
                HostedAt = domain.HostedAt,
                WhoIs = domain.WhoIs
            };

            return Ok(responseDTO);
        }
    }
}