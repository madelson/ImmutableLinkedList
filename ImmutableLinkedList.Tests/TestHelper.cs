using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medallion.Collections.Tests;

internal static class TestHelper
{
    public static T ShouldEqual<T>(this T actual, T expected, string message = null)
    {
        Assert.That(actual, Is.EqualTo(expected), message: message);
        return actual;
    }
}
