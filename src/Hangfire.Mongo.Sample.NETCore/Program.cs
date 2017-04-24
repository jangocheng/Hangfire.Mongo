using System;
using System.Collections.Generic;
using System.Threading;

namespace Hangfire.Mongo.Sample.NETCore
{
    public class Program
    {
        private const int JobCount = 100;

        public class BigKahuna : Dictionary<string, string>
        {
            public static BigKahuna Create(int propertyCount)
            {
                var obj = new BigKahuna();
                for (int i = 0; i < propertyCount; i++)
                {
                    obj[$"Property{i}"] = $"PropertyVaule{i}";
                }
                return obj;
            }
        }

        public static DateTime StarTime;
        public static ManualResetEvent Signal = new ManualResetEvent(false);
        public static void Main(string[] args)
        {
            JobStorage.Current = new MongoStorage("mongodb://localhost", "Mongo-Hangfire-Sample-NETCore");

            using (new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 25
            }))
            {
                Console.WriteLine("Running a single dummy job to create all db colletions");
                BackgroundJob.Enqueue(() => Warmup());

                Signal.WaitOne();

                for (var i = 0; i < JobCount; i++)
                {
                    var last = i == JobCount - 1;
                    var jobCount = i;
                    var bigkahuna = BigKahuna.Create(30);
                    BackgroundJob.Enqueue(() => Job(bigkahuna, jobCount, last));
                }

                Console.WriteLine($"{JobCount} job(s) has been enqued. They will be executed shortly!");
                Console.WriteLine($"");
                Console.WriteLine($"If you close this application before they are executed, ");
                Console.WriteLine($"they will be executed the next time you run this sample.");
                Console.WriteLine($"");
                Console.WriteLine($"Press any key to exit...");

                Console.ReadKey(true);
            }
        }

        public static void Warmup()
        {
            Signal.Set();
        }
        public static void Job(BigKahuna bigKahuna, int jobNumber, bool last)
        {
            if (jobNumber == 0)
            {
                StarTime = DateTime.Now;
            }
            Thread.Sleep(10000);
            if (last)
            {
                var elapsedMs = (DateTime.Now - StarTime).TotalMilliseconds;
                Console.WriteLine($"Processed {jobNumber + 1} longrunning jobs in {elapsedMs} ms");
            }
        }
    }
}
