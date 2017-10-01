using System;
using Aimtec.SDK.Events;
using Aimtec.SDK.Menu;

namespace EUtility
{
    class Program
    {
        private static Menu Root { get; set; }

        static void Main(string[] args)
        {
            GameEvents.GameStart += GameEventsOnGameStart;
        }

        private static void GameEventsOnGameStart()
        {
            Root = new Menu("EUtility", "EUtility", true);

            OHTracker overheadtracker = new OHTracker(Root);
            
            overheadtracker.Load();

            Root.Attach();

            MapHack mapHack = new MapHack(Root);

            mapHack.Load();

            Console.WriteLine("EUtility Loaded");

        }
    }
}
