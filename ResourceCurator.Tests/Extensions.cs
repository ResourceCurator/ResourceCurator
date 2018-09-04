using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Utils;
using System.Linq;

namespace ResourceCurator.Tests
{
    public class Extensions
    {
        [Fact]
        public void Contains()
        {
            string str = "checked";
            char symb1 = 'E';
            char symb2 = 'B';

            Assert.True(str.Contains(symb1, StringComparison.CurrentCultureIgnoreCase));
            Assert.False(str.Contains(symb2, StringComparison.CurrentCultureIgnoreCase));
            Assert.False(str.Contains(symb2, StringComparison.CurrentCulture));
            Assert.False(str.Contains(symb2, StringComparison.CurrentCulture));
        }

        [Fact]
        public void AsList()
        {
            IEnumerable<int> result = from value in Enumerable.Range(0, 2)
                                      select value;

            var a = result.GetType();
            var b = result.AsList<int>().GetType();

            Assert.False(a == typeof(List<int>) );
            Assert.True(b == typeof(List<int>));
        }
    }
}
