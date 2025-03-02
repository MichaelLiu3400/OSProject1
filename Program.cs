using System;
using System.Threading;


public class Train
{
    private static object track1Lock = new object();
    private static object track2Lock = new object();
    private int trainID;
    private string station;
    private bool useTrack1First;

    public Train(int id, string sta, bool useTr1First)
    {
        this.trainID = id;
        this.station = sta;
        useTrack1First = useTr1First;
    }

    public void Run()
    {
        Console.WriteLine($"Train {trainID} approaching {station}...");

        object firstLock = useTrack1First ? track1Lock : track2Lock;
        object secondLock = useTrack1First ? track2Lock : track1Lock;

        bool acqBothLocks = false;

        while (!acqBothLocks) {
            bool acqLock1 = false, acqLock2 = false;
            try
            {
                // Try acquiring the first lock
                acqLock1 = Monitor.TryEnter(firstLock, TimeSpan.FromSeconds(2));
                if (!acqLock1)
                {
                    Console.WriteLine($"Train {trainID} could not acquire first track, retrying later...");
                    Thread.Sleep(400);

                }

                Console.WriteLine($"Train {trainID} acquired first track, waiting for second...");
                Thread.Sleep(1000); // This is for train delay.

                // Try acquiring the second lock
                acqLock2 = Monitor.TryEnter(secondLock, TimeSpan.FromSeconds(2)); // Fixed lock variable
                if (!acqLock2)
                {
                    Console.WriteLine($"Train {trainID} could not acquire second track, releasing first...");
                    Monitor.Exit(firstLock);
                    Thread.Sleep(400);
                }

                Console.WriteLine($"Train {trainID} acquired both tracks, passing {station}...");
                Thread.Sleep(new Random().Next(1000, 4000)); // This simulates where the train stays at the station.
                acqBothLocks = true;
            }
            finally
            {
                if (acqLock2) { Monitor.Exit(secondLock); }
                if (acqLock1) { Monitor.Exit(firstLock); }
            }
        }
        Console.WriteLine($"Train {trainID} left {station}.");
    }
}

    class Program
    {
        public static void Main(string[] args)
        {
            string[] listOfStas = { "Station A", "Station B" };  // Corrected station list
            Thread[] trainThreads = new Thread[2];

            // Creating two trains with ordered lock acquisition
            Train tr1 = new Train(1, listOfStas[0], true);
            Train tr2 = new Train(2, listOfStas[1], false);

            trainThreads[0] = new Thread(new ThreadStart(tr1.Run));
            trainThreads[1] = new Thread(new ThreadStart(tr2.Run));

            foreach (Thread myThread in trainThreads)
            {
                myThread.Start();
            }

            foreach (Thread myThread in trainThreads)
            {
                myThread.Join();
            }

            Console.WriteLine("Trains finished journeys.");
        }
    }