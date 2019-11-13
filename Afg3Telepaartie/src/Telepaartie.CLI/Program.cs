namespace Telepaartie.CLI
{
    using CommandLine;
    using System;
    using System.Linq;
    public static class Program
    {
        public static void Main(string[] args)
        {
            Telepaartiee.MainFrame.GetGoals(5, 5).ForEach((x) => {Console.WriteLine(string.Join(',', x.Select(x => x.ToString())));});
        }
    }
}
