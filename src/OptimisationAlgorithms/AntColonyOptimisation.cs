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
        private FitnessFunctionType FitnessFunction;

        private int TargetIterations;

        private int CurrentIteration;

        public int NumberOfEvaluationFitnessFunction { get; private set; }

        public long Time { get; private set; }

        public const string DefaultStatePath = "state/ACO.csv";

        public string Name
        {
            get => "Ant Colony Optimization";
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
        public AntColonyOptimization(FitnessFunctionType fitnessFunction, int population, int targetIterations, int L = 10, double ksi = 1, double q = 0.9)
        {
            this.FitnessFunction = fitnessFunction;
            this.M = population;
            this.TargetIterations = targetIterations;
            this.L = L;
            this.ksi = ksi;
            this.q = q;
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
        public AntColonyOptimization(string filePath = AntColonyOptimization.DefaultStatePath)
        {
            var file = File.OpenText(filePath);
            file.ReadLine();
            var metadata = file.ReadLine().Split(';');

            var functionName = metadata[0];
            var dimensions = int.Parse(metadata[1]);
            this.FitnessFunction = FitnessFunctionType.FromParameters(functionName, dimensions);
            this.M = int.Parse(metadata[2]);
            this.TargetIterations = int.Parse(metadata[3]);
            this.CurrentIteration = int.Parse(metadata[4]);
            this.NumberOfEvaluationFitnessFunction = int.Parse(metadata[5]);
            this.Time = long.Parse(metadata[6]);
            this.L = int.Parse(metadata[7]);
            this.ksi = double.Parse(metadata[8]);
            this.q = double.Parse(metadata[9]);
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

            Directory.CreateDirectory("state");
            var file = File.CreateText(AntColonyOptimization.DefaultStatePath);

            file.WriteLine("fitnessFunction Name; fitnessFunction Dimensions; M; TargetIterations; CurrentIteration; NumberOfEvaluationFitnessFunction; Time; L; ksi; q;");

            file.WriteLine($"{FitnessFunction.Name}; {FitnessFunction.Dimensions}; {M}; {TargetIterations}; {CurrentIteration}; {NumberOfEvaluationFitnessFunction}; {Time}; {L}; {ksi}; {q};");

            file.WriteLine();

            for (int i = 0; i < L + M; i++)
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
            if (shouldExit) System.Environment.Exit(0);
            Console.CancelKeyPress -= preventExit;
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

            File.Delete(DefaultStatePath);
            return FBest;
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