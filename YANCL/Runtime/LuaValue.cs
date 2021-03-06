using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Collections;

namespace YANCL
{
    public enum LuaType {
        NIL = 0,
        BOOLEAN = 1,
        NUMBER = 3,
        STRING = 4,
        TABLE = 5,
        FUNCTION = 6,
        CFUNCTION = 7,
        THREAD = 8,
    }


    class WrongNumberOfArguments : Exception { }
    class NoIntegerRepresentation : Exception { }

    public delegate int LuaCFunction(LuaThread s);

    public readonly struct LuaValue : IEquatable<LuaValue> {
        public readonly LuaType Type;
        public readonly double Number;
        public readonly string? String;
        public readonly LuaTable? Table;
        public readonly LuaClosure? Function;
        public readonly LuaCFunction? CFunction;
        public readonly LuaThread? Thread;
        public readonly int Hash;

        public bool Boolean {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return !(Type == LuaType.NIL || (Type == LuaType.BOOLEAN && Number == 0));
            }
        }

        public int Length {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (Type == LuaType.STRING) {
                    return String!.Length;
                } else if (Type == LuaType.TABLE) {
                    return Table!.Length;
                } else {
                    throw new Exception("Not a string or table");
                }
            }
        }

        public LuaValue this[in LuaValue key] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (Table == null) {
                    throw new Exception("Not a table");
                }
                return Table[key];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                if (Table == null) {
                    throw new Exception("Not a table");
                }
                Table[key] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNumber(out double number) {
            switch (Type) {
                case LuaType.NUMBER:
                    number = Number;
                    return true;
                case LuaType.STRING:
                    if (double.TryParse(String!, out number)) {
                        return true;
                    } else {
                        number = 0;
                        return false;
                    }
                default:
                    number = 0;
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInteger(out long intVal) {
            if (TryGetNumber(out var n) && n % 1 == 0) {
                intVal = (long)n;
                return true;
            } else {
                intVal = 0;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetMetaValue(in LuaValue key, out LuaValue value) {
            if (Table?.MetaTable == null) {
                value = default;
                return false;
            }
            return Table.MetaTable.TryGetValue(key, out value);
        }

        public static readonly LuaValue Nil = new LuaValue();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaValue(bool value) {
            Type = LuaType.BOOLEAN;
            Number = value ? 1 : 0;
            String = null;
            Table = null;
            Function = null;
            CFunction = null;
            Thread = null;
            Hash = value.GetHashCode();
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaValue(double value) {
            Type = LuaType.NUMBER;
            Number = value;
            String = null;
            Table = null;
            Function = null;
            CFunction = null;
            Thread = null;
            Hash = value.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaValue(string value) {
            Type = LuaType.STRING;
            Number = 0;
            String = value;
            Table = null;
            Function = null;
            CFunction = null;
            Thread = null;
            Hash = value.GetHashCode();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaValue(LuaTable table) {
            Type = LuaType.TABLE;
            Number = 0;
            String = null;
            Table = table;
            Function = null;
            CFunction = null;
            Thread = null;
            Hash = table.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaValue(LuaClosure function) {
            Type = LuaType.FUNCTION;
            Number = 0;
            String = null;
            Table = null;
            Function = function;
            CFunction = null;
            Thread = null;
            Hash = function.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaValue(LuaCFunction function) {
            Type = LuaType.CFUNCTION;
            Number = 0;
            String = null;
            Table = null;
            Function = null;
            CFunction = function;
            Thread = null;
            Hash = function.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaValue(LuaThread thread) {
            Type = LuaType.THREAD;
            Number = 0;
            String = null;
            Table = null;
            Function = null;
            CFunction = null;
            Thread = thread;
            Hash = thread.GetHashCode();
        }

        bool IEquatable<LuaValue>.Equals(LuaValue other) => Equals(other);

        public bool Equals(in LuaValue other) {
            if (Type != other.Type) return false;
            switch (Type) {
                case LuaType.NIL:
                    return true;
                case LuaType.BOOLEAN:
                case LuaType.NUMBER:
                    return Number == other.Number;
                case LuaType.STRING:
                    return String == other.String;
                case LuaType.TABLE:
                    return Table == other.Table;
                case LuaType.FUNCTION:
                    return Function == other.Function;
                case LuaType.CFUNCTION:
                    return CFunction == other.CFunction;
                case LuaType.THREAD:
                    return Thread == other.Thread;
                default:
                    throw new Exception("Invalid LuaType");
            }
        }

        public override bool Equals(object? obj) {
            return obj is LuaValue other && Equals(other);
        }

        public override int GetHashCode() => Hash;

        public override string ToString()
        {
            switch (Type) {
                case LuaType.NIL:
                    return "nil";
                case LuaType.BOOLEAN:
                    return Boolean ? "true" : "false";
                case LuaType.NUMBER:
                    return Number.ToString();
                case LuaType.STRING:
                    return String!;
                case LuaType.TABLE:
                    return "<table>";
                case LuaType.CFUNCTION:
                case LuaType.FUNCTION:
                    return "<function>";
                case LuaType.THREAD:
                    return "<thread>";
                default:
                    throw new Exception("Invalid LuaType");
            }
        }

        public static bool operator ==(in LuaValue left, in LuaValue right) {
            return left.Equals(right);
        }
        public static bool operator !=(in LuaValue left, in LuaValue right) {
            return !left.Equals(right);
        }

        public static implicit operator LuaValue(bool value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(double value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(int value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(long value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(string value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(LuaClosure value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(LuaTable value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(LuaCFunction value) {
            return new LuaValue(value);
        }

        public static implicit operator LuaValue(LuaThread value) {
            return new LuaValue(value);
        }
    }
}
