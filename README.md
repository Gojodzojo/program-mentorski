# Algo benchmark
This program is used to test the performance and accuracy of different kinds of metaheuristic algorithms. 

## Usage
To test some algorithm you must type the following command: 
```
dotnet run test "algorithm name" "fitness function name" dimensions population iterations
```

Avaliable algorithms:
* Ant Colony Optimization
* Grey Wolf Optimizer
* Equilibrium Optimizer

Avaliable fitness functions:
* Quadratic Function
* Bent Cigar
* Rosenbrock Function
* Rastrigin Function
* Sphere Function
* Unknown Function
* Eggholder function

If the program was shut down during computations, you can resume its operation with the following command;
```
dotnet run resume
```