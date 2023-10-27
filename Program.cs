using System;
using System.Threading;

namespace ThreadSumSharp
{
    class Program
    {
        private static readonly int dim = 20000000;
        private static readonly int threadNum = 4; 

        private readonly Thread[] threads = new Thread[threadNum]; 

        static void Main(string[] args)
        {
            Program main = new Program();
            main.InitArr();
            //Console.WriteLine(main.PartSum(0, dim));

            Console.WriteLine(main.ParallelSum());

            int minValue = main.MinElement(out int minIndex);
            Console.WriteLine($"Minimum element: {minValue} at index: {minIndex}");
            Console.ReadKey();
        }

        private int threadCount = 0;
        private readonly object countLock = new object();
        private long sum = 0;
        private readonly object sumLock = new object();

        private long ParallelSum()
        {
            for (int i = 0; i < threadNum; i++)
            {
                int start = i * dim / threadNum;
                int end = (i + 1) * dim / threadNum;
                Console.WriteLine($"Thread {i + 1} processing range from {start} to {end}"); // виводимо межі
                
                threads[i] = new Thread(() => {
                    long threadSum = PartSum(start, end);
                    lock (sumLock)
                    {
                        sum += threadSum;
                    }
                    IncThreadCount();
                });
                threads[i].Start();
            }

            lock (countLock)
            {
                while (threadCount < threadNum)
                {
                    Monitor.Wait(countLock);
                }
            }
            return sum;
        }

        private readonly int[] arr = new int[dim];

        private void InitArr()
        {
            Random random = new Random();
            int randomIndex = random.Next(0, dim); // Вибираємо випадковий індекс
            int negativeNumber = -1 * random.Next(1, 100); // Генеруємо випадкове від'ємне число

            for (int i = 0; i < dim; i++)
            {
                if (i == randomIndex)
                {
                    arr[i] = negativeNumber; // Замінюємо випадковий елемент на від'ємне число
                }
                else
                {
                    arr[i] = i;
                }
            }
        }

        public int MinElement(out int minIndex)
        {
            minIndex = 0;
            int minValue = arr[0];

            for (int i = 1; i < dim; i++)
            {
                if (arr[i] < minValue)
                {
                    minValue = arr[i];
                    minIndex = i;
                }
            }
            return minValue;
        }

        public long PartSum(int startIndex, int finishIndex)
        {
            long threadSum = 0;
            for (int i = startIndex; i < finishIndex; i++)
            {
                threadSum += arr[i];
            }
            return threadSum;
        }

        private void IncThreadCount()
        {
            lock (countLock)
            {
                threadCount++;
                Monitor.Pulse(countLock);
            }
        }
    }
}
