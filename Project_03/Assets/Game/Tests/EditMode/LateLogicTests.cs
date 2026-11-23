using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace EW.Tests
{
    /// <summary>7~11장 규칙 (2026-11-15): 한계 방출 최대치 감소, 휴지통 복원 순서, 검수 프리셋, 배치 데이터 검사.</summary>
    public class LateLogicTests
    {
        static GameState S() => new GameState(SaveData.NewGame(), DefaultContent.Create());

        [Test]
        public void LimitRelease_ReducesMax20PercentEach_To40_NoDoubleOnRetry()
        {
            var s = S();
            int baseMax = s.MaxHp("lu");
            s.D.lu.hp = baseMax;
            Assert.That(s.ApplyLimitRelease(1), Is.True);
            Assert.That(s.MaxHp("lu"), Is.EqualTo(baseMax - Mathf.RoundToInt(baseMax * 0.2f)));
            Assert.That(s.D.lu.hp, Is.EqualTo(s.MaxHp("lu")), "현재 안정도는 새 최대치를 넘는 만큼만 줄어든다");
            var snap = s.D.Clone();
            Assert.That(s.ApplyLimitRelease(1), Is.False, "같은 번호는 한 번만");
            // 재시도: 체크포인트로 되돌린 뒤 다시 1번을 적용해도 누적되지 않는다
            s.RestoreForRetry(snap);
            s.ApplyLimitRelease(1);
            Assert.That(s.D.limitReleases, Is.EqualTo(1));
            Assert.That(s.MaxHp("lu"), Is.EqualTo(baseMax - Mathf.RoundToInt(baseMax * 0.2f)));
            s.D.lu.hp = 5;
            s.ApplyLimitRelease(2);
            Assert.That(s.D.lu.hp, Is.EqualTo(5), "새 최대치보다 낮은 현재값은 그대로");
            s.ApplyLimitRelease(3);
            Assert.That(s.MaxHp("lu"), Is.EqualTo(baseMax - 3 * Mathf.RoundToInt(baseMax * 0.2f)));
            Assert.That(s.MaxHp("lu"), Is.EqualTo(Mathf.RoundToInt(baseMax * 0.4f)).Within(1), "세 번 뒤 최초 최대치의 40%");
            Assert.That(s.ApplyLimitRelease(4), Is.False);
        }

        [Test]
        public void RecycleBin_SortSelectRestore_ThenRelease_EitherOrderWaits()
        {
            var b = RecycleBinUI.MakeDefault();
            Assert.That(b.RestoreSelected(), Does.Contain("먼저"));
            Assert.That(b.Items.FindIndex(i => i.isLu), Is.GreaterThan(0), "처음엔 이름 순으로 묻혀 있다");
            b.SortByDeleted();
            Assert.That(b.Items[0].isLu, Is.True, "최근 삭제 순 맨 위");
            Assert.That(b.Items[0].origin + b.Items[0].name, Is.EqualTo(RecycleBinUI.LuOrigin));
            b.Selected = b.Items.FindIndex(i => i.name.Contains("손상"));
            b.RestoreSelected();
            Assert.That(b.LuRestored, Is.False, "손상된 옛 사본은 복원되지 않는다");
            b.Released = true;   // 라비엘이 먼저 구속 해제를 눌러도
            Assert.That(b.CanConfirm, Is.False, "복원 전에는 내부 확인 불가 (대기)");
            b.Selected = 0;
            Assert.That(b.RestoreSelected(), Does.Contain(RecycleBinUI.LuOrigin));
            Assert.That(b.CanConfirm, Is.True);
            b.Selected = b.Items.FindIndex(i => i.isDoc);
            b.RestoreSelected();
            Assert.That(b.DocRestored, Is.True);
        }

        [Test]
        public void ReviewPresets_BuildConsistentStates()
        {
            WorldLayout.Reload();
            var L = WorldLayout.I;
            Assert.That(L.Errors, Is.Empty, string.Join("\n", L.Errors));
            Assert.That(L.Events.Count, Is.GreaterThanOrEqualTo(18));
            // 8장 이후 프리셋은 '동행 생존' 베넬을 고를 수 없다 (7장 격벽에서 사망)
            foreach (var e in L.Events.Where(x => x.chapter == "ch8" || x.chapter == "ch9" || x.chapter == "ch10" || x.chapter == "ch11"))
                Assert.That(ReviewMode.VenelleOptions(e), Does.Not.Contain("ally"), e.id);
            var d = ReviewMode.BuildPreset(SaveData.NewGame(), L.GetEvent("ch10_phase2"), "apart", out var ch);
            Assert.That(d.venelleState, Is.EqualTo("dead")); Assert.That(d.venelleDeathScene, Is.EqualTo("shelter"), "비동행 생존은 9장 붕괴에서 사망");
            Assert.That(d.limitReleases, Is.EqualTo(1)); Assert.That(d.ravielState, Is.EqualTo("injured")); Assert.That(d.chapterId, Is.EqualTo("ch10"));
            Assert.That(d.flags, Does.Contain("c3_boss_done")); Assert.That(d.flags, Does.Contain("holding_released"));
            Assert.That(ch.Count, Is.GreaterThan(5));
            var d2 = ReviewMode.BuildPreset(SaveData.NewGame(), L.GetEvent("ch7_bulkhead"), "ally", out _);
            Assert.That(d2.venelleState, Is.EqualTo("ally")); Assert.That(d2.flags, Does.Not.Contain("c7_bulkhead_closed"));
            var d3 = ReviewMode.BuildPreset(SaveData.NewGame(), L.GetEvent("ch8_reunion"), "ally_bulkhead", out _);
            Assert.That(d3.venelleState, Is.EqualTo("dead")); Assert.That(d3.venelleDeathScene, Is.EqualTo("bulkhead"));
            var d4 = ReviewMode.BuildPreset(SaveData.NewGame(), L.GetEvent("pre_venelle"), "keep", out _);
            Assert.That(d4.venelleState, Is.EqualTo("pending"));
            Assert.That(d4.flags, Does.Not.Contain("c3_boss_done"));
        }

        [Test]
        public void Layout_SafeSpawn_ForRemovedMapsAndChapters()
        {
            WorldLayout.Reload();
            var L = WorldLayout.I;
            var a = L.SafeSpawnFor("M05", "ch1"); Assert.That(a.map, Is.EqualTo("C02"));
            var b = L.SafeSpawnFor("M99_REMOVED", "ch9"); Assert.That(b.map, Is.EqualTo("M36"));
            Assert.That(L.SafeSpawnFor("X", "nochapter"), Is.Null);
            Assert.That(L.NextOf("M39").Select(n => n.map), Does.Contain("M42"));
            Assert.That(L.NextOf("M24").Select(n => n.map), Does.Contain("M27"));
        }

        [Test]
        public void Layout_Check_NoBrokenExits_Unreachable_OrDuplicates()
        {
            var problems = EW.EditorTools.EWLayoutCheck.Check();
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void SaveData_OldSaveWithoutNewFields_Sanitizes()
        {
            var json = "{\"version\":4,\"mapId\":\"M24\",\"flags\":[\"c3_boss_done\"]}";
            var d = JsonUtility.FromJson<SaveData>(json);
            d.Sanitize();
            Assert.That(d.ravielState, Is.EqualTo("none")); Assert.That(d.friendRoute, Is.EqualTo("none"));
            Assert.That(d.limitReleases, Is.EqualTo(0)); Assert.That(d.visitedMaps, Is.Not.Null);
        }
    }
}
