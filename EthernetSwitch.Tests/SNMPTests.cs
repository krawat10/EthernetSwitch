
using EthernetSwitch.Infrastructure.SNMP;
using EthernetSwitch.Infrastructure.SNMP.Queries;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Net;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Settings;
using Moq;
using EthernetSwitch.Data.Models;
using System;
using EthernetSwitch.Infrastructure.Extensions;
using EthernetSwitch.Infrastructure.SNMP.Commands;

namespace EthernetSwitch.Tests
{
    public class SNMPTests
    {
        public SNMPServices _service;
        public Mock<ISettingsRepository> _settingsRepositoryMock;

        private readonly SNMPConfiguration configuration = new SNMPConfiguration
        {
            AgentAddresses = "udp:161",
            SysContact = Guid.NewGuid().ToString(),
            SysLocation = Guid.NewGuid().ToString(),
            TrapRecieverIpAddress = IPAddress.Loopback.ToString(),
            TrapRecieverPort = 162
        };

        private readonly SNMPUser desUser = new SNMPUser
        {
            Encryption = StringExtensions.RandomString(12),
            EncryptionType = EncryptionType.DES,
            Password = StringExtensions.RandomString(12),
            UserName = StringExtensions.RandomString(12),
        };

        private readonly SNMPUser aesUser = new SNMPUser
        {
            Encryption = StringExtensions.RandomString(10),
            EncryptionType = EncryptionType.AES,
            Password = StringExtensions.RandomString(10),
            UserName = StringExtensions.RandomString(10)
        };

        [SetUp]
        public async Task SetupAsync()
        {
            _settingsRepositoryMock = new Mock<ISettingsRepository>();
            _settingsRepositoryMock.Setup(x => x.GetSettings()).ReturnsAsync(new Settings());

            _service = new SNMPServices(new LoggerFactory(), new BashCommand(), _settingsRepositoryMock.Object);

            await _service.Handle(configuration);
            await _service.Handle(aesUser);
            await _service.Handle(desUser);
            await Task.Delay(5000);
        }

        [Test]
        public async Task ShouldExecuteSNMPWalkResultAndGetOIDs()
        {

            var oids = await _service.Handle(new WalkQuery("public", "1.3.6.1.2.1.1", IPAddress.Loopback, 161));
            CollectionAssert.IsNotEmpty(oids);
            Assert.That(oids, Has.One.With.Property("Id").EqualTo("1.3.6.1.2.1.1.6.0"));
        }

        [Test]
        public async Task ShouldExecuteSNMPGetAndGetSysLocationOID()
        {
            var oid = await _service.Handle(new GetV3Query(
                aesUser.UserName, 
                VersionCode.V3, 
                IPAddress.Loopback, 161,
                aesUser.Password,
                aesUser.Encryption,
                aesUser.EncryptionType,
                "1.3.6.1.2.1.1.6.0"));

            Assert.IsNotNull(oid);
            Assert.AreEqual(oid.Value, configuration.SysLocation);
            Assert.AreEqual(oid.Id, "1.3.6.1.2.1.1.6.0");
        }


        [Test, MaxTime(30000)]
        public async Task ShouldExecuteSNMPGetAndSetSysLocationOID()
        {
            var newSysLocation = StringExtensions.RandomString(10);
            
            var old_oid = await _service.Handle(new GetV3Query(
                desUser.UserName,
                VersionCode.V3,
                IPAddress.Loopback,
                161,
                desUser.Password,
                desUser.Encryption,
                desUser.EncryptionType,
                "1.3.6.1.2.1.1.5.0"));


            await _service.Handle(new SetV3Command(
                desUser.UserName,
                VersionCode.V3,
                IPAddress.Loopback, 
                161,
                desUser.Password,
                desUser.Encryption,
                desUser.EncryptionType,
                new OID { Id = "1.3.6.1.2.1.1.5.0", Value = newSysLocation }));

            var oid = await _service.Handle(new GetV3Query(
                desUser.UserName,
                VersionCode.V3,
                IPAddress.Loopback,
                161,
                desUser.Password,
                desUser.Encryption,
                desUser.EncryptionType,
                "1.3.6.1.2.1.1.5.0"));

            Assert.IsNotNull(oid);
            Assert.AreEqual(oid.Value, newSysLocation);
            Assert.AreNotSame(oid.Value, old_oid.Value);
        }
    }
}