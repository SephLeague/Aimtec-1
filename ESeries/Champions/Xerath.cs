using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Events;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Menu.Config;
using Aimtec.SDK.Prediction.Skillshots;
using Aimtec.SDK.TargetSelector;
using Aimtec.SDK.Util;
using Aimtec.SDK.Util.Cache;
using ESeries.Abstractions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ESeries.Champions
{
    class Xerath : AChampion
    {
        private MenuKeyBind TapKey { get; set; }
        private UltTracker UltimateTracker { get; set; } = new UltTracker();

        public Xerath()
        {
            Console.WriteLine("E::Xerath Loaded.");

            Q = new Aimtec.SDK.Spell(SpellSlot.Q, 1550);
            W = new Aimtec.SDK.Spell(SpellSlot.W, 1100);
            E = new Aimtec.SDK.Spell(SpellSlot.E, 1050);
            R = new Aimtec.SDK.Spell(SpellSlot.R, 2500);

            Q.SetSkillshot(0.6f, 95f, float.MaxValue, false, Aimtec.SDK.Prediction.Skillshots.SkillshotType.Line);
            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 3.0f);

            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, Aimtec.SDK.Prediction.Skillshots.SkillshotType.Circle, false, HitChance.Medium);
            E.SetSkillshot(0.25f, 60f, 1400f, true, Aimtec.SDK.Prediction.Skillshots.SkillshotType.Line, false, HitChance.Medium);
            R.SetSkillshot(0.7f, 130f, float.MaxValue, false, Aimtec.SDK.Prediction.Skillshots.SkillshotType.Circle, false, HitChance.Medium);

            this.Menu();

            Game.OnUpdate += this.Game_OnUpdate;
            Orbwalker.PreAttack += this.Orbwalker_PreAttack;
            Orbwalker.PreMove += this.Orbwalker_PreMove;
            Render.OnPresent += this.OnPresent;
            Dash.HeroDashed += Dash_HeroDashed;
            this.Attach();
        }

        private void Dash_HeroDashed(object sender, Dash.DashArgs e)
        {
            if (this.Config["Misc"]["AntiDash"].Enabled)
            {
                if (e.Unit.IsValidTarget())
                {
                    if (e.EndPos.Distance(Player) <= E.Range)
                    {
                        var pred = E.GetPrediction(e.Unit);
                        if (pred != null && pred.HitChance >= E.HitChance)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }
                }
            }
        }

        private void Orbwalker_PreMove(object sender, Aimtec.SDK.Orbwalking.PreMoveEventArgs e)
        {
            if (this.UltimateTracker.CastingUltimate)
            {
                e.Cancel = true;
            }
        }

        private void Orbwalker_PreAttack(object sender, Aimtec.SDK.Orbwalking.PreAttackEventArgs e)
        {
            if (this.UltimateTracker.CastingUltimate)
            {
                e.Cancel = true;
            }
        }

        private void Game_OnUpdate()
        {
            if (CurrentRMode == RMode.Autocast && this.UltimateTracker.CastingUltimate && R.Ready)
            {
                CastR(RMode.Autocast);
            }

            if (GlobalKeys.ComboKey.Active)
            {
                this.Combo();
            }

            else if (GlobalKeys.HarassKey.Active)
            {
                this.Harass();
            }

            else if (GlobalKeys.WaveClearKey.Active)
            {
                this.Laneclear();
            }
        }

        private void Menu()
        {
            var combo = new Menu("Combo", "Combo")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("W", "W"),
                new MenuBool("E", "E"),
            };

            var harass = new Menu("Harass", "Harass")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("W", "W"),
                new MenuBool("E", "E"),
            };

            var killsteal = new Menu("Killsteal", "Killsteal")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("W", "W"),
                new MenuBool("E", "E"),
                new MenuBool("R", "R"),
            };

            var laneclear = new Menu("Laneclear", "Laneclear")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("W", "W"),
                new MenuBool("E", "E"),
            };

          
            var rmenu = new Menu("R", "R")
            {
                new MenuList("RMode", "R Mode", new string[] { "On Tap", "Autocast" }, 0),
                new MenuList("RTargetSelectMode", "Target Mode", new string[] { "Target Selector", "Closest to Mouse", "Least cast", "Auto" },  3),
                new MenuBool("UseDelays", "Use Delays", false),
                new MenuSlider("Delay0", "Delay 1", 0, 0, 3000),
                new MenuSlider("Delay1", "Delay 2", 0, 0, 3000),
                new MenuSlider("Delay2", "Delay 3", 0, 0, 3000),
                new MenuSlider("Delay3", "Delay 4", 0, 0, 3000),
                new MenuSlider("Delay4", "Delay 5", 0, 0, 3000),
            };


            var misc = new Menu("Misc", "Misc")
            {
                new MenuBool("AntiDash", "Anti Dash"),
            };

            TapKey = new MenuKeyBind("TapKey", "Tap Key", Aimtec.SDK.Util.KeyCode.T, KeybindType.Press);

            rmenu.Add(TapKey);

            TapKey.OnValueChanged += TapKey_ValueChanged;


            var hcMenu = new Menu("Hitchances", "Hitchances")
            {
                new MenuList("hcQ", "Q", new string [] { "Low", "Medium", "High", "VeryHigh", "Dashing", "Immobile" }, 1),
                new MenuList("hcW", "W", new string [] { "Low", "Medium", "High", "VeryHigh", "Dashing", "Immobile" }, 1),
                new MenuList("hcE", "E", new string [] { "Low", "Medium", "High", "VeryHigh", "Dashing", "Immobile" }, 1),
                new MenuList("hcR", "R", new string [] { "Low", "Medium", "High", "VeryHigh", "Dashing", "Immobile" }, 1),
            };

            hcMenu.OnValueChanged += HcMenu_ValueChanged;


            var drawmenu = new Menu("Drawings", "Drawings")
            {
                new MenuBool("DrawQ", "Draw Q", false),
                new MenuBool("DrawW", "Draw W", false),
                new MenuBool("DrawE", "Draw E", false),
                new MenuBool("DrawR", "Draw R", false),
            };

            this.Config.Add(combo);
            this.Config.Add(harass);
            this.Config.Add(killsteal);
            this.Config.Add(laneclear);
            this.Config.Add(rmenu);
            this.Config.Add(hcMenu);
            this.Config.Add(drawmenu);

        }

        private void HcMenu_ValueChanged(MenuComponent sender, ValueChangedArgs args)
        {
            if (args.InternalName == "hcQ")
            {
                this.Q.HitChance = (HitChance) args.GetNewValue<MenuList>().Value + 3;
            }

            if (args.InternalName == "hcW")
            {
                this.W.HitChance = (HitChance)args.GetNewValue<MenuList>().Value + 3;
            }

            if (args.InternalName == "hcE")
            {
                this.E.HitChance = (HitChance)args.GetNewValue<MenuList>().Value + 3;
            }
            
            if (args.InternalName == "hcR")
            {
                this.R.HitChance = (HitChance)args.GetNewValue<MenuList>().Value + 3;
            }
        }



        enum RMode
        {
            OnTap = 0,
            Autocast = 1,
        }

        enum RTSMode
        {
            TS = 0,
            ClosestMouse = 1,
            EasiestToKill = 2,
            Auto = 3,
        }

        private RMode CurrentRMode => (RMode)this.Config["R"]["RMode"].Value;



        private float UltRange
        {
            get { return new float[] { 3520, 4840, 6160 }[Player.SpellBook.GetSpell(SpellSlot.R).Level - 1]; }
        }

        private void TapKey_ValueChanged(MenuComponent sender, ValueChangedArgs args)
        {
            if (CurrentRMode == RMode.OnTap && this.UltimateTracker.CastingUltimate && R.Ready)
            {
                if (args.GetNewValue<MenuKeyBind>().Value)
                {
                    CastR(RMode.OnTap);
                }
            }
        }

        private void CastR(RMode mode)
        {
            R.Range = UltRange;

            var targetSelectionMode = (RTSMode)this.Config["R"]["RTargetSelectMode"].Value;

            Obj_AI_Hero target = null;

            if (targetSelectionMode == RTSMode.ClosestMouse)
            {
                target = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(UltRange)).OrderBy(x => x.Distance(Game.CursorPos)).FirstOrDefault();
            }

            else if (targetSelectionMode == RTSMode.EasiestToKill)
            {
                target = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(UltRange)).OrderBy(x => x.Health / Player.GetSpellDamage(x, SpellSlot.R)).FirstOrDefault();
            }

            else if (targetSelectionMode == RTSMode.TS)
            {
                target = TargetSelector.GetTarget(UltRange);
            }

            else if (targetSelectionMode == RTSMode.Auto)
            {
                target = TargetSelector.Implementation.GetOrderedTargets(UltRange).OrderBy(x => x.Health / Player.GetSpellDamage(x, SpellSlot.R)).ThenBy(x => x.Distance(Game.CursorPos)).FirstOrDefault();
            }

            //No suitable target found
            if (target == null)
            {
                return;
            }

            if (mode == RMode.Autocast)
            {
                if (Config["R"]["UseDelays"].Enabled)
                {
                    var delay = Config["R"]["Delay" + UltimateTracker.ShotNumber].Value;
                    if (Game.TickCount - UltimateTracker.LastChargeCastTime < delay)
                    {
                        return;
                    } 
                }

                R.Cast(target);
            }

            else if (mode == RMode.OnTap)
            {
                R.Cast(target);
            }
        }

        private void Combo()
        {
            if (this.Config["Combo"]["E"].Enabled)
            {
                var etarget = TargetSelector.GetTarget(E.Range);
                if (etarget != null)
                {
                    E.Cast(etarget);
                }
            }

            if (this.Config["Combo"]["Q"].Enabled)
            {
                var qtarget = TargetSelector.GetTarget(Q.ChargedMaxRange);

                if (qtarget != null)
                {
                    Q.Cast(qtarget);
                }
            }

            if (this.Config["Combo"]["W"].Enabled)
            {
                var wtarget = TargetSelector.GetTarget(W.Range);
                if (wtarget != null)
                {
                    W.Cast(wtarget);
                }
            }
        }

        private void Harass()
        {
            if (this.Config["Harass"]["E"].Enabled)
            {
                var etarget = TargetSelector.GetTarget(E.Range);
                if (etarget != null)
                {
                    E.Cast(etarget);
                }
            }

            if (this.Config["Harass"]["Q"].Enabled)
            {
                var qtarget = TargetSelector.GetTarget(Q.ChargedMaxRange);

                if (qtarget != null)
                {
                    Q.Cast(qtarget);
                }
            }

            if (this.Config["Harass"]["W"].Enabled)
            {
                var wtarget = TargetSelector.GetTarget(W.Range);
                if (wtarget != null)
                {
                    W.Cast(wtarget);
                }
            }
        }

        private void Killsteal()
        {
            if (this.Config["Killsteal"]["E"].Enabled)
            {
                var target = TargetSelector.Implementation.GetOrderedTargets(E.Range).Where(x => x.Health < Player.GetSpellDamage(x, SpellSlot.E)).FirstOrDefault();
                if (target != null)
                {
                    E.Cast(target);
                }
            }

            if (this.Config["Killsteal"]["Q"].Enabled)
            {
                var target = TargetSelector.Implementation.GetOrderedTargets(E.Range).Where(x => x.Health < Player.GetSpellDamage(x, SpellSlot.Q)).FirstOrDefault();
                if (target != null)
                {
                    Q.Cast(target);
                }
            }

            if (this.Config["Killsteal"]["W"].Enabled)
            {
                var target = TargetSelector.Implementation.GetOrderedTargets(W.Range).Where(x => x.Health < Player.GetSpellDamage(x, SpellSlot.W)).FirstOrDefault();
                if (target != null)
                {
                    W.Cast(target);
                }
            }

            if (this.Config["Killsteal"]["R"].Enabled)
            {
                if (this.UltimateTracker.CastingUltimate)
                {
                    var target = TargetSelector.Implementation.GetOrderedTargets(R.ChargedMaxRange).Where(x => x.Health < Player.GetSpellDamage(x, SpellSlot.R)).FirstOrDefault();
                    if (target != null)
                    {
                        R.Cast(target);
                    }
                }
            }
        }

        private void Laneclear()
        {
            if (this.Config["Laneclear"]["W"].Enabled)
            {
                var result = GetCircularClearLocation(W.Range, W.Width, 2);
                if (result != null && result.numberOfMinionsHit >= 2)
                {
                    W.Cast(result.CastPosition);
                }
            }

            if (this.Config["Laneclear"]["Q"].Enabled)
            {
                var result = GetLineClearLocation(Q.ChargedMaxRange, Q.Width);
                if (result != null)
                {
                    if (result.numberOfMinionsHit >= 2)
                    {
                        Q.Cast(result.CastPosition);
                    }


                    else if (Q.IsCharging && result.numberOfMinionsHit >= 1 && Q.ChargePercent > 80)
                    {
                        Q.Cast(result.CastPosition);
                    }
                }
            }
        }

        private void Lasthit()
        {
            if (this.Config["Combo"]["UseQ"].Enabled)
            {

            }

            if (this.Config["Combo"]["UseQ"].Enabled)
            {

            }

            if (this.Config["Combo"]["UseQ"].Enabled)
            {

            }
        }


        private void OnPresent()
        {
            if (this.Config["Drawings"]["DrawQ"].Enabled)
            {
                Render.Circle(Player.Position, Q.ChargedMaxRange, 36, System.Drawing.Color.White);
            }

            if (this.Config["Drawings"]["DrawW"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 36, System.Drawing.Color.Violet);
            }

            if (this.Config["Drawings"]["DrawE"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 36, System.Drawing.Color.SteelBlue);
            }

            if (this.Config["Drawings"]["DrawR"].Enabled)
            {
                Render.Circle(Player.Position, UltRange, 36, System.Drawing.Color.Silver);
            }
        }

        public class LaneclearResult
        {
            public LaneclearResult(int hit, Vector3 cp)
            {
                this.numberOfMinionsHit = hit;
                this.CastPosition = cp;
            }

            public int numberOfMinionsHit = 0;
            public Vector3 CastPosition;
        }

        public LaneclearResult GetCircularClearLocation(float range, float width, int minHit)
        {
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidSpellTarget(range));
            var positions = minions.Select(x => x.ServerPosition.To2D()).ToList();

            if (positions.Any())
            {
                return new LaneclearResult(1, positions.FirstOrDefault().To3D());
            }

            var positionCount = positions.Count;

            var lcount = Math.Max(positionCount, 4);

            if (positions.Count >= minHit)
            {
                Vector2 center;
                float radius;

                Mec.FindMinimalBoundingCircle(positions, out center, out radius);

                HashSet<LaneclearResult> results = new HashSet<LaneclearResult>();

                var hitMinions = minions.Where(x => x.Distance(center) <= 0.95f * radius);

                var count = hitMinions.Count();

                var result = new LaneclearResult(count, center.To3D());

                results.Add(result);

                for (int i = 0; i < lcount; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var positions2 = new Vector2[] { positions[i], positions[j] };

                        Mec.FindMinimalBoundingCircle(positions, out center, out radius);

                        hitMinions = minions.Where(x => x.Distance(center) <= 0.9f * radius);

                        count = hitMinions.Count();

                        if (count >= minHit)
                        {
                            results.Add(new LaneclearResult(count, center.To3D()));
                        }
                    }
                }

                return results.MaxBy(x => x.numberOfMinionsHit);
            }

            return null;
        }

        public LaneclearResult GetLineClearLocation(float range, float width)
        {
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidSpellTarget(range));

            var positions = minions.Select(x => x.ServerPosition).ToList();

            var locations = new List<Vector3>();

            locations.AddRange(positions);

            var max = positions.Count();

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (positions[j] != positions[i])
                    {
                        locations.Add((positions[j] + positions[i]) / 2);
                    }
                }
            }


            HashSet<LaneclearResult> results = new HashSet<LaneclearResult>();

            foreach (var p in locations)
            {
                var rect = new Rectangle(Player.Position, p, width);

                var count = 0;

                foreach (var m in minions)
                {
                    if (rect.Contains(m.Position))
                    {
                        count++;
                    }
                }

                results.Add(new LaneclearResult(count, p));
            }

            var maxhit = results.MaxBy(x => x.numberOfMinionsHit);

            return maxhit;
        }


        class UltTracker
        {
            public UltTracker()
            {
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
               // SpellBook.OnCastSpell += SpellBook_OnCastSpell;
            }

            public bool CastingUltimate => ObjectManager.GetLocalPlayer().HasBuff("XerathLocusOfPower2");

            public float UltStartTime { get; set; }

            public float LastChargeCastTime { get; set; }

            public int ShotNumber { get; set; }

            public int TotalBarragesAvailable => new int[] { 3, 4, 5 }[ObjectManager.GetLocalPlayer().SpellBook.GetSpell(SpellSlot.R).Level - 1];

            /*
            private void SpellBook_OnCastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs e)
            {
                if (sender.IsMe && e.Slot == SpellSlot.R)
                {
                    if (this.CastingUltimate)
                    {
             
                    }
                }
            }
            */

            private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
            {
                if (sender != null && sender.IsValid && sender.IsMe)
                {
                    if (e.SpellData.Name == "XerathLocusOfPower2")
                    {
                        this.ShotNumber = 0;
                        this.UltStartTime = Game.TickCount;
                        this.LastChargeCastTime = 0;
                    }

                    if (e.SpellData.Name == "XerathLocusPulse")
                    {
                        this.ShotNumber++;
                        this.LastChargeCastTime = Game.TickCount;
                    }
                }
            }
        }
    }
}
