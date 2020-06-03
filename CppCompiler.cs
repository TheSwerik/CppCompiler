using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CppCompiler
{
    internal static class CppCompiler
    {
        private const int MaxProblems = 2000;
        private static bool _quiet;

        internal static void Main(string[] args)
        {
            _quiet = args.Any(a => a.Contains("q"));
            if (args.Any(a => Regex.IsMatch(a, "\\d+"))) Compile(int.Parse(args.First(a => Regex.IsMatch(a, "\\d"))));
            else for (var i = 1; i <= MaxProblems; i++) Compile(i);
        }

        private static void Compile(int number)
        {
            var problem = @"\Problem" + new string('0', 3 - (int) Math.Log10(number)) + number;
            var source = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\src\main\cpp");
            var outDir = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\out\cpp");

            var compiler = new ProcessStartInfo
                           {
                               FileName = @"C:\MinGW\bin\g++",
                               Arguments = source + problem + ".cpp -o " + outDir + problem + ".exe",
                               UseShellExecute = false,
                               RedirectStandardOutput = true,
                               RedirectStandardError = true
                           };
            using var process = Process.Start(compiler);
            if (process == null) throw new CompilerNotFoundException();
            using var error = process.StandardError;
            process.WaitForExit();
            if (_quiet && process.ExitCode == 0) Console.WriteLine(problem + " compiled successfully.");
            else if (!_quiet) Console.Write(error.ReadToEnd());
        }
    }
}