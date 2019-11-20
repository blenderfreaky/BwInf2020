namespace Telepaartie.CLI
{
    using CommandLine;
    using System;
    using System.Linq;
    using System.Diagnostics;
    using System.Collections.Generic;

    public class Options
    {[Option('c', "Cups", Required = false, Default = null, HelpText = "Die Anzahl der Tassen")]
        public int? Cups { get; set; }

        [Option('e', "Elements", Required = false, Default = null, HelpText = "Die Anzahl aller Elemente.")]
        public int? Elements { get; set; }

        [Option('l', "List", Required = false, Default = null, HelpText = "Wenn nur ein bestimmter Fall abgedeckt werden soll")]
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
            //if(args.Length == 0) args = "-c 4 -e 600 -s -v".Split(' ');
            Parser.Default.ParseArguments<Options>(args).WithParsed(KrinschBoye);
        }

        private static void KrinschBoye(Options o)
        {
            if(o.List != null && (o.Elements != null || o.Cups != null))
            {
                Console.WriteLine("You dumb bitch");
            }
            
            else if(o.Elements == null || o.Cups == null || o.List == null)
            {
                string input = string.Empty;

                crinsch0:
                Console.Write("Wollen sie die benötigten Operationen für einen Verteilung von Bibern erfahren oder die maximal benötigten Operationen für eine Anzahl an Bibern. \"flase\" für eine Biber-Anzahl, \"true\" oder Enter für eine gegebene Verteilung. ");
                input = Console.ReadLine();
                if (input == "") { }
                else { try { if (!bool.Parse(input)) goto crinsch1; }
                catch { Console.WriteLine("Wrong input faggot!"); goto crinsch0; } }
                goto crinsch5;

                crinsch1:
                Console.Write("Wie viele Elemente sollen verteilt werden? Enter für 15. ");
                input = Console.ReadLine();
                if(input=="") o.Elements = 15;
                else { try { o.Elements = int.Parse(input); }
                catch { Console.WriteLine("Wrong input faggot!"); goto crinsch1; } }

                crinsch2:
                Console.Write("Auf wie viele Behälter sollen die Biber aufgeteilt werden? Enter für 3. ");
                input = Console.ReadLine();
                if(input=="") o.Cups = 3;
                else { try { o.Cups = int.Parse(input); }
                catch { Console.WriteLine("Wrong input faggot!"); goto crinsch2; } }

                crinsch3:
                Console.Write("Soll bei der Berechnung der aktuelle Zustand ausgegeben werden? \"flase\" für nein, \"true\" für ja und Enter für nein. ");
                input = Console.ReadLine();
                if(input=="") o.Verbose = false;
                else { try { o.Verbose = bool.Parse(input); }
                catch { Console.WriteLine("Wrong input faggot!"); goto crinsch3; } }

                crinsch4:
                Console.Write("Soll nach der Berechnung die benötigte Zeit ausgegeben werden? \"flase\" für nein, \"true\" für ja und Enter für ja. ");
                input = Console.ReadLine();
                if(input=="") o.Stopwatch = true;
                else { try { o.Stopwatch = bool.Parse(input); }
                catch { Console.WriteLine("Wrong input faggot!"); goto crinsch4; } }
                crinsch5:;
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine($"You'll need {Telepaartie.Teelepartie.LLL(o.Cups.Value, o.Elements.Value, (o.Verbose)?(Console.Write):(Action<string>)null, o.List}).ToString()} steps retard!");
            stopWatch.Stop();
            if(o.Stopwatch) Console.WriteLine($"Die Berechnung dauerte {stopWatch.Elapsed.ToString(@"m\:ss")} Minuten.");
        }
    }
}
