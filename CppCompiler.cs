using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace CppCompiler
{
    internal static class CppCompiler
    {
        private const int MaxProblems = 2000;
        private static bool _quiet;

        internal static void Main(string[] args)
        {
            SetCurrentProcessExplicitAppUserModelID("Swerik.CppCompiler.1.7");
            _quiet = args.Any(a => a.Contains("q"));
            if (args.Any(a => Regex.IsMatch(a, "\\d+"))) Compile(int.Parse(args.First(a => Regex.IsMatch(a, "\\d"))));
            else Compile(1, MaxProblems);
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string id);

        private static void Compile(int number) { Compile(number, number); }

        private static void Compile(int min, int max)
        {
            var source = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\src\main\cpp");
            var outDir = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\out\cpp");
            outDir.Create();

            var compiler = new ProcessStartInfo
                           {
                               FileName = "cmd.exe",
                               Arguments =
                                   "/k " +
                                   @"""C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat""",
                               WorkingDirectory = Directory.GetCurrentDirectory(),
                               UseShellExecute = false,
                               CreateNoWindow = true,
                               RedirectStandardOutput = true,
                               RedirectStandardInput = true,
                               RedirectStandardError = true
                           };
            for (var i = min; i <= max; i++)
            {
                var problem = @"\Problem" + new string('0', 3 - (int) Math.Log10(i)) + i;
                if (!File.Exists(source + problem + ".cpp"))
                {
                    if (!_quiet) Console.WriteLine($"File {problem} not found.");
                    continue;
                }

                using var process = Process.Start(compiler);
                if (process == null) throw new CompilerNotFoundException();
                using var output = process.StandardOutput;
                process.StandardInput.WriteLine("cl /EHsc " + source + problem + ".cpp /link /out:" +
                                                outDir + problem + ".exe");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
                var outString = output.ReadToEnd();
                var wasSuccessful = !outString.Contains("fatal error");
                if (wasSuccessful) Console.WriteLine(problem + " compiled successfully.");
                else if (!_quiet) Console.Write(outString);
                File.Delete(Directory.GetCurrentDirectory() + @"\" + problem + ".obj");
            }
        }
    }
}