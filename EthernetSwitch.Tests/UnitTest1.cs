
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
using EthernetSwitch.Data.Models;
using System;
using EthernetSwitch.Infrastructure.Extensions;
using EthernetSwitch.Infrastructure.SNMP.Commands;

namespace EthernetSwitch.Tests
{
    public class SNMPTests
    {
        public SNMPServices _service;
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
            Encryption = StringExtensions.RandomString(10),
            EncryptionType = EncryptionType.DES,
            Password = StringExtensions.RandomString(10),
            UserName = StringExtensions.RandomString(10)
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
            _service = new SNMPServices(new LoggerFactory(), new BashCommand(), new Mock<ISettingsRepository>().Object);

            await _service.Handle(configuration);
            await _service.Handle(aesUser);
            await _service.Handle(desUser);
        }

        [Test]
        public async Task ShouldExecuteSNMPWalkResultAndGetOIDs()
        {

            var oids = await _service.Handle(new WalkQuery("public", "1.3.6.1.2.1.1.6", IPAddress.Loopback, 161));
            CollectionAssert.IsNotEmpty(oids);
        }

        [Test]
        public async Task ShouldExecuteSNMPGetAndGetSysLocationOID()
        {
            var oid = await _service.Handle(new GetV3Query(
                aesUser.UserName, 
                Lextm.SharpSnmpLib.VersionCode.V3, 
                IPAddress.Loopback, 161,
                aesUser.Password,
                aesUser.Encryption,
                "1.3.6.1.2.1.1.6"));

            Assert.IsNotNull(oid);
            Assert.AreEqual(oid.Value, configuration.SysLocation);
        }


        [Test]
        public async Task ShouldExecuteSNMPGetAndSetSysLocationOID()
        {
            var newSysLocation = StringExtensions.RandomString(10);
            
            await _service.Handle(new SetV3Command(
                desUser.UserName,
                Lextm.SharpSnmpLib.VersionCode.V3,
                IPAddress.Loopback, 
                161,
                desUser.Password,
                desUser.Encryption,
                new OID { Id = "1.3.6.1.2.1.1.6", Value = newSysLocation }));

            var oid = await _service.Handle(new GetV3Query(
                desUser.UserName,
                Lextm.SharpSnmpLib.VersionCode.V3,
                IPAddress.Loopback,
                161,
                desUser.Password,
                desUser.Encryption,
                "1.3.6.1.2.1.1.6"));

            Assert.IsNotNull(oid);
            Assert.AreEqual(oid.Value, newSysLocation);
        }
    }
}