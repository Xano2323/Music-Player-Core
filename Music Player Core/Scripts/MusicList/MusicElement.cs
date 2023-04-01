using System;


namespace Music_Player.MusicList
{
    [Serializable]
    public class MusicElement
    {
        public int Index { get; set; }
        public bool Initiated { get; set; }
        public string MusicPath { get; set; }
        public string MusicName { get; set; }
        public string Duration { get; set; }
        public ulong DurationNumber { get; set; }
        public TimeSpan DurationTimeSpan { get; set; }

        // New
        public string Rating { get; set; }
        public string CreationDate { get; set; }
        public int ListenTimes { get; set; }
        public string ListenLength { get; set; }
        public string Groups { get; set; }
        public string Tags { get; set; }
    }
}