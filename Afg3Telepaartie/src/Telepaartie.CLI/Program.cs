namespace Telepaartie.CLI
{
    using CommandLine;
    using System;
    using System.Linq;
    using System.Diagnostics;
    using System.Collections.Generic;

    public class Options
    {[Option('c', "Cups", Required = false, SetName ="Adv", Default = null, HelpText = "Die Anzahl der Tassen")]
        public int? Cups { get; set; }

        [Option('e', "Elements", Required = false, SetName ="Adv", Default = null, HelpText = "Die Anzahl aller Elemente.")]
        public int? Elements { get; set; }

        [Option('l', "List", Required = false, SetName ="Eas", Default = null, Separator = ',', HelpText = "Wenn nur ein bestimmter Fall abgedeckt werden soll")]
        public IList<int>? List { get; set; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Ausgabe der aktuellen Iteration")]
        public bool Verbose { get; set; }
        
        [Option('s', "stopwatch", Required = false, Default = true, HelpText = "Ob die für die Berechnung notwendige Zeit gemessen werden soll.")]
        public bool Stopwatch { get; set; }
    }
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(RunWithOpts);
        }

        private static void RunWithOpts(Options o)
        {
            string input = string.Empty;
            o.List = o.List?.Count==0 ? null : o.List;

            if(!((o.Elements != null && o.Cups != null) || o.List != null))
            {
                crinsch0:
                Console.Write("Wollen sie die benötigten Operationen für einen Verteilung von Bibern erfahren oder die maximal benötigten Operationen für eine Anzahl an Bibern. \"flase\" für eine Biber-Anzahl, \"true\" oder Enter für eine gegebene Verteilung. ");
                input = Console.ReadLine();
                if (input == "") { }
                else { try { if (!bool.Parse(input)) goto crinsch1; }
                catch { Console.WriteLine("Diese Eingabe kann leider nicht interpretiert werden. Versuche es doch erneut. Viel Glück <3!"); goto crinsch0; } }

                crinsch1:
                Console.Write("Geben sie die Anzahl der Biber pro Behälter an: ");
                input = Console.ReadLine();
                try
                {
                    List<int> numbers = input.Split(',').Select(Int32.Parse).ToList();
                    if(numbers.Count == 0) throw new Exception();
                    o.List = numbers;
                }
                catch { Console.WriteLine("Diese Eingabe kann leider nicht interpretiert werden. Versuche es doch erneut. Viel Glück <3!"); goto crinsch1; } }
                goto crinsch6;

                crinsch2:
                Console.Write("Wie viele Elemente sollen verteilt werden? Enter für 15. ");
                input = Console.ReadLine();
                if(input=="") o.Elements = 15;
                else { try { o.Elements = int.Parse(input); }
                catch { Console.WriteLine("Diese Eingabe kann leider nicht interpretiert werden. Versuche es doch erneut. Viel Glück <3!"); goto crinsch2; } }

                crinsch3:
                Console.Write("Auf wie viele Behälter sollen die Biber aufgeteilt werden? Enter für 3. ");
                input = Console.ReadLine();
                if(input=="") o.Cups = 3;
                else { try { o.Cups = int.Parse(input); }
                catch { Console.WriteLine("Diese Eingabe kann leider nicht interpretiert werden. Versuche es doch erneut. Viel Glück <3!"); goto crinsch3; } }

                crinsch4:
                Console.Write("Soll bei der Berechnung der aktuelle Zustand ausgegeben werden? \"flase\" für nein, \"true\" für ja und Enter für nein. ");
                input = Console.ReadLine();
                if(input=="") o.Verbose = false;
                else { try { o.Verbose = bool.Parse(input); }
                catch { Console.WriteLine("Diese Eingabe kann leider nicht interpretiert werden. Versuche es doch erneut. Viel Glück <3!"); goto crinsch4; } }

                crinsch5:
                Console.Write("Soll nach der Berechnung die benötigte Zeit ausgegeben werden? \"flase\" für nein, \"true\" für ja und Enter für ja. ");
                input = Console.ReadLine();
                if(input=="") o.Stopwatch = true;
                else { try { o.Stopwatch = bool.Parse(input); }
                catch { Console.WriteLine("Diese Eingabe kann leider nicht interpretiert werden. Versuche es doch erneut. Viel Glück <3!"); goto crinsch5; } }
                crinsch6:;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int steps = Telepaartie.Teelepartie.LLL(
                o.Cups ?? 3,
                o.Elements ?? 15,
                (o.Verbose) ? (Console.Write) : (Action<string>)null,
                o.List?.ToList());

            Console.WriteLine($"\nFERTIG!\nMan benötigt {steps} Telepaartie-Schritte");
            stopWatch.Stop();
            if(o.Stopwatch) Console.WriteLine($"Die Berechnung dauerte {stopWatch.Elapsed.ToString(@"m\:ss")} Minuten.");
        }
    }
}
