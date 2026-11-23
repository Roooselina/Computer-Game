using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 진행 상태. 저장 파일의 SaveData 를 감싸서 플래그·아이템·성장 정보를 다룬다.
    /// 이 클래스는 MonoBehaviour 가 아니므로 자동 테스트에서 그대로 사용할 수 있다.
    /// </summary>
    public class GameState
    {
        public SaveData D;
        public GameDatabase Db;

        public GameState(SaveData d, GameDatabase db) { D = d; Db = db; D.Sanitize(); FillHp(D.lu, "lu"); }

        // ---------- 플래그 ----------
        public bool Has(string flag) => D.flags.Contains(flag);
        public void Set(string flag, bool on = true)
        {
            if (on) { if (!D.flags.Contains(flag)) D.flags.Add(flag); }
            else D.flags.Remove(flag);
        }
        /// <summary>처음 한 번만 true. 이벤트 중복 실행 방지용.</summary>
        public bool Once(string flag) { if (Has(flag)) return false; Set(flag); return true; }

        public int GetVar(string k) { foreach (var v in D.vars) if (v.k == k) return v.v; return 0; }
        public void SetVar(string k, int value) { foreach (var v in D.vars) if (v.k == k) { v.v = value; return; } D.vars.Add(new KV(k, value)); }

        // ---------- 아이템 ----------
        public int Count(string item) { foreach (var i in D.items) if (i.k == item) return i.v; return 0; }
        public void AddItem(string item, int n = 1)
        {
            foreach (var i in D.items) if (i.k == item) { i.v = Mathf.Clamp(i.v + n, 0, 99); return; }
            if (n > 0) D.items.Add(new KV(item, Mathf.Min(n, 99)));
        }
        public bool UseItem(string item)
        {
            foreach (var i in D.items) if (i.k == item && i.v > 0) { i.v--; return true; }
            return false;
        }

        // ---------- 집합형 기록 ----------
        public static bool AddUnique(List<string> list, string id) { if (list.Contains(id)) return false; list.Add(id); return true; }

        // ---------- 성장 ----------
        public CharacterGrowth Growth(string who) => Db.GetGrowth(who);
        public PartyMember Member(string who) => who == "venelle" ? D.venelle : D.lu;
        public int MaxHp(string who)
        {
            if (who == "lu")
            {
                var t = ActionTuning.I.player;
                int normal = t.baseStability + t.stabilityPerLevel * (D.lu.level - 1);
                if (D.limitReleases <= 0) return normal;
                int baseMax = D.limitBaseMax > 0 ? D.limitBaseMax : normal;
                return Mathf.Max(1, normal - Mathf.RoundToInt(baseMax * LimitStep * D.limitReleases));
            }
            var g = Growth(who); return g.baseHp + g.hpPerLevel * (Member(who).level - 1);
        }
        public int Atk(string who) { var g = Growth(who); return g.baseAtk + g.atkPerLevel * (Member(who).level - 1); }
        public int Def(string who) { var g = Growth(who); return g.baseDef + g.defPerLevel * (Member(who).level - 1); }
        public int MaxCharge(string who)
        {
            int lv = Member(who).level;
            int bonus = (lv >= 3 ? 1 : 0) + (lv >= 6 ? 1 : 0);
            if (who == "lu") return Mathf.Min(8, ActionTuning.I.player.chargeMax + bonus);
            var g = Growth(who); return g.baseCharge + bonus;
        }
        public void FillHp(PartyMember m, string who) { if (m.hp < 0 || m.hp > MaxHp(who)) m.hp = MaxHp(who); }

        public static readonly int[] ExpTable = { 0, 30, 80, 150, 240, 350, 480, 640, 830, 1050 };
        public static int ExpForLevel(int level) => ExpTable[Mathf.Clamp(level - 1, 0, ExpTable.Length - 1)];

        /// <summary>경험치 추가. 오른 레벨 수를 돌려준다. 레벨 상한 10.</summary>
        /// <summary>진행 보상: 한 번만 지급 (중복 방지 키). 오른 레벨 수.</summary>
        public int Reward(string key, int exp)
        {
            if (!Once("rw_" + key)) return 0;
            return GainExp("lu", exp);
        }

        public int GainExp(string who, int amount)
        {
            var m = Member(who);
            int oldMax = MaxHp(who);
            m.exp += Mathf.Max(0, amount);
            int ups = 0;
            while (m.level < 10 && m.exp >= ExpForLevel(m.level + 1)) { m.level++; ups++; }
            if (ups > 0) m.hp = Mathf.Min(MaxHp(who), m.hp + (MaxHp(who) - oldMax));
            return ups;
        }

        public void HealAll()
        {
            D.lu.hp = MaxHp("lu"); D.lu.charge = MaxCharge("lu"); D.lu.state = "stable";
            if (D.venelleState == "ally") { D.venelle.hp = MaxHp("venelle"); D.venelle.state = "stable"; }
        }

        // ---------- 한계 방출 (GDD 10장): 최초 최대 안정도의 20%씩 최대치를 영구히 줄인다. 세 번 뒤 40%.
        public const float LimitStep = 0.2f;
        /// <summary>n 번째(1~3) 방출을 적용한다. 같은 번호는 한 번만 (재시도·불러오기 뒤 중복 누적 없음). 현재 안정도는 새 최대치를 넘는 만큼만 줄인다.</summary>
        public bool ApplyLimitRelease(int n)
        {
            if (n < 1 || n > 3 || D.limitReleases >= n) return false;
            if (!Once("limit_release_" + n)) { D.limitReleases = Mathf.Max(D.limitReleases, n); return false; }
            if (D.limitBaseMax <= 0) D.limitBaseMax = MaxHp("lu");
            D.limitReleases = n;
            int max = MaxHp("lu");
            if (D.lu.hp > max) D.lu.hp = max;
            return true;
        }

        public bool RavielIsAlly => D.ravielState == "ally";

        public bool VenelleIsAlly => D.venelleState == "ally" && D.venelle.state != "dead";

        // ---------- 장치 ----------
        public DeviceSave GetDevice(string id) { foreach (var d in D.devices) if (d.id == id) return d; return null; }
        public void StoreDevice(string id, DevicePower p, bool hasBattery, int battery)
        {
            var s = GetDevice(id);
            if (s == null) { s = new DeviceSave { id = id }; D.devices.Add(s); }
            s.power = (int)p; s.hasBattery = hasBattery; s.battery = Mathf.Clamp(battery, 0, 100);
        }

        /// <summary>
        /// 전투 재시도용 복원. 소모품·경험치·장치 상태는 전투 직전으로 되돌리되,
        /// 대화·말풍선 출력 이력은 현재 값을 유지한다 (GDD 7.2).
        /// </summary>
        public void RestoreForRetry(SaveData snapshot)
        {
            var seenD = new List<string>(D.dialogueSeen);
            var seenB = new List<string>(D.bubblesSeen);
            float play = D.playSeconds; int retries = D.retries;
            D = snapshot.Clone();
            D.dialogueSeen = seenD; D.bubblesSeen = seenB; D.playSeconds = play; D.retries = retries;
            D.Sanitize();
        }
    }
}
