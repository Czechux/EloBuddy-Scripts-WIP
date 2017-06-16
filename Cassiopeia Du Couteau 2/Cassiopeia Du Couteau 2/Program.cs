using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using static SharpDX.Color;

namespace Cassiopeia_Du_Couteau_2
{
    static class Program
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        //tutaj spelle
        public static Spell.Skillshot _Q;
        public static Spell.Skillshot _W;
        public static Spell.Targeted _E;
        public static Spell.Skillshot _R;

        private static Menu StartMenu, ComboMenu, LastHitM, DebugC, DrawingsMenu, ClearMenu;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Cassiopeia"))
            {
                return;
            }
            Chat.Print("Cassiopeia Du Couteau - Loaded", System.Drawing.Color.Crimson);
            _Q = new Spell.Skillshot(SpellSlot.Q, 750, SkillShotType.Circular, 400, int.MaxValue, 130);
            _W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular, 250, 250, 160);
            _E = new Spell.Targeted(SpellSlot.E, 700);
            _R = new Spell.Skillshot(SpellSlot.R, 800, SkillShotType.Cone, 250, 250, 80);
            StartMenu = MainMenu.AddMenu("Cassio", "Cassio");
            ComboMenu = StartMenu.AddSubMenu("Combo Settings", "Combo Settings");
            DrawingsMenu = StartMenu.AddSubMenu("Draw", "Draw");
            DebugC = StartMenu.AddSubMenu("Debyug", "Debyug");
            ComboMenu.AddGroupLabel("Cassiopeia Du Couteau ");
            ComboMenu.Add("PredHit", new ComboBox("Prediction Hitchance", 0, "High", "Medium", "Low"));
            ComboMenu.Add("DrawStatus", new CheckBox("Draw Current Orbwalker mode ? [BETA]"));
            ComboMenu.AddLabel("If you wanna use drawing orbwalker mode you need 16:9 resoultion,");
            ComboMenu.AddLabel("In future i will add customizable position.");
            ComboMenu.Add("DisableAA", new CheckBox("Disable AA in Combo for faster Kite", false));
            ComboMenu.AddLabel("Q Spell Settings");
            ComboMenu.Add("UseQ", new CheckBox("Use [Q]"));
            ComboMenu.Add("UseS", new CheckBox("Use [Q] Mana Saver?", false));
            ComboMenu.Add("UseQI", new CheckBox("Use always [Q] if enemy is immobile?"));
            ComboMenu.Add("UseQ2", new CheckBox("Try to hit =< 2 champions if can ?"));
            ComboMenu.Add("UseQPok", new CheckBox("Use always [Q] if enemy is killable by Poison?"));


            ComboMenu.AddLabel("W Spell Settings");
            ComboMenu.Add("UseW", new CheckBox("Use [W]"));
            ComboMenu.Add("UseW2", new CheckBox("Try to hit =< 2 champions if can ?"));

            ComboMenu.Add("UseE", new CheckBox("Use [E]"));
            ComboMenu.Add("UseES", new CheckBox("Use [E] casting speedup ?"));
            ComboMenu.Add("UseEI", new CheckBox("Use [E] always if enemy is immobile?", false));

            ComboMenu.Add("UseR", new CheckBox("Use [R]"));
            ComboMenu.Add("UseRFace", new CheckBox("Use [R] only on facing enemy ?"));
            ComboMenu.Add("UseRG", new CheckBox("Use [R] use minimum enemys for R ?"));
            ComboMenu.Add("UseRGs", new Slider("Minimum people for R", 1, 1, 5));

            DrawingsMenu.AddGroupLabel("Drawing Settings");
            DrawingsMenu.AddLabel("Tick for enable/disable spell drawings");
            DrawingsMenu.Add("DQ", new CheckBox("- Draw [Q] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DW", new CheckBox("- Draw [W] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DE", new CheckBox("- Draw [E] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DR", new CheckBox("- Draw [R] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DI", new CheckBox("- Draw Is Killable"));

            DebugC.Add("Debug", new CheckBox("Debug Console+Chat"));
            DebugC.Add("DrawStatus1", new CheckBox("Debug Curret Orbawlker mode"));


            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }
        public static bool PoisonWillExpire(this Obj_AI_Base target, float time)
        {
            var buff = target.Buffs.OrderByDescending(x => x.EndTime).FirstOrDefault(x => x.Type == BuffType.Poison && x.IsActive && x.IsValid);
            return buff == null || time > (buff.EndTime - Game.Time) * 1000f;
        }


        private static void Game_OnUpdate(EventArgs args)
        {

            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);

            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();

            }
            ImmobileQ();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var DM = DrawingsMenu["DM"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Physical);
            var Combo = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo);
            var LastHit = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit);
            var LaneClear = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear);
            var Harass = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass);



            if (DebugC["DrawStatus1"].Cast<CheckBox>().CurrentValue && ComboMenu["DrawStatus"].Cast<CheckBox>().CurrentValue)

            {

                if (Harass && !Combo && LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (Harass && Combo && !LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (Harass && Combo && LaneClear && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (Harass && Combo && LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.95f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (Harass && !Combo && !LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (Harass && Combo && !LaneClear && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                }

                if (Harass && LaneClear && !Combo && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (Harass && !LaneClear && !Combo && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                }
                if (LaneClear && LastHit && !Combo && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (LaneClear && Combo && !LastHit && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (LaneClear && LastHit && Combo && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (LaneClear && !LastHit && !Combo && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }

                if (LastHit && Combo && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (LastHit && !Combo && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }

                if (Combo && !LastHit && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                }

                else if (!Combo && !LastHit && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: None ]");
                }
            }


            if (DrawingsMenu["DQ"].Cast<CheckBox>().CurrentValue && _Q.IsLearned)

            {
                if (!_Q.IsReady())
                    Circle.Draw(Red, _Q.Range, _Player);

                else if (_Q.IsReady())
                {
                    Circle.Draw(Cyan, _Q.Range, _Player);
                }


            }
            if (DrawingsMenu["DW"].Cast<CheckBox>().CurrentValue && _W.IsLearned)
            {
                if (!_W.IsReady())
                    Circle.Draw(Red, _W.Range, _Player);

                else if (_W.IsReady())
                {
                    Circle.Draw(Cyan, _W.Range, _Player);
                }
            }
            if (DrawingsMenu["DE"].Cast<CheckBox>().CurrentValue && _E.IsLearned)
            {
                if (!_E.IsReady())
                    Circle.Draw(Red, _E.Range, _Player);

                else if (_E.IsReady())
                {
                    Circle.Draw(Red, _E.Range, _Player);
                }
            }
            if (DrawingsMenu["DR"].Cast<CheckBox>().CurrentValue && _R.IsLearned)
            {
                if (!_R.IsReady())
                    Circle.Draw(Red, _R.Range, _Player);

                else if (_R.IsReady())
                {
                    Circle.Draw(Cyan, _R.Range, _Player);
                }
            }
        }


        private static void Combo()
        {

            var HighP = ComboMenu["PredHit"].Cast<ComboBox>().SelectedIndex == 0;
            var MediumP = ComboMenu["PredHit"].Cast<ComboBox>().SelectedIndex == 1;
            var LowP = ComboMenu["PredHit"].Cast<ComboBox>().SelectedIndex == 2;
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            var targetQ2 = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (ComboMenu["DisableAA"].Cast<CheckBox>().CurrentValue)
            {
                Orbwalker.DisableAttacking = true;
            }
            if (!ComboMenu["DisableAA"].Cast<CheckBox>().CurrentValue)
            {
                Orbwalker.DisableAttacking = false;
            }
            if (HighP)
            {
                if (ComboMenu["UseE"].Cast<CheckBox>().CurrentValue)
                {
                    if (!target.IsInRange(_Player, _E.Range))
                        return;
                    {
                        if (_E.IsReady() && ComboMenu["UseES"].Cast<CheckBox>().CurrentValue)
                        {

                            _E.Cast(target);
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                            if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                            {
                                Chat.Print("Casting E with speedup");
                                Console.WriteLine("Game.Time + Casting E with Speedup");
                            }

                        }
                        if (_E.IsReady() && !ComboMenu["UseES"].Cast<CheckBox>().CurrentValue)
                        {
                            _E.Cast(target);
                            if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                            {
                                Chat.Print("Casting E with normal");
                                Console.WriteLine(Game.Time + "Casting E with Speedup");
                            }
                        }
                    }
                }

                if (ComboMenu["UseW"].Cast<CheckBox>().CurrentValue)
                {
                    if (_W.IsReady() && _Player.Distance(target) > 500)
                    {

                        var Wpred = _W.GetPrediction(target);
                        if (Wpred.HitChance >= HitChance.High && target.IsValidTarget(_W.Range))
                        {
                            _W.Cast(target.ServerPosition);
                            if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                            {

                                Chat.Print("Casting W with HIGH pred ");
                                Console.WriteLine("Casting W with HIGH pred ");
                            }
                        }
                    }

                }

                if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)
                {
                    if (_Q.IsReady())
                    {
                        var canHitMoreThanOneTarget =
                          EntityManager.Heroes.Enemies.OrderByDescending(x => x.CountEnemyChampionsInRange(_Q.Width))
                          .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyChampionsInRange(_Q.Width) > 1);
                        if (canHitMoreThanOneTarget != null)
                        {
                            var getAllTargets = EntityManager.Heroes.Enemies.FindAll(x => x.IsValidTarget() && x.IsInRange(canHitMoreThanOneTarget, _Q.Width));
                            var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.ServerPosition) / getAllTargets.Count;
                            if (!center.IsZero)
                            {
                                var Qpred = _Q.GetPrediction(target);
                                if (Qpred.HitChance >= HitChance.High && target.IsValidTarget(_Q.Range))
                                {
                                    _Q.Cast(target);
                                    if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                    {

                                        Chat.Print("FOUND MORE THAN 2 PEOPLE FOR Q ");
                                        Console.WriteLine("FOUND MORE THAN 2 PEOPLE FOR Q");
                                    }
                                }

                            }

                        }
                    }

                }

                if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)
                {
                    if (_Q.IsReady())
                    {
                        var canHitMoreThanOneTarget =
                          EntityManager.Heroes.Enemies.OrderByDescending(x => x.CountEnemyChampionsInRange(_Q.Width))
                          .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyChampionsInRange(_Q.Width) >= 1);
                        if (canHitMoreThanOneTarget != null)
                        {
                            var getAllTargets = EntityManager.Heroes.Enemies.FindAll(x => x.IsValidTarget() && x.IsInRange(canHitMoreThanOneTarget, _Q.Width));
                            var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.ServerPosition) / getAllTargets.Count;
                            if (!center.IsZero)
                            {
                                var Qpred = _Q.GetPrediction(target);
                                if (Qpred.HitChance >= HitChance.High && target.IsValidTarget(_Q.Range))
                                {
                                    _Q.Cast(target);
                                    if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                    {

                                        Chat.Print("FOUND 1 PEOPLE FOR Q ");
                                        Console.WriteLine("FOUND 1 PEOPLE FOR Q");
                                    }
                                }

                            }
                        }

                    }
                }
                    if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue)
                    {

                        if (!target.IsInRange(_Player, _Q.Range))
                            return;
                        {
                            if (_Q.IsReady() && ComboMenu["UseS"].Cast<CheckBox>().CurrentValue)
                            {
                                var Qpred = _Q.GetPrediction(target);
                                if (Qpred.HitChance >= HitChance.High && target.IsValidTarget(_Q.Range))
                                {
                                    if (!target.PoisonWillExpire(250))
                                        return;
                                    {
                                        _Q.Cast(target.ServerPosition);
                                        if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                        {

                                            Chat.Print("Casting Q with HIGH pred saver ");
                                            Console.WriteLine("Casting Q with HIGH pred saver ");
                                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                        }
                                    }
                                }

                            }

                        }
                    }

                    if (!ComboMenu["UseS"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && !ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_Q.IsReady())
                        {

                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.HitChance >= HitChance.High && target.IsValidTarget(_Q.Range))
                            {
                                Core.DelayAction(() => _Q.Cast(target), 100);
                                if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                {

                                    Chat.Print("Casting Q with HIGH pred ");
                                    Console.WriteLine("Casting Q with HIGH pred ");
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                }
                            }
                        }



                    }

                    if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue && ComboMenu["UseRG"].Cast<CheckBox>().CurrentValue)
                    {
                    var Enemys = EntityManager.Heroes.Enemies.Where(x => x.IsInRange(_Player.Position, _R.Range - 25));

                    if (Enemys != null)
                    {
                        if (Enemys.Count() >= ComboMenu["UseRGs"].Cast<Slider>().CurrentValue && target.IsFacing(_Player) && ComboMenu["UseRFace"].Cast<CheckBox>().CurrentValue)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                        if (Enemys.Count() >= ComboMenu["UseRGs"].Cast<Slider>().CurrentValue && !ComboMenu["UseRFace"].Cast<CheckBox>().CurrentValue)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                    }

                    }

                    if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue)
                    {

                        if (_R.IsReady())
                        {
                            if (target.IsFacing(_Player) && target.IsInRange(_Player, _R.Range) && ComboMenu["UseRFace"].Cast<CheckBox>().CurrentValue)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, target);
                                _R.Cast(target.ServerPosition);
                            }
                        }
                     if (target.IsInRange(_Player, _R.Range) && !ComboMenu["UseRFace"].Cast<CheckBox>().CurrentValue)
                     {
                        Player.IssueOrder(GameObjectOrder.MoveTo, target);
                        _R.Cast(target.ServerPosition);
                     }

                    }
                
            }

            if (MediumP)
            {
                if (ComboMenu["UseE"].Cast<CheckBox>().CurrentValue)
                {
                    if (!target.IsInRange(_Player, _E.Range))
                        return;
                    {
                        if (_E.IsReady())
                        {

                            _E.Cast(target);
                            if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                            {
                                Chat.Print("Casting E with speedup");
                                Console.WriteLine("Game.Time + Casting E with Speedup");
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                            }

                        }
                    }
                }

                if (ComboMenu["UseW"].Cast<CheckBox>().CurrentValue)
                {
                    if (_W.IsReady() && _Player.Distance(target) > 500)
                    {

                        var Wpred = _W.GetPrediction(target);
                        if (Wpred.HitChance >= HitChance.Medium && target.IsValidTarget(_W.Range))
                        {
                            if (ComboMenu["UseW2"].Cast<CheckBox>().CurrentValue)
                            {
                                var Enemys = EntityManager.Heroes.Enemies.Where(x => x.IsInRange(_Player.Position, _W.Range));
                                if (Enemys != null)
                                {
                                    if (Enemys.Count() >= 2)
                                    {
                                        _W.Cast(target.ServerPosition);
                                        if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                        {

                                            Chat.Print("Casting W Found more than >= 2 People ");
                                            Console.WriteLine("Casting W Found more than >= 2 People");
                                        }
                                    }

                                }
                            }
                            if (!ComboMenu["UseW2"].Cast<CheckBox>().CurrentValue)
                            {
                                _W.Cast(target);
                                if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                {

                                    Chat.Print("Casting W ");
                                    Console.WriteLine("Casting W");
                                }
                            }
                        }

                    }

                    if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_Q.IsReady())
                        {
                            var canHitMoreThanOneTarget =
                              EntityManager.Heroes.Enemies.OrderByDescending(x => x.CountEnemyChampionsInRange(_Q.Width))
                              .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyChampionsInRange(_Q.Width) > 1);
                            if (canHitMoreThanOneTarget != null)
                            {
                                var getAllTargets = EntityManager.Heroes.Enemies.FindAll(x => x.IsValidTarget() && x.IsInRange(canHitMoreThanOneTarget, _Q.Width));
                                var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.ServerPosition) / getAllTargets.Count;
                                if (!center.IsZero)
                                {
                                    _Q.Cast(target);
                                    if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                    {

                                        Chat.Print("FOUND MORE THAN 2 PEOPLE FOR Q ");
                                        Console.WriteLine("FOUND MORE THAN 2 PEOPLE FOR Q");
                                    }
                                }

                            }
                        }

                    }

                    if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_Q.IsReady())
                        {
                            var canHitMoreThanOneTarget =
                              EntityManager.Heroes.Enemies.OrderByDescending(x => x.CountEnemyChampionsInRange(_Q.Width))
                              .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyChampionsInRange(_Q.Width) >= 1);
                            if (canHitMoreThanOneTarget != null)
                            {
                                var getAllTargets = EntityManager.Heroes.Enemies.FindAll(x => x.IsValidTarget() && x.IsInRange(canHitMoreThanOneTarget, _Q.Width));
                                var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.ServerPosition) / getAllTargets.Count;
                                if (!center.IsZero)
                                {
                                    _Q.Cast(target);
                                    if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                    {

                                        Chat.Print("FOUND ONLY 1 POEPLE FOR Q");
                                        Console.WriteLine("FOUND ONLY 1 POEPLE FOR Q");
                                        // Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                    }
                                }

                            }
                        }

                    }

                    if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQS"].Cast<CheckBox>().CurrentValue)

                    {

                        if (!target.IsInRange(_Player, _Q.Range))
                            return;
                        {
                            if (_Q.IsReady() && ComboMenu["UseS"].Cast<CheckBox>().CurrentValue)
                            {
                                var Qpred = _Q.GetPrediction(target);
                                if (Qpred.HitChance >= HitChance.Medium && target.IsValidTarget(_Q.Range))
                                {
                                    if (!target.PoisonWillExpire(250))
                                        return;
                                    {
                                        _Q.Cast(target.ServerPosition);
                                        if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                        {

                                            Chat.Print("Casting Q with HIGH pred saver ");
                                            Console.WriteLine("Casting Q with HIGH pred saver ");
                                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                        }
                                    }
                                }

                            }

                        }
                    }

                    if (!ComboMenu["UseS"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && !ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)

                    {
                        if (_Q.IsReady())
                        {

                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.HitChance >= HitChance.Medium && target.IsValidTarget(_Q.Range))
                            {
                                Core.DelayAction(() => _Q.Cast(target), 100);
                                if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                {

                                    Chat.Print("Casting Q with HIGH pred ");
                                    Console.WriteLine("Casting Q with HIGH pred ");
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                }
                            }
                        }



                    }
                    if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_R.IsReady())
                        {
                            if (target.IsFacing(_Player) && target.IsInRange(_Player, _R.Range))
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, target);
                                _R.Cast(target.ServerPosition);
                            }
                        }
                    }
                }
            }
                if (LowP)
                {
                    if (ComboMenu["UseE"].Cast<CheckBox>().CurrentValue)
                    {
                        if (!target.IsInRange(_Player, _E.Range))
                            return;
                        {
                            if (_E.IsReady())
                            {

                                _E.Cast(target);
                                if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                {
                                    Chat.Print("Casting E with speedup");
                                    Console.WriteLine("Game.Time + Casting E with Speedup");
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                }
                                //    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                //   Player.IssueOrder(GameObjectOrder.AttackTo)
                            }
                        }
                    }

                    if (ComboMenu["UseW"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_W.IsReady() && _Player.Distance(target) > 500)
                        {

                            var Wpred = _W.GetPrediction(target);
                            if (Wpred.HitChance >= HitChance.Low && target.IsValidTarget(_W.Range))
                            {
                                _W.Cast(target.ServerPosition);
                                if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                {

                                    Chat.Print("Casting W with HIGH pred ");
                                    Console.WriteLine("Casting W with HIGH pred ");
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                }
                            }
                        }

                    }

                    if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_Q.IsReady())
                        {
                            var canHitMoreThanOneTarget =
                              EntityManager.Heroes.Enemies.OrderByDescending(x => x.CountEnemyChampionsInRange(_Q.Width))
                              .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyChampionsInRange(_Q.Width) > 1);
                            if (canHitMoreThanOneTarget != null)
                            {
                                var getAllTargets = EntityManager.Heroes.Enemies.FindAll(x => x.IsValidTarget() && x.IsInRange(canHitMoreThanOneTarget, _Q.Width));
                                var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.ServerPosition) / getAllTargets.Count;
                                if (!center.IsZero)
                                {
                                    _Q.Cast(target);
                                    if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                    {

                                        Chat.Print("FOUND MORE THAN 2 PEOPLE FOR Q ");
                                        Console.WriteLine("FOUND MORE THAN 2 PEOPLE FOR Q");
                                        // Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                    }
                                }

                            }
                        }

                    }

                    if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_Q.IsReady())
                        {
                            var canHitMoreThanOneTarget =
                              EntityManager.Heroes.Enemies.OrderByDescending(x => x.CountEnemyChampionsInRange(_Q.Width))
                              .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyChampionsInRange(_Q.Width) >= 1);
                            if (canHitMoreThanOneTarget != null)
                            {
                                var getAllTargets = EntityManager.Heroes.Enemies.FindAll(x => x.IsValidTarget() && x.IsInRange(canHitMoreThanOneTarget, _Q.Width));
                                var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.ServerPosition) / getAllTargets.Count;
                                if (!center.IsZero)
                                {
                                    _Q.Cast(target);
                                    if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                    {

                                        Chat.Print("FOUND ONLY 1 POEPLE FOR Q");
                                        Console.WriteLine("FOUND ONLY 1 POEPLE FOR Q");
                                        // Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                    }
                                }

                            }
                        }

                    }

                    if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue)

                    {

                        if (!target.IsInRange(_Player, _Q.Range))
                            return;
                        {
                            if (_Q.IsReady() && ComboMenu["UseS"].Cast<CheckBox>().CurrentValue)
                            {
                                var Qpred = _Q.GetPrediction(target);
                                if (Qpred.HitChance >= HitChance.Low && target.IsValidTarget(_Q.Range))
                                {
                                    if (!target.PoisonWillExpire(250))
                                        return;
                                    {
                                        _Q.Cast(target.ServerPosition);
                                        if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                        {

                                            Chat.Print("Casting Q with HIGH pred saver ");
                                            Console.WriteLine("Casting Q with HIGH pred saver ");
                                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                        }
                                    }
                                }





                            }

                        }
                    }

                    if (!ComboMenu["UseS"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && !ComboMenu["UseQ2"].Cast<CheckBox>().CurrentValue)

                    {
                        if (_Q.IsReady())
                        {

                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.HitChance >= HitChance.Low && target.IsValidTarget(_Q.Range))
                            {
                                Core.DelayAction(() => _Q.Cast(target), 100);
                                if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                                {

                                    Chat.Print("Casting Q with HIGH pred ");
                                    Console.WriteLine("Casting Q with HIGH pred ");
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                }
                            }
                        }



                    }
                    if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue)
                    {
                        if (_R.IsReady())
                        {
                            if (target.IsFacing(_Player) && target.IsInRange(_Player, _R.Range))
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, target);
                                _R.Cast(target.ServerPosition);
                            }
                        }
                    }
                }

        }

        private static void ImmobileQ()

        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQI"].Cast<CheckBox>().CurrentValue)

            {
                if (_Q.IsReady())
                {

                    var Qpred = _Q.GetPrediction(target);
                    if (Qpred.HitChance >= HitChance.Immobile && target.IsValidTarget(_Q.Range))
                    {
                        _Q.Cast(target);
                        if (DebugC["Debug"].Cast<CheckBox>().CurrentValue)
                        {

                            Chat.Print("Casting Q for immobile enemy");
                            Console.WriteLine("Casting Q for immobile enemy ");
                        }
                    }
                }
            }

        }

    }    
 
}
