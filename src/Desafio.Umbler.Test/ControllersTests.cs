using Desafio.Umbler.Controllers;
using Desafio.Umbler.Models;
using Desafio.Umbler.Interfaces;
using Desafio.Umbler.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        public void Home_Index_returns_View()
        {
            //arrange 
            var controller = new HomeController();

            //act
            var response = controller.Index();
            var result = response as ViewResult;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Home_Error_returns_View_With_Model()
        {
            //arrange 
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //act
            var response = controller.Error();
            var result = response as ViewResult;
            var model = result.Model as ErrorViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task Domain_In_Database()
        {
            //arrange 
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var domainName = "test.com";
            var domain = new Domain { Id = 1, Ip = "192.168.0.1", Name = domainName, UpdatedAt = DateTime.Now, HostedAt = "umbler.corp", Ttl = 60, WhoIs = "Ns.umbler.com" };

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(domain);
                await db.SaveChangesAsync();
            }

            var mockDnsService = new Mock<IDnsService>();
            var mockWhoisService = new Mock<IWhoisService>();

            using (var db = new DatabaseContext(options))
            {
                var controller = new DomainController(db, mockWhoisService.Object, mockDnsService.Object);

                //act
                var response = await controller.Get(domainName);
                var result = response as OkObjectResult;
                var obj = result.Value as DomainResponseDTO;

                //assert
                Assert.IsNotNull(obj);
                Assert.AreEqual(domain.Ip, obj.Ip);
                Assert.AreEqual(domain.Name, obj.Name);
                Assert.AreEqual(domain.HostedAt, obj.HostedAt);

                mockDnsService.Verify(x => x.GetIpAndTtlAsync(It.IsAny<string>()), Times.Never);
            }
        }

        [TestMethod]
        public async Task Domain_Not_In_Database_Calls_Services_And_Saves()
        {
            //arrange 
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var domainName = "novo-dominio.com";
            var fakeIp = "10.0.0.1";
            var fakeTtl = 300;
            var fakeWhois = "Mocked Whois Data";
            var fakeHostedAt = "Mock Hosting LTDA";

            var mockDnsService = new Mock<IDnsService>();
            mockDnsService.Setup(x => x.GetIpAndTtlAsync(domainName)).ReturnsAsync((fakeIp, fakeTtl));

            var mockWhoisService = new Mock<IWhoisService>();
            mockWhoisService.Setup(x => x.GetWhoisRawDataAsync(domainName)).ReturnsAsync(fakeWhois);
            mockWhoisService.Setup(x => x.GetHostedAtAsync(fakeIp)).ReturnsAsync(fakeHostedAt);

            using (var db = new DatabaseContext(options))
            {
                var controller = new DomainController(db, mockWhoisService.Object, mockDnsService.Object);

                //act
                var response = await controller.Get(domainName);
                var result = response as OkObjectResult;
                var obj = result.Value as DomainResponseDTO;

                //assert
                Assert.IsNotNull(obj);
                Assert.AreEqual(fakeIp, obj.Ip);
                Assert.AreEqual(domainName, obj.Name);

                mockDnsService.Verify(x => x.GetIpAndTtlAsync(domainName), Times.Once);
                mockWhoisService.Verify(x => x.GetWhoisRawDataAsync(domainName), Times.Once);
            }

            using (var db = new DatabaseContext(options))
            {
                var savedDomain = await db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);
                Assert.IsNotNull(savedDomain);
                Assert.AreEqual(fakeIp, savedDomain.Ip);
            }
        }
    }
}