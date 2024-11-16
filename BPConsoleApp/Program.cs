using BPClassLibrary;
using System.IO;
using System.Reflection;

namespace BPConsoleApp
{
    internal class Program
    {
        public static string CommonAssemblyLocation => Assembly.GetExecutingAssembly().Location;

        public static string AssemblyDirectory => System.IO.Path.GetDirectoryName(CommonAssemblyLocation);

        public static string BPPath => System.IO.Path.Combine(AssemblyDirectory, "BPConsole.json");

        public static BPFactory bPFactory { get; set; }

        static void Main(string[] args)
        {
            if (!File.Exists(BPPath))
            {
                //bPFactory = new BPFactory(2, [1, 2], 2, 1, 2, [0.5, 1.0], true);
                bPFactory = new BPFactory(1, [1], 2, 1, 1, [2], true);
            }
            else
            {
                bPFactory = JsonUtils.JsonToObject<BPFactory>(BPPath);
                bPFactory.Link(1);
                bPFactory.SetInputNodes([2]);
            }
            bPFactory.Learn();
            bPFactory.Work();
            JsonUtils.ObjectToJson(bPFactory, BPPath);
            Console.ReadKey();         
        }
    }
}