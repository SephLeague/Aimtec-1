using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using System.Drawing;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;

namespace EUtility
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
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy || this.Config["DrawAllies"].Enabled))
            {
                var floatingTracker = new FloatingTracker(hero);
                FloatingTrackers.Add(floatingTracker);
            }
        }

        public void Menu(Menu root)
        {
            this.Config = new Menu("OVHTracker", "Overhead Tracker");
            this.Config.Add(new MenuBool("DrawAllies", "Track Allies"));

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

                if (tracker.Unit.IsAlly && !this.Config["DrawAllies"].Enabled)
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
                foreach (var spell in this.Unit.SpellBook.Spells)
                {
                    if (spell.Slot == SpellSlot.Summoner1 || spell.Slot == SpellSlot.Summoner2)
                    {
                        var summSpell = new SpellObject(spell, 32, 32);
                        this.Summoners.Add(summSpell);
                    }

                    else if (spell.Slot == SpellSlot.Q || spell.Slot == SpellSlot.W || spell.Slot == SpellSlot.E ||
                        spell.Slot == SpellSlot.R)
                    {
                        var normalSpell = new SpellObject(spell, 32, 32);
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
                for (int i = 0; i <= 1; i++)
                {
                    var spell = Summoners[i];
                    {
                        var startPosition = this.Unit.FloatingHealthBarPosition + new Vector2(-25, -30);

                        var offset = startPosition + new Vector2(0, 35 * i);

                        spell.Draw(offset);
                    }
                }
            }

            public void DrawRegularSpells()
            {
                for (int i = 0; i < Spells.Count; i++)
                {
                    var spellobj = Spells[i];
                    var startSpellPos = this.BasePosition + new Vector2(10, -30);
                    var pos = startSpellPos + new Vector2(i * 35, 0);
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
            public SpellObject(Aimtec.Spell spell, int x, int y)
            {
                this.Spell = spell;
                this.Width = x;
                this.Height = y;
                this.LoadTexture();
            }

            public void LoadTexture()
            {
                var bitmap = Utility.GetBitMap(this.Spell.Name);
                if (bitmap != null)
                {
                    var resized = Utility.ResizeImage(Utility.GetBitMap(this.Spell.Name),
                        new Size(this.Width, this.Height));
                    this.SpellTexture = new Texture(resized);
                }

                else
                {
                    Console.WriteLine($"Could not find BitMap for {Spell.Name}");
                }
            }

            public int Width { get; set; }

            public int Height { get; set; }
            public Texture SpellTexture { get; set; }

            public Spell Spell { get; set; }

            public void Draw(Vector2 pos)
            {
                if (this.SpellTexture == null)
                {
                    return;
                }

                var color = this.Ready ? Color.Green : Color.Red;

                this.SpellTexture.Draw(pos);

                Utility.DrawRectangleOutline(pos, this.Width, this.Height, 3, color);

                var spellRect = new Aimtec.Rectangle((int)pos.X, (int)pos.Y, (int)pos.X + 32, (int)pos.Y + 32);

                if (!this.Ready)
                {
                    Render.Rectangle(pos, 32, 32, Color.FromArgb(150, 0, 0, 0));
                    Render.Text(this.CooldownTime.ToString("0.0"), spellRect,
                        RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                }
            }

            public bool Ready => this.CooldownTime <= 0.0f;

            public float CooldownTime => this.Spell.CooldownEnd - Game.ClockTime;
        }
    }
}
