using System;

namespace Wikiled.Console.Arguments;

public class ExecutionContext
{
    public ExecutionContext(string name, AppConfig config)
    {
        CommandName = name ?? throw new ArgumentNullException(nameof(name));
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public string CommandName { get; }

    public AppConfig Config { get; }
}