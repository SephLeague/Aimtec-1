using System;
using Aimtec.SDK.Events;
using Aimtec.SDK.Menu;

namespace Ewareness
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
            Root = new Menu("Ewareness", "Ewareness", true);

            OHTracker overheadtracker = new OHTracker(Root);
            
            overheadtracker.Load();

            Root.Attach();

            MapHack mapHack = new MapHack(Root);

            mapHack.Load();

            Console.WriteLine("Ewareness Loaded");

        }
    }
}
