using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Versioning;
using Aimtec;
using Aimtec.SDK.Menu;
using System.Reflection;
using System.IO;
using System.Resources;
using Aimtec.SDK.Menu.Components;
using Ewareness.Properties;

namespace Ewareness
{
    class OHTrackerHud : IAwarenessModule
    {
        public Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();
        public static Texture HudTexture { get; set; }
        public static Texture HudTextureSelf { get; set; }

        public Menu Config { get; set; }

        List<FloatingTracker> Trackers = new List<FloatingTracker>();


        private void Render_OnPresent()
        {
            foreach (var tracker in this.Trackers)
            {
                if (tracker.Unit.IsAlly && !this.Config["TrackAllies"].Enabled)
                {
                    continue;
                }

                if (tracker.Unit.IsMe && !this.Config["TrackMe"].Enabled)
                {
                    continue;
                }

                tracker.Draw();
            }
        }

        class FloatingTracker
        {
            public Obj_AI_Hero Unit { get; set; }

            public Texture HUD { get; set; }

            public OHTrackerHud.TrackingSpell Summoner1 { get; set; }

            public OHTrackerHud.TrackingSpell Summoner2 { get; set; }

            List<TrackingSpell> Spells = new List<TrackingSpell>();


            public FloatingTracker(Obj_AI_Hero unit)
            {
                //Load teextres
                
                if (unit.IsMe)
                {
                    var bmpHudSelf = Resources.OhTrackerHudSelf;
                    this.HUD = new Texture(bmpHudSelf);
                }
                

                else
                {
                    var bmpHud = Resources.OhTrackerHud;
                    this.HUD = new Texture(bmpHud);
                }

                this.Unit = unit;

                //Load Summoners Textures
                foreach (var spell in this.Unit.SpellBook.Spells)
                {
                    TrackingSpell tspell = null;

                    if (spell.Slot == SpellSlot.Summoner1)
                    {
                        tspell = new TrackingSpell(spell, true);
                        Summoner1 = tspell;
                    }

                    else if (spell.Slot == SpellSlot.Summoner2)
                    {
                        tspell = new TrackingSpell(spell, true);
                        Summoner2 = tspell;
                    }

                    else if (spell.Slot == SpellSlot.Q || spell.Slot == SpellSlot.W || spell.Slot == SpellSlot.E ||
                             spell.Slot == SpellSlot.R)
                    {
                        tspell = new TrackingSpell(spell);
                        this.Spells.Add(tspell);
                    }
                }
            }


