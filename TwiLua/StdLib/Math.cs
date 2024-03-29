using System;
using static System.Math;

namespace TwiLua.StdLib
{
    public static class LibMath
    {
        public static Lua LoadMath(this Lua lua)
        {
            var random = new Random();
            var rndBuf = new byte[sizeof(double)];

            lua.Globals["math"] = new LuaTable {
                {"abs", s => s.Return(Abs(s.Number()))},
                {"acos", s => s.Return(Acos(s.Number()))},
                {"asin", s => s.Return(Asin(s.Number()))},
                {"atan", s => s.Return(Atan(s.Number()))},
                {"ceil", s => s.Return(Ceiling(s.Number()))},
                {"cos", s => s.Return(Cos(s.Number()))},
                {"floor", s => s.Return(Floor(s.Number()))},
                {"deg", s => s.Return(s.Number() * 180 / PI)},
                {"rad", s => s.Return(s.Number() * PI / 180)},
                {"exp", s => s.Return(Exp(s.Number()))},
                {"sin", s => s.Return(Sin(s.Number()))},
                {"tan", s => s.Return(Tan(s.Number()))},
                {"sqrt", s => s.Return(Sqrt(s.Number()))},
                {"pi", PI},
                {"huge", double.PositiveInfinity},
                {"maxinteger", Pow(2, 53)},
                {"mininteger", -Pow(2, 53)},
                {"fmod", s => s.Return(s.Number(0) % s.Number(1))},
                {"ult", s => s.Return((ulong)s.Integer(1) < (ulong)s.Integer(2))},
                {"random", s => {
                    switch (s.Count) {
                        case 0:
                            return s.Return(random.NextDouble());
                        case 1:
                            var n = s.Integer(1);
                            if (n == 0) {
                                random.NextBytes(rndBuf);
                                return s.Return(BitConverter.ToDouble(rndBuf, 0));
                            } else {
                                return s.Return(random.Next(1, (int)n));
                            }
                        case 2:
                            return s.Return(random.Next((int)s.Integer(1), (int)s.Integer(2)));
                        default:
                            throw new WrongNumberOfArguments();
                    }
                }},
                {"randomseed", s => {
                    random = s.Count switch {
                        0 => new Random(),
                        1 or 2 => new Random((int)(s.Integer(1) ^ (long)s[2].Number)),
                        _ => throw new WrongNumberOfArguments()
                    };
                    return 0;
                }},
                {"tointeger", s => {
                    if (s[1].TryGetInteger(out var i)) {
                        return s.Return(i);
                    } else {
                        return s.Return(LuaValue.Fail);
                    }
                }},
                {"type", s => {
                    if (s[1].TryGetInteger(out _)) {
                        return s.Return("integer");
                    } else if (s[1].IsNumber) {
                        return s.Return("float");
                    } else {
                        return s.Return(LuaValue.Fail);
                    }
                }},
                {"max", s => {
                    var a = s.Number(1);
                    for (int i = 2; i <= s.Count; i++) {
                        a = Max(a, s.Number(i));
                    }
                    return s.Return(a);
                }},
                {"min", s => {
                    var a = s.Number(0);
                    for (int i = 2; i <= s.Count; i++) {
                        a = Min(a, s.Number(i));
                    }
                    return s.Return(a);
                }}
            };
            return lua;
        }
    }
}