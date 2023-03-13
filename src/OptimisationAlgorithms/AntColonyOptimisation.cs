using MathNet.Numerics.Distributions;

namespace AlgoBenchmark
{
    class AntColonyOptimizationAlgorithm : OptimizationAlgorithm
    {
        // Number of ants in population
        int M = 100;

        // Number of pheromone spots
        int L = 10;

        // ξ
        double ksi = 1;

        double q = 0.9;

        public string Name
        {
            get => "Ant Colony Optimization Algorithm";
        }

        public double Solve(MinimizedFunction minimizedFunction, int iterations)
        {
            // Pheromone spots
            var T = new PheromoneSpot[L + M];
            Random rnd = new Random();

            for (int i = 0; i < L + M; i++)
            {
                T[i].x = new double[minimizedFunction.UnknownParametersNumber];

                for (int j = 0; j < minimizedFunction.UnknownParametersNumber; j++)
                {
                    T[i].x[j] = minimizedFunction.MinCoordinateVal + rnd.NextDouble() * (minimizedFunction.MaxCoordinateVal - minimizedFunction.MinCoordinateVal);
                }
            }


            for (int a = 0; a < iterations; a++)
            {
                for (int i = 0; i < L + M; i++)
                {
                    T[i].result = minimizedFunction.TargetFunction(T[i].x);
                }

                Array.Sort(T, (a, b) => a.result.CompareTo(b.result));

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

                    for (int j = 0; j < minimizedFunction.UnknownParametersNumber; j++)
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

                        if (num < minimizedFunction.MinCoordinateVal)
                            num = minimizedFunction.MinCoordinateVal;
                        else if (num > minimizedFunction.MaxCoordinateVal)
                            num = minimizedFunction.MaxCoordinateVal;

                        T[L + i].x[j] = num;
                    }


                }
            }

            Array.Sort(T, (a, b) => a.result.CompareTo(b.result));

            return T[0].result;

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