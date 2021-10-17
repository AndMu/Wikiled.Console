namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter
    {
        string Name { get; }

        IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new();
    }
}