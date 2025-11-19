using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests
{
    [TestFixture]
    class BinaryReaderTests : CgmTest
    {
        [Test]
        public void ReadBinaryFiles()
        {
            var assembly = this.GetType().Assembly;

            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith(".cgm", StringComparison.OrdinalIgnoreCase))
                {
                    var binaryFile = ReadBinaryFile(name, assembly);

                    binaryFile.Messages.Count().ShouldBe(0, "Messages: " + string.Join("\r\n", binaryFile.Messages.Select(m => m.ToString())));
                }
            }
        }

    }
}
