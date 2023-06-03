namespace AlgoBenchmark
{
    class GreyWolfOptimizer : IOptimizationAlgorithm
    {
        // Number of Wolves in population
        public int Population;
        private double[][] Wolves;
        private Random rnd = new Random();
        private int testNumber;
        private FitnessFunctionType FitnessFunction;
        private int TargetIterations;
        private int CurrentIteration;
        public int NumberOfEvaluationFitnessFunction { get; private set; }
        public long Time { get; private set; }

        public string Name
        {
            get => "Grey Wolf Optimizer";
        }

        public string Acronym
        {
            get => "GWO";
        }

        public double[] XBest
        {
            get
            {
                double bestResult = FitnessFunction.Fn(Wolves[0]);
                int bestIndex = 0;

                for (int i = 1; i < Population; i++)
                {
                    double result = FitnessFunction.Fn(Wolves[i]);

                    if (result < bestResult)
                    {
                        bestResult = result;
                        bestIndex = i;
                    }
                }

                return Wolves[bestIndex];
            }
        }

        public double FBest
        {
            get
            {
                double bestResult = FitnessFunction.Fn(Wolves[0]);

                for (int i = 1; i < Population; i++)
                {
                    double result = FitnessFunction.Fn(Wolves[i]);

                    if (result < bestResult)
                    {
                        bestResult = result;
                    }
                }

                return bestResult;
            }
        }

        // Create new instance of object from scratch
        public GreyWolfOptimizer(FitnessFunctionType fitnessFunction, int population, int targetIterations)
        {
            this.FitnessFunction = fitnessFunction;
            this.Population = population;
            this.TargetIterations = targetIterations;
            this.testNumber = Utils.findTestNumber(Acronym);
            this.Time = 0;
            this.CurrentIteration = 0;
            this.NumberOfEvaluationFitnessFunction = 0;
            this.Wolves = new double[Population][];

            for (int i = 0; i < Population; i++)
            {
                Wolves[i] = new double[fitnessFunction.Dimensions];

                for (int j = 0; j < fitnessFunction.Dimensions; j++)
                {
                    Wolves[i][j] = fitnessFunction.MinCoordinates[j] + rnd.NextDouble() * (fitnessFunction.MaxCoordinates[j] - fitnessFunction.MinCoordinates[j]);
                }
            }
        }

        // Create new instance of object based on state file
        public GreyWolfOptimizer(int testNumber)
        {
            var file = File.OpenText(Utils.getStateFilePath(Acronym, testNumber));
            file.ReadLine();    // Metadata headers
            var metadata = file.ReadLine().Split(';');

            var functionName = metadata[0];
            var dimensions = int.Parse(metadata[1]);
            this.FitnessFunction = AlgoBenchmark.FitnessFunctionType.FromParameters(functionName, dimensions);
            this.Population = int.Parse(metadata[2]);
            this.TargetIterations = int.Parse(metadata[3]);
            this.CurrentIteration = int.Parse(metadata[4]);
            this.NumberOfEvaluationFitnessFunction = int.Parse(metadata[5]);
            this.Time = long.Parse(metadata[6]);
            this.Wolves = new double[Population][];

            file.ReadLine();    // Empty line
            file.ReadLine();    // X headers

            for (int i = 0; i < Population; i++)
            {
                var line = file.ReadLine().Split(';');
                Wolves[i] = new double[FitnessFunction.Dimensions];

                for (int j = 0; j < FitnessFunction.Dimensions; j++)
                {
                    Wolves[i][j] = double.Parse(line[j]);
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

        public void SaveLoadableState()
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
                foreach (var x in Wolves[i])
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
                foreach (var x in Wolves[i])
                {
                    file.Write($"{x} ");
                }
                file.WriteLine();
            }

            file.Close();
        }

        public double Solve()
        {
            for (; CurrentIteration < TargetIterations; CurrentIteration++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                double a = 2.0 - CurrentIteration * (2.0 / TargetIterations);
                (var alphaPosition, var betaPosition, var deltaPosition) = GetAlphaBetaDelta();

                for (int wolfIndex = 0; wolfIndex < Population; wolfIndex++)
                {
                    for (int parameterIndex = 0; parameterIndex < FitnessFunction.Dimensions; parameterIndex++)
                    {
                        double X1 = GetXValue(a, alphaPosition[parameterIndex], Wolves[wolfIndex][parameterIndex]);
                        double X2 = GetXValue(a, betaPosition[parameterIndex], Wolves[wolfIndex][parameterIndex]);
                        double X3 = GetXValue(a, deltaPosition[parameterIndex], Wolves[wolfIndex][parameterIndex]);

                        double val = (X1 + X2 + X3) / 3;

                        if (val < FitnessFunction.MinCoordinates[parameterIndex])
                            val = FitnessFunction.MinCoordinates[parameterIndex];
                        else if (val > FitnessFunction.MaxCoordinates[parameterIndex])
                            val = FitnessFunction.MaxCoordinates[parameterIndex];

                        Wolves[wolfIndex][parameterIndex] = val;
                    }
                }

                watch.Stop();
                this.Time += watch.ElapsedMilliseconds;
                SaveToFileStateOfAlghoritm();
            }

            // Getting FBest in return statement requires calling FitnessFunction for each wolf
            NumberOfEvaluationFitnessFunction += Population;
            File.Delete(Utils.getStateFilePath(Acronym, testNumber));
            return FBest;
        }

        private (double[], double[], double[]) GetAlphaBetaDelta()
        {
            double firstResult = CalculateFitnessFunction(Wolves[0]);

            double[] alphaPosition = Wolves[0], betaPosition = Wolves[0], deltaPosition = Wolves[0];
            double alphaResult = firstResult, betaResult = firstResult, deltaResult = firstResult;

            for (int i = 1; i < Population; i++)
            {
                double result = CalculateFitnessFunction(Wolves[i]);

                if (result < alphaResult)
                {
                    (deltaResult, deltaPosition) = (betaResult, betaPosition);
                    (betaResult, betaPosition) = (alphaResult, alphaPosition);
                    (alphaResult, alphaPosition) = (result, Wolves[i]);
                }
                else if (result < betaResult)
                {
                    (deltaResult, deltaPosition) = (betaResult, betaPosition);
                    (betaResult, betaPosition) = (result, Wolves[i]);
                }
                else if (result < deltaResult)
                {
                    (deltaResult, deltaPosition) = (result, Wolves[i]);
                }
            }

            return (alphaPosition, betaPosition, deltaPosition);
        }

        private double GetXValue(double a, double posP, double pos)
        {
            Random rnd = new Random();
            double r1 = rnd.NextDouble();
            double r2 = rnd.NextDouble();

            double A1 = 2.0 * a * r1 - a; //Equation (3.3)
            double C1 = 2.0 * r2; //Equation (3.4)

            double D = Math.Abs(C1 * posP - pos); //Equation (3.5)-part 1
            return posP - A1 * D; //Equation (3.6)-part 1
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


        private double CalculateFitnessFunction(double[] args)
        {
            NumberOfEvaluationFitnessFunction++;
            return FitnessFunction.Fn(args);
        }
    }
}