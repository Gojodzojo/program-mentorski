# Algo benchmark
This program is used to test the performance and accuracy of different kinds of metaheuristic algorithms. 

## Before running 
To compile this program you must first generate some files by running 
```
dotnet restore
```

## Usage
### Test
To test some algorithm you must type the following command: 
```
dotnet run test --algorithm "algorithm name or acronym" --fitness-function "fitness function name"
```

Flags:
* --algorithm - used to specify tested algorithm
* --fitness-function - this is the function for which you want to find the minimum
* --dimensions - optional, default is 2
* --population - optional, default is 30
* --iterations - optional, default is 50

Avaliable fitness functions:
* Quadratic Function
* Bent Cigar
* Rosenbrock Function
* Rastrigin Function
* Sphere Function
* Unknown Function
* Eggholder function

Avaliable algorithms and their flags:
* Ant Colony Optimization (ACO)
	* --L - optional, default is 10
	* --ksi - optional, default is 1
	* --q - optional, default is 0,9
* Grey Wolf Optimizer (GWO)
* Equilibrium Optimizer (EQ)
	* --a1 - optional, default is 2
	* --a2 - optional, default is 1
	* --GP - optional, default is 0,5

### Resume
If the program was shut down during computations, you can resume its operation with the following command;
```
dotnet run resume --algorithm "algorithm name or acronym" --test "test number"
```