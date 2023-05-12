using System.IO;

namespace AlgoBenchmark
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args[0] == "test")
            {
                test(args);
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

        static void test(string[] args)
        {
            string algorithName = args[1];
            string fitnessFunctionName = args[2];
            int dimensions = int.Parse(args[3]);
            int population = int.Parse(args[4]);
            int iterations = int.Parse(args[5]);

            var fitnessFunction = FitnessFunctionType.FromParameters(fitnessFunctionName, dimensions);
            var algorithm = IOptimizationAlgorithm.FromParameters(algorithName, fitnessFunction, population, iterations);
            var result = algorithm.Solve();
            algorithm.SaveResult();
        }

        static void resume(string[] args)
        {
            if (File.Exists(AntColonyOptimization.DefaultStatePath))
            {
                IOptimizationAlgorithm algorithm = new AntColonyOptimization();
                var result = algorithm.Solve();
                algorithm.SaveResult();
            }

            if (File.Exists(GreyWolfOptimizer.DefaultStatePath))
            {
                IOptimizationAlgorithm algorithm = new GreyWolfOptimizer();
                var result = algorithm.Solve();
                algorithm.SaveResult();
            }

            if (File.Exists(EquilibriumOptimizer.DefaultStatePath))
            {
                IOptimizationAlgorithm algorithm = new EquilibriumOptimizer();
                var result = algorithm.Solve();
                algorithm.SaveResult();
            }
        }
    }
}
