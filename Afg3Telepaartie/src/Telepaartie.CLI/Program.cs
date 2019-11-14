using System;
using System.Diagnostics;
namespace Telepaartie.CLI
{
    using CommandLine;
    using System;
    using System.Linq;
    using System.Diagnostics;

    public class Options
    {[Option('c', "Cups", Required = false, Default = 3, HelpText = "Die Anzahl der Tassen")]
        public int Cups { get; set; }

        [Option('e', "Elements", Required = false, Default = 15, HelpText = "Die Anzahl aller Elemente.")]
        public int Elements { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Ausgabe der aktuellen Iteration")]
        public bool Verbose { get; set; }
        
        [Option('s', "stopwatch", Required = false, HelpText = "Ob die für die Berechnung notwendige Zeit gemessen werden soll.")]
        public bool Stopwatch { get; set; }
    }
    public static class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length == 0) args = "-c 3 -e 1000 -s -v".Split(' ');
            Parser.Default.ParseArguments<Options>(args).WithParsed(KrinschBoye);
        }

        private static void KrinschBoye(Options o)
        {
            Console.WriteLine($"You'll need {Telepaartie.Teelepartie.LLL(o.Cups, o.Elements, (o.Verbose)?(Console.WriteLine):(Action<string>)null).ToString()} steps retard!");
        }
    }
}
