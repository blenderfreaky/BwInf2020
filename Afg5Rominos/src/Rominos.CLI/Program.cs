namespace Nummernmerker.CLI
{
    using CommandLine;
    using Rominos;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
                    Dictionary<int, Romino[]> rominos = Romino.GetRominosUntilSize(10);
                    IEnumerable<string> asciiArt = rominos
                        .Select(x => $"{x.Key}: {Environment.NewLine}{string.Join(", ", x.Value.Select(x => x.ToAsciiArt() + Environment.NewLine))}");
                    Console.WriteLine(string.Join(Environment.NewLine, asciiArt));
                });
        }
    }
}
