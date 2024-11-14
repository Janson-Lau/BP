using BPClassLibrary;

namespace BPConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {           
            //BPFactory bPFactory = new BPFactory([1, 2], 2, 1, 2, 2, 4);
            BPFactory bPFactory = new BPFactory([1, 2], 0, 0, 2, 0.5, 1.0);
            bPFactory.Learn();
            bPFactory.Work();
            Console.ReadKey();
        }
    }
}