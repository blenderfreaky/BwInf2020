﻿namespace Nummernmerker.CLI
{
    using BenchmarkDotNet.Running;
    using CommandLine;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class Options
    {
        [Option('f', "file", SetName = "File", HelpText = "The input file to evaluate.")]
        public string File { get; set; }

        [Option('s', "numberS", SetName = "NumberS", HelpText = "The number to split as a string. Can contain letters, number is split soley based on the '0' character. May be any size including sizes > " + nameof(Int64.MaxValue))]
        public string NumberString { get; set; }

        [Option('n', "number", SetName = "Number", HelpText = "The number to split. Must be smaller or equal to " + nameof(Int64.MaxValue))]
        public long? Number { get; set; }

        [Option('l', "leastDigits", Default = 2, SetName = "Bench", HelpText = "The lowest number of digits that can be used consecutiely.")]
        public int MinSequenceLength { get; set; }

        [Option('m', "mostDigits", Default = 4, SetName = "Bench", HelpText = "The highest number of digits that can be used consecutively without splitting.")]
        public int MaxSequenceLength { get; set; }

        [Option('b', "benchmark", SetName = "Bench", HelpText = "Whether to run a benchmark.")]
        public bool Benchmark { get; set; }

        [Option('d', "benchmarkDigits", Default = 1000, SetName = "Bench", HelpText = "The amount of digits to benchmark with random numbers with.")]
        public int BenchmarkForLength { get; set; }

        [Option('p', "benchmarkZeroPropability", Default = 0.5, SetName = "Bench", HelpText = "The propability a digits in the benchmark is going to be zero.")]
        public double BenchmarkZeroPropability { get; set; }
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
            if (o.Number.HasValue)
            {
                Run(o.Number.ToString(), o.MinSequenceLength, o.MaxSequenceLength, o.Benchmark);
            }
            else if (o.NumberString != null)
            {
                Run(o.NumberString, o.MinSequenceLength, o.MaxSequenceLength, o.Benchmark);
            }
            else if (o.File != null)
            {
                foreach (var line in File.ReadAllLines(o.File))
                {
                    Run(line, o.MinSequenceLength, o.MaxSequenceLength, o.Benchmark);
                }
            }
            else if (o.Benchmark && o.BenchmarkForLength > 0)
            {
                NummernmerkerBenchmark.Length = o.BenchmarkForLength;
                NummernmerkerBenchmark.ZeroPropability = o.BenchmarkZeroPropability;
                NummernmerkerBenchmark.MinSequenceLength = o.MinSequenceLength;
                NummernmerkerBenchmark.MaxSequenceLength = o.MaxSequenceLength;
                BenchmarkRunner.Run<NummernmerkerBenchmark>();
            }
            else
            {
                Console.WriteLine("Either --number, --numberS or --file need to be set, or --benchmark needs to be true.");
            }
        }

        private static void Run(string numberText, int minSequenceLength, int maxSequenceLength, bool bench)
        {
            if (!bench)
            {
                // Very big numbers produce a stack overflow => just use a bigger stack
                RunWithStackSize(() => RunCore(numberText, minSequenceLength, maxSequenceLength), numberText.Length * 1000);
            }
            else
            {
                NummernmerkerBenchmark.GlobalText = numberText.Select(x => x == '0').ToArray();
                NummernmerkerBenchmark.MinSequenceLength = minSequenceLength;
                NummernmerkerBenchmark.MaxSequenceLength = maxSequenceLength;
                BenchmarkRunner.Run<NummernmerkerBenchmark>();
            }
        }

        private static void RunWithStackSize(Action action, int stackSize)
        {
            //action();return;
            var thread = new Thread(new ThreadStart(action), stackSize);
            thread.Start();

            // Synchronize
            while (thread.IsAlive)
            {
                Task.Delay(5);
            }
        }

        private static void RunCore(string numberText, int minSequenceLength, int maxSequenceLength)
        {
            Console.WriteLine("Starting splitting of number " + numberText + " with segments of length " + minSequenceLength + ".." + maxSequenceLength);
            Console.WriteLine("  Digits:             " + numberText.Length);

            var result = Nummernmerker.MerkNummern(numberText, minSequenceLength, maxSequenceLength);

            Console.WriteLine("Results: ");

            if (result.IsSuccessful)
            {
                Console.WriteLine("  Leading zeros hit:  " + result.LeadingZerosHit);
                Console.WriteLine("  Final distribution: " + string.Join(' ', result.ApplyDistribution(numberText)));
            }
            else
            {
                Console.WriteLine("  Failure");
            }
        }
    }
}