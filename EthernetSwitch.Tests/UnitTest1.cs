
using EthernetSwitch.Infrastructure.SNMP;
using EthernetSwitch.Infrastructure.SNMP.Queries;
using Lextm.SharpSnmpLib;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Net;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Settings;
using Moq;

namespace EthernetSwitch.Tests
{
    public class SNMPTests
    {
        public SNMPServices _service;

        [SetUp]
        public void Setup()
        {
            _service = new SNMPServices(new LoggerFactory(), new BashCommand(), new Mock<ISettingsRepository>().Object, new Mock<ISNMPUsersRepository>().Object);
        }

        [Test]
        public async Task ShouldExecuteSNMPWalkResultAndGetOIDs()
        {
            var oids = await _service.Handle(new WalkQuery("public", "", IPAddress.Loopback, 161));

            CollectionAssert.IsNotEmpty(oids);
        }

        [Test]
        public async Task ShouldExecuteSNMPGetAndGetOID()
        {
            var oid = await _service.Handle(new GetV3Query("username", VersionCode.V3, IPAddress.Loopback, 161, "password", "encryption", "oidId"));

            Assert.NotNull(oid);
        }
    }
}