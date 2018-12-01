using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task Acceptance()
        {
            instance.RegisterCommand<SampleCommand, ConfigOne>("One");
            instance.RegisterCommand<SampleCommandTwo, ConfigTwo>("Two");
            await instance.StartAsync(CancellationToken.None).ConfigureAwait(false);
            string resultText = ((SampleCommand)instance.Command).Config.Data;
            Assert.AreEqual("Test", resultText);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentException>(() => new AutoStarter(null, new []{"Test"}));
            Assert.Throws<ArgumentNullException>(() => new AutoStarter("Test", null));
        }

        private AutoStarter CreateInstance()
        {
            return new AutoStarter("Test", new[] { "One", "-Data=Test" });
        }
    }
}
