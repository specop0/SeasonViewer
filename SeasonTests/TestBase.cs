using NUnit.Framework;
using System;

namespace SeasonTests
{
    [TestFixture]
    public abstract class TestBase
    {
        public string GetUniqueName(string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return Guid.NewGuid().ToString();
            }

            return $"{prefix} {Guid.NewGuid()}";
        }
    }
}
