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
            IEnumerable<int> checkOnList = from value in Enumerable.Range(0, 2) select value;
            var listAlready = new List<int>();

            var a = checkOnList;                  // unknown type
            var b = checkOnList.AsList<int>();    // convert to list
            var c = listAlready.AsList<int>();    // already list

            Assert.False(a.GetType() == typeof(List<int>)); // "a" isn't List
            Assert.True( b.GetType() == typeof(List<int>)); // "b" has been converted to List
            Assert.True( c.GetType() == typeof(List<int>)); // "c" is already List
            Assert.False(b == checkOnList);                 // "b" points to the new object
            Assert.True( c == listAlready);                 // "c" points to the same object
        }
    }
}