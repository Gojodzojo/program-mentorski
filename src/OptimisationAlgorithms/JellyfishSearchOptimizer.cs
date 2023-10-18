// Based on https://www.mathworks.com/matlabcentral/fileexchange/78961-jellyfish-search-optimizer-js

namespace AlgoBenchmark
{
    class JellyfishSearchOptimizer : IOptimizationAlgorithm
    {
        public int Population;
        private Jellyfish[] jellies;
        private readonly Random rnd = new();
        private int testNumber;
        private FitnessFunctionType FitnessFunction;
        private int TargetIterations;
        private int CurrentIteration;
        public int NumberOfEvaluationFitnessFunction { get; private set; }
        public long Time { get; private set; }

        public string Name
        {
            get => "Jellyfish Search Optimizer";
        }

        public string Acronym
        {
            get => "JS";
        }

        public double[] XBest
        {
            get
            {
                double bestResult = jellies[0].fitness;
                int bestIndex = 0;

                for (int i = 1; i < Population; i++)
                {
                    double result = jellies[i].fitness;

                    if (result < bestResult)
                    {
                        bestResult = result;
                        bestIndex = i;
                    }
                }

                return jellies[bestIndex].x;
            }
        }

        public double FBest
        {
            get
            {
                double bestResult = jellies[0].fitness;

                for (int i = 1; i < Population; i++)
                {
                    double result = jellies[0].fitness;

                    if (result < bestResult)
                    {
                        bestResult = result;
                    }
                }

                return bestResult;
            }
        }

        // Create new instance of object from scratch
        public JellyfishSearchOptimizer(FitnessFunctionType fitnessFunction, int population, int targetIterations, Dictionary<string, string> flags)
        {
            this.FitnessFunction = fitnessFunction;
            this.Population = population;
            this.TargetIterations = targetIterations;
            this.Time = 0;
            this.CurrentIteration = 0;
            this.NumberOfEvaluationFitnessFunction = 0;
            this.testNumber = Utils.findTestNumber(Acronym);
            this.jellies = new Jellyfish[Population];

            jellies[0].x = new double[FitnessFunction.Dimensions];
            for (int i = 0; i < fitnessFunction.Dimensions; i++)
            {
                jellies[0].x[i] = rnd.NextDouble();
            }

            double a = 4;

            for (int i = 1; i < Population; i++)
            {
                jellies[i].x = new double[FitnessFunction.Dimensions];

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    jellies[i].x[j] = a * jellies[i - 1].x[j] * (1 - jellies[i - 1].x[j]);
                }
            }

            for (int k = 0; k < FitnessFunction.Dimensions; k++)
            {
                for (int i = 0; i < Population; i++)
                {
                    jellies[i].x[k] = FitnessFunction.MinCoordinates[k] + jellies[i].x[k] * (FitnessFunction.MaxCoordinates[k] - FitnessFunction.MinCoordinates[k]);
                    jellies[i].fitness = CalculateFitnessFunction(jellies[i].x);
                }
            }

            Array.Sort(jellies, (a, b) => a.fitness.CompareTo(b.fitness));
        }

