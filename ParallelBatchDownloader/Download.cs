using System;
using System.ComponentModel.DataAnnotations;

namespace ParallelBatchDownloader
{
    class Download
    {
        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTime Created { get; set; }
        public DateTime Finished { get; set; }
        public string Uri { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        public int Id { get; set; }
        [ConcurrencyCheck]
        public Status State { get; set; } = Status.Queued;
        public int Size { get; set; }

        public enum Status
        {
            Queued=1,
            Downloading=2,
            Downloaded=3,
            Completed=4,
            Failed=5
        }

        static readonly Uri Base = new("file:\\D:\\");

        public Download(string uri, string path)
        {
            Uri = uri;
            Path = path;
        }

        public static Download Parse(string line)
        {
            var parts = line.Trim().Split('\t');
            return new Download(parts[0], parts[1]);
        }

        public override string ToString()
        {
            return $"{Uri}\t{Path}";
        }
    }
}
