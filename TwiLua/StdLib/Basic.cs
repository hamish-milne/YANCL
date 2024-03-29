using System;

namespace TwiLua.StdLib
{
    public static class Basic
    {
        public static Lua LoadBase(this Lua lua) {
            var globals = lua.Globals;
            globals["collectgarbage"] = new LuaCFunction(s => {
                switch (s.Count >= 1 ? s.String(1) : "collect") {
                case "collect": GC.Collect(); break;
                case "stop":
                case "restart":
                case "step":
                case "incremental":
                case "generational":
                    // Not supported
                    break;
                case "count":
                    return s.Return(0);
                case "isrunning":
                    return s.Return(true);
                }
                return 0;
            });
            globals["print"] = new LuaCFunction(s => {
                var o = Console.Out;
                for (int i = 1; i <= s.Count; i++) {
                    if (i > 1) {
                        o.Write("\t");
                    }
                    o.Write(s[i].ToString());
                }
                o.WriteLine();
                o.Flush();
                return 0;
            });
            globals["select"] = new LuaCFunction(s => {
                if (s.Count == 0) {
                    throw new WrongNumberOfArguments();
                }
                if (s[1] == "#") {
                    return s.Return(s.Count - 1);
                } else {
                    var idx = s.Integer(1);
                    if (idx <= 0) {
                        throw new ArgumentOutOfRangeException();
                    }
                    if (idx > s.Count - 1) {
                        return s.Return(LuaValue.Nil);
                    } else {
                        return s.Return(s[(int)idx + 1]);
                    }
                }
            });
            var next = new LuaCFunction(s => {
                if (s.Count < 1) {
                    throw new WrongNumberOfArguments();
                }
                var table = s.Table(1);
                var key = s.Count >= 2 ? s[2] : LuaValue.Nil;
                (s[0], s[1]) = table.Next(key) ?? (LuaValue.Nil, LuaValue.Nil);
                return 2;
            });
            globals["next"] = next;
            globals["_G"] = globals;
            globals["_VERSION"] = "Lua 5.4";
            globals["type"] = new LuaCFunction(s => {
                if (s.Count < 1) {
                    throw new WrongNumberOfArguments();
                }
                return s.Return(s[1].Type switch {
                    TypeTag.Nil => "nil",
                    TypeTag.True or TypeTag.False => "boolean",
                    TypeTag.Number => "number",
                    _ => s[1].Object switch {
                        string => "string",
                        LuaTable => "table",
                        LuaFunction or LuaCFunction => "function",
                        _ => "userdata"
                    }
                });
            });
            globals["error"] = new LuaCFunction(s => {
                // TODO: Error position
                throw new LuaRuntimeError(s[1]);
            });
            globals["pcall"] = new LuaCFunction(s => {
                if (s.Count < 1) {
                    throw new WrongNumberOfArguments();
                }
                var ciptr = s.CallDepth;
                try {
                    var nResults = s.Callback(1, s.Count, 1);
                    s[0] = true;
                    return nResults + 1;
                } catch (Exception e) {
                    s.UnwindStack(ciptr + 1);
                    s[0] = false;
                    s[1] = e is LuaRuntimeError lerror ? lerror.Value : e.Message;
                    s.IsDead = false;
                    return 2;
                }
            });
            globals["pairs"] = new LuaCFunction(s => {
                if (s.Count < 1) {
                    throw new WrongNumberOfArguments();
                }
                s[0] = next;
                s[2] = LuaValue.Nil;
                return 3;
            });
            globals["ipairs"] = new LuaCFunction(s => {
                if (s.Count == 0) {
                    throw new WrongNumberOfArguments();
                }
                if (s.Count == 1) {
                    s[2] = 0;
                    return 3;
                }
                var table = s.Table(1);
                var idx = s.Integer(2) + 1;
                if (idx > table.Length) {
                    s[0] = LuaValue.Nil;
                    return 1;
                } else {
                    s[0] = idx;
                    s[1] = table[idx];
                    return 2;
                }
            });
            globals["getmetatable"] = new LuaCFunction(s => {
                if (s.Count == 0) {
                    throw new WrongNumberOfArguments();
                }
                return s.Return(s[1].Table?.MetaTable ?? LuaValue.Nil);
            });
            globals["setmetatable"] = new LuaCFunction(s => {
                if (s.Count < 2) {
                    throw new WrongNumberOfArguments();
                }
                var table = s.Table(1);
                if (s[2] == LuaValue.Nil) {
                    table.MetaTable = null;
                } else {
                    table.MetaTable = s.Table(2);
                }
                return s.Return(table);
            });
            globals["rawget"] = new LuaCFunction(s => {
                if (s.Count < 2) {
                    throw new WrongNumberOfArguments();
                }
                var table = s.Table(1);
                return s.Return(table[s[2]]);
            });
            globals["rawset"] = new LuaCFunction(s => {
                if (s.Count < 3) {
                    throw new WrongNumberOfArguments();
                }
                var table = s.Table(1);
                table[s[2]] = s[3];
                return s.Return(table);
            });
            return lua;
        }
    }
}