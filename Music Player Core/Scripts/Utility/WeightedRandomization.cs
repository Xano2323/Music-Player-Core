using Music_Player.MusicList;
using Music_Player.MusicOrganization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Music_Player.Utility
{
    /// <summary>
    /// Randomization based on music ratings <br/>
    /// Rates 1,1,1,2,3 means that 1-3 musics have 12,5% chance to be choosen, 4 has 25% and 5 has 37,5%
    /// </summary>
    public static class WeightedRandomization
    {
        public static List<int> weightedMusics;
        public static Playlist referencedPlaylist;


        public static int RandomizeWeighted(Playlist playlist, bool refreshed)
        {
            if (referencedPlaylist == null
                || referencedPlaylist != playlist
                || refreshed)
            {
                referencedPlaylist = playlist;
                CalculateWeights();
            }

            if (weightedMusics.Count > 0)
                return weightedMusics[new Random().Next(0, weightedMusics.Count)];
            else return 0;
        }

        private static void CalculateWeights()
        {
            weightedMusics = new List<int>();

            foreach (MusicElement musicElement in referencedPlaylist.listOfMusicElements)
            {
                if (!decimal.TryParse(musicElement.Rating, out decimal result))
                    continue;

                AddMusic(result, musicElement.Index);
            }
        }

        private static void AddMusic(decimal result, int index)
        {
            for (int i = 0; i < result; i++)
            {
                weightedMusics.Add(index);
            }
        }
    }
}