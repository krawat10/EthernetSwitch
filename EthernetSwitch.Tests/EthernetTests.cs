using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Ethernet;
using EthernetSwitch.Infrastructure.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace EthernetSwitch.Tests
{
    public class EthernetTests
    {
        private EthernetServices _service;
        public Mock<ISettingsRepository> _settingsRepositoryMock;

        [SetUp]
        public async Task SetupAsync()
        {
            _settingsRepositoryMock = new Mock<ISettingsRepository>();
            _settingsRepositoryMock.Setup(x => x.GetSettings()).ReturnsAsync(new Settings());

            _service = new EthernetServices(new BashCommand(), new NullLoggerFactory());
        }

        [Test]
        public async Task ShouldReturnListOfInterface()
        {
            var ethernetInterfaces = _service.GetEthernetInterfaces().ToList();
            CollectionAssert.IsNotEmpty(ethernetInterfaces);
        }

        [Test]
        public async Task ShouldApplyWLANToInterface()
        {
            var vlans = new List<string> { "1" };

            var ethernetInterfaceBefore = _service
                .GetEthernetInterfaces()
                .First();
            
            _service.ClearEthernetInterfaceVLANs(ethernetInterfaceBefore.Name);
            _service.ApplyEthernetInterfaceVLANs(ethernetInterfaceBefore.Name, false, vlans);
            _service.SetEthernetInterfaceType(ethernetInterfaceBefore.Name, InterfaceType.Off);

            var ethernetInterfaceAfter = _service
                .GetEthernetInterfaces()
                .First(x => x.Name == ethernetInterfaceBefore.Name);

            Assert.That(ethernetInterfaceAfter.VirtualLANs, Has.Count.EqualTo(1));
            Assert.That(ethernetInterfaceAfter.VirtualLANs, Has.One.EqualTo("1"));  
        }

        [Test]
        public async Task ShouldApplyTaggedWLANToInterface()
        {
            var vlans = new List<string> { "0", "4094" };

            var ethernetInterfaceBefore = _service
                .GetEthernetInterfaces()
                .First();
            
            _service.ClearEthernetInterfaceVLANs(ethernetInterfaceBefore.Name);
            _service.ApplyEthernetInterfaceVLANs(ethernetInterfaceBefore.Name, true, vlans);

            var ethernetInterfaceAfter = _service
                .GetEthernetInterfaces()
                .First(x => x.Name == ethernetInterfaceBefore.Name);

            Assert.True(ethernetInterfaceAfter.Tagged);
            Assert.That(ethernetInterfaceAfter.VirtualLANs, Is.EquivalentTo(vlans));
        }

        [Test]
        public async Task ShouldApplyInterfaceType([Values(InterfaceType.Off, InterfaceType.Isolated, InterfaceType.Promiscuous)] InterfaceType type)
        {
            var vlans = new List<string> { "1" };

            var ethernetInterfaceBefore = _service
                .GetEthernetInterfaces()
                .First();

            _service.ClearEthernetInterfaceVLANs(ethernetInterfaceBefore.Name);
            _service.ApplyEthernetInterfaceVLANs(ethernetInterfaceBefore.Name, false, vlans);
            _service.SetEthernetInterfaceType(ethernetInterfaceBefore.Name, type);

            var ethernetInterfaceAfter = _service
                .GetEthernetInterfaces()
                .First(x => x.Name == ethernetInterfaceBefore.Name);

            Assert.AreEqual(ethernetInterfaceAfter.Type, type);
            Assert.False(ethernetInterfaceAfter.Tagged);
            Assert.That(ethernetInterfaceAfter.VirtualLANs, Is.EquivalentTo(vlans));
        }
    }
}