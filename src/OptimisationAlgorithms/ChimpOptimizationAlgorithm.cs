// Based on https://www.mathworks.com/matlabcentral/fileexchange/76763-chimp-optimization-algorithm

namespace AlgoBenchmark
{
    class ChimpOptimizationAlgorithm : IOptimizationAlgorithm
    {
        public int Population;
        private Chimp[] Chimps;
        private Chimp attacker;
        private Chimp barrier;
        private Chimp chaser;
        private Chimp driver;
        private readonly Random rnd = new();
        private int testNumber;
        private FitnessFunctionType FitnessFunction;
        private int TargetIterations;
        private int CurrentIteration;
        public int NumberOfEvaluationFitnessFunction { get; private set; }
        public long Time { get; private set; }

        public string Name
        {
            get => "Chimp Optimization Algorithm";
        }

        public string Acronym
        {
            get => "ChOA";
        }

        public double[] XBest
        {
            get
            {
                double bestResult = Chimps[0].fitness;
                int bestIndex = 0;

                for (int i = 1; i < Population; i++)
                {
                    double result = Chimps[i].fitness;

                    if (result < bestResult)
                    {
                        bestResult = result;
                        bestIndex = i;
                    }
                }

                return Chimps[bestIndex].x;
            }
        }

        public double FBest
        {
            get
            {
                double bestResult = Chimps[0].fitness;

                for (int i = 1; i < Population; i++)
                {
                    double result = Chimps[0].fitness;

                    if (result < bestResult)
                    {
                        bestResult = result;
                    }
                }

                return bestResult;
            }
        }

        // Create new instance of object from scratch
        public ChimpOptimizationAlgorithm(FitnessFunctionType fitnessFunction, int population, int targetIterations, Dictionary<string, string> flags)
        {
            this.FitnessFunction = fitnessFunction;
            this.Population = population;
            this.TargetIterations = targetIterations;
            this.Time = 0;
            this.CurrentIteration = 0;
            this.NumberOfEvaluationFitnessFunction = 0;
            // this.a1 = double.Parse(flags.GetValueOrDefault("a1", "2"));
            // this.a2 = double.Parse(flags.GetValueOrDefault("a2", "1")); ;
            // this.GP = double.Parse(flags.GetValueOrDefault("GP", "0,5")); ;
            this.testNumber = Utils.findTestNumber(Acronym);
            this.Chimps = new Chimp[Population];

            for (int i = 0; i < Population; i++)
            {
                Chimps[i].x = new double[FitnessFunction.Dimensions];

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    Chimps[i].x[j] = FitnessFunction.MinCoordinates[j] + rnd.NextDouble() * (FitnessFunction.MaxCoordinates[j] - FitnessFunction.MinCoordinates[j]);
                }

                Chimps[i].fitness = CalculateFitnessFunction(Chimps[i].x);
            }

            Array.Sort(Chimps, (a, b) => a.fitness.CompareTo(b.fitness));

            this.attacker = Chimps[0];
            this.barrier = Chimps[1];
            this.chaser = Chimps[2];
            this.driver = Chimps[3];
        }

        // Create new instance of object based on state file
        public ChimpOptimizationAlgorithm(int testNumber)
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
            // this.a1 = double.Parse(metadata[8]);
            // this.a2 = double.Parse(metadata[9]);
            // this.GP = double.Parse(metadata[10]);
            this.Chimps = new Chimp[Population];

            file.ReadLine();    // Empty line
            file.ReadLine();    // X headers

