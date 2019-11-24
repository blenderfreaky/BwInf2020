namespace Nummernmerker.CLI
{
    using CommandLine;
    using Rominos;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class Options
    {
        [Option('c', "size", Required = true, HelpText = "The highest number of blocks to generate rominos for.")]
        public int Size { get; set; }

        [Option('t', "targetFile", Required = false, HelpText = "Path of the file to write outputs to. If not set the console is used.")]
        public string? TargetFilePath { get; set; }

        [Option('d', "highlightDiagonalBlockade", Required = false, HelpText = "Whether to visually mark where the diagonal blockade is in every Romino.")]
        public bool HighlightDiagonalBlockade { get; set; }

        [Option('e', "highlightPossibleExtensions", Required = false, HelpText = "Whether to visually mark where new blocks can be appended in every Romino.")]
        public bool HighlightPossibleExtensions { get; set; }

        [Option('s', "stopwatch", Required = false, HelpText = "Whether to supress output other than number of rominos found in order to measure calculation time.")]
        public bool Stopwatch { get; set; }

        [Option('l', "latex", Required = false, HelpText = "Whether to output latex code.")]
        public bool Latex { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions);
        }

        private static readonly object _padlockRunWithOptions = new object();

        public static void RunWithOptions(Options o)
        {
            if (o.Size > 10)
            {
                Console.WriteLine("Sizes greater then 10 are, altough supported, very resource intensive. If you have less then 32GB RAM, I'd advise against running.");
                Console.WriteLine("Continue? Y/N");

                if (Console.ReadLine().StartsWith("N", StringComparison.OrdinalIgnoreCase)) return;
            }

            if (o.Stopwatch)
            {
                Console.WriteLine("Starting");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                foreach (var rom in Romino.GetRominosUntilSize(o.Size)) Console.WriteLine(rom.Size + " done - " + rom.Rominos.Count);
                stopwatch.Stop();
                Console.WriteLine("Done " + stopwatch.ElapsedMilliseconds + "ms");

                return;
            }

            TextWriter consoleOut = Console.Out;

            FileStream? fileStream = null;
            StreamWriter? fileStreamWriter = null;

            if (o.TargetFilePath != null)
            {
                fileStream = new FileStream(o.TargetFilePath, FileMode.OpenOrCreate);
                fileStreamWriter = new StreamWriter(fileStream);

                Console.SetOut(fileStreamWriter);
            }
            else
            {
                Console.WriteLine("Target file path not set; Defaulting to console");
            }

            Task worker = Task.Run(() => { });

            foreach (var rominoSizeClass in Romino.GetRominosUntilSize(o.Size))
            {
                //worker.ContinueWith(_ =>
                {
                    lock (_padlockRunWithOptions)
                    {
                        if (!o.Latex)
                        {
                            Console.WriteLine($"Rominos with {rominoSizeClass.Size} blocks ({rominoSizeClass.Rominos.Count}) \n"
                                + (o.TargetFilePath == null ? "\tLoading..." : string.Empty));
                        }

                        if (o.TargetFilePath != null)
                        {
                            ConsoleWriteTo(consoleOut, $"Calculated Rominos with size {rominoSizeClass.Size} ({rominoSizeClass.Rominos.Count})");
                        }

                        if (o.Latex)
                        {
                            Console.WriteLine($"\\subsection{{Rominos of size {rominoSizeClass.Size} ({rominoSizeClass.Rominos.Count})}}");
                            Console.WriteLine();
                            Console.WriteLine("\\begin{center}");

                            int i = 0;

                            foreach (var romino in rominoSizeClass.Rominos)
                            {
                                Console.WriteLine("\\begin{minipage}{0.23\\textwidth}");
                                Console.WriteLine(string.Join(Environment.NewLine, romino.ToLatex(o.HighlightDiagonalBlockade, o.HighlightPossibleExtensions)));
                                Console.WriteLine("\\end{minipage}");

                                i++;

                                if ((i % 4) == 0)
                                {
                                    i = 0;
                                    Console.WriteLine();
                                }
                            }

                            Console.WriteLine("\\end{center}");
                            Console.WriteLine();

                            continue;
                        }

                        string[][] text = rominoSizeClass.Rominos
                            .Select(x => x.ToAsciiArt(o.HighlightDiagonalBlockade, o.HighlightPossibleExtensions).ToArray())
                            //.Take(50)
                            .ToArray();

                        int bufferWidth = Math.Min(Console.BufferWidth, 200);

                        if (o.TargetFilePath == null)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine(new string(' ', bufferWidth));
                        }

                        Console.WriteLine();

                        for (int i = 0; i < text.Length;)
                        {
                            int length = 0;
                            int k = i;

                            for (int j = 0; j < rominoSizeClass.Size + 2; j++)
                            {
                                for (k = i; k < text.Length; k++)
                                {
                                    var lines = text[k];

                                    if (bufferWidth - Console.CursorLeft < lines[0].Length + 3) break;

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