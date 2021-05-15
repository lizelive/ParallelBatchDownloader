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
    record DownloadRequest(string Uri, string Path)
    {
        static readonly Uri Base = new("file:\\D:\\");
        public static DownloadRequest Parse(string line)
        {
            var parts = line.Trim().Split('\t');
            var u1 = "./"+new Uri(parts[1]).AbsolutePath.Substring(3);
            return new DownloadRequest(parts[0], u1);
        }

        public override string ToString()
        {
            return $"{Uri}\t{Path}";
        }
    }
    class Program
    {
        static object failock = new object();
        static object winlock = new object();

        static async Task DoSomething(string stuff)
        {
            var dr = DownloadRequest.Parse(stuff);
            if (File.Exists(dr.Path))
            {
                return;
            }
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
                lock (failock)
                {
                    File.AppendAllText("D:\\failed.tsv", dr.ToString() + '\n');
                }
                Console.Error.WriteLine(e);
                if(File.Exists(dr.Path))
                    File.Delete(dr.Path);
            }
        }

        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("D:\\");
            Stopwatch timer = new();
            timer.Start();
            var toDownload = Utils.ReadLinesAsync("./todownload.tsv").AsyncParallelForEach(DoSomething, maxDegreeOfParallelism: 30);
            toDownload.Wait();
            timer.Stop();

            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}
