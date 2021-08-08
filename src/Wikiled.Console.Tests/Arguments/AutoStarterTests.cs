using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;
using Wikiled.Console.Tests.Data;

namespace Wikiled.Console.Tests.Arguments
{
    [TestFixture]
    public class AutoStarterTests : ReactiveTest
    {
        private AutoStarter instance;

        private TestScheduler scheduler;

        [SetUp]
        public void SetUp()
        {
            instance = CreateInstance();
            scheduler = new TestScheduler();
        }

        [Test]
        public async Task Acceptance()
        {
            instance.RegisterCommand<BlockingCommand, ConfigOne>("One");
            instance.RegisterCommand<SampleCommandTwo, ConfigTwo>("Two");
            var observer = scheduler.CreateObserver<bool>();
            scheduler.AdvanceBy(100);
            instance.Status.Subscribe(observer);
            await instance.StartAsync(CancellationToken.None).ConfigureAwait(false);
            string resultText = ((BlockingCommand)instance.Command).Config.Data;
            Assert.AreEqual("Test", resultText);
            scheduler.AdvanceBy(200);
            await instance.StopAsync(CancellationToken.None).ConfigureAwait(false);
            observer.Messages.AssertEqual(OnNext(100, true), OnNext(300, false), OnCompleted<bool>(300));
        }

        [Test]
        public async Task AcceptanceCompleted()
        {
            instance.RegisterCommand<SampleCommand, ConfigOne>("One");
            var observer = scheduler.CreateObserver<bool>();
            scheduler.AdvanceBy(100);
            instance.Status.Subscribe(observer);
            await instance.StartAsync(CancellationToken.None).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            scheduler.AdvanceBy(200);
            await instance.StopAsync(CancellationToken.None).ConfigureAwait(false);
            observer.Messages.AssertEqual(OnNext(100, true), OnNext(100, false), OnCompleted<bool>(100));
        }

        [Test]
        public async Task Blocking()
        {
            instance.RegisterCommand<BlockingCommand, ConfigOne>("One");
            await instance.StartAsync(CancellationToken.None).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            var command = ((BlockingCommand)instance.Command);
            string resultText = command.Config.Data;
            Assert.AreEqual("Test", resultText);
            await instance.StopAsync(CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(2, command.Stage);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentException>(() => new AutoStarter(null, new[] { "Test" }, builder => { }));
            Assert.Throws<ArgumentNullException>(() => new AutoStarter("Test", null, builder => { }));
        }

        [Test]
        public void BadArguments()
        {
            instance = new AutoStarter( "Test", new[] { "Test", "-Data=", "Test" }, builder => { });
            instance.RegisterCommand<SampleCommand, ConfigOne>("Test");
            var observer = scheduler.CreateObserver<bool>();
            scheduler.AdvanceBy(100);
            instance.Status.Subscribe(observer);
            Assert.ThrowsAsync<ArgumentException>(async () => await instance.StartAsync(CancellationToken.None).ConfigureAwait(false));
            observer.Messages.AssertEqual(OnNext(100, true), OnNext(100, false), OnCompleted<bool>(100));
        }

        private AutoStarter CreateInstance()
        {
            return new AutoStarter( "Test", new[] { "One", "-Data=Test" }, builder => { });
        }
    }
}
