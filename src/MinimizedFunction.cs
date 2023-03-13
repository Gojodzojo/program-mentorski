namespace AlgoBenchmark
{
    public delegate double TargetFunctionType(double[] args);

    public class MinimizedFunction
    {
        public TargetFunctionType TargetFunction = default!;
        public string Name = default!;
        public double MinCoordinateVal;
        public double MaxCoordinateVal;
        public int UnknownParametersNumber;

        public static MinimizedFunction[] GetTestMinimizedFunctions(int unknownParametersNumber)
        {
            return new[]
            {
                new MinimizedFunction
                {
                    Name = "Quadratic Function",
                    MaxCoordinateVal = 100,
                    MinCoordinateVal = -100,
                    UnknownParametersNumber = unknownParametersNumber,
                    TargetFunction = (double[] x) => x[0] * x[0],
                },
                new MinimizedFunction
                {
                    Name = "Bent Cigar",
                    MaxCoordinateVal = 10,
                    MinCoordinateVal = -10,
                    UnknownParametersNumber = unknownParametersNumber,
                    TargetFunction = (double[] x) => {
                        int n = x.Length;
                        double sum = 0;
                        for (int i = 1; i < n; i++)
                            sum += x[i] * x[i];
                        return x[0] * x[0] + Math.Pow(10, 6) * sum;
                    }
                },
                new MinimizedFunction
                {
                    Name = "Rosenbrock Function",
                    MaxCoordinateVal = 10,
                    MinCoordinateVal = -5,
                    UnknownParametersNumber = unknownParametersNumber,
                    TargetFunction = (double[] x) => {
                        int n = x.Length;
                        double suma = 0;
                        for (int i = 0; i < n - 1; i++)
                        suma += 100 * Math.Pow(x[i + 1] - x[i] * x[i], 2) + Math.Pow(1 - x[i], 2);
                        return suma;
                    }
                },
                new MinimizedFunction
                {
                    Name = "Rastrigin Function",
                    MaxCoordinateVal = 5.12,
                    MinCoordinateVal = -5.12,
                    UnknownParametersNumber = unknownParametersNumber,
                    TargetFunction = (double[] x) => {
                        int n = x.Length;
                        double suma = 0;
                        for (int i = 0; i < n; i++)
                            suma += x[i] * x[i] - 10 * Math.Cos(2 * Math.PI * x[i]);
                        return 10 * n + suma;
                    }
                },
                new MinimizedFunction
                {
                    Name = "Unknown Function",
                    MaxCoordinateVal = 10,
                    MinCoordinateVal = -10,
                    UnknownParametersNumber = unknownParametersNumber,
                    TargetFunction = (double[] x) => {
                        int n = x.Length;
                        double suma = 0;
                        for (int i = 0; i < n; i++)
                        suma += Math.Abs(x[i] * Math.Sin(x[i]) + 0.1 * x[i]);
                        return suma;
                    }
                },
            };
        }
    }
}



