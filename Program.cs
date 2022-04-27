using SuRGeoNix.BitSwarmLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentToGoogleDrive
{
    internal class Program
    {
        public static List<Torrent> queue = new List<Torrent>();
        static void Main(string[] args)
        {
            redo:
            Console.Write("How many concurrent downloads: ");
            string? concurrentdownloads = Console.ReadLine();
            int concurrentdownloadsint = 0;
            try { concurrentdownloadsint = Convert.ToInt32(concurrentdownloads); } catch { goto redo; }

            Console.WriteLine("Loading 'links.txt'");
            try
            {
                foreach (string link in File.ReadAllLines("links.txt").ToList())
                    queue.Add(new Torrent(link));
            }
            catch (Exception ex) { Console.WriteLine("I/O error: " + ex.Message); }

            Console.WriteLine($"Queued {queue.Count} torrents!");

            int started = 0;
            foreach (Torrent torrent in queue.ToList())
            {
                new Thread(() => TorrentDownloader.StartDownload(torrent)).Start();
                started++;
                if (started == concurrentdownloadsint)
                    break;
            }

            Thread.Sleep(-1);
        }
    }
}