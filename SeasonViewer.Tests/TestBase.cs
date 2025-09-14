using NUnit.Framework;
using System;

namespace SeasonViewer.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        public string GetUniqueName(string? prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return Guid.NewGuid().ToString();
            }

            return $"{prefix} {Guid.NewGuid()}";
        }
    }
}
