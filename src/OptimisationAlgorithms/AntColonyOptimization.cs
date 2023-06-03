using MathNet.Numerics.Distributions;

namespace AlgoBenchmark
{
    class AntColonyOptimization : IOptimizationAlgorithm
    {
        // Number of ants in population
        private int M;

        // Number of pheromone spots
        private int L;

        // ξ
        private double ksi;

        private double q;

        // Pheromone spots
        private PheromoneSpot[] T;

        private Random rnd = new Random();
        private int testNumber;
        private FitnessFunctionType FitnessFunction;

        private int TargetIterations;

        private int CurrentIteration;

        public int NumberOfEvaluationFitnessFunction { get; private set; }

        public long Time { get; private set; }

        public string Name
        {
            get => "Ant Colony Optimization";
        }

        public string Acronym
        {
            get => "ACO";
        }

        public double[] XBest
        {
            get
            {
                this.SortPopulation();
                return T[0].x;
            }
        }

        public double FBest
        {
            get
            {
                this.SortPopulation();
                return T[0].result;
            }
        }

        // Create new instance of object from scratch
        public AntColonyOptimization(FitnessFunctionType fitnessFunction, int population, int targetIterations, Dictionary<string, string> flags)
        {
            this.FitnessFunction = fitnessFunction;
            this.M = population;
            this.TargetIterations = targetIterations;
            this.L = int.Parse(flags.GetValueOrDefault("L", "10"));
            this.ksi = double.Parse(flags.GetValueOrDefault("ksi", "1"));
            this.q = double.Parse(flags.GetValueOrDefault("q", "0,9"));
            this.testNumber = Utils.findTestNumber(Acronym);
            this.Time = 0;
            this.CurrentIteration = 0;
            this.NumberOfEvaluationFitnessFunction = 0;
            this.T = new PheromoneSpot[L + M];

            for (int i = 0; i < L + M; i++)
            {
                T[i].x = new double[fitnessFunction.Dimensions];

                for (int j = 0; j < fitnessFunction.Dimensions; j++)
                {
                    T[i].x[j] = fitnessFunction.MinCoordinates[j] + rnd.NextDouble() * (fitnessFunction.MaxCoordinates[j] - fitnessFunction.MinCoordinates[j]);
                }
            }
        }

        // Create new instance of object based on state file
        public AntColonyOptimization(int testNumber)
        {
            var file = File.OpenText(Utils.getStateFilePath(Acronym, testNumber));
            file.ReadLine();
            var metadata = file.ReadLine().Split(';');

            var functionName = metadata[1];
            var dimensions = int.Parse(metadata[2]);
            this.FitnessFunction = FitnessFunctionType.FromParameters(functionName, dimensions);
            this.testNumber = int.Parse(metadata[0]);
            this.M = int.Parse(metadata[3]);
            this.TargetIterations = int.Parse(metadata[4]);
            this.CurrentIteration = int.Parse(metadata[5]);
            this.NumberOfEvaluationFitnessFunction = int.Parse(metadata[6]);
            this.Time = long.Parse(metadata[7]);
            this.L = int.Parse(metadata[8]);
            this.ksi = double.Parse(metadata[9]);
            this.q = double.Parse(metadata[10]);
            this.T = new PheromoneSpot[L + M];

            file.ReadLine();    // Empty line
            file.ReadLine();    // X headers

            for (int i = 0; i < M + L; i++)
            {
                var xValues = file.ReadLine().Split(';');
                T[i].x = new double[FitnessFunction.Dimensions];

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    T[i].x[j] = double.Parse(xValues[j]);
                }
            }
        }

        public void SaveToFileStateOfAlghoritm()
        {
            bool shouldExit = false;

            ConsoleCancelEventHandler preventExit = (sender, e) =>
            {
                shouldExit = true;
                e.Cancel = true;
            };

            Console.CancelKeyPress += preventExit;

            SaveIterationState();
            SaveLoadableState();

            if (shouldExit) System.Environment.Exit(0);
            Console.CancelKeyPress -= preventExit;
        }

        private void SaveIterationState()
        {
            string iterationStateDirectory = Utils.getTestDirectory(Acronym, testNumber);
            Directory.CreateDirectory(iterationStateDirectory);

            var file = File.CreateText($"{iterationStateDirectory}/iteration_{CurrentIteration}.txt");

            file.WriteLine($"{CurrentIteration} [numer iteracji]");
            file.WriteLine($"{NumberOfEvaluationFitnessFunction} [liczba wywołań funkcji celu]");

            for (int i = 0; i < M; i++)
            {
                foreach (var x in T[i].x)
                {
                    file.Write($"{x} ");
                }
                file.WriteLine();
            }

            file.Close();
        }

