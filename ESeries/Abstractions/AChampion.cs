using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aimtec.SDK;
using Aimtec.SDK.Menu;
using Aimtec;
using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Menu.Components;

namespace ESeries.Abstractions
{
    abstract class AChampion : IChampion
    {
        public Spell Q { get; set; }
        public Spell W { get; set; }
        public Spell E { get; set; }
        public Spell R { get; set; }

        public Menu Config { get; set; }

        public Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public Orbwalker Orbwalker { get; set; }

        public AChampion()
        {
            Program.RootMenu = new Menu("ESeries", "ESeries", true);

            Config = new Menu($"ESeries.{Player.ChampionName}", Player.ChampionName);

            this.Orbwalker = new Orbwalker();

            this.Orbwalker.Attach(this.Config);
        }

        protected void Attach()
        {
            Program.RootMenu.Add(this.Config);
            Program.RootMenu.Attach();
        }

    }
}
