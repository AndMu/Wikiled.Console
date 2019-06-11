using System;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using Wikiled.Console.Arguments;
using Wikiled.Console.Tests.Data;

namespace Wikiled.Console.Tests.Arguments
{
    [TestFixture]
    public class SyncExecutorTests
    {
        private AutoStarter autoStarter;

        private SyncExecutor instance;

        [SetUp]
        public void SetUp()
        {
            autoStarter = new AutoStarter(new NullLoggerFactory(), "Test", new[] { "One", "-Data=Test" });
            autoStarter.RegisterCommand<SampleCommand, ConfigOne>("One");
            autoStarter.RegisterCommand<SampleCommandTwo, ConfigTwo>("Two");
            instance = CreateSyncExecutor();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new SyncExecutor(null));
        }

        private SyncExecutor CreateSyncExecutor()
        {
            return new SyncExecutor(autoStarter);
        }
    }
}
