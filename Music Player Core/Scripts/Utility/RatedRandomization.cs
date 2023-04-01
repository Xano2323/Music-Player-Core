using Music_Player.MusicList;
using Music_Player.MusicOrganization;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Music_Player.Utility
{
    /// <summary>
    /// Randomization based on ratings of musics first then from selected rating random music is choosen
    /// Example: 50x musics with rate 1 and 2x musics with rate 9. <br/>
    /// 10% chance to choose one random music within these 50 musics with rating 1 <br/>
    /// and 90% chance to choose one random music from rating 9
    /// </summary>
    public static class RatedRandomization
    {
        // Components
        private class MinMax
        {
            public decimal key;
            public int min;
            public int max;
        }

        // References / Fields
        public static SortedDictionary<decimal, List<int>> ratedMusics;
        private static List<MinMax> minMaxes;
        public static Playlist referencedPlaylist;


        // Methods
        public static int RandomizeRated(Playlist playlist, bool refreshed)
        {
            if (referencedPlaylist == null
                || referencedPlaylist != playlist
                || refreshed)
            {
                referencedPlaylist = playlist;
                CalculateRates();
                CalculateWeights();
            }

            if (ratedMusics.Count > 0)
            {
                Random randomWeight = new Random();
                int randomWeightNumber = randomWeight.Next(0, minMaxes.Last().max + 1);

                IEnumerable<MinMax> collection = 
                    from minMax 
                    in minMaxes 
                    where randomWeightNumber >= minMax.min && randomWeightNumber <= minMax.max 
                    select minMax;

                foreach (MinMax minMax in collection)
                    return ratedMusics[minMax.key][randomWeight.Next(0, ratedMusics[minMax.key].Count)];
            }

            return 0;
        }

        private static void CalculateRates()
        {
            ratedMusics = new SortedDictionary<decimal, List<int>>();

            foreach (MusicElement musicElement in referencedPlaylist.listOfMusicElements)
            {
                if (!decimal.TryParse(musicElement.Rating, out decimal result))
                    continue;

                if (!ratedMusics.ContainsKey(result))
                    ratedMusics[result] = new List<int>();

                ratedMusics[result].Add(musicElement.Index);
            }
        }

        private static void CalculateWeights()
        {
            minMaxes = new List<MinMax>();

            int i = 0;

            foreach (decimal dict in ratedMusics.Keys)
            {
                MinMax minMax = new MinMax()
                {
                    key = dict,
                    min = i,
                    max = i + (int)dict - 1
                };

                minMaxes.Add(minMax);

                i = minMax.max + 1;
            }
        }
    }
}