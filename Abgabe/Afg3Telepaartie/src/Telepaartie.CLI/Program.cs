namespace Telepaartie.CLI
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class Options
    {
        [Option('c', "Cups", Required = false, SetName = "Adv", Default = null, HelpText = "Die Anzahl der Tassen.")]
        public int? Cups { get; set; }

        [Option('e', "Elements", Required = false, SetName = "Adv", Default = null, HelpText = "Die Anzahl aller Elemente.")]
        public int? Elements { get; set; }

        [Option('l', "List", Required = false, SetName = "Eas", Default = null, Separator = ',', HelpText = "Ein bestimmter Zustand für den die LLL ermittelt werden soll.")]
        public IList<int>? List { get; set; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Ob die aktuelle Iteration ausgegeben werden soll.")]
        public bool Verbose { get; set; }

        [Option('s', "stopwatch", Required = false, Default = true, HelpText = "Ob die für die Berechnung notwendige Zeit gemessen werden soll.")]
        public bool Stopwatch { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(RunWithOptions);
        }

        private static void RunWithOptions(Options o)
        {
            string input = string.Empty;
            o.List = o.List?.Count == 0 ? null : o.List;

            if (!((o.Elements != null && o.Cups != null) || o.List != null))
            {
                bool set = ParseInteractive("Wollen sie die LLL eines Zustands, oder L einer Biberanzahl? \"LLL\" für LLL(s), \"L\" oder ENTER für L(n).",
                    TrySwitch(("LLL", false), ("L", true)), true);

                if (set)
                {
                    while (true)
                    {
                        Console.Write("Geben sie die Anzahl der Biber pro Behälter an: ");
                        input = Console.ReadLine();
                        try
                        {
                            List<int> numbers = input.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                            if (numbers.Count == 0) throw new FormatException();
                            o.List = numbers;
                            break;
                        }
                        catch (FormatException) // Less effort than using TryParse here
                        {
                            Console.WriteLine("Invalide Eingabe");
                        }
                    }
                }
                else
                {
                    o.Elements = ParseInteractive("Wie viele Elemente sollen verteilt werden? ENTER für 15. ", int.TryParse, 15);

                    o.Cups = ParseInteractive("Auf wie viele Behälter sollen die Biber aufgeteilt werden? ENTER für 3. ", int.TryParse, 3);
                }

                o.Verbose = ParseInteractive("Soll bei der Berechnung der aktuelle Zustand ausgegeben werden? ENTER für nein. (y/n) ", TryParseBool, false);

                o.Stopwatch = ParseInteractive("Soll nach der Berechnung die benötigte Zeit ausgegeben werden? ENTER für ja. (y/n) ", TryParseBool, true);
            }

            if (o.Cups != null && o.Elements != null)
            {
                RunCore(() => Telepaartie.LLL(
                        o.Cups.Value,
                        o.Elements.Value,
                        o.Verbose ? Console.Write : (Action<string>?)null),
                    o.Stopwatch);
            }

            if (o.List != null)
            {
                RunCore(() => Telepaartie.L(
                    o.List,
                    o.Verbose ? Console.Write : (Action<string>?)null),
                    o.Stopwatch);
            }
        }

        private static void RunCore(Func<int> stepsCalc, bool doStopwatch)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int steps = stepsCalc();

            Console.WriteLine($"\nFERTIG!\nMan benötigt {steps} Telepaartie-Schritte");
            stopwatch.Stop();
            //if (doStopwatch) Console.WriteLine($"Die Berechnung dauerte {stopwatch.Elapsed.ToString(@"m\:ss")} Minuten.");
            if (doStopwatch) Console.WriteLine($"Die Berechnung dauerte {stopwatch.ElapsedMilliseconds / 1000f}s.");
        }

        private delegate bool Try<T>(string text, out T result);

        private static T ParseInteractive<T>(string request, Try<T> parser, T @default)
        {
            while (true)
            {
                Console.WriteLine(request);

                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return @default;
                }

                if (parser(input, out T result))
                {
                    return result;
                }

                Console.WriteLine("Invalide Eingabe");
            }
        }

        private static bool TryParseBool(string text, out bool result)
        {
            result = text.StartsWith("y", StringComparison.OrdinalIgnoreCase) || text.StartsWith("j", StringComparison.OrdinalIgnoreCase) || text.StartsWith("t", StringComparison.OrdinalIgnoreCase);
            return true;
        }

        private static Try<T> TrySwitch<T>(params (string Text, T Value)[] values)
        {
            var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            foreach (var (text, value) in values) dict.Add(text, value);

            return dict.TryGetValue;
        }
    }
}