using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Menu.Theme;
using Aimtec.SDK.Util;

namespace Ewareness
{
    class MapHack : IAwarenessModule
    {
        private Dictionary<int, HeroMapInfo> Data = new Dictionary<int, HeroMapInfo>();

        public static Vector3 EnemySpawnPosition { get; set; }

        public MapHack(Menu rootMenu)
        {
            Config = new Menu("MapHack", "MapHack");
            Config.Add(new MenuBool("Enabled", "Enabled"));
            rootMenu.Add(Config);
        }


        public void Load()
        {
            var sp = ObjectManager
                .Get<GameObject>().FirstOrDefault(x => x.Type == GameObjectType.obj_HQ && x.Team != ObjectManager.GetLocalPlayer().Team);

            if (sp != null)
            {
                EnemySpawnPosition = sp.Position;
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy || x.IsAlly))
            {
                this.Data.Add(hero.NetworkId, new HeroMapInfo(hero));
            }

      
            Render.OnPresent += Render_OnPresent;
           // Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport; //to do
        }

        private void Obj_AI_Base_OnTeleport(Obj_AI_Base sender, Obj_AI_BaseTeleportEventArgs e)
        {
            var hero = sender as Obj_AI_Hero;

            if (hero == null)
            {
                return;
            }

            if (!hero.IsValid || !hero.IsEnemy)
            {
                return;
            }

            if (e.DisplayName != null)
            {
                var name = e.DisplayName.ToLower();

                var data = Data[hero.NetworkId];

                if (name.Contains("recall"))
                {
                    data.IsRecalling = true;
                }

                else
                {
                    data.IsRecalling = false;
                    data.RecallComplete = true;
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

                if (value.HeroTexture == null)
                {
                    continue;
                }

                
                if (value.Hero.IsVisible)
                {
                    value.LastSeenTime = Game.TickCount;
                    value.Position = value.Hero.ServerPosition;
                    continue;
                }
 
                if (value.Hero == null || !value.Hero.IsValid || value.Hero.IsDead)
                {
                    continue;
                }
                

                var lastSeenPos = value.Hero.ServerPosition;

                /*
                if (value.RecallComplete)
                {
                    lastSeenPos = EnemySpawnPosition;
                    value.RecallComplete = false;
                }
                */

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

                    Render.Text(text, textPosition, center, Color.Azure);
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
        }

        public Menu Config { get; set; }

        class HeroMapInfo
        {
            public HeroMapInfo(Obj_AI_Hero hero)
            {
                this.Hero = hero;
                this.LoadTexture();
            }

            public void LoadTexture()
            {
                var bitmap = Utility.GetHeroBitMap(this.Hero.UnitSkinName);
                if (bitmap != null)
                {
                    var circular = Utility.CropImageToCircle(bitmap, 0, 0, bitmap.Width);
                    var resized = Utility.ResizeImage(circular,
                        new Size(32, 32));
                    var greyed = Utility.MakeGrayscale(resized);
                    var outlined = Utility.AddCircleOutline(greyed, Color.OrangeRed, 2);
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
        }
    }

}
