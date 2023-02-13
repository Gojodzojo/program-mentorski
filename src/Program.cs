namespace AlgoBenchmark
{
    public delegate double TargetFunctionType(double[] args);

    class Program
    {
        public static void Main(string[] args)
        {
            var functions = MinimizedFunction.GetMinimizedFunctions();
            var algorithms = OptimizationAlgorithm.GetOptimisationAlgorithms();

            foreach (var function in functions)
            {
                foreach (var algorithm in algorithms)
                {
                    Console.WriteLine($"Optimising {function.Name} with {algorithm.Name}");

                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var result = algorithm.Solve(function.TargetFunction);
                    watch.Stop();

                    Console.WriteLine($"Minimum: {result}");
                    Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms\n");
                }
            }
        }
    }
}
