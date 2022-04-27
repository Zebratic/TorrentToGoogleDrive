using SuRGeoNix.BitSwarmLib;

namespace TorrentToGoogleDrive
{
    public class Torrent
    {
        public string Magnet { get; set; } = "";
        public bool Finished { get; set; } = false;
        public BitSwarm Client { get; set; } = null;
        public Torrent(string magnet)
        {
            Magnet = magnet;
        }
    }
}
