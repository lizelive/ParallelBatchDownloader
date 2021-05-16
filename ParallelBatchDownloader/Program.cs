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

namespace ParallelBatchDownloader
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "stats", Required = false, HelpText = "Show progress stats on startup.")]
        public bool ShowStats { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]

        public string ImportTsv { get; set; }


    }
    class Program
    {
        static object failock = new object();
        static object winlock = new object();

        static async Task DoSomething(Download dr)
        {
            using var context = new DownloadContext();
            dr = await context.Downloads.FindAsync(dr.Id);
            //if (dr.State != Download.Status.Queued && dr.State != Download.Status.Failed)
            //    throw new Exception($"DL Request is in invalid state {dr.State}");
            dr.State = Download.Status.Downloading;
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
                var writeTask = File.WriteAllBytesAsync(dr.Path, data);
                await writeTask;
                dr.State = Download.Status.Completed;
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);

                dr.State = Download.Status.Failed;
                await context.SaveChangesAsync();
            }
        }

        static void ImportTsv()
        {
            using var context = new DownloadContext();
            context.AddRange(File.ReadAllLines("D:\\todownload.tsv").Select(Download.Parse));
            context.SaveChanges();
        }

        static void ShowStatus(object sender = null, ElapsedEventArgs e = null)
        {
            using var context = new DownloadContext();

            var totalBytesToDownloaded = context.Downloads.Where(x => x.State == Download.Status.Completed).Average(x => x.Size) * context.Downloads.Count();

            Console.WriteLine(totalBytesToDownloaded + " bytes to download");
            
            Console.WriteLine(DateTimeOffset.Now);
            var summery = context.Downloads.AsEnumerable().GroupBy(x => x.State).Select(x => (x.Key, Value: x.Count()));
            foreach (var value in summery)
            {
                Console.WriteLine($"{value.Key}\t{value.Value}");
            }
        }

        static Task DoDownload()
        {
            using var context = new DownloadContext();
            var toDo = context.Downloads.Where(x => x.State != Download.Status.Completed);
            return toDo.AsyncParallelForEach(DoSomething, maxDegreeOfParallelism: 10);
        }
        static async Task Main(string[] args)
        {
            //ImportTsv();
            ShowStatus();
            //Timer showStatusTimer = new(60);
            //showStatusTimer.Elapsed += ShowStatus;
            //showStatusTimer.Start();

            Directory.SetCurrentDirectory("D:\\");
            Stopwatch timer = new();
            timer.Start();
            var downloadAllTask = DoDownload();
            await downloadAllTask;
            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}
