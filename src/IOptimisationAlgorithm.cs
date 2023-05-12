namespace AlgoBenchmark
{
    public interface IOptimizationAlgorithm
    {
        // Metoda zaczynająca rozwiązywanie zagadnienia poszukiwania minimum funkcji celu
        // od numeru iteracji (populację z numerem możemy wczytać z pliku)
        double Solve();

        // Metoda zapisująca do pliku stan algorytmu (w odpowiednim formacie)
        // stan algorytmu: numer iteracji, populacja, liczba wywołań funkcji celu
        void SaveToFileStateOfAlghoritm();

        // Metoda wczytująca z pliku stan algorytmu (w odpowiednim formacie)
        // stan algorytmu: numer iteracji, populacja, liczba wywołań funkcji celu
        // void LoadFromFileStateOfAlghoritm();

        // Metoda zapisując do pliku wynik działania algorytmu wraz z paramterami
        void SaveResult()
        {
            var file = GetFile();
            file.WriteLine("Optimization algorithm; NumberOfEvaluationFitnessFunction; Result; Number Of Evaluation Fitness Function; Time[ms]");
            file.WriteLine($"{Name}; {NumberOfEvaluationFitnessFunction}; {FBest}; {NumberOfEvaluationFitnessFunction}; {Time}");
            file.Flush();
        }

        string Name { get; }

        // Właściowść zwracająca tablicę z najlepszym osobnikiem
        double[] XBest { get; }

        // Właściwość zwracająca wartość funkcji dopasowania (celu) dla najlepszego osobnika
        double FBest { get; }

        // Właściwość zwracająca liczbę wywołań funkcji dopasowania (celu)
        int NumberOfEvaluationFitnessFunction { get; }
        long Time { get; }

        static IOptimizationAlgorithm FromParameters(string name, FitnessFunctionType fitnessFunction, int population, int targetIterations)
        {
            foreach (var algorithm in GetOptimisationAlgorithms(fitnessFunction, population, targetIterations))
            {
                if (name == algorithm.Name)
                {
                    return algorithm;
                }
            }

            throw new Exception("Algorithm not found");
        }
        static IOptimizationAlgorithm[] GetOptimisationAlgorithms(FitnessFunctionType fitnessFunction, int population, int targetIterations)
        {
            return new IOptimizationAlgorithm[]
            {
                new AntColonyOptimization(fitnessFunction, population, targetIterations),
                new GreyWolfOptimizer(fitnessFunction, population, targetIterations),
                new EquilibriumOptimizer(fitnessFunction, population, targetIterations),
            };
        }

        private static StreamWriter GetFile()
        {
            Directory.CreateDirectory("tests");

            for (var fileNumber = 0; true; fileNumber++)
            {
                var path = $"tests/test_{fileNumber}.csv";

                if (!File.Exists(path))
                {
                    return File.CreateText(path);
                }
            }
        }
    }
}
