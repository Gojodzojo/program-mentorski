namespace AlgoBenchmark
{
    public delegate double FnType(double[] args);

    public class FitnessFunctionType
    {
        public FnType Fn = default!;
        public string Name = default!;
        public double[] MinCoordinates = { };
        public double[] MaxCoordinates = { };
        public int Dimensions;

        public static FitnessFunctionType FromParameters(string functionName, int dimensions)
        {
            foreach (var fun in GetFitnessFunctions(dimensions))
            {
                if (functionName == fun.Name)
                {
                    return fun;
                }
            }

            throw new Exception("FitnessFunction not found");
        }

        public static FitnessFunctionType[] GetFitnessFunctions(int dimensions)
        {
            return new FitnessFunctionType[]
            {
                new FitnessFunctionType
                {
                    Name = "Quadratic Function",
                    MinCoordinates = FilledArray(dimensions, 100),
                    MaxCoordinates = FilledArray(dimensions, -100),
                    Dimensions = dimensions,
                    Fn = (double[] x) => x[0] * x[0],
                },
                new FitnessFunctionType
                {
                    Name = "Bent Cigar",
                    MinCoordinates = FilledArray(dimensions, 10),
                    MaxCoordinates = FilledArray(dimensions, -10),
                    Dimensions = dimensions,
                    Fn = (double[] x) => {
                        int n = x.Length;
                        double sum = 0;
                        for (int i = 1; i < n; i++)
                            sum += x[i] * x[i];
                        return x[0] * x[0] + Math.Pow(10, 6) * sum;
                    }
                },
                new FitnessFunctionType
                {
                    Name = "Rosenbrock Function",
                    MinCoordinates = FilledArray(dimensions, 10),
                    MaxCoordinates = FilledArray(dimensions, -5),
                    Dimensions = dimensions,
                    Fn = (double[] x) => {
                        int n = x.Length;
                        double suma = 0;
                        for (int i = 0; i < n - 1; i++)
                        suma += 100 * Math.Pow(x[i + 1] - x[i] * x[i], 2) + Math.Pow(1 - x[i], 2);
                        return suma;
                    }
                },
                new FitnessFunctionType
                {
                    Name = "Rastrigin Function",
                    MinCoordinates = FilledArray(dimensions, 5.12),
                    MaxCoordinates = FilledArray(dimensions, -5.12),
                    Dimensions = dimensions,
                    Fn = (double[] x) => {
                        int n = x.Length;
                        double suma = 0;
                        for (int i = 0; i < n; i++)
                            suma += x[i] * x[i] - 10 * Math.Cos(2 * Math.PI * x[i]);
                        return 10 * n + suma;
                    }
                },
                new FitnessFunctionType
                {
                    Name = "Sphere Function",
                    MinCoordinates = FilledArray(dimensions, 10000),
                    MaxCoordinates = FilledArray(dimensions, 10000),
                    Dimensions = dimensions,
                    Fn = (double[] x) => {
                        double sum = 0;
                        for (int i = 0; i < x.Length; i++)
                        {
                            sum += x[i] * x[i];
                        }

                        return sum;
                    }
                },
                new FitnessFunctionType
                {
                    Name = "Unknown Function",
                    MinCoordinates = FilledArray(dimensions, 10),
                    MaxCoordinates = FilledArray(dimensions, -10),
                    Dimensions = dimensions,
                    Fn = (double[] x) => {
                        int n = x.Length;
                        double suma = 0;
                        for (int i = 0; i < n; i++)
                        suma += Math.Abs(x[i] * Math.Sin(x[i]) + 0.1 * x[i]);
                        return suma;
                    }
                },
                new FitnessFunctionType
                {
                    Name = "Eggholder function",
                    MinCoordinates = new double[] {-512, -512},
                    MaxCoordinates = new double[] {512, 512},
                    Dimensions = 2,
                    Fn = (double[] x) => {
                        return -(x[1] + 47) * Math.Sin(Math.Sqrt(Math.Abs((x[0]/2) + (x[1] + 47)))) -x[0]* Math.Sin(Math.Sqrt(Math.Abs(x[0] + (x[1] + 47))));
                    }
                }
            };
        }

        static double[] FilledArray(int size, double value)
        {
            double[] a = new double[size];
            Array.Fill(a, value);
            return a;
        }
    }
}



