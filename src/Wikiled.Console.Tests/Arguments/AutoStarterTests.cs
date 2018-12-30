using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Wikiled.Common.Logging;
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
            instance.RegisterCommand<SampleCommand, ConfigOne>("One");
            instance.RegisterCommand<SampleCommandTwo, ConfigTwo>("Two");
            var observer = scheduler.CreateObserver<bool>();
            scheduler.AdvanceBy(100);
            instance.Status.Subscribe(observer);
            await instance.StartAsync(CancellationToken.None).ConfigureAwait(false);
            string resultText = ((SampleCommand)instance.Command).Config.Data;
            Assert.AreEqual("Test", resultText);
            scheduler.AdvanceBy(200);
            await instance.StopAsync(CancellationToken.None).ConfigureAwait(false);
            observer.Messages.AssertEqual(OnNext(100, true), OnNext(300, false), OnCompleted<bool>(300));
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentException>(() => new AutoStarter(new NullLoggerFactory(), null, new []{"Test"}));
            Assert.Throws<ArgumentNullException>(() => new AutoStarter(null, "Test", new[] { "Test" }));
            Assert.Throws<ArgumentNullException>(() => new AutoStarter(new NullLoggerFactory(), "Test", null));
        }

        private AutoStarter CreateInstance()
        {
            return new AutoStarter(new NullLoggerFactory(), "Test", new[] { "One", "-Data=Test" });
        }
    }
}
