namespace Nummernmerker.CLI
{
    using CommandLine;
    using Rominos;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    // Execute IO in parallel
                    Parallel.ForEach(Romino.GetRominosUntilSize(int.MaxValue), rominoSizeClass =>
                    {
                        Console.WriteLine($"Rominos with {rominoSizeClass.Size} blocks \n\tLoading...");

                        string[][] text = rominoSizeClass.Rominos
                            .Select(x => x.ToAsciiArt().ToArray()).ToArray();

                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine(new string(' ', Console.BufferWidth));
                        Console.WriteLine();

                        for (int i = 0; i < text.Length;)
                        {
                            int length = 0;
                            int k = i;

                            for (int j = 0; j < rominoSizeClass.Size; j++)
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
                    });
                });
        }
    }
}
