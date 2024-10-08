using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework.Legacy;
using Wikiled.Console.Arguments;
using Wikiled.Console.Tests.Data;

namespace Wikiled.Console.Tests.Arguments;

[TestFixture]
public class AutoStarterTests
{
    private AutoStarter instance;

    private CancellationTokenSource token;

    private IHost host;

    [SetUp]
    public void SetUp()
    {
        token = new CancellationTokenSource();
        instance = CreateInstance();
        instance.RegisterCommand<BlockingCommand, ConfigOne>("One");
        instance.RegisterCommand<SampleCommandTwo, ConfigTwo>("Two");
    }

    [TearDown]
    public void TearDown()
    {
        token.Cancel();
        host?.Dispose();
    }

    [Test]
    public async Task Acceptance()
    {
        host = instance.Build(new[] { "One", "-Data=Test" }).Build();
        await host.StartAsync(token.Token);
        await Task.Delay(100);
            
        var command = (BlockingCommand)host.Services.GetRequiredService<ICommand>();
        var config = host.Services.GetRequiredService<ConfigOne>();
        string resultText = config.Data;
        ClassicAssert.AreEqual("Test", resultText);
        ClassicAssert.AreEqual(1, command.Stage);
        await host.StopAsync(token.Token).ConfigureAwait(false);
        ClassicAssert.AreEqual(2, command.Stage);
    }

    [Test]
    public void Construct()
    {
        ClassicAssert.Throws<ArgumentException>(() => new AutoStarter(null));
    }

    [Test]
    public void BadArguments()
    {
        ClassicAssert.Throws<Exception>(() => instance.Build(new[] { "Test", "-Data=Test" }));
    }

    private AutoStarter CreateInstance()
    {
        return new AutoStarter("Test");
    }
}