using System;
using System.ComponentModel.DataAnnotations;

namespace ParallelBatchDownloader
{
    class Download
    {
        [Key]
        public string Uri { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        //public int Id { get; set; }
        public byte[] Data { get; set; }
        public Status State { get; set; } = Status.Queued;
        public int Size { get; set; }

        public enum Status
        {
            Queued,
            Downloading,
            Downloaded,
            Completed,
            Failed
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
