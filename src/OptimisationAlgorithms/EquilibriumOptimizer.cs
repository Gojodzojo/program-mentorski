namespace AlgoBenchmark
{
    class EquilibriumOptimizer : IOptimizationAlgorithm
    {
        private double a1;
        private double a2;
        private double GP;
        public int Population;
        private Particle[] Particles;
        private double[][] C_pool;
        private Random rnd = new Random();
        private int testNumber;
        private FitnessFunctionType FitnessFunction;
        private int TargetIterations;
        private int CurrentIteration;
        public int NumberOfEvaluationFitnessFunction { get; private set; }
        public long Time { get; private set; }

        public string Name
        {
            get => "Equilibrium Optimizer";
        }

        public string Acronym
        {
            get => "EO";
        }

        public double[] XBest
        {
            get
            {
                double bestResult = Particles[0].fitness;
                int bestIndex = 0;

                for (int i = 1; i < Population; i++)
                {
                    double result = Particles[i].fitness;

                    if (result < bestResult)
                    {
                        bestResult = result;
                        bestIndex = i;
                    }
                }

                return Particles[bestIndex].C;
            }
        }

        public double FBest
        {
            get
            {
                double bestResult = Particles[0].fitness;

                for (int i = 1; i < Population; i++)
                {
                    double result = Particles[0].fitness;

                    if (result < bestResult)
                    {
                        bestResult = result;
                    }
                }

                return bestResult;
            }
        }

        // Create new instance of object from scratch
        public EquilibriumOptimizer(FitnessFunctionType fitnessFunction, int population, int targetIterations, Dictionary<string, string> flags)
        {
            this.FitnessFunction = fitnessFunction;
            this.Population = population;
            this.TargetIterations = targetIterations;
            this.Time = 0;
            this.CurrentIteration = 0;
            this.NumberOfEvaluationFitnessFunction = 0;
            this.a1 = double.Parse(flags.GetValueOrDefault("a1", "2"));
            this.a2 = double.Parse(flags.GetValueOrDefault("a2", "1")); ;
            this.GP = double.Parse(flags.GetValueOrDefault("GP", "0,5")); ;
            this.testNumber = Utils.findTestNumber(Acronym);
            this.Particles = new Particle[Population];

            for (int i = 0; i < Population; i++)
            {
                Particles[i].C = new double[FitnessFunction.Dimensions];

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    Particles[i].C[j] = FitnessFunction.MinCoordinates[j] + rnd.NextDouble() * (FitnessFunction.MaxCoordinates[j] - FitnessFunction.MinCoordinates[j]);
                }

                Particles[i].fitness = CalculateFitnessFunction(Particles[i].C);
            }

            Array.Sort(Particles, (a, b) => a.fitness.CompareTo(b.fitness));

            double[] Ceq1 = Particles[0].C;
            double[] Ceq2 = Particles[1].C;
            double[] Ceq3 = Particles[2].C;
            double[] Ceq4 = Particles[3].C;

            double[] Ceq_ave = new double[FitnessFunction.Dimensions];

            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                Ceq_ave[i] = (Ceq1[i] + Ceq2[i] + Ceq3[i] + Ceq4[i]) / 4;
            }

            C_pool = new double[][] { Ceq1, Ceq2, Ceq3, Ceq4, Ceq_ave };
        }

        // Create new instance of object based on state file
        public EquilibriumOptimizer(int testNumber)
        {
            var file = File.OpenText(Utils.getStateFilePath(Acronym, testNumber));
            file.ReadLine();    // Metadata headers
            var metadata = file.ReadLine().Split(';');

            var functionName = metadata[1].Trim();
            var dimensions = int.Parse(metadata[2]);
            this.FitnessFunction = AlgoBenchmark.FitnessFunctionType.FromParameters(functionName, dimensions);
            this.testNumber = int.Parse(metadata[0]);
            this.Population = int.Parse(metadata[3]);
            this.TargetIterations = int.Parse(metadata[4]);
            this.CurrentIteration = int.Parse(metadata[5]);
            this.NumberOfEvaluationFitnessFunction = int.Parse(metadata[6]);
            this.Time = long.Parse(metadata[7]);
            this.a1 = double.Parse(metadata[8]);
            this.a2 = double.Parse(metadata[9]);
            this.GP = double.Parse(metadata[10]);
            this.Particles = new Particle[Population];

            file.ReadLine();    // Empty line
            file.ReadLine();    // X headers

            for (int i = 0; i < Population; i++)
            {
                var line = file.ReadLine().Split(';');
                Particles[i].fitness = double.Parse(line[0]);
                Particles[i].C = new double[FitnessFunction.Dimensions];

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    Particles[i].C[j] = double.Parse(line[j + 1]);
                }
            }

            Array.Sort(Particles, (a, b) => a.fitness.CompareTo(b.fitness));

            double[] Ceq1 = Particles[0].C;
            double[] Ceq2 = Particles[1].C;
            double[] Ceq3 = Particles[2].C;
            double[] Ceq4 = Particles[3].C;

            double[] Ceq_ave = new double[FitnessFunction.Dimensions];

            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                Ceq_ave[i] = (Ceq1[i] + Ceq2[i] + Ceq3[i] + Ceq4[i]) / 4;
            }

            C_pool = new double[][] { Ceq1, Ceq2, Ceq3, Ceq4, Ceq_ave };
        }

        void LoadFromFileStateOfAlghoritm() { }

        private void SaveLoadableState()
        {
            var file = File.CreateText(Utils.getStateFilePath(Acronym, testNumber));

            file.WriteLine("testNumber; fitnessFunction Name; fitnessFunction Dimensions; Population; TargetIterations; CurrentIteration; NumberOfEvaluationFitnessFunction; Time; a1; a2; GP");
            file.WriteLine($"{testNumber}; {FitnessFunction.Name}; {FitnessFunction.Dimensions}; {Population}; {TargetIterations}; {CurrentIteration}; {NumberOfEvaluationFitnessFunction}; {Time}; {a1}; {a2}; {GP}");

            file.WriteLine();

            file.Write("fitness; ");
            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                file.Write($"x{i}; ");
            }

            file.WriteLine();

            for (int i = 0; i < Population; i++)
            {
                file.Write($"{Particles[i].fitness}; ");

                foreach (var x in Particles[i].C)
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
                foreach (var x in Particles[i].C)
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

                double progress = CurrentIteration / TargetIterations;
                double t = Math.Pow(1 - progress, a2 * progress);           // Eq (9)

                var newParticles = new double[Population][];

                for (int i = 0; i < Population; i++)
                {
                    newParticles[i] = new double[FitnessFunction.Dimensions];


                    var Ceq = C_pool[rnd.Next() % 5];                   // random selection of one candidate from the pool

                    double r1 = rnd.NextDouble(), r2 = rnd.NextDouble(); // r1 and r2 in Eq(15)

                    for (int j = 0; j < FitnessFunction.Dimensions; j++)
                    {
                        double r = rnd.NextDouble();                                         // r in Eq(11)
                        double lambda = rnd.NextDouble();                                    // lambda in Eq(11)
                        double F = a1 * Math.Sign(r - 0.5) * (Math.Exp(-lambda * t) - 1); // Eq(11)

                        double GCP = 0;
                        if (r2 >= GP)
                        {
                            GCP = 0.5 * r1;
                        }

                        double G0 = GCP * (Ceq[j] - lambda * Particles[i].C[j]);
                        double G = G0 * F;
                        int V = 1;
                        newParticles[i][j] = Ceq[j] + (Particles[i].C[j] - Ceq[j]) * F + (G / lambda * V) * (1 - F);      // Eq(16)

                        if (newParticles[i][j] < FitnessFunction.MinCoordinates[j])
                            newParticles[i][j] = FitnessFunction.MinCoordinates[j];
                        else if (newParticles[i][j] > FitnessFunction.MaxCoordinates[j])
                            newParticles[i][j] = FitnessFunction.MaxCoordinates[j];
                    }
                }

                C_pool = GetEqilibrumPool(newParticles);
                for (int i = 0; i < Population; i++)
                {
                    double fit = CalculateFitnessFunction(newParticles[i]);
                    if (fit < Particles[i].fitness)
                    {
                        Particles[i].fitness = fit;
                        Particles[i].C = newParticles[i];
                    }
                }

                watch.Stop();
                this.Time += watch.ElapsedMilliseconds;
                Array.Sort(Particles, (a, b) => a.fitness.CompareTo(b.fitness));
                SaveToFileStateOfAlghoritm();
            }

            // Getting FBest in return statement requires calling FitnessFunction for each wolf
            NumberOfEvaluationFitnessFunction += Population;
            File.Delete(Utils.getStateFilePath(Acronym, testNumber));
            return FBest;
        }

        private double[][] GetEqilibrumPool(double[][] newParticles)
        {
            double[] Ceq1 = newParticles[0];
            double[] Ceq2 = newParticles[0];
            double[] Ceq3 = newParticles[0];
            double[] Ceq4 = newParticles[0];
            double Ceq1_fit = double.MaxValue;
            double Ceq2_fit = double.MaxValue;
            double Ceq3_fit = double.MaxValue;
            double Ceq4_fit = double.MaxValue;

            for (int i = 0; i < Population; i++)
            {
                double fit = CalculateFitnessFunction(newParticles[i]);

                if (fit < Ceq1_fit)
                {
                    Ceq1_fit = fit;
                    Ceq1 = newParticles[i];
                }
                else if (fit > Ceq1_fit && fit < Ceq2_fit)
                {
                    Ceq2_fit = fit;
                    Ceq2 = newParticles[i];
                }
                else if (fit > Ceq1_fit && fit > Ceq2_fit && fit < Ceq3_fit)
                {
                    Ceq3_fit = fit;
                    Ceq3 = newParticles[i];
                }
                else if (fit > Ceq1_fit && fit > Ceq2_fit && fit > Ceq3_fit && fit < Ceq4_fit)
                {
                    Ceq4_fit = fit;
                    Ceq4 = newParticles[i];
                }
            }

            double[] Ceq_ave = new double[FitnessFunction.Dimensions];

            for (int i = 0; i < FitnessFunction.Dimensions; i++)
            {
                Ceq_ave[i] = (Ceq1[i] + Ceq2[i] + Ceq3[i] + Ceq4[i]) / 4;
            }

            return new double[][] { Ceq1, Ceq2, Ceq3, Ceq4, Ceq_ave };
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
            resultFile.WriteLine($"{a1} [a1]");
            resultFile.WriteLine($"{a2} [a2]");
            resultFile.WriteLine($"{GP} [GP]");

            resultFile.Close();
        }

        private double CalculateFitnessFunction(double[] args)
        {
            NumberOfEvaluationFitnessFunction++;
            return FitnessFunction.Fn(args);
        }
    }

    struct Particle
    {
        public double[] C;
        public double fitness;
    }
}