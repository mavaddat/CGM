using codessentials.CGM.Classes;
using Shouldly;
using NUnit.Framework;

namespace codessentials.CGM.Tests
{
    [TestFixture]
    public class CgmLineTests
    {
        [Test]
        public void Sets_Line_When_A_Is_Before_B()
        {
            var a = new CgmPoint(0, 0);
            var b = new CgmPoint(10, 10);

            var line = new CgmLine(a, b);

            line.A.X.ShouldBe(0);
            line.A.Y.ShouldBe(0);

            line.B.X.ShouldBe(10);
            line.B.Y.ShouldBe(10);
        }

        [Test]
        public void Sets_Line_When_A_Is_Before_B_But_Below()
        {
            var a = new CgmPoint(0, 10);
            var b = new CgmPoint(10, 0);

            var line = new CgmLine(a, b);

            line.A.X.ShouldBe(0);
            line.A.Y.ShouldBe(10);

            line.B.X.ShouldBe(10);
            line.B.Y.ShouldBe(0);
        }

        [Test]
        public void Sets_Line_When_B_Is_Before_A()
        {
            var a = new CgmPoint(10, 10);
            var b = new CgmPoint(0, 0);

            var line = new CgmLine(a, b);

            line.A.X.ShouldBe(0);
            line.A.Y.ShouldBe(0);

            line.B.X.ShouldBe(10);
            line.B.Y.ShouldBe(10);
        }

        [Test]
        public void Sets_Line_When_A_Is_On_B()
        {
            var a = new CgmPoint(10, 0);
            var b = new CgmPoint(10, 10);

            var line = new CgmLine(a, b);

            line.A.X.ShouldBe(10);
            line.A.Y.ShouldBe(0);

            line.B.X.ShouldBe(10);
            line.B.Y.ShouldBe(10);
        }

        [Test]
        public void Sets_Line_When_A_Is_On_B_But_Below()
        {
            var a = new CgmPoint(10, 10);
            var b = new CgmPoint(10, 0);

            var line = new CgmLine(a, b);

            line.A.X.ShouldBe(10);
            line.A.Y.ShouldBe(0);

            line.B.X.ShouldBe(10);
            line.B.Y.ShouldBe(10);
        }
    }
}
