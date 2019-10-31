namespace Nummernmerker.CLI
{
    using CommandLine;
    using System;
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
                    long number = 00321003003102030312;

                    Console.WriteLine(string.Join(' ', Nummernmerker.MerkNummernToText(number, 2, 4)));
                });
        }
    }
}
