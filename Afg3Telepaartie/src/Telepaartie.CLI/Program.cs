using Telepaartie;

namespace Nummernmerker.CLI
{
    using CommandLine;
    using System;

    public class Options
    {
        [Option('n', Required = false, Default = 3, HelpText = "The total sum of elements distributed over the buckets.")]
        public int N { get; set; }

        [Option('b', "buckets", Required = false, Default = 3, HelpText = "The number of buckets to use. (Default: 3)")]
        public int Buckets { get; set; }
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
            Console.WriteLine(Telepaartie.Telepaartie.CalculateLLLForN(o.N, o.Buckets));
            // TODO: Maybe show the longest running solution(s)
        }
    }
}
