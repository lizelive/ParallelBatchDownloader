using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ParallelBatchDownloader
{
    class Program
    {
        static object failock = new object();
        static object winlock = new object();

        static async Task DoSomething(Download dr)
        {
            if (dr.State != Download.Status.Queued)
                throw new Exception($"DL Request is in invalid state {dr.State}");
            dr.State = Download.Status.Downloading;


            using var transaction = Context.Database.BeginTransaction();
            Directory.CreateDirectory(Path.GetDirectoryName(dr.Path));
            //using WebClient client = new();

            try
            {
                using HttpClient client = new();
                using SHA256 mySHA256 = SHA256.Create();
                Stopwatch timer = new();
                timer.Start();
                var data = await client.GetByteArrayAsync(dr.Uri);
                var writeTask = File.WriteAllBytesAsync(dr.Path, data);
                var hash = mySHA256.ComputeHash(data);
                await writeTask;
                timer.Stop();
                lock (winlock)
                {
                    File.AppendAllTextAsync("D:\\downloaded.tsv", $"{Convert.ToHexString(hash)}\t{timer.ElapsedMilliseconds}\t{data.Length}\t{dr.Uri}\n");
                }
            }
            catch (Exception e)
            {
                dr.State = Download.Status.Downloading;
                dr
            }
        }

        static void ImportTsv()
        {
            using var context = new DownloadContext();
            context.AddRange(File.ReadAllLines("D:\\todownload.tsv").Select(Download.Parse));
            context.SaveChanges();
        }
        static DownloadContext Context;
        static void Main(string[] args)
        {
            using var context = new DownloadContext();
            Context = context;
            var toDo = context.Downloads.Where(x => x.State == Download.Status.Queued).Take(10).ToList();
            
            
            //Directory.SetCurrentDirectory("D:\\");
            Stopwatch timer = new();
            timer.Start();
            var downloadAllTask = toDo.AsyncParallelForEach(DoSomething);
            downloadAllTask.Wait();
            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}
