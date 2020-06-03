using System;

namespace CppCompiler
{
    public class CompilerNotFoundException : Exception
    {
        public override string Message { get; } = "g++ not found";
    }
}