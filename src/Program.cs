using System.IO;

namespace AlgoBenchmark
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args[0] == "test")
            {
                var flags = parseArgs(args);
                test(flags);
            }
            else if (args[0] == "resume")
            {
                resume(args);
            }
            else
            {
                Console.WriteLine("Unknown mode");
            }
        }

        static void test(Dictionary<string, string> flags)
        {
            if (!flags.ContainsKey("algorithm") || !flags.ContainsKey("fitness-function"))
            {
                Console.WriteLine("algorithm and fitness-function must be specified");
                return;
            }
            string algorithName = flags["algorithm"];
            string fitnessFunctionName = flags["fitness-function"];
            int dimensions = int.Parse(flags.GetValueOrDefault("dimensions", "2"));
            int population = int.Parse(flags.GetValueOrDefault("population", "30"));
            int iterations = int.Parse(flags.GetValueOrDefault("iterations", "50"));

            var fitnessFunction = FitnessFunctionType.FromParameters(fitnessFunctionName, dimensions);
            var algorithm = Utils.FromParameters(algorithName, fitnessFunction, population, iterations, flags);
            var result = algorithm.Solve();
            algorithm.SaveResult();
        }

        static void resume(string[] args)
        {
            // if (File.Exists(AntColonyOptimization.DefaultStatePath))
            // {
            //     IOptimizationAlgorithm algorithm = new AntColonyOptimization();
            //     var result = algorithm.Solve();
            //     algorithm.SaveResult();
            // }

            // if (File.Exists(GreyWolfOptimizer.DefaultStatePath))
            // {
            //     IOptimizationAlgorithm algorithm = new GreyWolfOptimizer();
            //     var result = algorithm.Solve();
            //     algorithm.SaveResult();
            // }

            // if (File.Exists(EquilibriumOptimizer.DefaultStatePath))
            // {
            //     IOptimizationAlgorithm algorithm = new EquilibriumOptimizer();
            //     var result = algorithm.Solve();
            //     algorithm.SaveResult();
            // }
        }

        static Dictionary<string, string> parseArgs(string[] args)
        {
            var flags = new Dictionary<string, string>();

            for (int i = 1; i < args.Length; i += 2)
            {
                var flag = args[i].Substring(2);
                flags[flag] = args[i + 1];
            }

            return flags;
        }
    }
}
