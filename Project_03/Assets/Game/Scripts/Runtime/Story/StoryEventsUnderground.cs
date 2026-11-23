using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 4~6장 (2026-11-15): 화물 승강장 → 폐기소(F·008, 과자/펌프/비살상 충돌) → 정비동(펄스 버스트, 컨베이어 홀, 수직 배선통로 추격)
    /// → 숙소(세탁실·베넬의 방·옥상) → 7장 수용동 연결문 직전.
    /// 베넬 상태 세 갈래: 동행(ally) / 생존·비동행(apart) / 사망(dead). 부재 경로에서는 살아 있는 베넬의 말·행동을 재생하지 않고
    /// 루의 관찰과 짧은 반응으로 따로 구성한다. 모든 1회성 보상·장면은 플래그로 막아 저장·불러오기·재방문 뒤에도 반복되지 않는다.
    /// 대사 원문: Data/Dialogue/dialogue_ko.json (src 에 GDD/사용자 지시/추가 구분, 추가 대사는 added).
    /// </summary>
    public static partial class StoryEvents
    {
        static readonly Dictionary<string, System.Func<Game, IEnumerator>> underground = new Dictionary<string, System.Func<Game, IEnumerator>>
        {
            { "m16_lift", Lift },
            { "waste_meet", WasteMeetZone }, { "waste_f", WasteF }, { "waste_008", Waste008 }, { "waste_jugs", WasteJugs },
            { "waste_hatch", g => g.Say("c4_hatch") }, { "waste_survivors", WasteSurvivors }, { "waste_fight_end", WasteFightEnd },
            { "m18_enter", g => MonoOnce(g, "c4_pump_enter") },
            { "m19_enter", g => { g.State.Set("m19_visited"); return MonoOnce(g, "c4_market_enter"); } },
            { "market_lantern", MarketLantern }, { "market_scrap", g => SayRepeat(g, "c4_scrap", "c4_scrap_again") }, { "market_lines", MarketLines },
            { "m20_crawl_hint", g => { if (g.State.Once("mono_m20_crawl")) g.Monologue("천장이 낮아... " + GameInput.Label(Act.Down) + " 를 누른 채 웅크려서 지나가자."); return Nothing(); } },
            { "ws_manual", WorkshopManual }, { "ws_after", WorkshopAfter },
            { "m22_enter", g => MonoOnce(g, "c5_hall_enter") }, { "hall_off_line", HallOff },
            { "m23_chase", ShaftChase }, { "m23_l2", ShaftL2 }, { "m23_l3", ShaftL3 }, { "m23_top", ShaftTop },
            { "m23_plate_zap", ShaftPlateZap }, { "m23_shutter", ShaftShutter },
            { "m24_enter", DormEnter }, { "laundry_twins", LaundryTwins }, { "laundry_others", g => SayRepeat(g, "c6_others", "c6_others_again") },
            { "m25_enter", RoomEnter }, { "room_photo", RoomPhoto }, { "room_letter", RoomLetter }, { "room_wrappers", RoomWrappers },
            { "roof_sit", RoofSit }, { "ch7_gate", Ch7Gate },
        };

        static bool TryUnderground(string id, out System.Func<Game, IEnumerator> f) => underground.TryGetValue(id, out f);

        static IEnumerator SayRepeat(Game g, string first, string again)
        {
            yield return g.Say(g.State.Once("seen_ev_" + first) ? first : again);
        }

        /// <summary>베넬 상태 (부재 이유 구분).</summary>
        public static bool VAlly(Game g) => g.State.VenelleIsAlly && g.Companion != null;
        public static bool VDead(Game g) => g.State.D.venelleState == "dead";

        /// <summary>베넬 신뢰 (GDD 9.3 venelle_trust 0~3). 같은 이유로는 한 번만.</summary>
        static void Bond(Game g, string key)
        {
            var s = g.State;
            if (!s.VenelleIsAlly || !s.Once("bond_" + key)) return;
            s.D.venelleTrust = Mathf.Min(3, s.D.venelleTrust + 1);
        }

        // ============================================================ 맵 진입 (StoryEvents.OnMapEnter 에서 호출)
        static IEnumerator OnEnterUnderground(Game g, string mapId)
        {
            var s = g.State;
            switch (mapId)
            {
                case "M16":
                    if (s.Once("ev_c4_start")) { s.D.chapterId = "ch4"; g.SaveNow("ch4_start", false); }
                    if (s.Once("mono_m16_enter")) g.MonologueId("c4_platform_enter");
                    break;
                case "M17":
                    // 펌프를 고치고 돌아왔다: 물통이 찬다 (해결 방식에 따라 다른 짧은 반응, 한 번만)
                    if (s.Has("waste_pump_fixed") && s.Once("waste_pump_thanks")) yield return PumpThanks(g);
                    break;
                case "M20":
                    if (s.Once("ev_c5_start")) { s.D.chapterId = "ch5"; g.SaveNow("ch5_start", false); }
                    break;
                case "M23":
                    Guard.NoGo.Clear();
                    s.SetVar("m23_strong_base", s.GetVar("strong_uses"));
                    if (s.Has("m23_acid") && !s.Has("m23_plate_done")) AddAcidNoGo(g);
                    break;
            }
        }

        // ============================================================ 16 화물 승강장
        static IEnumerator Lift(Game g)
        {
            var s = g.State; var p = g.Player;
            bool atBottom = s.Has("m16_lift_down");
            yield return g.AskEx("c4_lift_panel", null, true, new[] { atBottom ? "승강장으로 올라간다" : "지하로 내려간다", "상층 (봉쇄)", "그만둔다" });
            if (g.LastChoice == 1) { yield return SayRepeat(g, "c4_lift_up_sealed", "c4_lift_up_again"); yield break; }
            if (g.LastChoice != 0) yield break;
            // 짧은 승강 (조작 잠김 2초 남짓). 발판·루·동료가 함께 움직이고 카메라가 따라간다.
            var deck = Marker.Find("lift_deck");
            var top = g.Map.GetSpawn("lift_top"); var bottom = g.Map.GetSpawn("lift_bottom");
            if (deck == null || top == null || bottom == null) { if (atBottom) s.Set("m16_lift_down", false); else s.Set("m16_lift_down"); yield break; }
            float yTop = top.transform.position.y, yBot = bottom.transform.position.y;
            float from = atBottom ? yBot : yTop, to = atBottom ? yTop : yBot;
            float x = 34.5f;
            p.ScriptControl = true;
            yield return p.WalkTo(x);
            p.Frozen = true; p.AnimOverride = "Idle";
            var comp = g.Companion;
            Sfx.Play("sfx_metal_creak", 0.8f, 0.7f); Sfx.Play("sfx_door_slide", 0.6f, 0.6f);
            Vector3 deck0 = deck.transform.position;
            for (float t = 0; t < 2.2f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0, 1, t / 2.2f);
                float y = Mathf.Lerp(from, to, k);
                deck.transform.position = new Vector3(deck0.x, y - 0.3f, deck0.z);
                p.transform.position = new Vector3(x, y, 0);
                if (comp != null) comp.transform.position = new Vector3(x - 0.9f, y, 0);
                yield return null;
            }
            if (atBottom) s.Set("m16_lift_down", false); else s.Set("m16_lift_down");
            p.Teleport(new Vector2(atBottom ? top.transform.position.x : bottom.transform.position.x, to));
            if (comp != null) comp.PlaceBehindPlayer();
            p.Frozen = false; p.AnimOverride = null; p.ScriptControl = false;
            Sfx.Play("sfx_land", 0.6f, 0.6f);
            if (!atBottom && s.Once("c4_lift_first")) yield return g.Say(VAlly(g) ? "c4_lift_ride_v" : "c4_lift_ride");
        }

        // ============================================================ 17 폐기소 거처
        static IEnumerator WasteMeetZone(Game g)
        {
            var s = g.State;
            if (s.Has("waste_met")) yield break;
            yield return Cutscene.Play(g, "c4_waste_meet", WasteMeet(g), FinWasteMeet);
            yield return WasteDeal(g, true);
        }

        static IEnumerator WasteMeet(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true;
            var meet = g.Map.GetSpawn("meet");
            yield return Cutscene.LuMove(g, meet != null ? meet.transform.position.x : 8f, 3f, "Walk");
            p.SetFacing(1);
            // 바리케이드 뒤에서 파이프를 든 사람들이 일어선다 / 루가 손을 든다
            foreach (var sv in Scavenger.All) if (sv != null) Cutscene.Parallel(g, Nudge(sv.transform, -0.4f, 0.25f));
            Sfx.Play("sfx_clatter", 0.8f);
            Cutscene.Shake(0.03f, 0.2f);
            p.AnimOverride = p.View.HasState("HandsUp") ? "HandsUp" : "Fear";
            var kid = A("kid008");
            if (kid != null) kid.view.Play("Hide");
            // F 가 거처 안쪽에서 바리케이드 앞까지 걸어 나온다 (카메라는 생활 공간이 보이게 넓게)
            var f = A("f");
            if (f != null) Cutscene.Parallel(g, Cutscene.Walk(f, 15.6f, 1.6f, "Walk", "Talk"));
            yield return Cutscene.Pan(new Vector2(14.5f, p.transform.position.y + 1.6f), 1.2f);
            yield return Cutscene.Wait(0.3f);
            yield return g.Say("c4_meet_1");
            if (kid != null) kid.view.Play("Idle");
            p.AnimOverride = "Idle";
            // 거처 쪽을 훑는다: 난로·빈 물통·빨랫줄·상자 침대
            yield return Cutscene.Pan(new Vector2(22.5f, p.transform.position.y + 1.6f), 1.4f);
            yield return g.Say("c4_meet_2");
            if (f != null) Cutscene.Parallel(g, Cutscene.Walk(f, 22.5f, 1.4f, "Walk", "Idle"));
            yield return Cutscene.Pan(new Vector2(12f, p.transform.position.y + 1.6f), 0.8f);
        }

        static IEnumerator Nudge(Transform t, float dx, float time)
        {
            if (t == null) yield break;
            Vector3 a = t.position, b = a + new Vector3(dx, 0, 0);
            for (float k = 0; k < time; k += Time.deltaTime) { if (t == null) yield break; t.position = Vector3.Lerp(a, b, k / time); yield return null; }
            if (t != null) t.position = b;
        }

        static void FinWasteMeet(Game g)
        {
            var s = g.State;
            s.Set("waste_met");
            GameState.AddUnique(s.D.shelterGroups, "waste_residents");   // F 의 대피실에 모인 사람들 (시설 밖으로 나간 사람 없음)
            var p = g.Player; if (p != null) { p.AnimOverride = null; p.ScriptControl = false; }
            var kid = A("kid008"); if (kid != null && kid.view != null) kid.view.Play("Idle");
            var f = A("f");
            if (f != null) { f.transform.position = new Vector3(22.5f, f.transform.position.y, 0); f.view.Face(-1); f.view.Play("Idle"); }   // 건너뛰어도 같은 자리
        }

        /// <summary>
        /// 교섭: 과자 10개(실제 소지량 확인) / 물 펌프를 고쳐 주겠다 / 거절(→ 겁먹은 생존자들과 짧은 비살상 충돌).
        /// 과자가 부족하면 첫 항목은 흐리게 보이고 고를 수 없다 (없는 과자를 주거나 음수로 빼지 않는다). 기본 선택 없음.
        /// </summary>
        static IEnumerator WasteDeal(Game g, bool first)
        {
            var s = g.State;
            if (s.Has("waste_resolved") || s.Has("waste_fight")) yield break;
            int have = s.Count("snack");
            bool promised = s.Has("waste_pump_promised");
            var opts = new[] { "과자 10개를 나눈다 (가진 과자 " + have + "개)", promised ? "펌프는 지금 고치는 중이야" : "물 펌프를 고쳐 줄게", "못 줘! 그냥 지나갈게" };
            yield return g.AskEx(first ? "c4_deal" : "c4_deal_again", new[] { have >= 10, true, true }, true, opts);
            switch (g.LastChoice)
            {
                case 0:
                    if (s.Count("snack") < 10) { yield return g.Say("c4_deal_short"); yield break; }   // 방어: 선택 사이에 줄었을 때
                    s.AddItem("snack", -10);
                    g.UI.hud.Refresh(s);
                    yield return GiftScene(g);
                    break;
                case 1:
                    if (!promised) { s.Set("waste_pump_promised"); yield return g.Say("c4_deal_pump"); }
                    else yield return g.Say("c4_deal_pump_again");
                    break;
                default:
                    yield return g.Say("c4_deal_fail");
                    s.Set("waste_fight");
                    g.Checkpoint("waste_fight");      // 충돌 중 쓰러지면 충돌 시작점에서 다시 (대화는 다시 보지 않음)
                    if (s.Once("hint_waste_fight")) g.UI.hud.SetHint("겁먹은 사람들이다. " + GameInput.Label(Act.Skill1) + " 로 찌릿하게만 — 다치게 하지 않고 진정시키자.");
                    break;
            }
        }

        static IEnumerator GiftScene(Game g)
        {
            var s = g.State;
            s.D.wasteResult = "gift";
            Resolve(g);
            var f = A("f");
            if (f != null) f.view.Play("Talk");
            yield return g.Say("c4_gift");
            foreach (var sv in Scavenger.All) if (sv != null && sv.View != null) sv.View.Play("Idle");
            if (f != null) f.view.Play("Idle");
            s.Reward("waste_gift", ActionTuning.I.expNegotiate);
        }

        static void Resolve(Game g)
        {
            var s = g.State;
            if (s.Has("waste_resolved")) return;
            s.Set("waste_resolved");
            s.Reward("section_waste", ActionTuning.I.expSection);
            g.Checkpoint("waste_resolved");
        }

        static IEnumerator WasteFightEnd(Game g)
        {
            var s = g.State;
            g.UI.hud.SetHint("");
            s.Set("waste_fight", false);
            s.D.wasteResult = "fight";
            var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            yield return new WaitForSeconds(0.4f);
            yield return g.Say("c4_fight_end");
            p.AnimOverride = null; p.ScriptControl = false;
            Resolve(g);
        }

        static IEnumerator WasteF(Game g)
        {
            var s = g.State;
            if (!s.Has("waste_resolved"))
            {
                if (s.Has("waste_fight")) { yield return g.Say("c4_f_fight"); yield break; }
                yield return WasteDeal(g, false);
                yield break;
            }
            if (s.Once("waste_f_name"))
            {
                var f = A("f");
                if (f != null) f.view.Play("Tag");
                yield return g.Say("c4_f_name");
                // F 가 빈 상자를 툭 던진다 (과자를 나눴다면 그 과자 상자)
                if (f != null) f.view.Play("Toss");
                var p = g.Player;
                var box = new GameObject("EmptyBox");
                var sr = box.AddComponent<SpriteRenderer>();
                sr.sprite = PropSprite("U_BeltCrate");
                sr.sortingOrder = 30; box.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                Vector3 a = f != null ? f.transform.position + new Vector3(0, 1.2f, 0) : p.transform.position + new Vector3(2, 1.2f, 0);
                box.transform.position = a;
                yield return Cutscene.Arc(box.transform, p.transform.position + new Vector3(0, 1.3f, 0), 1.2f, 0.45f);
                Sfx.Play("sfx_clatter", 0.6f, 1.4f);
                Onoma(g, (Vector2)p.transform.position + new Vector2(0, 2.0f), "툭", new Color(1f, 0.95f, 0.8f));
                p.View.Flash(new Color(1f, 0.9f, 0.7f), 0.1f);
                yield return Cutscene.Arc(box.transform, p.transform.position + new Vector3(0.6f, 0.05f, 0), 0.4f, 0.3f);
                Object.Destroy(box, 1.2f);
                if (f != null) f.view.Play("Idle");
                yield return g.Say(s.D.wasteResult == "gift" ? "c4_f_box_gift" : "c4_f_box");
                yield break;
            }
            if (s.D.wasteResult == "fight" && !s.Has("waste_pump_fixed")) { yield return g.Say("c4_f_pump_hint"); yield break; }
            yield return g.Say("c4_f_again");
        }

        /// <summary>임시 소품 그림을 이름으로 (GameConfig 의 U_* 세트). 없으면 null.</summary>
        static Sprite PropSprite(string name)
        {
            var cfg = Game.I != null ? Game.I.Config : GameConfig.Load();
            var set = cfg != null ? cfg.GetSet(name) : null;
            return set != null && set.frames != null && set.frames.Length > 0 ? set.frames[0] : null;
        }

        static IEnumerator Waste008(Game g)
        {
            var s = g.State;
            var kid = A("kid008");
            if (s.Has("waste_fight")) { yield return g.Say("c4_008_hiding"); yield break; }
            if (!s.D.child008Fed && s.Count("snack") > 0)
            {
                yield return g.AskEx("c4_008_ask", null, true);
                if (g.LastChoice != 0) { yield return g.Say("c4_008_later"); yield break; }
                if (!s.UseItem("snack")) yield break;
                s.D.child008Fed = true;
                g.UI.hud.Refresh(s);
                if (kid != null) kid.view.Play("Hold");
                yield return g.Say("c4_008_fed");
                if (VAlly(g))
                {
                    var comp = g.Companion;
                    comp.View.Face(kid != null && kid.transform.position.x > comp.transform.position.x ? 1 : -1);
                    comp.View.Play("Idle");
                    yield return new WaitForSeconds(0.9f);          // 잠깐 말을 잃는다
                    yield return g.Say("c4_008_venelle");
                    yield return new WaitForSeconds(0.4f);
                    // 루는 이유를 묻지 않고 자기 과자를 반으로 나눈다 (가진 과자가 있어야)
                    if (s.UseItem("snack"))
                    {
                        g.UI.hud.Refresh(s);
                        g.Player.AnimOverride = "Share";
                        yield return g.Say("c4_008_split");
                        g.Player.AnimOverride = null;
                    }
                    else yield return g.Say("c4_008_split_none");
                    Bond(g, "008");
                }
                else yield return g.Say("c4_008_solo");
                if (kid != null) kid.view.Play("Eat");
                yield break;
            }
            if (s.Once("waste_008_name")) { yield return g.Say("c4_008_name"); yield break; }
            if (!s.D.child008Fed) { yield return g.Say("c4_008_spark"); Fx.Spawn("Spark", g.Player.Chest + Vector2.right * g.Player.Facing * 0.4f, false, 31, 0.5f); yield break; }
            yield return g.Say("c4_008_after");
        }

        static IEnumerator WasteJugs(Game g)
        {
            yield return g.Say(g.State.Has("waste_water_on") ? "c4_jugs_full" : "c4_jugs_empty");
        }

        static IEnumerator WasteSurvivors(Game g)
        {
            var r = g.State.D.wasteResult;
            yield return g.Say(r == "gift" ? "c4_sv_gift" : r == "fight" ? (g.State.D.wasteReconciled ? "c4_sv_reconciled" : "c4_sv_fight") : "c4_sv_repair");
        }

        // ============================================================ 18 물 펌프실
        /// <summary>펌프가 처음 제대로 돌았을 때 (PumpRig). 해결 방식 기록: 다른 해결 전이면 '수리', 충돌 뒤면 '화해'.</summary>
        public static IEnumerator PumpStarted(Game g)
        {
            var s = g.State;
            Sfx.Play("sfx_pc_on", 0.7f, 0.7f); Sfx.Play("sfx_bubble", 0.8f, 0.8f);
            if (!s.Once("waste_pump_fixed")) { yield return g.Say("pump_running_again"); yield break; }
            s.Set("waste_water_on");
            s.Reward("pump_fixed", ActionTuning.I.expSection);
            if (s.D.wasteResult == "none") { s.D.wasteResult = "repair"; Resolve(g); }
            else if (s.D.wasteResult == "fight") s.D.wasteReconciled = true;
            yield return g.Say("pump_running");
        }

        static IEnumerator PumpThanks(Game g)
        {
            var s = g.State;
            var r = s.D.wasteResult;
            var f = A("f");
            if (f != null) f.view.Play("Talk");
            yield return g.Say(r == "repair" ? "c4_pump_thanks_repair" : r == "fight" ? "c4_pump_thanks_fight" : "c4_pump_thanks_gift");
            if (f != null) f.view.Play("Idle");
        }

        // ============================================================ 19 폐품 시장 (선택)
        static IEnumerator MarketLantern(Game g)
        {
            var s = g.State;
            if (s.Has("market_lantern_lit")) { yield return g.Say("c4_lantern_after"); yield break; }
            bool can = s.D.lu.charge >= 1;
            yield return g.AskEx("c4_lantern_ask", new[] { can, true }, true);
            if (g.LastChoice != 0) { yield return g.Say("c4_lantern_no"); yield break; }
            s.D.lu.charge -= 1;
            s.Set("market_lantern_lit");
            g.Player.AnimOverride = "Point";
            Fx.Spawn("Spark", g.Player.Chest + Vector2.right * g.Player.Facing * 0.5f, false, 31, 0.5f);
            Sfx.Play("sfx_light_on", 0.8f);
            yield return new WaitForSeconds(0.3f);
            g.Player.AnimOverride = null;
            yield return g.Say("c4_lantern_lit");
            if (s.Once("market_snacks_given")) { s.AddItem("snack", 2); g.UI.Toast("과자 2개를 받았다.", 2f); g.UI.hud.Refresh(s); }
        }

        static IEnumerator MarketLines(Game g)
        {
            var s = g.State;
            if (s.Has("side_lines_done")) { yield return g.Say("c4_lines_again"); yield break; }
            var p = g.Player; var kid = A("kid008_m");
            yield return g.Say("c4_lines");
            s.Set("side_lines_done");
            GameState.AddUnique(s.D.sideStoryDone, "lines");
            // 함께 앉은 작은 장면
            p.ScriptControl = true;
            if (kid != null) { yield return p.WalkTo(kid.transform.position.x - 0.8f); kid.view.Play("Sit"); }
            p.SetFacing(1); p.AnimOverride = "Sit";
            yield return new WaitForSeconds(1.6f);
            yield return g.Say("c4_lines_sit");
            yield return new WaitForSeconds(0.6f);
            p.AnimOverride = null; p.ScriptControl = false;
            if (kid != null) kid.view.Play("Draw");
        }

        // ============================================================ 21 정비 작업장: 펄스 버스트
        static IEnumerator WorkshopManual(Game g)
        {
            var s = g.State; var p = g.Player;
            if (s.Has("pulse_learned")) { yield return g.Say("c5_manual_again"); yield break; }
            yield return g.Say("c5_manual");
            p.Skills.Unlock("pulse");
            s.Set("pulse_learned");
            // 넣을 칸 고르기: 전원선 이동(전선) 칸은 바꾸지 않는다 (진행에 필요). 바꾼 기술은 메뉴 '기술 장착'에서 언제든 되돌릴 수 있다.
            var slots = s.D.slots;
            var names = new List<string>(); var idx = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (slots[i] == "wire") continue;
                var sk = ActionTuning.I.Skill(slots[i]);
                names.Add(GameInput.Label(GameInput.SkillAct(i)) + " 칸 (" + (sk != null ? sk.shortName : "빈 칸") + " 대신)");
                idx.Add(i);
            }
            names.Add("나중에 (메뉴 '기술 장착')");
            yield return g.Info("펄스 버스트를 어느 칸에 넣을까? 뺀 기술은 메뉴에서 다시 넣을 수 있다.");
            yield return g.Choose(names, null, true);
            int pick = g.LastChoice;
            if (pick >= 0 && pick < idx.Count)
            {
                slots[idx[pick]] = "pulse";
                g.UI.hud.Refresh(s);
                if (s.Once("hint_pulse")) g.UI.hud.SetHint("시험대 위에서 " + GameInput.Label(GameInput.SkillAct(idx[pick])) + " — 펄스 버스트 (전하 3)");
            }
            else yield return g.Info("메뉴(" + GameInput.Label(Act.Cancel) + ") → '기술 장착'에서 펄스 버스트를 칸에 넣을 수 있다.");
        }

        static IEnumerator WorkshopAfter(Game g)
        {
            var s = g.State; var p = g.Player;
            g.UI.hud.SetHint("");
            p.ScriptControl = true; p.AnimOverride = "HairUp";
            yield return new WaitForSeconds(0.5f);
            if (VAlly(g))
            {
                var comp = g.Companion;
                comp.View.Face(p.transform.position.x > comp.transform.position.x ? 1 : -1);
                yield return g.Say("c5_hair_v");
                // 베넬이 빗을 꺼내려다 금속 빗인 것을 보고 다시 넣는다
                comp.View.Play("Comb", true);
                yield return new WaitForSeconds(0.55f);
                Onoma(g, (Vector2)comp.transform.position + new Vector2(0.3f, 1.9f), "...아.", new Color(0.85f, 0.85f, 0.95f));
                Fx.Spawn("Spark", (Vector2)comp.transform.position + new Vector2(0.4f * comp.View.Facing, 1.0f), false, 31, 0.3f);
                yield return new WaitForSeconds(0.7f);
                comp.View.Play("Idle");
                yield return g.Say("c5_comb_v");
                Bond(g, "workshop");
            }
            else yield return g.Say("c5_hair_solo");
            p.AnimOverride = null;
            var lamp = Device.Find("ws_lamp"); var lampB = Device.Find("ws_lamp_battery");
            if (lamp != null && lampB != null && lamp.Model.power != DevicePower.On && lampB.Model.power == DevicePower.On)
                yield return g.Say("c5_lamp_note");
            p.ScriptControl = false;
        }

        // ============================================================ 22 컨베이어 홀
        static IEnumerator HallOff(Game g)
        {
            var p = g.Player;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            yield return new WaitForSeconds(0.3f);
            yield return g.Say(VAlly(g) ? "c5_hall_off_v" : "c5_hall_off_solo");
            p.AnimOverride = null; p.ScriptControl = false;
        }

        // ============================================================ 23 수직 배선통로 (추격)
        static IEnumerator ShaftChase(Game g)
        {
            var s = g.State;
            if (!s.Once("m23_chase")) yield break;
            Sfx.Play("sfx_alarm", 0.7f); Sfx.Play("sfx_door_bang", 0.8f);
            Onoma(g, LuPos(g) + new Vector2(-3f, 2.4f), "쾅!", new Color(1f, 0.7f, 0.6f));
            yield return null; yield return null;
            AlertById("M23_g1", LuPos(g));
            g.MonologueId("c5_chase");
            if (s.Once("hint_chase")) g.UI.hud.SetHint("쫓아온다! 사다리로 위층으로 — 경비는 사다리를 타지 못한다.");
        }

        static void AlertById(string id, Vector2 at)
        {
            foreach (var gd in Guard.All.ToArray()) if (gd != null && gd.guardId == id) gd.Alert(at, true);
        }

        static void AddAcidNoGo(Game g)
        {
            // 2층 바닥판 위 x 7.9~12.6 (경비는 들어가지 않는다)
            float l2 = 0;
            foreach (var m in Object.FindObjectsByType<Transform>()) if (m.name == "MetalPlate") { l2 = m.position.y; break; }
            Guard.NoGo.Add(new Rect(7.9f, l2 - 0.5f, 4.7f, 2.5f));
        }

        static IEnumerator ShaftL2(Game g)
        {
            var s = g.State; var p = g.Player;
            if (!s.Once("m23_l2_guard")) yield break;
            g.UI.hud.SetHint("");
            Sfx.Play("sfx_door_bang", 0.7f, 0.9f);
            yield return null; yield return null;
            AlertById("M23_g2", LuPos(g)); AlertById("M23_d1", LuPos(g));
            if (VAlly(g))
            {
                // 베넬: 산성 약품을 금속 바닥판에 던져 추격로를 막는다
                var comp = g.Companion;
                p.ScriptControl = true; p.AnimOverride = "Idle";
                comp.View.Face(-1); comp.View.Play("Toss", true);
                yield return new WaitForSeconds(0.25f);
                Vector3 from = comp.transform.position + new Vector3(0, 1.4f, 0);
                var flask = new GameObject("AcidFlask");
                var fsr = flask.AddComponent<SpriteRenderer>(); var fset = g.Config.GetSet("FX_Flask"); fsr.sprite = fset != null && fset.frames.Length > 0 ? fset.frames[0] : PropSprite("U_Jug"); fsr.color = new Color(0.6f, 1f, 0.5f); fsr.sortingOrder = 30;
                flask.transform.position = from;
                float plateY = from.y - 1.4f;
                foreach (var m in Object.FindObjectsByType<Transform>()) if (m.name == "MetalPlate") { plateY = m.position.y; break; }
                yield return Cutscene.Arc(flask.transform, new Vector3(10.5f, plateY + 0.1f, 0), 2.2f, 0.6f);
                Object.Destroy(flask);
                Sfx.Play("sfx_glass_break", 0.8f); Sfx.Play("sfx_acid", 0.8f);
                Fx.Spawn("AcidSplash", new Vector2(10.5f, plateY + 0.3f), false, 31, 1f);
                s.Set("m23_acid");
                AddAcidNoGo(g);
                comp.View.Play("Idle");
                yield return g.Say("c5_acid_v");
                p.AnimOverride = null; p.ScriptControl = false;
                g.UI.hud.SetHint("바닥판 연결함에서 " + GameInput.Label(Act.Interact) + " — 젖은 바닥으로 전류");
            }
            else
            {
                g.MonologueId("c5_shaft_solo");
                g.UI.hud.SetHint("벨트가 경비를 되민다 · 셔터 패널 " + GameInput.Label(Act.Interact) + " 로 길을 막을 수 있다");
            }
        }

        static IEnumerator ShaftPlateZap(Game g)
        {
            var s = g.State; var p = g.Player;
            if (!s.Has("m23_acid") || s.Has("m23_plate_done")) yield break;
            g.UI.hud.SetHint("");
            p.ScriptControl = true; p.AnimOverride = p.View.HasState("Discharge") ? "Discharge" : "Point";
            Sfx.Play("sfx_discharge_big", 0.9f);
            float plateY = p.transform.position.y;
            foreach (var m in Object.FindObjectsByType<Transform>()) if (m.name == "MetalPlate") { plateY = m.position.y; break; }
            for (int i = 0; i < 8; i++) Fx.Spawn("Spark", new Vector2(6.2f + i * 0.95f, plateY + 0.2f), false, 31, 0.6f);
            Cutscene.Shake(0.05f, 0.3f);
            foreach (var gd in Guard.All.ToArray())
                if (gd != null && gd.Alive && !gd.flying && gd.transform.position.x > 5.5f && gd.transform.position.x < 13.4f && Mathf.Abs(gd.transform.position.y - plateY) < 0.6f) gd.KnockOut();
            s.Set("m23_plate_done"); s.Set("m23_acid", false);
            Guard.NoGo.Clear();
            s.SetVar("strong_uses", s.GetVar("strong_uses") + 1);
            yield return new WaitForSeconds(0.4f);
            p.AnimOverride = null;
            yield return g.Say("c5_plate_v");
            s.Set("m23_arm_said");
            p.ScriptControl = false;
        }

        static IEnumerator ShaftShutter(Game g)
        {
            var s = g.State;
            if (s.Has("m23_shutter")) { yield return g.Info("셔터가 내려가 있다. 다시 올리는 손잡이는 저쪽에 있다."); yield break; }
            s.Set("m23_shutter");
            Sfx.Play("sfx_door_bang", 1f); Cutscene.Shake(0.05f, 0.25f);
            Onoma(g, new Vector2(13.6f, LuPos(g).y + 2.6f), "쿵!", new Color(1f, 0.85f, 0.6f));
            g.UI.hud.SetHint("");
            yield return g.Say("c5_shutter");
        }

        static IEnumerator ShaftL3(Game g)
        {
            var s = g.State;
            if (!s.Once("m23_l3_said")) yield break;
            int slot = s.D.slots.IndexOf("wire");
            g.Monologue(slot >= 0 ? "전원선이다! " + GameInput.Label(GameInput.SkillAct(slot)) + " 로 타고 저 위 턱까지!" : "전원선이다... 메뉴에서 '전선' 기술을 칸에 넣어야 탈 수 있어.");
        }

        static IEnumerator ShaftTop(Game g)
        {
            var s = g.State;
            if (!s.Once("m23_top_said")) yield break;
            g.UI.hud.SetHint("");
            bool strong = s.GetVar("strong_uses") > s.GetVar("m23_strong_base");
            if (s.Has("m23_arm_said")) yield break;
            if (strong) yield return g.Say("c5_arm_ok");
            else g.MonologueId("c5_shaft_clear");
        }

        // ============================================================ 24 공동 세탁실 (6장)
        static IEnumerator DormEnter(Game g)
        {
            var s = g.State;
            if (!s.Once("ev_c6_start")) yield break;
            s.D.chapterId = "ch6"; g.SaveNow("ch6_start", false);
            g.MonologueId(VAlly(g) ? "c6_enter_v" : "c6_enter");
        }

        static IEnumerator LaundryTwins(Game g)
        {
            var s = g.State;
            if (VAlly(g))
            {
                if (s.Once("laundry_twins_v"))
                {
                    var comp = g.Companion;
                    comp.View.Face(1);
                    yield return g.Say("c6_twins_v");
                    Bond(g, "twins");
                    yield break;
                }
                if (!s.D.sideStoryDone.Contains("clothes"))
                {
                    yield return g.AskEx("c6_clothes_ask", null, true);
                    if (g.LastChoice != 0) yield break;
                    // 옷의 주인 (선택 이야기): 베넬이 옷을 고쳐 입는 짧은 장면
                    yield return g.UI.Fade(1f, 0.4f);
                    Sfx.Play("sfx_bubble", 0.4f, 1.4f);
                    yield return new WaitForSeconds(0.6f);
                    yield return g.UI.Fade(0f, 0.4f);
                    yield return g.Say("c6_clothes");
                    GameState.AddUnique(s.D.sideStoryDone, "clothes");
                    Bond(g, "clothes");
                    yield break;
                }
                yield return g.Say("c6_twins_again_v");
                yield break;
            }
            yield return SayRepeat(g, VDead(g) ? "c6_twins_dead" : "c6_twins_apart", "c6_twins_again");
        }

        // ============================================================ 25 베넬의 방
        static IEnumerator RoomEnter(Game g)
        {
            var s = g.State;
            if (!s.Once("m25_entered")) yield break;
            if (VAlly(g)) yield return g.Say("c6_room_enter_v");
            else g.MonologueId(VDead(g) ? "c6_room_enter_dead" : "c6_room_enter_apart");
        }

        static IEnumerator RoomPhoto(Game g)
        {
            var s = g.State;
            bool first = s.Once("room_photo_seen");
            if (VAlly(g)) yield return g.Say(first ? "c6_photo_v" : "c6_photo_again_v");
            else yield return g.Say(first ? (VDead(g) ? "c6_photo_dead" : "c6_photo_apart") : "c6_photo_again");
            yield return MaybeMemory(g);
        }

        static IEnumerator RoomLetter(Game g)
        {
            var s = g.State;
            bool first = s.Once("room_letter_seen");
            var doc = g.Dialogue.docs.TryGetValue("D_IVY", out var d) ? d : null;
            if (doc != null)
            {
                yield return DocPickup.DocView(g, doc);
                if (GameState.AddUnique(s.D.docs, "D_IVY")) g.UI.Toast("기록 획득: " + doc.title);
            }
            if (first) yield return g.Say(VAlly(g) ? "c6_letter_v" : VDead(g) ? "c6_letter_dead" : "c6_letter_apart");
            yield return MaybeMemory(g);
        }

        static IEnumerator RoomWrappers(Game g)
        {
            var s = g.State;
            bool first = s.Once("room_wrappers_seen");
            if (VAlly(g))
            {
                if (s.Has("ivy_room_done")) { yield return g.Say("c6_wrappers_after_v"); yield break; }
                if (first) yield return g.Say("c6_wrappers_v");
                yield return MaybeMemory(g);
                yield break;
            }
            yield return g.Say(first ? "c6_wrappers_solo" : "c6_wrappers_again");
        }

        /// <summary>동행 중, 사진·편지·포장지를 모두 본 뒤 한 번: 아이비의 과거(짧은 장면 다섯) → '또' 에서 현재 빈 포장지 → 현재 대화.</summary>
        static IEnumerator MaybeMemory(Game g)
        {
            var s = g.State;
            if (!VAlly(g) || s.Has("ivy_memory_done")) yield break;
            if (!s.Has("room_photo_seen") || !s.Has("room_letter_seen") || !s.Has("room_wrappers_seen")) yield break;
            yield return Cutscene.Play(g, "ivy_memory", IvyMemory(g), gg => { gg.State.Set("ivy_memory_done"); ShowPresent(gg); });
            yield return Cutscene.Play(g, "ivy_room_talk", RoomTalk(g), FinRoomTalk);
        }

        static GameObject Still(string art, string anim, Vector2 at, int face, Color tint, int order = 52)
        {
            var go = Cutscene.Temp(new GameObject("Mem_" + art));
            go.transform.position = at;
            var v = CharacterView.Create(go.transform, Game.I.Config.GetArt(art), order);
            v.Face(face); v.Play(anim);
            if (v.sr != null) v.sr.color = tint;
            return go;
        }

        static IEnumerator IvyMemory(Game g)
        {
            var p = g.Player; var comp = g.Companion;
            p.ScriptControl = true; p.AnimOverride = "Idle";
            Vector2 c = Marker.Pos("mem_center", LuPos(g));
            var sepia = new Color(0.92f, 0.78f, 0.6f);
            // 무대: 현재 방을 덮는 어두운 막 위에 과거 인물을 세피아로 (새 관찰자·설명자 없음)
            var cam = Camera.main;
            var veil = Cutscene.Temp(new GameObject("MemVeil"));
            var vsr = veil.AddComponent<SpriteRenderer>(); vsr.sprite = g.Config.pixel; vsr.sortingOrder = 50; vsr.color = new Color(0.12f, 0.09f, 0.07f, 0f);
            float pxs = g.Config.pixel != null ? Mathf.Max(0.001f, g.Config.pixel.bounds.size.x) : 0.125f;
            veil.transform.position = (Vector3)c + new Vector3(0, 2.5f, 0); veil.transform.localScale = new Vector3(40f / pxs, 24f / pxs, 1);
            yield return g.UI.Fade(1f, 0.5f);
            vsr.color = new Color(0.12f, 0.09f, 0.07f, 1f);
            // 과거 장면 동안 지금의 루·베넬은 화면에서 뺀다 (과거 인물과 섞이지 않게)
            p.View.SetVisible(false); if (comp != null) comp.View.SetVisible(false);
            Cutscene.CamSnap(c + new Vector2(0, 1.2f));
            var stage = new List<GameObject>();
            void Clear() { foreach (var o in stage) if (o != null) Object.Destroy(o); stage.Clear(); }
            // 1. 엄마의 죽음
            stage.Add(Still("ivy", "Kneel", c + new Vector2(-0.6f, 0), 1, sepia));
            stage.Add(Still("linden", "Idle", c + new Vector2(-1.5f, 0), 1, sepia));
            var bed = Cutscene.Temp(new GameObject("MemBed")); var bsr = bed.AddComponent<SpriteRenderer>(); bsr.sprite = PropSprite("U_DormBed"); bsr.sortingOrder = 51; bsr.color = sepia * 0.8f; bed.transform.position = c + new Vector2(1.2f, 0);
            stage.Add(bed);
            yield return g.UI.Fade(0f, 0.6f);
            yield return g.Say("iv_1");
            yield return g.UI.Fade(1f, 0.35f); Clear();
            // 2. 아빠의 사고
            stage.Add(Still("adult", "Crutch", c + new Vector2(0.9f, 0), -1, sepia * 0.7f));
            stage.Add(Still("ivy", "Idle", c + new Vector2(-0.6f, 0), 1, sepia));
            stage.Add(Still("linden", "Idle", c + new Vector2(-1.4f, 0), 1, sepia));
            yield return g.UI.Fade(0f, 0.45f);
            yield return g.Say("iv_2");
            yield return g.UI.Fade(1f, 0.35f); Clear();
            // 3. 아이비의 노동 (상자를 나르며 지나간다)
            var ivy = Still("ivy", "Carry", c + new Vector2(-2.5f, 0), 1, sepia); stage.Add(ivy);
            yield return g.UI.Fade(0f, 0.45f);
            Cutscene.Parallel(g, Cutscene.MoveObj(ivy.transform, (Vector3)(c + new Vector2(2.5f, 0)), 3.2f, false));
            yield return g.Say("iv_3");
            yield return g.UI.Fade(1f, 0.35f); Clear();
            // 4. 시약 체험 지원 (전단)
            stage.Add(Still("ivy", "Idle", c + new Vector2(-0.3f, 0), 1, sepia));
            var fl = Cutscene.Temp(new GameObject("MemFlyer")); var fsr = fl.AddComponent<SpriteRenderer>(); fsr.sprite = PropSprite("U_TrialFlyer"); fsr.sortingOrder = 53; fsr.color = sepia; fl.transform.position = c + new Vector2(0.55f, 1.2f);
            stage.Add(fl);
            yield return g.UI.Fade(0f, 0.45f);
            yield return g.Say("iv_4");
            yield return g.UI.Fade(1f, 0.35f); Clear();
            // 5. 동생에게 남긴 초코바 열 개 → 아이비의 독백 → '또' 에서 현재의 빈 포장지
            stage.Add(Still("ivy", "Give", c + new Vector2(-0.2f, 0), 1, sepia));
            stage.Add(Still("linden", "Hold", c + new Vector2(0.7f, 0), -1, sepia));
            yield return g.UI.Fade(0f, 0.45f);
            yield return g.Say("iv_5");
            yield return g.Say("iv_mono", false);
            // 마지막 '또' 에서 바로 현재로: 빈 포장지 열 장
            Clear();
            vsr.color = new Color(0, 0, 0, 0);
            g.UI.dialogue.Show(false); g.UI.portraits.HideAll();
            var wr = GameObject.Find("Inspect_m25_wrappers");
            Cutscene.CamSnap((wr != null ? (Vector2)wr.transform.position : c) + new Vector2(0, 0.6f));
            Sfx.Play("sfx_bubble", 0.3f, 0.6f);
            yield return Cutscene.Wait(1.4f);
            ShowPresent(g);
            if (comp != null) comp.PlaceBehindPlayer();
        }

        static void ShowPresent(Game g)
        {
            if (g.Player != null) g.Player.View.SetVisible(true);
            if (g.Companion != null) g.Companion.View.SetVisible(true);
        }

        static IEnumerator RoomTalk(Game g)
        {
            var p = g.Player; var comp = g.Companion;
            p.ScriptControl = true;
            var bedPos = new Vector2(7.5f, p.transform.position.y);
            if (comp != null) { comp.transform.position = new Vector3(bedPos.x - 0.4f, bedPos.y, 0); comp.View.Face(1); comp.View.Play("SitQuiet"); }
            p.Teleport(new Vector2(bedPos.x + 1.6f, bedPos.y)); p.SetFacing(-1); p.AnimOverride = "Idle";
            Cutscene.CamSnap(bedPos + new Vector2(0.6f, 1.2f));
            yield return Cutscene.Wait(0.8f);
            yield return g.Say("c6_v_confused");
            // 루가 서두르지 않고 기다린다 (곁에 앉는다)
            yield return Cutscene.Wait(0.6f);
            p.Teleport(new Vector2(bedPos.x + 0.7f, bedPos.y)); p.AnimOverride = "Sit";
            yield return Cutscene.Wait(1.6f);
            yield return g.Say("c6_room_talk_1");
            yield return Cutscene.Wait(0.7f);
            if (comp != null) comp.View.Play("Share");
            yield return g.Say("c6_room_talk_2");
            p.AnimOverride = p.View.HasState("Share") ? "Share" : "Sit";
            yield return g.Say("c6_room_talk_3");
            p.AnimOverride = "Sit";
            if (comp != null) comp.View.Play("SitQuiet");
            yield return Cutscene.Wait(1.5f);       // 여운 (대화창 없이)
        }

        static void FinRoomTalk(Game g)
        {
            var s = g.State;
            s.Set("ivy_room_done");
            Bond(g, "room");
            var p = g.Player;
            if (p != null) { p.AnimOverride = null; p.ScriptControl = false; }
            if (g.Companion != null) { g.Companion.View.Play("Idle"); g.Companion.PlaceBehindPlayer(); }
        }

        // ============================================================ 26 기숙사 옥상 (선택)
        static IEnumerator RoofSit(Game g)
        {
            var s = g.State;
            s.Set("m26_visited");
            if (s.Has("roof_done")) { yield return g.Info("바람이 분다."); yield break; }
            yield return Cutscene.Play(g, "roof_wind", RoofBody(g), gg => { gg.State.Set("roof_done"); var pp = gg.Player; if (pp != null) { pp.AnimOverride = null; pp.ScriptControl = false; } if (gg.Companion != null) gg.Companion.View.Play("Idle"); });
        }

        static IEnumerator RoofBody(Game g)
        {
            var p = g.Player; var comp = VAlly(g) ? g.Companion : null;
            p.ScriptControl = true; p.SetFacing(1); p.AnimOverride = "Wind";
            if (comp != null) { comp.transform.position = p.transform.position + new Vector3(-0.9f, 0, 0); comp.View.Face(1); comp.View.Play("Wind"); }
            yield return Cutscene.Wait(1.2f);
            if (comp != null) { yield return g.Say("c6_roof_v"); Bond(g, "roof"); }
            else yield return g.Say(VDead(g) ? "c6_roof_dead" : "c6_roof_apart");
            yield return Cutscene.Wait(1.0f);
        }

        // ============================================================ 7장 수용동 연결문 (직전)
        /// <summary>남아 있는 회수 가능한 물건·미완료 대화를 보여 주고, 들어갈지 묻는다 (기본 선택 없음). 7장 본문은 아직 없다.</summary>
        static IEnumerator Ch7Gate(Game g)
        {
            var s = g.State;
            yield return g.Say("c7_gate_look");
            yield return g.Info(Ch7Checklist(g, 0));
            yield return g.Info(Ch7Checklist(g, 1));
            yield return g.AskEx("c7_gate_ask", null, true);
            if (g.LastChoice != 0) yield break;
            yield return Ch7Enter(g);
        }

        /// <summary>4~6장 맵에 놓은 과자 (EWRoomsUnderground 와 같은 위치·ID). 회수 확인용.</summary>
        public static readonly string[] Ch46Snacks = { "M16_snack_15_6", "M16_snack_45_14", "M20_snack_27_10", "M22_snack_30_5", "M23_snack_10_24" };

        public static string Ch7Checklist(Game g, int page)
        {
            var s = g.State;
            if (page == 0)
            {
                int left = 0; foreach (var id in Ch46Snacks) if (!s.D.pickedUp.Contains(id)) left++;
                var l = new List<string>();
                l.Add("줍지 않은 과자: " + (left == 0 ? "없음" : left + "개 (승강장·터널·홀·배선통로)"));
                l.Add("물 펌프: " + (s.Has("waste_pump_fixed") ? "고침" : "아직 (폐기소 옆 펌프실)"));
                l.Add("008 에게 과자: " + (s.D.child008Fed ? "줌" : "아직") + " · 시장 등불: " + (s.Has("market_lantern_lit") ? "켬" : s.Has("m19_visited") ? "아직" : "시장 안 들름"));
                return string.Join("\n", l);
            }
            else
            {
                var l = new List<string>();
                l.Add("선택 장소: 폐품 시장 " + (s.Has("m19_visited") ? "들름" : "안 들름") + " · 기숙사 옥상 " + (s.Has("m26_visited") ? "들름" : "안 들름"));
                l.Add("008 의 선: " + (s.Has("side_lines_done") ? "봄" : "아직"));
                if (s.VenelleIsAlly) l.Add("베넬: 방 이야기 " + (s.Has("ivy_room_done") ? "봄" : "아직") + " · 옷 이야기 " + (s.D.sideStoryDone.Contains("clothes") ? "봄" : "아직"));
                else l.Add("숙소 3호: 사진 " + (s.Has("room_photo_seen") ? "봄" : "아직") + " · 편지 " + (s.Has("room_letter_seen") ? "봄" : "아직"));
                l.Add("수용동에 들어가면 이 구역으로는 돌아오지 못한다.");
                return string.Join("\n", l);
            }
        }
    }
}
