namespace EthernetSwitch.Infrastructure
{
    public interface IBashCommand
    {
        string Execute(string command);
    }
}