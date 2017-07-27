using Aimtec.SDK.Menu;
using Aimtec;
using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Events;

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


        protected abstract void PreMove(object sender, Aimtec.SDK.Orbwalking.PreMoveEventArgs e);

        protected abstract void PreAttack(object sender, Aimtec.SDK.Orbwalking.PreAttackEventArgs e);

        protected abstract void PostAttack(object sender, Aimtec.SDK.Orbwalking.PostAttackEventArgs e);

        protected abstract void OnUpdate();

        protected abstract void Combo();

        protected abstract void Harass();

        protected abstract void Laneclear();

        protected abstract void Lasthit();

        protected abstract void Killsteal();

        protected abstract void OnPresent();

        protected abstract void OnDash(object sender, Dash.DashArgs e);
    }
}
