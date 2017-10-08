using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Menu.Theme;
using Aimtec.SDK.Util;
using System.Diagnostics;

namespace Ewareness
{
    class MapHack : IAwarenessModule
    {
        private Dictionary<int, HeroMapInfo> Data = new Dictionary<int, HeroMapInfo>();

        public static Vector3 EnemySpawnPosition { get; set; }

        public MapHack()
        {
     
        }

        public void Load(Menu rootMenu)
        {
            Config = new Menu("MapHack", "MapHack");
            Config.Add(new MenuBool("Enabled", "Enabled"));
            rootMenu.Add(Config);

            var sp = ObjectManager
                .Get<GameObject>().FirstOrDefault(x => x.Type == GameObjectType.obj_HQ && x.Team != ObjectManager.GetLocalPlayer().Team);

            if (sp != null)
            {
                EnemySpawnPosition = sp.Position;
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                if (hero.ChampionName == "PracticeTool_TargetDummy")
                {
                    continue;
                }

                this.Data.Add(hero.NetworkId, new HeroMapInfo(hero));
            }

      
            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
            // AttackableUnit.OnEnterVisible += AttackableUnit_OnEnterVisible; //broken
        }

        private void Game_OnUpdate()
        {
            foreach (var data in this.Data)
            {
                var dv = data.Value;

                var mpos = ObjectManager.GetLocalPlayer().ServerPosition;
                var dvpos = dv.Position;

                var mcell = NavMesh.WorldToCell(mpos);
                var dvcell = NavMesh.WorldToCell(dvpos);

                var mgrass = mcell.Flags.HasFlag(NavCellFlags.Grass);
                var enemygrass = dvcell.Flags.HasFlag(NavCellFlags.Grass);
                var distance = mpos.Distance(dvpos);

                if (distance <= 1200 && (!enemygrass || mgrass && distance < 500))
                {
                    if (!dv.Hero.IsVisible)
                    {
                        dv.Displaying = false;
                    }
                }

                if (dv.IsRecalling && dv.RecallCancelTime - dv.RecallStartTime > 5500) //Recall completed
                {
                    dv.Position = EnemySpawnPosition;
                    dv.IsRecalling = false;
                    dv.RecallComplete = true;
                }

                if (dv.Hero.IsVisible)
                {
                    dv.Displaying = true;
                    dv.LastSeenTime = Game.TickCount;
                    dv.Position = dv.Hero.ServerPosition;
                }

                if (dv.Hero.IsDead)
                {
                    dv.Position = EnemySpawnPosition;
                    dv.LastSeenTime = Game.TickCount;
                }
            }
        }

        private void AttackableUnit_OnEnterVisible(AttackableUnit sender, EventArgs e)
        {
            var hero = sender as Obj_AI_Hero;

            if (hero == null || hero.IsAlly)
            {
                return;
            }

            Data[hero.NetworkId].Displaying = true;
        }


        private void Obj_AI_Base_OnTeleport(Obj_AI_Base sender, Obj_AI_BaseTeleportEventArgs e)
        {
            var hero = sender as Obj_AI_Hero;

            if (hero == null)
            {
                return;
            }

            if (!hero.IsValid || hero.IsAlly)
            {
                return;
            }

            if (e.DisplayName != null)
            {
                var name = e.DisplayName.ToLower();

                var data = Data[hero.NetworkId];

                if (name.Contains("recall"))
                {
                    data.RecallStartTime = Game.TickCount;
                    data.IsRecalling = true;
                }

                else
                {
                    data.RecallCancelTime = Game.TickCount; 
                }
            }
        }

        private void Render_OnPresent()
        {
            if (!Config["Enabled"].Enabled)
            {
                return;
            }

            foreach (var info in this.Data)
            {
                var value = info.Value;

                if (value.HeroTexture == null || !value.Displaying)
                {
                    continue;
                }

                if (value.Hero == null || value.Hero.IsVisible || !value.Hero.IsValid || value.Hero.IsDead)
                {
                    continue;
                }
                
                var lastSeenPos = value.Position;

                Vector2 mmPosition;

                if (Render.WorldToMinimap(lastSeenPos, out mmPosition))
                {
                    if (mmPosition.IsZero)
                    {
                        continue;
                    }

                    var adjusted = mmPosition + new Vector2(-16, -16);
                    value.HeroTexture.Draw(adjusted);
                    var seconds = (int) Math.Ceiling(value.TimeMIA / 1000f);

                    var text = seconds.ToString();

                    var center = RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter | RenderTextFlags.NoClip
                                 | RenderTextFlags.SingleLine;

                    var textPosition = new Aimtec.Rectangle((int)adjusted.X, (int)adjusted.Y, (int)(adjusted.X + 32), (int)(adjusted.Y + 32));

                    Render.Text(text, textPosition, center, Color.White);
                }

                if (value.TimeMIA < 25000)
                {
                    var distanceTravelled = (value.TimeMIA / 1000f) * value.Hero.MoveSpeed;
                    DrawCircleOnMinimap(lastSeenPos, distanceTravelled, Color.Red, 3, 100);
                }
            }
        }

        public static void DrawCircleOnMinimap(
            Vector3 center,
            float radius,
            Color color,
            int thickness = 1,
            int quality = 100)
        {
            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float) Math.Cos(angle),
                        center.Y,
                        center.Z + radius * (float) Math.Sin(angle))
                );
            }
            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                Render.WorldToMinimap(a, out Vector2 aonScreen);
                Render.WorldToMinimap(b, out Vector2 bonScreen);

                Render.Line(aonScreen, bonScreen, color);
            }
        }


        public void Unload()
        {
            Render.OnPresent -= Render_OnPresent;
            Game.OnUpdate -= Game_OnUpdate;
            Obj_AI_Base.OnTeleport -= Obj_AI_Base_OnTeleport;
            this.Data = null;
            this.Config.Dispose();
        }

        public Menu Config { get; set; }

        class HeroMapInfo
        {
            public HeroMapInfo(Obj_AI_Hero hero)
            {
                this.Hero = hero;
                this.LoadTexture();
            }

            public Dictionary<string, string> RiotIsRetarded = new Dictionary<string, string>()
            {
                { "FiddleSticks", "Fiddlesticks" }
            };

            public void LoadTexture()
            {
                string championName = this.Hero.UnitSkinName;

                if (this.RiotIsRetarded.ContainsKey(championName))
                {
                    championName = this.RiotIsRetarded[championName];
                }

                var bitmap = Utility.GetHeroBitMap(championName);
                if (bitmap != null)
                {
                    var circular = Utility.CropImageToCircle(bitmap, 0, 0, bitmap.Width);
                    var resized = Utility.ResizeImage(circular,
                        new Size(32, 32));
                    var greyed = Utility.MakeGrayscale(resized);
                    var outlined = Utility.AddCircleOutline(greyed, Color.White, 2);
                    this.HeroTexture = new Texture(outlined);
                }

                else
                {
                    Console.WriteLine($"Could not find BitMap for {Hero.UnitSkinName}");
                }
            }

            public Texture HeroTexture { get; set; }

            public Obj_AI_Hero Hero { get; set; }

            public int LastSeenTime { get; set; } 

            public int TimeMIA => Game.TickCount - this.LastSeenTime;

            public bool IsRecalling { get; set; }

            public bool RecallComplete { get; set; }

            public Vector3 Position { get; set; } = EnemySpawnPosition;

            public bool Displaying { get; set; } = true;

            public int RecallStartTime { get; set; }

            public int RecallCancelTime { get; set; }
        }
    }

}
