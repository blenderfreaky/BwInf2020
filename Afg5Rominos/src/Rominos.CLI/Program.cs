#define PerfOpt

namespace Nummernmerker.CLI
{
    using CommandLine;
    using Rominos;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class Options
    {
        [Option('s', "size", Required = true, HelpText = "The highest number of blocks to generate rominos for.")]
        public int Size { get; set; }

        [Option('t', "targetFile", Required = false, HelpText = "Path of the file to write outputs to. If not set the console is used.")]
        public string? TargetFilePath { get; set; }

        [Option('d', "highlightDiagonalBlockade", Required = false, HelpText = "Whether to visually mark where the diagonal blockade is in every Romino.")]
        public bool HighlightDiagonalBlockade { get; set; }

        [Option('e', "highlightPossibleExtensions", Required = false, HelpText = "Whether to visually mark where new blocks can be appended in every Romino.")]
        public bool HighlightPossibleExtensions { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions);
        }

        private static readonly object _padlockRunWithOptions = new object();

        public static void RunWithOptions(Options options)
        {
#if PerfOpt
            Console.WriteLine("Starting");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var rom in Romino.GetRominosUntilSize(10)) Console.WriteLine(rom.Size + " done - " + rom.Rominos.Length);
            stopwatch.Stop();
            Console.WriteLine("Done " + stopwatch.ElapsedMilliseconds + "ms");

            return;
#endif

            TextWriter consoleOut = Console.Out;

            FileStream? fileStream = null;
            StreamWriter? fileStreamWriter = null;

            if (options.TargetFilePath != null)
            {
                fileStream = new FileStream(options.TargetFilePath, FileMode.OpenOrCreate);
                fileStreamWriter = new StreamWriter(fileStream);

                Console.SetOut(fileStreamWriter);
            }
            else
            {
                Console.WriteLine("Target file path not set; Defaulting to console");
            }

            Task worker = Task.Run(() => { });

            foreach (var rominoSizeClass in Romino.GetRominosUntilSize(options.Size))
            {
                //worker.ContinueWith(_ =>
                {
                    lock (_padlockRunWithOptions)
                    {
                        Console.WriteLine($"Rominos with {rominoSizeClass.Size} blocks ({rominoSizeClass.Rominos.Length}) \n"
                            + (options.TargetFilePath == null ? "\tLoading..." : string.Empty));

                        if (options.TargetFilePath != null)
                        {
                            ConsoleWriteTo(consoleOut, $"Calculated Rominos with size {rominoSizeClass.Size} ({rominoSizeClass.Rominos.Length})");
                        }

                        string[][] text = rominoSizeClass.Rominos
                            .Select(x => x.ToAsciiArt(options.HighlightDiagonalBlockade, options.HighlightPossibleExtensions).ToArray()).ToArray();

                        if (options.TargetFilePath == null)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine(new string(' ', Console.BufferWidth));
                        }

                        Console.WriteLine();

                        for (int i = 0; i < text.Length;)
                        {
                            int length = 0;
                            int k = i;

                            for (int j = 0; j < rominoSizeClass.Size+2; j++)
                            {
                                for (k = i; k < text.Length; k++)
                                {
                                    var lines = text[k];

                                    if (Console.BufferWidth - Console.CursorLeft < lines[0].Length + 3) break;

                                    if (k != i) Console.Write(" │ ");
                                    Console.Write(lines.Length > j ? lines[j] : new string(' ', lines[0].Length));
                                }

                                length = Console.CursorLeft;

                                Console.WriteLine();
                            }

                            i = k;

                            if (i < text.Length) Console.WriteLine(new string('─', length));
                        }

                        Console.WriteLine();
                    }
                }//);
            }

            worker.Wait();

            Console.SetOut(consoleOut);

            fileStreamWriter?.Dispose();
            fileStream?.Dispose();
        }

        private static readonly object _padlockConsoleWriteTo = new object();

        private static void ConsoleWriteTo(TextWriter writer, string text)
        {
            // Make sure this only runs in one thread at a time
            lock (_padlockConsoleWriteTo)
            {
                var previousOut = Console.Out;

                Console.SetOut(writer);
                Console.WriteLine(text);

                Console.SetOut(previousOut);
            }
        }
    }
}
