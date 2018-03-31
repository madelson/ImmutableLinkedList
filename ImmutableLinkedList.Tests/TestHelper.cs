using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medallion.Collections.Tests
{
    internal static class TestHelper
    {
        public static T ShouldEqual<T>(this T actual, T expected, string message = null)
        {
            Assert.AreEqual(actual: actual, expected: expected, message: message);
            return actual;
        }
    }
}
