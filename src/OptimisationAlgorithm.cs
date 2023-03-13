namespace AlgoBenchmark
{
    public interface OptimizationAlgorithm
    {
        double Solve(MinimizedFunction minimizedFunction, int iterations);
        string Name { get; }
        // void SaveStartPoputation();
        // void SaveStateOfAlghoritm();


        static OptimizationAlgorithm[] GetOptimisationAlgorithms()
        {
            return new[]
            {
                new AntColonyOptimizationAlgorithm()
            };
        }
    }
}
