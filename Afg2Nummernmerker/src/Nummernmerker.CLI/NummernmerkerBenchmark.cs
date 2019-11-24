namespace Nummernmerker.CLI
{
    using BenchmarkDotNet.Attributes;
    using System;
    using System.Linq;

    /// <summary>
    /// Benchmark for Nummernmerker
    /// </summary>
    /// <remarks>Do not instantiate multiple instances of this class at once</remarks>
    [SimpleJob(launchCount: 10, warmupCount: 2, targetCount: 50)]
    public class NummernmerkerBenchmark
    {
        public static int Length;
        public static double ZeroPropability;
        public static bool[] GlobalText;

        public static int MinSequenceLength;
        public static int MaxSequenceLength;

        private readonly Random _random = new Random();
        private bool[] _text;

        [IterationSetup]
        public void IterationSetup() => _text = GlobalText ?? Enumerable.Range(0, Length).Select(_ => _random.NextDouble() < ZeroPropability).ToArray();

        [IterationCleanup]
        public void IterationCleanup() => Nummernmerker.ClearCache();

        [Benchmark]
        public NummerMerkingSolution MerkNummer() => MerkNummer(_text, MinSequenceLength, MaxSequenceLength);

        private static NummerMerkingSolution MerkNummer(bool[] numberText, int minSequenceLength, int maxSequenceLength) => Nummernmerker.MerkNummern(numberText, minSequenceLength, maxSequenceLength);
    }
}