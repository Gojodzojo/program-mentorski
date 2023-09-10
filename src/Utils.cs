namespace AlgoBenchmark
{
    class Utils
    {
        public static IOptimizationAlgorithm FromParameters(string name, FitnessFunctionType fitnessFunction, int population, int targetIterations, Dictionary<string, string> flags)
        {
            foreach (var algorithm in GetOptimisationAlgorithms(fitnessFunction, population, targetIterations, flags))
            {
                if (name == algorithm.Name || name == algorithm.Acronym)
                {
                    return algorithm;
                }
            }

            throw new Exception("Algorithm not found");
        }
        public static IOptimizationAlgorithm[] GetOptimisationAlgorithms(FitnessFunctionType fitnessFunction, int population, int targetIterations, Dictionary<string, string> flags)
        {
            return new IOptimizationAlgorithm[]
            {
                new AntColonyOptimization(fitnessFunction, population, targetIterations, flags),
                new GreyWolfOptimizer(fitnessFunction, population, targetIterations),
                new EquilibriumOptimizer(fitnessFunction, population, targetIterations, flags),
                new ChimpOptimizationAlgorithm(fitnessFunction, population, targetIterations, flags),
            };
        }

        public static string getTestDirectory(string algorithmAcronym, int testNumber)
        {
            return $"tests/{algorithmAcronym}_test_{testNumber}";
        }

        public static string getStateFilePath(string algorithmAcronym, int testNumber)
        {
            return $"{getTestDirectory(algorithmAcronym, testNumber)}/state.csv";
        }

        public static string getResultFilePath(string algorithmAcronym, int testNumber)
        {
            return $"{getTestDirectory(algorithmAcronym, testNumber)}/result.csv";
        }

        public static int findTestNumber(string algorithmAcronym)
        {
            for (int i = 0; true; i++)
            {
                if (!Directory.Exists(getTestDirectory(algorithmAcronym, i)))
                {
                    return i;
                }
            }
        }
    }
}