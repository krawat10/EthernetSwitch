using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Bash;

namespace EthernetSwitch.Tests
{
    public class LLDPTests
    {
        public LLDPServices Service;

        [SetUp]
        public async Task SetupAsync()
        {
            Service = new LLDPServices(new BashCommand());
            Service.ActivateLLDPAgent();
            await Task.Delay(30000);
        }

        [Test]
        public async Task ShouldGetAtLeastOneNeighbor()
        {
            var neighbour = Service.GetNeighbours().FirstOrDefault();

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