using System;
using Aimtec.SDK.Events;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;

namespace Ewareness
{
    class Program
    {
        private static Menu Root { get; set; }

        static void Main(string[] args)
        {
            GameEvents.GameStart += GameEventsOnGameStart;
        }

        private static IAwarenessModule MapHack { get; set; }
        private static IAwarenessModule SpellTracker { get; set; }

        private static void GameEventsOnGameStart()
        {
            Root = new Menu("Ewareness", "Ewareness", true);

            var modules = new Menu("Modules", "Modules");
            var spelltracker = new MenuBool("SpellTracker", "SpellTracker");
            var spelltrackerType = new MenuList("SpellTrackerType", "SpellTracker Type", new[] { "V1", "V2" }, 1);
            var maphack = new MenuBool("Maphack", "Maphack");

            modules.Add(spelltracker);
            modules.Add(spelltrackerType);
            modules.Add(maphack);
         

            Root.Add(modules);

            if (spelltracker.Enabled)
            {
                var type = spelltrackerType.Value;
                LoadSpellTracker(type);
            }

            if (maphack.Enabled)
            {
                MapHack = new MapHack();
                MapHack.Load(Root);
            }

            Root.Attach();

            spelltracker.OnValueChanged += (s, e) =>
            {
 
                var newvalue = e.GetNewValue<MenuBool>();
                if (newvalue.Enabled)
                {
                    var type = spelltrackerType.Value;
                    LoadSpellTracker(type);
                }

                else
                {
                    if (SpellTracker != null)
                    {
                        SpellTracker.Unload();
                        SpellTracker = null;
                    }
                }
            };

            spelltrackerType.OnValueChanged += (sender, args) =>
            {
                if (spelltracker.Enabled)
                {
                    var newvalue = args.GetNewValue<MenuList>();
                    var newType = newvalue.Value;

                    if (SpellTracker != null)
                    {
                        SpellTracker.Unload();
                        SpellTracker = null;
                    }

                    LoadSpellTracker(newType);
                }
            };

            maphack.OnValueChanged += (s, args) =>
            {
                var newValue = args.GetNewValue<MenuBool>();
                if (newValue.Enabled)
                {
                    if (MapHack != null)
                    {
                        MapHack.Unload();
                        MapHack = null;
                    }

                    MapHack = new MapHack();
                    MapHack.Load(Root);
                }

                else
                {
                    if (MapHack != null)
                    {
                        MapHack.Unload();
                        MapHack = null;
                    }
                }
            };


            Console.WriteLine("Ewareness Loaded");
        }

        private static void Spelltracker_OnValueChanged(MenuComponent sender, ValueChangedArgs args)
        {
            throw new NotImplementedException();
        }

        static void LoadSpellTracker(int type)
        {
            if (type == 1)
            {
                SpellTracker = new OHTracker();
                SpellTracker.Load(Root);
            }

            else
            {
                SpellTracker = new OHTrackerHud();
                SpellTracker.Load(Root);
            }
        }
    }
}
