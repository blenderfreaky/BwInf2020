namespace Nummernmerker.CLI
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Running;
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Benchmark for Nummernmerker
    /// </summary>
    /// <remarks>Do not instantiate multiple instances of this class at once</remarks>
    [SimpleJob(launchCount: 1, warmupCount: 2, targetCount: 50)]
    public class NummernmerkerBenchmark
    {
        public static int Length = 1000;
        public static double ZeroPropability = 0.5;
        private readonly Random Random = new Random();
        private bool[] Text;

        [IterationSetup]
        public void IterationSetup() => Text = Enumerable.Range(0, Length).Select(_ => Random.NextDouble() < ZeroPropability).ToArray();

        [IterationCleanup]
        public void IterationCleanup() => Nummernmerker.ClearCache();

        [Benchmark]
        public NummerMerkingSolution MerkNummer() => MerkNummer(Text, 2, 4);

        private static NummerMerkingSolution MerkNummer(bool[] numberText, int minSequenceLength, int maxSequenceLength) => Nummernmerker.MerkNummern(numberText, minSequenceLength, maxSequenceLength);
    }
}
