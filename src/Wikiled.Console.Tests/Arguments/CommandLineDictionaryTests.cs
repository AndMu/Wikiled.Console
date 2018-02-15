using NUnit.Framework;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Arguments
{
    [TestFixture]
    public class CommandLineDictionaryTests
    {
        private CommandLineDictionary instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateCommandLineDictionary();

        }

        [Test]
        public void Construct()
        {
            Assert.IsNotNull(instance);
        }

        private CommandLineDictionary CreateCommandLineDictionary()
        {
            return new CommandLineDictionary();
        }
    }
}