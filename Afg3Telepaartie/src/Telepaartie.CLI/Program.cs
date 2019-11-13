namespace Telepaartie.CLI
{
    using CommandLine;
    using System;
    using System.Linq;
    public static class Program
    {
        public static void Main(string[] args)
        {
            #if false
            Telepaartiee.MainFrame.GetGoals(5, 15)
                .ToList()
                .ForEach(x => {Console.WriteLine(string.Join(',', x.Select(x => x.ToString())));});
            #else
            Console.WriteLine(Telepaartie.MainFrame.LLL(3, 5,Console.WriteLine));
            Console.WriteLine("I lick ur feet");
            #endif
        }
    }
}
