using System.Collections.Generic;
using Shouldly;

namespace codessentials.CGM.Tests
{
    internal static class ShouldlyExtensions
    {
        public static List<T> ShouldHaveCount<T>(this List<T> actual, int expectedAmount)
        {
            actual.Count.ShouldBe(expectedAmount);
            return actual;
        }

        public static T[] ShouldHaveCount<T>(this T[] actual, int expectedAmount)
        {
            actual.Length.ShouldBe(expectedAmount);
            return actual;
        }
    }
}
