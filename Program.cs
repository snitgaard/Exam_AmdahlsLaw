using System.Diagnostics;

namespace Exam_AmdahlsLaw
{
    class Program
    {
        static object counterlock = new object();
        static int first = 1;
        static int last = 100000;

        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            var cores = GetDegreesOfParallelism();
            var sequentialWorkLoad = new List<Action> { PrimeCalculator, PrimeCalculator };
            var parallelizableWorkLoad = new List<Action> { PrimeCalculator, PrimeCalculator, PrimeCalculator, PrimeCalculator, PrimeCalculator, PrimeCalculator };

            stopwatch.Start();
            foreach(var sequentialWork in sequentialWorkLoad)
            {
                sequentialWork();
            }
            var s1 = stopwatch.ElapsedMilliseconds;

            foreach(var notParallelWork in parallelizableWorkLoad)
            {
                notParallelWork();
            }
            stopwatch.Stop();
            var s2 = stopwatch.ElapsedMilliseconds - s1;

            stopwatch.Reset();

            stopwatch.Start();
            foreach (var sequentialWork in sequentialWorkLoad)
            {
                sequentialWork();
            }

            var p1 = stopwatch.ElapsedMilliseconds;

            Parallel.ForEach(
                parallelizableWorkLoad,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = cores
                },
                (workToDo) => workToDo());

            stopwatch.Stop();
            var p2 = stopwatch.ElapsedMilliseconds - p1;

            var speedup = (double)(s1 + s2) / (p1 + p2);


            Console.WriteLine("Serial took  : {2}ms, {0}ms for serial work and {1}ms for parallelizable work", s1, s2, s1 + s2);
            Console.WriteLine("Parallel took: {2}ms, {0}ms for serial work and {1}ms for parallelizable work", p1, p2, p1 + p2);
            Console.WriteLine("Speedup was {0:F}x", speedup);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
        }

        private static int GetDegreesOfParallelism()
        {
            var numberOfLogicalProcessors = Environment.ProcessorCount;

            // Get number of processors you want to use from user
            Console.Write("Enter the number of processors you want to use (1 to {0}, or press <enter> for {0}):",
                          numberOfLogicalProcessors);

            var stringDegreeOfParallelism = Console.ReadLine();
            int degreeOfParallelism;

            if (string.IsNullOrWhiteSpace(stringDegreeOfParallelism) ||
                !int.TryParse(stringDegreeOfParallelism, out degreeOfParallelism) ||
                degreeOfParallelism > numberOfLogicalProcessors)
            {
                degreeOfParallelism = numberOfLogicalProcessors;
            }

            return degreeOfParallelism;
        }

        public static void PrimeCalculator()
        {
            List<long> list = new List<long>();
            bool isPrime = true;
            for(int i=first; i<last; i++)
            {
                if (i > 1)
                {
                    for (int j = 2; j < i; j++)
                    {
                        if (i % j == 0)
                        {
                            isPrime = false;
                            break;
                        }
                    }
                    if (isPrime == true)
                    {
                        lock (counterlock)
                        {
                            list.Add(i);
                        }
                    }
                }
            }
           
        }
    }
}