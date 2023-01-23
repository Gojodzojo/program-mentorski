using MathNet.Numerics.Distributions;

var opt = new Optimizer();
var results = opt.run();

for (int i = 0; i < results.Length; i++)
{
    Console.WriteLine(results[i]);
}

class Optimizer
{
    // Minimized function
    Func<double[], double> f = x => (x[0] + 1) * (x[0] + 1);

    // Number of unknown parameters
    int n = 1;

    // Number of ants in population
    int M = 100;

    // Number of iterations
    int I = 100;

    // Number of pheromone spots
    int L = 10;

    // ξ
    double ksi = 1;

    double q = 0.9;

    public double[] run()
    {
        // Pheromone spots
        var T = new PheromoneSpot[L + M];
        Random rnd = new Random();

        for (int i = 0; i < L + M; i++)
        {
            T[i].x = new double[n];

            for (int j = 0; j < n; j++)
            {
                T[i].x[j] = rnd.NextDouble() * rnd.NextInt64();
            }
        }


        for (int a = 0; a < I; a++)
        {
            for (int i = 0; i < L + M; i++)
            {
                T[i].result = f(T[i].x);
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

            var ants = new PheromoneSpot[M];

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

                for (int j = 0; j < n; j++)
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

                    T[L + i].x[j] = Normal.Sample(mu, sigma);
                }


            }
        }

        Array.Sort(T, (a, b) => a.result.CompareTo(b.result));

        return T[0].x;

    }
}


struct PheromoneSpot
{
    public double result;

    // Array with n parameters that are passed to function f
    public double[] x;

    // Probability that ant will choose that spot
    public double p;
}
