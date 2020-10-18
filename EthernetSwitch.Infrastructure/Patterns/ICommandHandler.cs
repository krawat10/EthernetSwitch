using System.Threading.Tasks;

namespace EthernetSwitch.Infrastructure.Patterns
{
    public interface ICommandHandler<in TCommand>
    {
        Task Handle(TCommand command);
    }
}