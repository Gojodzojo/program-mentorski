namespace AlgoBenchmark
{
    public interface OptimizationAlgorithm
    {
        double Solve(TargetFunctionType TargetFunction);
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
