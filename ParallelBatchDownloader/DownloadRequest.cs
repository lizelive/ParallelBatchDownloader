using System;

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
}
