namespace Nummernmerker.CLI
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public class Options
    {
        [Option('f', "file", Required = false, HelpText = "The input file to evaluate.")]
        public string File { get; set; }

        [Option('s', "numberS", Required = false, HelpText = "The number to split as a string. Can contain letters, number is split soley based on the '0' character. May be any size including sizes > " + nameof(Int64.MaxValue))]
        public string NumberString { get; set; }

        [Option('n', "number", Required = false, HelpText = "The number to split. Must be smaller or equal to " + nameof(Int64.MaxValue))]
        public long? Number { get; set; }

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
            if (o.Number.HasValue && o.NumberString != null) Console.WriteLine("Can't have --number and --numberS set at the same time.");

            if (o.Number.HasValue) Run(o.Number.ToString(), o.MinSequenceLength, o.MaxSequenceLength);
            else if (o.NumberString != null) Run(o.NumberString, o.MinSequenceLength, o.MaxSequenceLength);
            else Console.WriteLine("Either --number or --numberS need to be set.");
        }

        private static void Run(string numberText, int minSequenceLength, int maxSequenceLength)
        {
            RunWithStackSize(() => RunCore(numberText, minSequenceLength, maxSequenceLength), numberText.Length * 1000);
        }

        private static void RunWithStackSize(Action action, int stackSize)
        {
            var thread = new Thread(new ThreadStart(action),  stackSize);
            thread.Start();
        }

        private static void RunCore(string numberText, int minSequenceLength, int maxSequenceLength)
        {
            Console.WriteLine("Starting splitting of number " + numberText + " with segments of length " + minSequenceLength + ".." + maxSequenceLength);
            Console.WriteLine("  Digits:             " + numberText.Length);

            Stopwatch stopwatch = new Stopwatch();
            var result = Nummernmerker.MerkNummern(numberText, minSequenceLength, maxSequenceLength);
            stopwatch.Stop();

            Console.WriteLine("Results: ");
            Console.WriteLine("  Calculation time:   " + stopwatch.ElapsedMilliseconds + "ms");
            Console.WriteLine("  Leading zeros hit:  " + result.LeadingZerosHit);
            Console.WriteLine("  Final distribution: " + string.Join(' ', result.ApplyDistribution(numberText)));
        }
    }
}
