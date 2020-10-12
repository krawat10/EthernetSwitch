using System.Threading.Tasks;

namespace EthernetSwitch.Infrastructure.Patterns
{
    public interface IQueryHandler<in TQuery, TResponse>
    {
        Task<TResponse> Handle(TQuery command);
    }
}