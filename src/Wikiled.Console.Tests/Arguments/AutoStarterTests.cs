using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Wikiled.Console.Arguments;
using Wikiled.Console.Tests.Data;

namespace Wikiled.Console.Tests.Arguments
{
    [TestFixture]
    public class AutoStarterTests
    {
        private AutoStarter instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateInstance();
        }

        [Test]
        public void Acceptance()
        {
            instance.RegisterCommand<SampleCommand, ConfigOne>("One");
            instance.RegisterCommand<SampleCommandTwo, ConfigTwo>("Two");
            instance.Start(new[] { "One", "-Data=Test"}, CancellationToken.None).ConfigureAwait(false);
            var resultText = ((SampleCommand)instance.Command).Config.Data;
            Assert.AreEqual("Test", resultText);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new AutoStarter(new NullLoggerFactory(), null));
            Assert.Throws<ArgumentNullException>(() => new AutoStarter(null, "Test"));
        }

        private AutoStarter CreateInstance()
        {
            return new AutoStarter(new NullLoggerFactory(), "Test");
        }
    }
}
