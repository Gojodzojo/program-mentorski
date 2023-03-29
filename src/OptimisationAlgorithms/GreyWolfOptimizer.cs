using MathNet.Numerics.Distributions;

namespace AlgoBenchmark
{
    class GreyWolfOptimizer : OptimizationAlgorithm
    {
        // Number of wolves in population
        public int population = 20;

        public string Name
        {
            get => "Grey Wolf Optimizer";
        }

        public double Solve(MinimizedFunction minimizedFunction, int iterations)
        {
            int targetFunctionCalls = 0;
            TargetFunctionType targetFunction = (args) =>
            {
                targetFunctionCalls++;
                return minimizedFunction.TargetFunction(args);
            };

            var wolves = GenerateWolves(minimizedFunction);

            for (int i = 0; i < iterations; i++)
            {
                double a = 2.0 - i * (2.0 / iterations);
                (var alphaPosition, var betaPosition, var deltaPosition) = GetAlphaBetaDelta(wolves, minimizedFunction, targetFunction);

                for (int wolfIndex = 0; wolfIndex < population; wolfIndex++)
                {
                    for (int parameterIndex = 0; parameterIndex < minimizedFunction.UnknownParametersNumber; parameterIndex++)
                    {
                        double X1 = GetXValue(a, alphaPosition[parameterIndex], wolves[wolfIndex][parameterIndex]);
                        double X2 = GetXValue(a, betaPosition[parameterIndex], wolves[wolfIndex][parameterIndex]);
                        double X3 = GetXValue(a, deltaPosition[parameterIndex], wolves[wolfIndex][parameterIndex]);

                        wolves[wolfIndex][parameterIndex] = (X1 + X2 + X3) / 3;
                    }
                }
            }

            double bestResult = targetFunction(wolves[0]);

            for (int i = 1; i < population; i++)
            {
                double result = targetFunction(wolves[i]);

                if (result < bestResult)
                {
                    bestResult = result;
                }
            }

            return bestResult;
        }

        public double[][] GenerateWolves(MinimizedFunction minimizedFunction)
        {
            // Wolves and their positions
            var wolves = new double[population][];
            Random rnd = new Random();

            for (int i = 0; i < population; i++)
            {
                wolves[i] = new double[minimizedFunction.UnknownParametersNumber];

                for (int j = 0; j < minimizedFunction.UnknownParametersNumber; j++)
                {
                    wolves[i][j] = minimizedFunction.MinCoordinateVal + rnd.NextDouble() * (minimizedFunction.MaxCoordinateVal - minimizedFunction.MinCoordinateVal);
                }
            }

            return wolves;
        }

        public (double[], double[], double[]) GetAlphaBetaDelta(double[][] wolves, MinimizedFunction minimizedFunction, TargetFunctionType targetFunction)
        {
            double firstResult = targetFunction(wolves[0]);

            double[] alphaPosition = wolves[0], betaPosition = wolves[0], deltaPosition = wolves[0];
            double alphaResult = firstResult, betaResult = firstResult, deltaResult = firstResult;

            for (int i = 1; i < population; i++)
            {
                double result = targetFunction(wolves[i]);

                if (result < alphaResult)
                {
                    (deltaResult, deltaPosition) = (betaResult, betaPosition);
                    (betaResult, betaPosition) = (alphaResult, alphaPosition);
                    (alphaResult, alphaPosition) = (result, wolves[i]);
                }
                else if (result < betaResult)
                {
                    (deltaResult, deltaPosition) = (betaResult, betaPosition);
                    (betaResult, betaPosition) = (result, wolves[i]);
                }
                else if (result < deltaResult)
                {
                    (deltaResult, deltaPosition) = (result, wolves[i]);
                }
            }

            return (alphaPosition, betaPosition, deltaPosition);
        }

        public double GetXValue(double a, double posP, double pos)
        {
            Random rnd = new Random();
            double r1 = rnd.NextDouble();
            double r2 = rnd.NextDouble();

            double A1 = 2.0 * a * r1 - a; //Equation (3.3)
            double C1 = 2.0 * r2; //Equation (3.4)

            double D = Math.Abs(C1 * posP - pos); //Equation (3.5)-part 1
            return posP - A1 * D; //Equation (3.6)-part 1
        }
    }
}