using TwiLua.StdLib;
using Xunit;

namespace TwiLua.Test
{
    public class OSTests
    {
        [Fact]
        public void Clock()
        {
            var l = new Lua().LoadOS();
            var result = l.DoString(@"
                return os.clock()
            ")[0];
            Assert.True(result.ExpectNumber() >= 0);
        }

        [Fact]
        public void Date()
        {
            var l = new Lua().LoadOS();
            var ts = 3600 + 60 + 1;
            Assert.Equal(
                new LuaValue[] { "1970-01-01 01:01:01" },
                l.DoString($"return os.date('%Y-%m-%d %H:%M:%S', {ts})")
            );
        }

        [Fact]
        public void DateDefault()
        {
            var l = new Lua().LoadOS();
            var result = l.DoString(@"
                return os.date()
            ")[0];
            Assert.IsType<string>(result.Object);
        }

        [Fact]
        public void Difftime()
        {
            var l = new Lua().LoadOS();
            var result = l.DoString(@"
                return os.difftime(1, 2)
            ")[0];
            Assert.Equal(-1, result.Number);
        }

        [Fact]
        public void Time()
        {
            var l = new Lua().LoadOS();
            Assert.Equal(
                new LuaValue[] { 3600 },
                l.DoString("return os.time({year=1970, month=1, day=1, hour=1, min=0, sec=0})")
            );
        }
    }
}