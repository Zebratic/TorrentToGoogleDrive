using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using System.Web;
using System.Threading.Tasks;
using SuRGeoNix.BitSwarmLib;
using System.Runtime.CompilerServices;

namespace TorrentToGoogleDrive
{
    public class TorrentDownloader
    {
        public static BitSwarm StartDownload(Torrent torrent)
        {
            string magnet = torrent.Magnet;
            Console.WriteLine("Queuing " + magnet + " for download...");
            Options opt = new Options();
#if DEBUG
            opt.FolderTorrents = Environment.CurrentDirectory + @"\Downloads\Temp\_Torrents";
            opt.FolderSessions = Environment.CurrentDirectory + @"\Downloads\Temp\_Sessions";
            opt.FolderIncomplete = Environment.CurrentDirectory + @"\Downloads\Temp\_Incomplete";
            opt.FolderComplete = Environment.CurrentDirectory + @"\Downloads\Temp";
#else
            opt.FolderTorrents = @"G:\Fællesdrev\Temp\_Torrents";
            opt.FolderSessions = @"G:\Fællesdrev\Temp\_Sessions";
            opt.FolderIncomplete = @"G:\Fællesdrev\Temp\_Incomplete";
            opt.FolderComplete = @"G:\Fællesdrev\Temp";
#endif
            torrent.Client = new BitSwarm(opt);

            // Step 2: Subscribe events
            torrent.Client.MetadataReceived += BitSwarm_MetadataReceived;
            torrent.Client.StatsUpdated += BitSwarm_StatsUpdated;
            torrent.Client.StatusChanged += BitSwarm_StatusChanged;
            torrent.Client.OnFinishing += BitSwarm_OnFinishing;

            torrent.Client.Open(magnet);
            torrent.Client.Start();

            //bitSwarm.Dispose(); // when done
            return torrent.Client;
        }

        private static void BitSwarm_MetadataReceived(object source, BitSwarm.MetadataReceivedArgs e)
        {
            BitSwarm bs = source as BitSwarm;
            Console.WriteLine("MetadataReceived:" + e.Torrent.file.name);
        }

        private static void BitSwarm_StatsUpdated(object source, BitSwarm.StatsUpdatedArgs e)
        {
            BitSwarm bs = source as BitSwarm;
            string name = bs.torrent.file.name.Length >= 30 ? bs.torrent.file.name.Substring(0, 30) + "..." : bs.torrent.file.name;
            Console.WriteLine("StatsUpdated Progress: " + e.Stats.Progress + "%  ETA: " + TimeSpan.FromSeconds((e.Stats.ETA + e.Stats.AvgETA) / 2).ToString(@"hh\:mm\:ss") + "  " + name);
        }

        private static void BitSwarm_StatusChanged(object source, BitSwarm.StatusChangedArgs e)
        {
            BitSwarm bs = source as BitSwarm;
            string name = bs.torrent.file.name.Length >= 30 ? bs.torrent.file.name.Substring(0, 30) + "..." : bs.torrent.file.name;
            Console.WriteLine("StatusChanged.Status: " + e.Status + "  " + name);
            Console.WriteLine("StatusChanged.ErrorMsg: " + e.ErrorMsg + "  " + name);
        }

        private static void BitSwarm_OnFinishing(object source, BitSwarm.FinishingArgs e)
        {
            try
            {
                BitSwarm bs = source as BitSwarm;
                string name = bs.torrent.file.name.Length >= 30 ? bs.torrent.file.name.Substring(0, 30) + "..." : bs.torrent.file.name;
                Console.WriteLine("OnFinishing  " + name);
                bs?.Dispose();
                try
                {
                    Torrent torrent = Program.queue.Find(x => x.Client == bs);
                    torrent.Finished = true;
                }
                catch (Exception ex) { Console.WriteLine(ex); }
                try
                {
                    if (Program.queue.Count != 0)
                    {
                        Torrent newtorrent = Program.queue.Find(x => x.Finished != true);
                        new Thread(() => StartDownload(newtorrent)).Start();
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}