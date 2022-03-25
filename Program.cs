using System.Diagnostics;

namespace Exam_AmdahlsLaw
{
    class Program
    {
        static int cores;
        static object counterlock = new object();
        static int first = 1;
        static int last = 100000;

        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            GetCores(); 
            stopwatch.Start();
            for (int i = first; i <= last; i++)  
            {
                PrimeCalculator(i);
            }   
            var s1 = stopwatch.ElapsedMilliseconds;

            for (int i = first; i <= last + 100000; i++)
            {
                PrimeCalculator(i);
            }
            stopwatch.Stop();
            var s2 = stopwatch.ElapsedMilliseconds - s1;

            stopwatch.Reset();

            stopwatch.Start();
            for (int i = first; i <= last; i++)
            {
                PrimeCalculator(i);
            }

            var p1 = stopwatch.ElapsedMilliseconds;

            Parallel.For(first, last + 100000, i =>
            {
                PrimeCalculator(i);
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = cores
                };
            });
            stopwatch.Stop();
            var p2 = stopwatch.ElapsedMilliseconds - p1;

            var speedup = (double)(s1 + s2) / (p1 + p2);


            Console.WriteLine("Serial took  : {2}ms, {0}ms for serial work and {1}ms for parallelizable work", s1, s2, s1 + s2);
            Console.WriteLine("Parallel took: {2}ms, {0}ms for serial work and {1}ms for parallelizable work", p1, p2, p1 + p2);
            Console.WriteLine("Speedup was {0:F}x", speedup);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
        }

        public static List<long> GetPrimesSequential(long first, long last)
        {
            List<long> list = new List<long>();
            Console.WriteLine("Sequential calculation started...");

            for (long i = first; i <= last; i++)
            {
                PrimeCalculator(i);
            }
            Console.WriteLine("Found: " + list.Count() + " prime numbers.");
            return list;
        }

        private static int GetCores()
        {
            var numberOfLogicalProcessors = Environment.ProcessorCount;

            Console.Write("Enter the number of processor cores you want to use (1 to {0}, or press <enter> for {0}):",
                          numberOfLogicalProcessors);

            var stringDegreeOfParallelism = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(stringDegreeOfParallelism) ||
                !int.TryParse(stringDegreeOfParallelism, out cores) ||
                cores > numberOfLogicalProcessors)
            {
                cores = numberOfLogicalProcessors;
            }

            return cores;
        }

        public static void PrimeCalculator(long i)
        {
            List<long> list = new List<long>();
            bool isPrime = true;
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

        public static void GetPrimesParallel(long first, long last)
        {
            List<long> list = new List<long>();
            Console.WriteLine("Parallel calculation started...");

            Parallel.For(first, last + 1, i =>
            {
                PrimeCalculator(i);
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = cores
                };
            });

        }

        private static void MeasureTime(Action ac)
        {
            Stopwatch sw = Stopwatch.StartNew();
            ac.Invoke();
            sw.Stop();
            Console.WriteLine($"Time = {sw.Elapsed.TotalSeconds} seconds");
        }
    }
}