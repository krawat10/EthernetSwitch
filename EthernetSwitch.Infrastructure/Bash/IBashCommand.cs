namespace EthernetSwitch.Infrastructure.Bash
{
    public interface IBashCommand
    {
        string Execute(string command);
    }
}