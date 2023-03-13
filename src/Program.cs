using System.IO;

namespace AlgoBenchmark
{
    class Program
    {
        public static void Main(string[] args)
        {
            var numberOfIterations = 100;
            var algorithms = OptimizationAlgorithm.GetOptimisationAlgorithms();

            var file = GetFile();
            file.WriteLine("Minimized function, Optimization algorithm, Number of iterations, Number of unknown parameters, Result, Time[ms]");

            for (int unknownParametersNumber = 1; unknownParametersNumber < 50; unknownParametersNumber++)
            {
                foreach (var function in MinimizedFunction.GetTestMinimizedFunctions(unknownParametersNumber))
                {
                    foreach (var algorithm in algorithms)
                    {
                        Console.WriteLine($"Optimising {function.Name} with {algorithm.Name}");

                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        var result = algorithm.Solve(function, numberOfIterations);
                        watch.Stop();

                        file.WriteLine($"{function.Name}, {algorithm.Name}, {numberOfIterations}, {unknownParametersNumber}, {result}, {watch.ElapsedMilliseconds}");
                    }
                }
            }

            file.Flush();
        }

        static StreamWriter GetFile()
        {
            var fileNumber = 0;

            Directory.CreateDirectory("tests");

            while (true)
            {
                var path = $"tests/test_{fileNumber}.csv";

                if (!File.Exists(path))
                {
                    return File.CreateText(path);
                }

                fileNumber++;
            }

        }
    }
}
