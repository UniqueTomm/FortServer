﻿using FortLibrary.EpicResponses.LeaderBoard;
using FortBackend.src.App.Utilities.MongoDB;
using FortLibrary.MongoDB.Module;
using MongoDB.Driver;
using System.Collections.Generic;
using FortLibrary;

namespace FortBackend.src.App.Utilities.Helpers
{

    public class UpdateLeaderBoard
    {
        static IMongoCollection<StatsInfo> Collection { get; set; }
        public static LeaderBoardData LeaderboardCached { get; set; } = new LeaderBoardData();
        public static async Task<List<LeaderBoardStats>> GrabTop100()
        {
            var TempData = new List<LeaderBoardStats>();

            var data = await Collection.Find(FilterDefinition<StatsInfo>.Empty).ToListAsync();

            foreach (var statName in GetStatNames())
            {
                var ListStatsData = new Dictionary<string, int>();

                var top100 = data
                .OrderByDescending(x => x.stats.TryGetValue(statName, out int value) ? value : int.MinValue)
                .Take(100)
                .ToList();


                foreach (var stats in top100)
                {
                    int statsValue = -1;
                    if (stats.stats.TryGetValue(statName, out int value69))
                        statsValue = value69;

                    ListStatsData.Add(stats.AccountId, statsValue);
                }

                TempData.Add(new LeaderBoardStats
                {
                    statName = statName,
                    stat = ListStatsData
                });

            }
            
            return TempData;
        }
        public static async Task GrabLatest()
        {
            if (MongoDBStart.Database is null) return;

            Collection = MongoDBStart.Database.GetCollection<StatsInfo>("StatsInfo");

            LeaderboardCached.Data = await GrabTop100();
            //LeaderboardCached.Duos = await GrabTop100("duos");
            //LeaderboardCached.Trios = await GrabTop100("trios");
            //LeaderboardCached.Squads = await GrabTop100("squad");
            //LeaderboardCached.Ltms = await GrabTop100("ltm");
        }

        public static async Task LeaderboardLoop()
        {
            while (true)
            {
                await GrabLatest();
                Logger.Log("Grabbed Latest Leaderboard", "[Leaderboard]");

                await Task.Delay(900000);
            }
        }

        //pc_m0_p2 ~ solos
        //pc_m0_p10 ~ duos
        //pc_m0_p9 ~ squads
        public static IEnumerable<string> GetStatNames()
        {
            return new List<string>
            {
                "br_score_pc_m0_p2",
                "br_matchesplayed_pc_m0_p2",
                "br_kills_pc_m0_p2",
                "br_minutesplayed_pc_m0_p2",
                "br_placetop1_pc_m0_p2",
                "br_placetop10_pc_m0_p2",
                "br_placetop25_pc_m0_p2",
                "br_placetop1_keyboardmouse_m0_playlist_defaultsolo",
                "br_placetop10_keyboardmouse_m0_playlist_defaultsolo",
                "br_placetop25_keyboardmouse_m0_playlist_defaultsolo",


                "br_score_pc_m0_p10",
                "br_matchesplayed_pc_m0_p10",
                "br_kills_pc_m0_p10",
                "br_minutesplayed_pc_m0_p10",
                "br_placetop1_pc_m0_p10",
                "br_placetop5_pc_m0_p10",
                "br_placetop12_pc_m0_p10",
                "br_placetop1_keyboardmouse_m0_playlist_defaultduo",
                "br_placetop5_keyboardmouse_m0_playlist_defaultduo",
                "br_placetop12_keyboardmouse_m0_playlist_defaultduo",

                "br_score_pc_m0_p9",
                "br_matchesplayed_pc_m0_p9",
                "br_kills_pc_m0_p9",
                "br_minutesplayed_pc_m0_p9",
                "br_placetop1_pc_m0_p9",
                "br_placetop3_pc_m0_p9",
                "br_placetop6_pc_m0_p9",
                "br_placetop1_keyboardmouse_m0_playlist_defaultsquad",
                "br_placetop3_keyboardmouse_m0_playlist_defaultsquad",
                "br_placetop6_keyboardmouse_m0_playlist_defaultsquad",
            };
        }
    }
}
