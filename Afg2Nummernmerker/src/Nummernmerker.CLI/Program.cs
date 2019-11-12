namespace Nummernmerker.CLI
{
    using CommandLine;
    using System;
    using System.Linq;

    public class Options
    {
        [Option('f', "file", Required = false, HelpText = "The input file to evaluate.")]
        public string File { get; set; }

        [Option('s', "numberS", Required = false, HelpText = "The number to split as a string. Can contain letters, number is split soley based on the '0' character. May be any size including sizes > " + nameof(Int64.MaxValue))]
        public string NumberString { get; set; }

        [Option('n', "number", Required = false, HelpText = "The number to split. Must be smaller or equal to " + nameof(Int64.MaxValue))]
        public long Number { get; set; }

        [Option('l', "leastDigits", Required = false, Default = 2, HelpText = "The lowest number of digits that can be used consecutiely. Defaults to 2.")]
        public int MinSequenceLength { get; set; }

        [Option('m', "mostDigits", Required = false, Default = 4, HelpText = "The highest number of digits that can be used consecutively without splitting. Defaults to 4.")]
        public int MaxSequenceLength { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions);
        }

        private static void RunWithOptions(Options o)
        {
            string number = "011000000011000100111111101011";

            Console.WriteLine(string.Join(' ', Nummernmerker.MerkNummernToText(number, 2, 4)));
        }
    }
}
