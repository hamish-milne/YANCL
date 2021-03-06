
using System;

namespace YANCL
{
    public sealed class LuaRuntimeError : Exception {
        public LuaValue Value { get; }

        public LuaRuntimeError(LuaValue value) {
            Value = value;
        }

        public override string Message => $"Runtime error: {Value}";
    }
}