using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Bash;

namespace EthernetSwitch.Tests
{
    public class LLDPTests
    {
        public LLDPServices _service;

        [SetUp]
        public async Task SetupAsync()
        {
            _service = new LLDPServices(new BashCommand());
            _service.ActivateLLDPAgent();
            await Task.Delay(30000);
        }

        [Test]
        public async Task ShouldExecuteSNMPWalkResultAndGetOIDs()
        {
            var neighbour = _service.GetNeighbours().FirstOrDefault();

            Assert.That(neighbour.Capability, Is.Not.Null);
            Assert.That(neighbour.EthernetInterfaceName, Is.Not.Null);
            Assert.That(neighbour.IPAddress, Is.Not.Null);
            Assert.That(neighbour.MAC, Is.Not.Null);
            Assert.That(neighbour.SystemDescription, Is.Not.Null);
            Assert.That(neighbour.SystemName, Is.Not.Null);
            Assert.That(neighbour.Updated, Is.Not.Null);

        }
    }
}