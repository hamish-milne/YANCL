using System;
using System.IO;
using TwiLua.StdLib;
using Xunit;

namespace TwiLua.Test
{
    public class IntegrationTests
    {
        static void AssertEqual(string str, LuaValue expected) {
            var f = Lua.Compile(str);
            var s = new LuaThread(isMain: true, 16, 2);
            var g = new Lua().LoadMath().Globals;
            var closure = new LuaClosure(f, new []{new LuaUpValue { Value = g }});
            Assert.Equal(expected, s.Execute(closure)[0]);
        }

        [Fact]
        public void Add() => AssertEqual("return 2 + 3", 5);

        [Fact]
        public void Sub() => AssertEqual("return 2 - 3", -1);

        [Fact]
        public void Mul() => AssertEqual("return 2 * 3", 6);

        [Fact]
        public void Div() => AssertEqual("return 6 / 2", 3);

        [Fact]
        public void Mod() => AssertEqual("return 5 % 2", 1);

        [Fact]
        public void Pow() => AssertEqual("return 2 ^ 3", 8);

        [Fact]
        public void UnaryMinus() => AssertEqual("return -2", -2);

        [Fact]
        public void Not() => AssertEqual("return not true", false);

        [Fact]
        public void And() => AssertEqual("return true and false", false);

        [Fact]
        public void Or() => AssertEqual("return true or false", true);

        [Fact]
        public void Concat() => AssertEqual("return 'foo'..'bar'..'baz'", "foobarbaz");

        [Fact]
        public void Compare() => AssertEqual("return 2 == 2", true);

        [Fact]
        public void Compare2() => AssertEqual("return 2 ~= 2", false);

        [Fact]
        public void LessThan() => AssertEqual("return 2 < 3", true);

        [Fact]
        public void LessThanOrEqual() => AssertEqual("return 2 <= 2", true);

        [Fact]
        public void GreaterThan() => AssertEqual("return 2 > 3", false);

        [Fact]
        public void GreaterThanOrEqual() => AssertEqual("return 2 >= 2", true);

        [Fact]
        public void If() => AssertEqual("if true then return 1 else return 2 end", 1);

        [Fact]
        public void If2() => AssertEqual("if false then return 1 else return 2 end", 2);

        [Fact]
        public void If3() => AssertEqual("if false then return 1 elseif true then return 2 else return 3 end", 2);

        [Fact]
        public void While() => AssertEqual("local i = 0 while i < 3 do i = i + 1 end return i", 3);

        [Fact]
        public void Repeat() => AssertEqual("local i = 0 repeat i = i + 1 until i == 3 return i", 3);

        [Fact]
        public void For() => AssertEqual("local x = 0 for i = 1, 5 do x = x + i end return x", 15);

        [Fact]
        public void For2() => AssertEqual("local x = 0 for i = 1, 3, 2 do x = x + i end return x", 4);

        [Fact]
        public void Upvalues() => AssertEqual("do local x = 1; f = function() x = x + 2 return x end end f() return f()", 5);

        [Fact]
        public void Hashbang() => AssertEqual("#!/usr/bin/env lua\nreturn 1", 1);

        [Fact]
        public void ShortComment() => AssertEqual("-- foo\nreturn 1", 1);

        [Fact]
        public void LongComment() => AssertEqual("--[[ ]] foo\nbar\n]] return 1", 1);

        [Fact]
        public void Hex() => AssertEqual("return 0xFF", 255);

        [Fact]
        public void BigFile() {
            var lua = new Lua().LoadBase();
            lua.DoString(File.ReadAllText("../../../../TwiLua.Benchmark/lua/loadBigFile.lua"));
        }

        [Fact]
        public void GlobalFn() {
            var lua = new Lua();
            lua.Globals["globalFn"] = (LuaCFunction)((LuaThread s) => {
                return s.Return(s.Number(1) + s.Number(2));
            });
            lua.DoString(File.ReadAllText("../../../../TwiLua.Benchmark/lua/call.lua"));
        }

        [Fact]
        public void GlobalDelegate() {
            var lua = new Lua();
            lua.Globals["globalFn"] = LuaValue.From<Func<int, int, int>>((a, b) => a + b);
            lua.DoString(File.ReadAllText("../../../../TwiLua.Benchmark/lua/call.lua"));
        }
    }
}