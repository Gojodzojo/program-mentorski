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
        void LoadFromFileStateOfAlghoritm() { }

        // Metoda zapisująca do pliku wynik działania algorytmu wraz z paramterami
        void SaveResult();

        string Name { get; }
        string Acronym { get; }

        // Właściowść zwracająca tablicę z najlepszym osobnikiem
        double[] XBest { get; }

        // Właściwość zwracająca wartość funkcji dopasowania (celu) dla najlepszego osobnika
        double FBest { get; }

        // Właściwość zwracająca liczbę wywołań funkcji dopasowania (celu)
        int NumberOfEvaluationFitnessFunction { get; }
        long Time { get; }
    }
}
