using Neotales.Collections.Generic;

namespace Neotales.Collections.Tests
{

    public class Map_Tests
    {
        [Fact]
        public void Map_AddTuple_AddsKeyValue()
        {
            var map = new Map<int, string>();

            map.Add((1, "one"));

            Assert.Equal("one", map[1]);
        }

        [Fact]
        public void Map_AddRange_AddsAllItems()
        {
            var map = new Map<int, string>();

            map.AddRange([(1, "one"), (2, "two")]);

            Assert.Equal(2, map.Count);
            Assert.Equal("two", map[2]);
        }

        [Fact]
        public void MapOfTValue_DefaultComparer_IsCaseInsensitive()
        {
            var map = new Map<int>();
            map["Key"] = 7;

            var ok = map.TryGetValue("key", out var value);

            Assert.True(ok);
            Assert.Equal(7, value);
        }
    }
}