            for (int i = 0; i < Population; i++)
            {
                var line = file.ReadLine().Split(';');
                Chimps[i].fitness = double.Parse(line[0]);
                Chimps[i].x = new double[FitnessFunction.Dimensions];

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    Chimps[i].x[j] = double.Parse(line[j + 1]);
                }
            }

            Array.Sort(Chimps, (a, b) => a.fitness.CompareTo(b.fitness));
        }

        private void SaveLoadableState()
        {
            // var file = File.CreateText(Utils.getStateFilePath(Acronym, testNumber));

            // file.WriteLine("testNumber; fitnessFunction Name; fitnessFunction Dimensions; Population; TargetIterations; CurrentIteration; NumberOfEvaluationFitnessFunction; Time; a1; a2; GP");
            // file.WriteLine($"{testNumber}; {FitnessFunction.Name}; {FitnessFunction.Dimensions}; {Population}; {TargetIterations}; {CurrentIteration}; {NumberOfEvaluationFitnessFunction}; {Time}; {a1}; {a2}; {GP}");

            // file.WriteLine();

            // file.Write("fitness; ");
            // for (int i = 0; i < FitnessFunction.Dimensions; i++)
            // {
            //     file.Write($"x{i}; ");
            // }

            // file.WriteLine();

            // for (int i = 0; i < Population; i++)
            // {
            //     file.Write($"{Chimps[i].fitness}; ");

            //     foreach (var x in Chimps[i].x)
            //     {
            //         file.Write($"{x}; ");
            //     }
            //     file.WriteLine();
            // }

            // file.Close();
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
                foreach (var x in Chimps[i].x)
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

                // At this point Chimps are sorted from one with lowest fitness (best) to one with highest fitness (worst)
                // Their dimensions are also provided to be between min and max coordinates

                for (int i = 0; i < Chimps.Length; i++)
                {
                    // Update Attacker, Barrier, Chaser, and Driver
                    if (Chimps[i].fitness < attacker.fitness)
                    {
                        attacker = Chimps[i]; // Update Attacker
                    }

                    if (Chimps[i].fitness > attacker.fitness && Chimps[i].fitness < barrier.fitness)
                    {
                        barrier = Chimps[i]; // Update Barrier
                    }

                    if (Chimps[i].fitness > attacker.fitness && Chimps[i].fitness > barrier.fitness && Chimps[i].fitness < chaser.fitness)
                    {
                        chaser = Chimps[i]; // Update Chaser
                    }

                    if (Chimps[i].fitness > attacker.fitness && Chimps[i].fitness > barrier.fitness && Chimps[i].fitness > chaser.fitness && Chimps[i].fitness > driver.fitness)
                    {
                        driver = Chimps[i]; // Update Driver
                    }
                }

                double f = 2d - CurrentIteration * (2d / TargetIterations); // a decreases linearly fron 2 to 0

                //  The Dynamic Coefficient of f Vector as Table 1.

                //Group 1
                double C1G1 = 1.95d - (2d * Math.Pow(CurrentIteration, 1d / 3d) / Math.Pow(TargetIterations, 1d / 3d));
                double C2G1 = 2d * (Math.Pow(CurrentIteration, 1d / 3d) / Math.Pow(TargetIterations, 1d / 3d)) + 0.5d;

                //Group 2
                double C1G2 = 1.95d - (2d * Math.Pow(CurrentIteration, 1d / 3d) / Math.Pow(TargetIterations, 1d / 3d));
                double C2G2 = (2d * Math.Pow(CurrentIteration, 3d) / Math.Pow(TargetIterations, 3d)) + 0.5d;

                //Group 3
                double C1G3 = (-2d * Math.Pow(CurrentIteration, 3d) / Math.Pow(TargetIterations, 3d)) + 2.5d;
                double C2G3 = Math.Pow(2d * CurrentIteration, 1d / 3d) / Math.Pow(TargetIterations, 1d / 3d) + 0.5d;

                //Group 4
                double C1G4 = (-2d * Math.Pow(CurrentIteration, 3d) / Math.Pow(TargetIterations, 3d)) + 2.5d;
                double C2G4 = (2d * Math.Pow(CurrentIteration, 3d) / Math.Pow(TargetIterations, 3d)) + 0.5d;

                // Update the Position of search agents including omegas
                for (int i = 0; i < Population; i++)
                {
                    for (int j = 0; j < FitnessFunction.Dimensions; j++)
                    {
                        //// Please note that to choose a other groups you should use the related group strategies
                        double r11 = C1G1 * rnd.NextDouble(); // r1 is a random number in [0,1]
                        double r12 = C2G1 * rnd.NextDouble(); // r2 is a random number in [0,1]

                        double r21 = C1G2 * rnd.NextDouble(); // r1 is a random number in [0,1]
                        double r22 = C2G2 * rnd.NextDouble(); // r2 is a random number in [0,1]

                        double r31 = C1G3 * rnd.NextDouble(); // r1 is a random number in [0,1]
                        double r32 = C2G3 * rnd.NextDouble(); // r2 is a random number in [0,1]

                        double r41 = C1G4 * rnd.NextDouble(); // r1 is a random number in [0,1]
                        double r42 = C2G4 * rnd.NextDouble(); // r2 is a random number in [0,1]

                        double A1 = 2 * f * r11 - f; // Equation (3)
                        double C1 = 2 * r12; // Equation (4)

                        // Please note that to choose various Chaotic maps you should use the related Chaotic maps strategies
                        // double m=chaos(3,1,1); // Equation (5)
                        double m = 0.7;
                        double D_Attacker = Math.Abs(C1 * attacker.x[j] - m * this.Chimps[i].x[j]); // Equation (6)
                        double X1 = attacker.x[j] - A1 * D_Attacker; // Equation (7)

                        double A2 = 2 * f * r21 - f; // Equation (3)
                        double C2 = 2 * r22; // Equation (4)

                        double D_Barrier = Math.Abs(C2 * barrier.x[j] - m * this.Chimps[i].x[j]); // Equation (6)
                        double X2 = barrier.x[j] - A2 * D_Barrier; // Equation (7)     


                        double A3 = 2 * f * r31 - f; // Equation (3)
                        double C3 = 2 * r32; // Equation (4)

                        double D_Chaser = Math.Abs(C3 * chaser.x[j] - m * this.Chimps[i].x[j]); // Equation (6)
                        double X3 = chaser.x[j] - A3 * D_Chaser; // Equation (7)      

                        double A4 = 2 * f * r41 - f; // Equation (3)
                        double C4 = 2 * r42; // Equation (4)

                        double D_Driver = Math.Abs(C4 * driver.x[j] - m * this.Chimps[i].x[j]); // Equation (6)
                        double X4 = driver.x[j] - A4 * D_Driver; // Equation (7)       

                        this.Chimps[i].x[j] = (X1 + X2 + X3 + X4) / 4;// Equation (8)

                        if (this.Chimps[i].x[j] < FitnessFunction.MinCoordinates[j])
                            this.Chimps[i].x[j] = FitnessFunction.MinCoordinates[j];
                        else if (this.Chimps[i].x[j] > FitnessFunction.MaxCoordinates[j])
                            this.Chimps[i].x[j] = FitnessFunction.MaxCoordinates[j];
                    }

                    this.Chimps[i].fitness = CalculateFitnessFunction(this.Chimps[i].x);
                }

                watch.Stop();
                this.Time += watch.ElapsedMilliseconds;
                Array.Sort(Chimps, (a, b) => a.fitness.CompareTo(b.fitness));
                SaveToFileStateOfAlghoritm();
            }

            // Getting FBest in return statement requires calling FitnessFunction for each wolf
            NumberOfEvaluationFitnessFunction += Population;
            File.Delete(Utils.getStateFilePath(Acronym, testNumber));
            return FBest;
        }

        double[] Chaos(int index, int max_iter, double Value)
        {
            double[] G = FitnessFunctionType.FilledArray(max_iter, 0);
            double[] x = FitnessFunctionType.FilledArray(max_iter, 0);
            x[0] = 0.7;

            switch (index)
            {
                //Chebyshev map
                case 1:
                    for (int i = 0; i < max_iter; i++)
                    {
                        x[i + 1] = Math.Cos(i * Math.Acos(x[i]));
                        G[i] = ((x[i] + 1) * Value) / 2;
                    }
                    break;

                case 2:
                    {
                        //Circle map
                        double a = 0.5;
                        double b = 0.2;
                        for (int i = 0; i < max_iter; i++)
                        {
                            x[i + 1] = (x[i] + b - (a / (2 * Math.PI)) * Math.Sin(2 * Math.PI * x[i])) % 1;
                            G[i] = x[i] * Value;
                        }
                    }
                    break;

                case 3:
                    //Gauss/mouse map
                    for (int i = 0; i < max_iter; i++)
                    {
                        if (x[i] == 0)
                            x[i + 1] = 0;
                        else
                            x[i + 1] = (1 / x[i]) % 1;

                        G[i] = x[i] * Value;
                    }
                    break;

                case 4:
                    {
                        //Iterative map
                        double a = 0.7;
                        for (int i = 0; i < max_iter; i++)
                        {
                            x[i + 1] = Math.Sin((a * Math.PI) / x[i]);
                            G[i] = ((x[i] + 1) * Value) / 2;
                        }
                    }
                    break;

                case 5:
                    //Logistic map
                    {
                        double a = 4;
                        for (int i = 0; i < max_iter; i++)
                        {
                            x[i + 1] = a * x[i] * (1 - x[i]);
                            G[i] = x[i] * Value;
                        }
                    }
                    break;

                case 6:
                    //Piecewise map
                    {
                        double P = 0.4;
                        for (int i = 0; i < max_iter; i++)
                        {
                            if (x[i] >= 0 && x[i] < P)
                                x[i + 1] = x[i] / P;

                            if (x[i] >= P && x[i] < 0.5)
                                x[i + 1] = (x[i] - P) / (0.5 - P);

                            if (x[i] >= 0.5 && x[i] < 1 - P)
                                x[i + 1] = (1 - P - x[i]) / (0.5 - P);

                            if (x[i] >= 1 - P && x[i] < 1)
                                x[i + 1] = (1 - x[i]) / P;

                            G[i] = x[i] * Value;
                        }
                    }
                    break;

                case 7:
                    //Sine map
                    for (int i = 0; i < max_iter; i++)
                    {
                        x[i + 1] = Math.Sin(Math.PI * x[i]);
                        G[i] = (x[i]) * Value;
                    }
                    break;

                case 8:
                    //Singer map 
                    {
                        double u = 1.07;
                        for (int i = 0; i < max_iter; i++)
                        {
                            x[i + 1] = u * (7.86 * x[i] - 23.31 * Math.Pow(x[i], 2) + 28.75 * Math.Pow(x[i], 3) - 13.302875 * Math.Pow(x[i], 4));
                            G[i] = (x[i]) * Value;
                        }
                    }
                    break;

                case 9:
                    //Sinusoidal map
                    for (int i = 0; i < max_iter; i++)
                    {
                        x[i + 1] = 2.3 * Math.Pow(x[i], 2) * Math.Sin(Math.PI * x[i]);
                        G[i] = (x[i]) * Value;
                    }
                    break;

                case 10:
                    //Tent map
                    x[0] = 0.6;
                    for (int i = 0; i < max_iter; i++)
                    {
                        if (x[i] < 0.7)
                            x[i + 1] = x[i] / 0.7;

                        if (x[i] >= 0.7)
                            x[i + 1] = (10 / 3) * (1 - x[i]);

                        G[i] = (x[i]) * Value;
                    }
                    break;
            }
            return G;
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
            // resultFile.WriteLine($"{a1} [a1]");
            // resultFile.WriteLine($"{a2} [a2]");
            // resultFile.WriteLine($"{GP} [GP]");

            resultFile.Close();
        }

        private double CalculateFitnessFunction(double[] args)
        {
            NumberOfEvaluationFitnessFunction++;
            return FitnessFunction.Fn(args);
        }
    }

    struct Chimp
    {
        public double[] x;
        public double fitness;
    }
}