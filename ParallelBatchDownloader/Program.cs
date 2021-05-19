using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Timers;
using CommandLine;
using System.Threading;

namespace ParallelBatchDownloader
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "stats", Required = false, HelpText = "Show progress stats on startup.")]
        public bool ShowStats { get; set; }

        [Option('t', "import-tsv", Required = false, HelpText = "verbose messages")]

        public string ImportTsv { get; set; }
        [Option('c', "max-concurrent-connections", Required = false, HelpText = "max numbers of concurrent connections")]

        public int MaxConcurrentConnections { get; set; }
    }
    class Program
    {
        static object writelock = new object();
        static Semaphore writeLimiter = new(0, 4, "batchdownloaderdisk");
        static async Task DoSomething(Download dr)
        {
            using var context = new DownloadContext();
            dr = await context.Downloads.FindAsync(dr.Id);
            //if (dr.State != Download.Status.Queued && dr.State != Download.Status.Failed)
            //    throw new Exception($"DL Request is in invalid state {dr.State}");
            dr.State = Download.Status.Downloading;
            dr.Created = DateTime.UtcNow;
            await context.SaveChangesAsync();

            //using var transaction = Context.Database.BeginTransaction();
            Directory.CreateDirectory(Path.GetDirectoryName(dr.Path));
            //using WebClient client = new();

            try
            {
                using HttpClient client = new();
                using SHA256 mySHA256 = SHA256.Create();
                Stopwatch timer = new();
                timer.Start();
                var data = await client.GetByteArrayAsync(dr.Uri);
                var hash = mySHA256.ComputeHash(data);
                dr.Size = data.Length;
                dr.Hash = Convert.ToHexString(hash);
                dr.State = Download.Status.Downloaded;
                await context.SaveChangesAsync();

                Directory.CreateDirectory(Path.GetDirectoryName(dr.Path));
                lock (writelock)
                {
                    // for hard drive only write one file at a time
                    File.WriteAllBytes(dr.Path, data);
                }

                //writeLimiter.WaitOne();
                //File.WriteAllBytes(dr.Path, data);
                //writeLimiter.Release();

                //var writeTask = File.WriteAllBytesAsync(dr.Path, data);
                //await writeTask;

                dr.State = Download.Status.Completed;
                dr.Finished = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);

                dr.State = Download.Status.Failed;
                await context.SaveChangesAsync();
                Console.Error.WriteLine($"{dr} failed");
            }
        }

        static void ImportTsv(string path)
        {
            using var context = new DownloadContext();
            context.AddRange(File.ReadAllLines(path).Select(Download.Parse));
            context.SaveChanges();
        }


        static DateTime lastStatusTime = DateTime.UtcNow;
        static long lastTodo;
        static void ShowStatus(object sender = null, ElapsedEventArgs e = null)
        {
            using var context = new DownloadContext();
            var todo = context.Downloads.Count(x => x.State == Download.Status.Queued);
            var now = DateTime.UtcNow;
            var totalBytesToDownloaded = context.Downloads.Where(x => x.State == Download.Status.Completed).Average(x => x.Size) * todo;

            var elapsed = now - lastStatusTime;
            var doneSenseLast = lastTodo - todo;
            var ratePerSecond = doneSenseLast / elapsed.TotalSeconds;
            var estimatedTimeSeconds = todo / ratePerSecond;
            var estimated = TimeSpan.FromSeconds(estimatedTimeSeconds);


            Console.WriteLine($"{DateTimeOffset.Now} : {todo} ( ~{totalBytesToDownloaded / 1E+09} GB ) est {estimated}, {ratePerSecond * 60} dpm");

            //foreach (var status in Enum.GetValues<Download.Status>())
            //{
            //    var count = context.Downloads.Count(x => x.State == status);
            //    Console.WriteLine($"{status}\t{count}");
            //}

            lastStatusTime = now;
            lastTodo = context.Downloads.Count(x => x.State != Download.Status.Completed);
        }


        static void ShowCounts()
        {

            using var context = new DownloadContext();
            foreach (var status in Enum.GetValues<Download.Status>())
            {
                var count = context.Downloads.Count(x => x.State == status);
                Console.WriteLine($"{status}\t{count}");
            }
        }
        static void Validate()
        {
            using var context = new DownloadContext();
            foreach (var download in context.Downloads.Where(x => x.State == Download.Status.Completed))
            {
                using SHA256 mySHA256 = SHA256.Create();
                var hash = mySHA256.ComputeHash(File.OpenRead(download.Path));
                var good = download.Hash == Convert.ToHexString(hash);
                if (!good)
                {
                    Console.WriteLine($"hasherror {download}");
                    download.State = Download.Status.HashError;

                }
                else
                {
                    download.State = Download.Status.Validated;
                }
                context.SaveChanges();
            }
        }

        static Task DoDownload()
        {
            var allowFailed = true;
            using var context = new DownloadContext();
            var toDo = context.Downloads.Where(x => x.State != Download.Status.Completed && (allowFailed || x.State != Download.Status.Failed));
            return toDo.AsyncParallelForEach(DoSomething, maxDegreeOfParallelism: 10);
        }
        static async Task Main(string[] args)
        {
            Directory.SetCurrentDirectory("F:\\");

            //ImportTsv();
            //ShowStatus();
            //System.Timers.Timer showStatusTimer = new(60*1000);
            //showStatusTimer.Elapsed += ShowStatus;
            //showStatusTimer.Start();
            ShowCounts();
            Validate();
            ShowCounts();
            return;
            Stopwatch timer = new();
            timer.Start();
            var downloadAllTask = DoDownload();
            await downloadAllTask;
            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}

