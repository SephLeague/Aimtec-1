using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using System.Drawing;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Newtonsoft.Json;

namespace Ewareness
{
    class OHTracker : IAwarenessModule
    {
        public OHTracker(Menu root)
        {
            this.Menu(root);
            Render.OnPresent += Render_OnPresent;
        }

        public List<FloatingTracker> FloatingTrackers = new List<FloatingTracker>();
        public Menu Config { get; set; }

        public void Load()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!this.Config["TrackMe"].Enabled && hero.IsMe)
                {
                    continue;
                }

                if (!this.Config["TrackAllies"].Enabled && hero.IsAlly)
                {
                    continue;
                }

                var floatingTracker = new FloatingTracker(hero);
                FloatingTrackers.Add(floatingTracker);
            }
        }

        public void Menu(Menu root)
        {
            this.Config = new Menu("OVHTracker", "Overhead Tracker");
            this.Config.Add(new MenuBool("TrackAllies", "Track Allies"));
            this.Config.Add(new MenuBool("TrackMe", "Track Me"));

            root.Add(this.Config);
        }


        public void Unload()
        {
            throw new NotImplementedException();
        }

    
        private void Render_OnPresent()
        {
            foreach (var tracker in this.FloatingTrackers)
            {
                if (!tracker.Unit.IsValid || !tracker.Unit.IsVisible || tracker.Unit.IsDead || tracker.Unit.FloatingHealthBarPosition.IsZero)
                {
                    continue;
                }

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

        public class FloatingTracker
        {
            public FloatingTracker(Obj_AI_Hero unit)
            {
                this.Unit = unit;
                this.FindSpells();
            }

            public void FindSpells()
            {
                //Summoners
                foreach (var spell in this.Unit.SpellBook.Spells)
                {
                    if (spell.Slot == SpellSlot.Summoner1 || spell.Slot == SpellSlot.Summoner2)
                    {
                        var summSpell = new SpellObject(spell, null, 25, 25);
                        this.Summoners.Add(summSpell);
                    }
                }
                
                var herodata = Utility.GetChampionData(Unit.ChampionName);

                if (herodata == null)
                {
                    Console.WriteLine("Data not found");
                    return;
                }

                var spells = herodata.Spells;

                for (int i = 0; i < spells.Length; i++)
                {
                    var jsp = spells[i];
                    Aimtec.Spell spell = null;

                    if (i == 0)
                    {
                        spell = this.Unit.GetSpell(SpellSlot.Q);
                    }

                    else if (i == 1)
                    {
                        spell = this.Unit.GetSpell(SpellSlot.W);
                    }

                    else if (i == 2)
                    {
                        spell = this.Unit.GetSpell(SpellSlot.E);
                    }

                    else if (i == 3)
                    {
                        spell = this.Unit.GetSpell(SpellSlot.R);
                    }

                    if (spell != null)
                    {
                        var normalSpell = new SpellObject(spell, jsp, 25, 25);
                        this.Spells.Add(normalSpell);
                    }
                }
     
            }

            public List<SpellObject> Summoners { get; set; } = new List<SpellObject>();

            public List<SpellObject> Spells { get; set; } = new List<SpellObject>();

            public Obj_AI_Hero Unit { get; set; }

            public Vector2 BasePosition => Unit.FloatingHealthBarPosition;

            public void DrawSummoners()
            {
                for (int i = 0; i < Summoners.Count; i++)
                {
                    var spell = Summoners[i];

                    if (spell != null)
                    {
                        var startPosition = this.BasePosition + new Vector2(-20, -25);

                        var offset = startPosition + new Vector2(0, 30 * i);

                        spell.Draw(offset);
                    }
                }
            }

            public void DrawRegularSpells()
            {
                for (int i = 0; i < Spells.Count; i++)
                {
                    var spellobj = Spells[i];
                    var startSpellPos = this.BasePosition + new Vector2(10, -25);
                    var pos = startSpellPos + new Vector2(i * 33, 0);
                    spellobj.Draw(pos);
                }
            }

            public void Draw()
            {
                DrawSummoners();
                DrawRegularSpells();
            }
        }

        public class SpellObject
        {
            public SpellObject(Aimtec.Spell spell, Spell dData, int x, int y)
            {
                this.Spell = spell;
                this.Width = x;
                this.Height = y;
                this.ParsedData = dData;
                this.LoadTexture();
            }

            public void LoadTexture()
            {
                string name = "";

                if (this.IsSummoner)
                {
                    if (this.Spell.Name.ToLower().Contains("smite"))
                    {
                        name = "SummonerSmite";
                    }

                    else
                    {
                        name = this.Spell.Name;
                    }
                }

                else if (this.ParsedData != null)
                {
                    var fullname = this.ParsedData.Image.Full;
                    name = fullname.Remove(fullname.Length - 4, 4); // remove the extension 
                }

                var bitmap = Utility.GetSkillBitMap(name);
                if (bitmap != null)
                {
                    var resized = Utility.ResizeImage(bitmap,
                        new Size(this.Width, this.Height));
                    this.SpellTexture = new Texture(resized);
                }

                else
                {
                    Console.WriteLine($"Could not find BitMap for {name}");
                }
            }

            public bool IsSummoner => this.Spell.Slot == SpellSlot.Summoner1 || this.Spell.Slot == SpellSlot.Summoner2;

            public int Width { get; set; }

            public int Height { get; set; }

            public Texture SpellTexture { get; set; }

            public Aimtec.Spell Spell { get; set; }

            public Spell ParsedData { get; set; }

            public void Draw(Vector2 pos)
            {
                var color = this.Ready ? Color.Green : Color.Red;

                if (this.SpellTexture != null)
                {
                    this.SpellTexture.Draw(pos);
                }

                Utility.DrawRectangleOutline(pos, this.Width, this.Height, 3, color);

                var spellRect = new Aimtec.Rectangle((int) pos.X, (int) pos.Y, (int) pos.X + 25, (int) pos.Y + 25);

                var state = this.Spell.State;

                if (state.HasFlag(SpellState.Cooldown) || state.HasFlag(SpellState.Unknown) ||
                    state.HasFlag(SpellState.NotLearned) || state.HasFlag(SpellState.Disabled))
                {
                    Render.Rectangle(pos, 25, 25, Color.FromArgb(150, 0, 0, 0));

                    if (state.HasFlag(SpellState.Cooldown))
                    {
                        string cdString = this.CooldownTime < 1
                            ? this.CooldownTime.ToString("0.0")
                            : Math.Ceiling(this.CooldownTime).ToString();
                        Render.Text(cdString, spellRect,
                            RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                    }
                }
            }


            public bool Ready => this.CooldownTime <= 0.0f;

            public float CooldownTime => this.Spell.CooldownEnd - Game.ClockTime;
        }
    }
}