        // Create new instance of object based on state file
        public JellyfishSearchOptimizer(int testNumber)
        {
            var file = File.OpenText(Utils.getStateFilePath(Acronym, testNumber));
            file.ReadLine();    // Metadata headers
            var metadata = file.ReadLine().Split(';');

            var functionName = metadata[0].Trim();
            var dimensions = int.Parse(metadata[1]);
            this.FitnessFunction = AlgoBenchmark.FitnessFunctionType.FromParameters(functionName, dimensions);
            this.Population = int.Parse(metadata[2]);
            this.TargetIterations = int.Parse(metadata[3]);
            this.CurrentIteration = int.Parse(metadata[4]);
            this.NumberOfEvaluationFitnessFunction = int.Parse(metadata[5]);
            this.Time = long.Parse(metadata[6]);
            this.jellies = new Jellyfish[Population];

            file.ReadLine();    // Empty line
            file.ReadLine();    // X headers

            for (int i = 0; i < Population; i++)
            {
                var line = file.ReadLine().Split(';');

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    jellies[i].x[j] = double.Parse(line[j]);
                }
            }
        }

        private void SaveLoadableState()
        {
            Directory.CreateDirectory("state");
            var file = File.CreateText(Utils.getStateFilePath(Acronym, testNumber));

            file.WriteLine("testNumber; fitnessFunction Name; fitnessFunction Dimensions; Population; TargetIterations; CurrentIteration; NumberOfEvaluationFitnessFunction; Time;");
            file.WriteLine($"{testNumber}; {FitnessFunction.Name}; {FitnessFunction.Dimensions}; {Population}; {TargetIterations}; {CurrentIteration}; {NumberOfEvaluationFitnessFunction}; {Time};");

            file.WriteLine();

            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                file.Write($"x{i}; ");
            }

            file.WriteLine();

            for (int i = 0; i < Population; i++)
            {
                foreach (var x in jellies[i].x)
                {
                    file.Write($"{x}; ");
                }
                file.WriteLine();
            }

            file.Close();
        }

        private void SaveIterationState()
        {
            string iterationStateDirectory = Utils.getTestDirectory(Acronym, testNumber);
            Directory.CreateDirectory(iterationStateDirectory);

            var file = File.CreateText($"{iterationStateDirectory}/iteration_{CurrentIteration}.txt");

            file.WriteLine($"{CurrentIteration} [numer iteracji]");
            file.WriteLine($"{NumberOfEvaluationFitnessFunction} [liczba wywołań funkcji celu]");

            for (int i = 0; i < Population; i++)
            {
                foreach (var x in jellies[i].x)
                {
                    file.Write($"{x} ");
                }
                file.WriteLine();
            }

            file.Close();
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

        public double Solve()
        {
            for (; CurrentIteration < TargetIterations; CurrentIteration++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                double[] Meanvl = mean();
                Jellyfish BestSol = jellies[0];
                for (int i = 0; i < Population; i++)
                {
                    // Calculate time control c(t) using Eq. (17);
                    double Ar = (1 - CurrentIteration * ((1) / TargetIterations)) * (2 * rnd.NextDouble() - 1);
                    if (Math.Abs(Ar) >= 0.5)
                    {
                        //// Folowing to ocean current using Eq. (11)
                        Jellyfish newsol = new()
                        {
                            x = new double[FitnessFunction.Dimensions]
                        };

                        for (int j = 0; j < FitnessFunction.Dimensions; j++)
                        {
                            newsol.x[j] = jellies[i].x[j] + rnd.NextDouble() * (BestSol.x[j] - 3.0 * rnd.NextDouble() * Meanvl[j]);
                        }

                        // Check the boundary using Eq. (19)
                        newsol = Simplebounds(newsol);
                        // Evaluation
                        newsol.fitness = CalculateFitnessFunction(newsol.x);
                        // Comparison
                        if (newsol.fitness < jellies[i].fitness)
                        {
                            jellies[i] = newsol;
                            if (jellies[i].fitness < BestSol.fitness)
                            {
                                BestSol = jellies[i];
                            }
                        }
                    }
                    else
                    {
                        Jellyfish newsol = new()
                        {
                            x = new double[FitnessFunction.Dimensions]
                        };

                        //// Moving inside swarm
                        if (rnd.NextDouble() <= (1d - Ar))
                        {
                            // Determine direction of jellyfish by Eq. (15)
                            int j = i;
                            while (j == i)
                            {
                                j = rnd.Next() % Population;
                            }


                            var Step = new double[FitnessFunction.Dimensions];
                            for (int k = 0; k < FitnessFunction.Dimensions; k++)
                            {
                                Step[k] = jellies[i].x[k] - jellies[j].x[k];
                            }

                            if (jellies[j].fitness < jellies[i].fitness)
                            {
                                for (int k = 0; k < FitnessFunction.Dimensions; k++)
                                {
                                    Step[k] *= -1;
                                }
                            }

                            // Active motions (Type B) using Eq. (16)
                            for (int k = 0; k < FitnessFunction.Dimensions; k++)
                            {
                                newsol.x[k] = jellies[i].x[k] + rnd.NextDouble() * Step[k];
                            }

                        }
                        else
                        {
                            // Passive motions (Type A) using Eq. (12)
                            for (int j = 0; j < FitnessFunction.Dimensions; j++)
                            {
                                newsol.x[j] = jellies[i].x[j] + 0.1 * (FitnessFunction.MaxCoordinates[j] - FitnessFunction.MinCoordinates[j]) * rnd.NextDouble();
                            }
                        }
                        // Check the boundary using Eq. (19)
                        newsol = Simplebounds(newsol);
                        // Evaluation
                        newsol.fitness = CalculateFitnessFunction(newsol.x);
                        // Comparison
                        if (newsol.fitness < jellies[i].fitness)
                        {
                            jellies[i] = newsol;
                            if (jellies[i].fitness < BestSol.fitness)
                            {
                                BestSol = jellies[i];
                            }
                        }
                    }
                }

                watch.Stop();
                this.Time += watch.ElapsedMilliseconds;
                Array.Sort(jellies, (a, b) => a.fitness.CompareTo(b.fitness));
                SaveToFileStateOfAlghoritm();
            }

            // Getting FBest in return statement requires calling FitnessFunction for each wolf
            NumberOfEvaluationFitnessFunction += Population;
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
            resultFile.WriteLine($"{Population} [populacja]");
            resultFile.WriteLine($"{TargetIterations} [liczba iteracji]");

            resultFile.Close();
        }

        private double[] mean()
        {
            var m = new double[FitnessFunction.Dimensions];

            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                for (int j = 0; j < Population; j++)
                {
                    m[i] += jellies[j].x[i];
                }

                m[i] /= Population;
            }

            return m;
        }

        private Jellyfish Simplebounds(Jellyfish s)
        {
            Jellyfish ns_tmp = s;

            // Apply to the lower bound
            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                while (ns_tmp.x[i] < FitnessFunction.MinCoordinates[i])
                {
                    ns_tmp.x[i] = FitnessFunction.MaxCoordinates[i] + (ns_tmp.x[i] - FitnessFunction.MinCoordinates[i]);
                }
            }

            // Apply to the upper bound
            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                while (ns_tmp.x[i] > FitnessFunction.MaxCoordinates[i])
                {
                    ns_tmp.x[i] = FitnessFunction.MinCoordinates[i] + (ns_tmp.x[i] - FitnessFunction.MaxCoordinates[i]);
                }
            }

            return ns_tmp;
        }

        private double CalculateFitnessFunction(double[] args)
        {
            NumberOfEvaluationFitnessFunction++;
            return FitnessFunction.Fn(args);
        }

        struct Jellyfish
        {
            public double[] x;
            public double fitness;
        }
    }
}