            internal void Draw()
            {
                if (!this.Unit.IsValid || !this.Unit.IsVisible || this.Unit.IsDead || this.Unit.FloatingHealthBarPosition.IsZero)
                {
                    return;
                }

  
                if (!this.Unit.IsMe)
                {

                    var bpos = this.Unit.FloatingHealthBarPosition + new Vector2(-33, 10);

                    if (this.HUD != null)
                    {
                        this.HUD.Draw(bpos);
                    }


                    var summonerStart = bpos + new Vector2(3f, 2f);

                    var s1Bar = summonerStart + new Vector2(0.5f, 17.5f);
                    if (this.Summoner1 != null && this.Summoner1.SpellTexture != null)
                    {
                        this.Summoner1.SpellTexture.Draw(summonerStart);
                        Render.Rectangle(s1Bar, this.Summoner1.CurrentWidth, this.Summoner1.statusBoxHeight,
                            this.Summoner1.CurrentColor);


                        if (this.Summoner1.Spell.State.HasFlag(SpellState.Cooldown) && this.Summoner1.TimeUntilReady > 0.0f)
                        {
                            string cdString = this.Summoner1.TimeUntilReady < 1
                                ? this.Summoner1.TimeUntilReady.ToString("0.0")
                                : Math.Ceiling(this.Summoner1.TimeUntilReady).ToString();

                            var tpos = s1Bar + new Vector2((int) (this.Summoner1.statusBoxWidth / 2), 6);
                            Render.Text(cdString, tpos, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                        }
                    }


                    var s2Bar = s1Bar + new Vector2(17, 0);
                    if (this.Summoner2 != null && this.Summoner2.SpellTexture != null)
                    {
                        var s2Position = summonerStart + new Vector2(18, 0);
                        this.Summoner2.SpellTexture.Draw(s2Position);
                        Render.Rectangle(s2Bar, this.Summoner2.CurrentWidth, this.Summoner2.statusBoxHeight,
                            this.Summoner2.CurrentColor);

                        if (this.Summoner2.Spell.State.HasFlag(SpellState.Cooldown) && this.Summoner2.TimeUntilReady > 0.0f)
                        {
                            string cdString = this.Summoner2.TimeUntilReady < 1
                                ? this.Summoner2.TimeUntilReady.ToString("0.0")
                                : Math.Ceiling(this.Summoner2.TimeUntilReady).ToString();

                            var tpos = s2Bar + new Vector2((int)(this.Summoner2.statusBoxWidth / 2) - Render.MeasureText(cdString) / 2, 6);
                            Render.Text(cdString, tpos, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                        }
                    }


                    var nSpellStart = s2Bar + new Vector2(18, 0);
                    for (int i = 0; i < this.Spells.Count; i++)
                    {
                        var sp = this.Spells[i];
                        var position = nSpellStart + new Vector2(i * 27, 0);
                        Render.Rectangle(position, sp.CurrentWidth, sp.statusBoxHeight, sp.CurrentColor);

                        if (sp.Spell.State.HasFlag(SpellState.Cooldown) && sp.TimeUntilReady > 0.0f)
                        {
                            string cdString = sp.TimeUntilReady < 1
                                ? sp.TimeUntilReady.ToString("0.0")
                                : Math.Ceiling(sp.TimeUntilReady).ToString();

                            var pos = position + new Vector2((int)(sp.statusBoxWidth / 2) - Render.MeasureText(cdString) / 2, 6);
                            Render.Text(cdString, pos,
                                RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                        }
                    }
                }

                else
                {
                    var bpos = this.Unit.FloatingHealthBarPosition + new Vector2(33, 0);

                    if (this.HUD != null)
                    {
                        this.HUD.Draw(bpos);
                    }

                    var nSpellStart = bpos + new Vector2(2.5f, 19.5f);
                    for (int i = 0; i < this.Spells.Count; i++)
                    {
                        var sp = this.Spells[i];
                        var position = nSpellStart + new Vector2(i * 27, 0);
                        Render.Rectangle(position, sp.CurrentWidth, sp.statusBoxHeight, sp.CurrentColor);

                        if (sp.Spell.State.HasFlag(SpellState.Cooldown))
                        {
                            string cdString = sp.TimeUntilReady < 1
                                ? sp.TimeUntilReady.ToString("0.0")
                                : Math.Ceiling(sp.TimeUntilReady).ToString();

                            var pos = position + new Vector2((int)(sp.statusBoxWidth / 2) - Render.MeasureText(cdString) / 2, 6);
                            Render.Text(cdString, pos,
                                RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                        }
                    }


                    var summonerStart = bpos + new Vector2(110f, 2f);

                    var s1Bar = summonerStart + new Vector2(0.5f, 17.5f);
                    if (this.Summoner1 != null && this.Summoner1.SpellTexture != null)
                    {
                        this.Summoner1.SpellTexture.Draw(summonerStart);
                        Render.Rectangle(s1Bar, this.Summoner1.CurrentWidth, this.Summoner1.statusBoxHeight,
                            this.Summoner1.CurrentColor);

                        if (this.Summoner1.Spell.State.HasFlag(SpellState.Cooldown) && this.Summoner1.TimeUntilReady > 0.0f)
                        {
                            string cdString = this.Summoner1.TimeUntilReady < 1
                                ? this.Summoner1.TimeUntilReady.ToString("0.0")
                                : Math.Ceiling(this.Summoner1.TimeUntilReady).ToString();

                            var tpos = s1Bar + new Vector2((int)(this.Summoner1.statusBoxWidth / 2) - Render.MeasureText(cdString) / 2, 6);
                            Render.Text(cdString, tpos, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                        }
                    }

                    var s2Bar = s1Bar + new Vector2(17, 0);
                    if (this.Summoner2 != null && this.Summoner2.SpellTexture != null)
                    {
                        var s2Position = summonerStart + new Vector2(18, 0);
                        this.Summoner2.SpellTexture.Draw(s2Position);
                        Render.Rectangle(s2Bar, this.Summoner2.CurrentWidth, this.Summoner2.statusBoxHeight,
                            this.Summoner2.CurrentColor);


                        if (this.Summoner2.Spell.State.HasFlag(SpellState.Cooldown) && this.Summoner2.TimeUntilReady > 0.0f)
                        {
                            string cdString = this.Summoner1.TimeUntilReady < 1
                                ? this.Summoner2.TimeUntilReady.ToString("0.0")
                                : Math.Ceiling(this.Summoner2.TimeUntilReady).ToString();

                            var tpos = s2Bar + new Vector2((int)(this.Summoner2.statusBoxWidth / 2), 6);
                            Render.Text(cdString, tpos, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                        }
                    }
                }
            }
        }

        class TrackingSpell
        {
            public Aimtec.Spell Spell { get; set; }

            public bool IsSummoner { get; set; }

            public int statusBoxWidth { get; set; }
            public int statusBoxHeight { get; set; }

            public float TimeUntilReady => this.Spell.CooldownEnd - Game.ClockTime;

            public int CurrentWidth
            {
                get
                {
                    if (this.TimeUntilReady <= 0)
                    {
                        return statusBoxWidth;
                    }

                    else
                    {
                        var timeLeft = this.TimeUntilReady;

                        var percent = timeLeft / this.Spell.Cooldown;

                        var width = percent * this.statusBoxWidth;

                        return (int) width;
                    }
                }
            }

            public Color CurrentColor
            {
                get
                {
                    var state = this.Spell.State;

                    if (state.HasFlag(SpellState.Unknown) ||
                        state.HasFlag(SpellState.NotLearned) || state.HasFlag(SpellState.Disabled))
                    {
                        return Color.Gray;
                    }

                    if (this.TimeUntilReady <= 0)
                    {
                        return Color.Green;
                    }

                    else
                    {
                        return Color.OrangeRed;
                    }
                }
            }

            public TrackingSpell(Aimtec.Spell spell, bool summ = false)
            {
                this.Spell = spell;
                this.IsSummoner = summ;

                if (this.IsSummoner)
                {
                    this.LoadTexture();
                    this.statusBoxHeight = 3;
                    this.statusBoxWidth = 14;
                }

                else
                {
                    this.statusBoxHeight = 3;
                    this.statusBoxWidth = 23;
                }
            }

            internal void LoadTexture()
            {
                var name = this.Spell.Name;
                if (this.Spell.Name.ToLower().Contains("smite"))
                {
                    name = "SummonerSmite";
                }

                Bitmap bmp = null;
                try
                {
                     bmp = Utility.ResizeImage((Bitmap) Resources.ResourceManager.GetObject(name),
                        new Size(15, 15));
                }

                catch (Exception e)
                {
                    Console.WriteLine($"Texture not found for {this.Spell.Name} :(");
                    bmp = Utility.ResizeImage((Bitmap)Resources.ResourceManager.GetObject("SummonerDot"),
                        new Size(15, 15));
                }

                if (bmp != null)
                {
                    this.SpellTexture = new Texture(bmp);
                }
            }

            internal Texture SpellTexture { get; set; }
        }

        public void Menu(Menu root)
        {
            this.Config = new Menu("OVHTracker", "Overhead Tracker");
            this.Config.Add(new MenuBool("TrackAllies", "Track Allies"));
            this.Config.Add(new MenuBool("TrackEnemies", "Track Enemies"));
            this.Config.Add(new MenuBool("TrackMe", "Track Me"));
            root.Add(this.Config);
        }

        public void Load(Menu rootMenu)
        {
            this.Menu(rootMenu);

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsAlly && !this.Config["TrackAllies"].Enabled)
                {
                    continue;
                }

                if (hero.IsMe && !this.Config["TrackMe"].Enabled)
                {
                    continue;
                }

                var tracker = new FloatingTracker(hero);
                this.Trackers.Add(tracker);
            }

            Render.OnPresent += Render_OnPresent;
        }

        public void Unload()
        {
            Render.OnPresent -= Render_OnPresent;
            this.Config.Dispose();
            this.Trackers = null;
        }
    }
}