        public void SaveLoadableState()
        {
            var file = File.CreateText(Utils.getStateFilePath(Acronym, testNumber));

            file.WriteLine("testNumber; fitnessFunction Name; fitnessFunction Dimensions; M; TargetIterations; CurrentIteration; NumberOfEvaluationFitnessFunction; Time; L; ksi; q;");

            file.WriteLine($"{testNumber}; {FitnessFunction.Name}; {FitnessFunction.Dimensions}; {M}; {TargetIterations}; {CurrentIteration}; {NumberOfEvaluationFitnessFunction}; {Time}; {L}; {ksi}; {q};");

            file.WriteLine();

            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                file.Write($"x{i}; ");
            }

            file.WriteLine();

            for (int i = 0; i < L + M; i++)
            {
                foreach (var x in T[i].x)
                {
                    file.Write($"{x}; ");
                }
                file.WriteLine();
            }

            file.Close();
        }

        private void SortPopulation()
        {
            Array.Sort(T, (a, b) => a.result.CompareTo(b.result));
        }

        public double Solve()
        {
            for (; CurrentIteration < TargetIterations; CurrentIteration++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                for (int i = 0; i < L + M; i++)
                {
                    T[i].result = CalculateFitnessFunction(T[i].x);
                }

                this.SortPopulation();

                var omegas = new double[L];
                double omega_acc = 0;

                for (int i = 0; i < L; i++)
                {
                    omegas[i] = Math.Exp((-(i - 1) * (i - 1)) / 2 * q * q * L * L * 2) / (q * L * Math.Sqrt(Math.PI * 2));
                    omega_acc += omegas[i];
                }

                for (int i = 0; i < L; i++)
                {
                    T[i].p = omegas[i] / omega_acc;
                }

                for (int i = 0; i < M; i++)
                {
                    var prob = rnd.NextDouble();

                    double p_acc = 0;
                    int spot_index = 0;
                    for (int j = 0; j < L; j++)
                    {
                        p_acc += T[j].p;


                        if (prob <= p_acc)
                        {
                            spot_index = j;
                            break;
                        }
                    }

                    for (int j = 0; j < FitnessFunction.Dimensions; j++)
                    {
                        // μ
                        double mu = T[spot_index].x[j];

                        // σ
                        double sigma = 0;
                        for (int p = 0; p < L; p++)
                        {
                            sigma += Math.Abs(T[p].x[j] - mu);
                        }
                        sigma *= ksi / (L - 1);

                        double num = Normal.Sample(mu, sigma);

                        if (num < FitnessFunction.MinCoordinates[j])
                            num = FitnessFunction.MinCoordinates[j];
                        else if (num > FitnessFunction.MaxCoordinates[j])
                            num = FitnessFunction.MaxCoordinates[j];

                        T[L + i].x[j] = num;
                    }
                }

                watch.Stop();
                this.Time += watch.ElapsedMilliseconds;
                SaveToFileStateOfAlghoritm();
            }

            File.Delete(Utils.getStateFilePath(Acronym, testNumber));
            return FBest;
        }

        public void SaveResult()
        {
            var resultFile = File.CreateText(Utils.getResultFilePath(Acronym, testNumber));
            resultFile.WriteLine($"{NumberOfEvaluationFitnessFunction} [liczba wywołań funkcji celu]");
            resultFile.Write($"{FBest} ");

            foreach (var x in XBest)
            {
                resultFile.Write($"{x} ");
            }

            resultFile.WriteLine("[najlepszy osobnik wraz z wartością funkcji celu]");
            resultFile.WriteLine($"{M} [liczba mrówek]");
            resultFile.WriteLine($"{L} [liczba plam]");
            resultFile.WriteLine($"{TargetIterations} [liczba iteracji]");
            resultFile.WriteLine($"{q} [parametr q]");
            resultFile.WriteLine($"{ksi} [parametr ksi]");

            resultFile.Close();
        }

        private double CalculateFitnessFunction(double[] args)
        {
            NumberOfEvaluationFitnessFunction++;
            return FitnessFunction.Fn(args);
        }
    }


    struct PheromoneSpot
    {
        public double result;

        // Array with parameters that are passed to TargetFunction
        public double[] x;

        // Probability that an ant will choose this spot
        public double p;
    }
}