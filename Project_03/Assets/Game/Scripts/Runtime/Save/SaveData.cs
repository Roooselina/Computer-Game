using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    [Serializable] public class KV { public string k; public int v; public KV() { } public KV(string k, int v) { this.k = k; this.v = v; } }

    [Serializable]
    public class PartyMember
    {
        public int level = 1;
        public int exp;
        public int hp = -1;          // -1 = 최대치로 채움
        public int charge = 3;
        /// <summary>GDD 9.3 character_state: stable / incapacitated / dead</summary>
        public string state = "stable";
    }

    [Serializable]
    public class DeviceSave
    {
        public string id;
        public int power;            // DevicePower 값
        public bool hasBattery;
        public int battery;          // 0~100
    }

    /// <summary>
    /// 저장 파일에 그대로 기록되는 데이터. GDD 9.3 저장 데이터 표의 항목 이름을 따른다.
    /// version 필드가 0이면 (없으면) 손상/구형 파일로 취급한다.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version;                     // 새로 만들 때 SaveSystem.CurrentVersion
        public string savedAtUtc;
        public float playSeconds;

        // story_checkpoint + 현재 위치
        public string mapId;
        public string spawnId;
        public bool hasPos;
        public float posX, posY;
        public int facing = 1;
        public string checkpointId;
        public string chapterId;

        public List<string> flags = new List<string>();
        public List<KV> vars = new List<KV>();
        public List<KV> items = new List<KV>();          // item_inventory

        public PartyMember lu = new PartyMember();
        public PartyMember venelle = new PartyMember();
        public string venelleState = "pending";          // pending / ally / left / dead
        public string venelleDeathScene = "none";
        public int venelleTrust;                         // 0~3

        public List<DeviceSave> devices = new List<DeviceSave>();   // world_devices
        public List<string> dialogueSeen = new List<string>();      // dialogue_seen
        public List<string> bubblesSeen = new List<string>();
        public List<string> docs = new List<string>();              // docs_collected
        public List<string> clearedEncounters = new List<string>();
        public List<string> pickedUp = new List<string>();
        public List<string> registeredProps = new List<string>();
        public string difficulty = "story";                         // difficulty_profile
        // ---- v3 실시간 잠입·액션
        public List<string> skills = new List<string>();            // 장착 스킬 (순서 = Q/E 순환 순서)
        public int skillIndex;
        public int retries;                                         // 구간 재시도 횟수 (기록용)
        public int quietSections;                                   // 발견되지 않고 통과한 구간 수
        public int venelleStage;                                    // 베넬 대화 단계 0~3 (만남 대결 중)
        // ---- v4 Q/E/R/F 직접 사용 (2026-11-15)
        public List<string> slots = new List<string>();             // Q/E/R/F 슬롯에 장착한 기술 id (빈 칸 = "")
        // ---- 4~6장 (2026-11-15, v4 와 호환: 없으면 기본값)
        public string wasteResult = "none";                         // GDD 9.3 waste_result: none / gift / repair / fight
        public bool wasteReconciled;                                // 충돌 뒤 수리로 화해
        public bool child008Fed;                                    // 008 에게 과자를 줌
        /// <summary>F 의 지하 대피실에 모인 무리 (시설 밖으로 나간 사람은 없다). 지금은 "waste_residents" 만.
        /// 7장 수용동에서 풀려난 사람은 그때 "holding_released" 로 더한다 — 아직 일어나지 않은 구출을 미리 넣지 않는다.</summary>
        public List<string> shelterGroups = new List<string>();
        public List<string> sideStoryDone = new List<string>();     // GDD 9.3 side_story_done: clothes / cup / lines
        // 7~11장 (2026-11-15)
        public string ravielState = "none";                         // none / ally / injured / dead
        public string friendRoute = "none";                         // GDD 9.3 friend_route: flee / fight
        public string reunionChoice = "none";                       // GDD 9.3 reunion_choice: hand / sit
        public string lacrimosaResult = "none";                     // GDD 9.3 lacrimosa_result: persuaded / subdued
        public int limitReleases;                                   // 한계 방출 횟수 0~3 (최대 안정도 영구 감소)
        public int limitBaseMax;                                    // 첫 방출 때의 최초 최대 안정도
        public List<string> visitedMaps = new List<string>();       // 방문 순서 (검수 도구 '이전 맵')

        public static SaveData NewGame()
        {
            var d = new SaveData { version = SaveSystem.CurrentVersion, mapId = "P01", spawnId = "start", chapterId = "prologue" };
            d.lu.hp = -1; d.lu.charge = 3;
            d.skills.AddRange(PlayerSkills.DefaultSkills);
            d.slots.AddRange(PlayerSkills.DefaultSlots);
            return d;
        }

        public SaveData Clone() => JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(this));

        /// <summary>null 목록과 범위를 벗어난 값을 바로잡는다. 손상 일부는 안전한 값으로 복구.</summary>
        public void Sanitize()
        {
            flags ??= new List<string>(); vars ??= new List<KV>(); items ??= new List<KV>();
            devices ??= new List<DeviceSave>(); dialogueSeen ??= new List<string>(); bubblesSeen ??= new List<string>();
            docs ??= new List<string>(); clearedEncounters ??= new List<string>(); pickedUp ??= new List<string>();
            registeredProps ??= new List<string>();
            shelterGroups ??= new List<string>(); sideStoryDone ??= new List<string>(); visitedMaps ??= new List<string>();
            if (string.IsNullOrEmpty(ravielState)) ravielState = "none";
            if (string.IsNullOrEmpty(friendRoute)) friendRoute = "none";
            if (string.IsNullOrEmpty(reunionChoice)) reunionChoice = "none";
            if (string.IsNullOrEmpty(lacrimosaResult)) lacrimosaResult = "none";
            limitReleases = Mathf.Clamp(limitReleases, 0, 3); limitBaseMax = Mathf.Max(0, limitBaseMax);
            if (visitedMaps.Count > 64) visitedMaps.RemoveRange(0, visitedMaps.Count - 64);
            if (string.IsNullOrEmpty(wasteResult)) wasteResult = "none";
            skills ??= new List<string>();
            skills.RemoveAll(x => string.IsNullOrEmpty(x));
            if (skills.Count == 0) skills.AddRange(PlayerSkills.DefaultSkills);
            skillIndex = Mathf.Clamp(skillIndex, 0, skills.Count - 1);
            slots ??= new List<string>();
            if (slots.Count == 0) slots.AddRange(PlayerSkills.DefaultSlots);
            while (slots.Count < 4) slots.Add("");
            if (slots.Count > 4) slots.RemoveRange(4, slots.Count - 4);
            for (int i = 0; i < 4; i++) if (slots[i] == null || (slots[i] != "" && !skills.Contains(slots[i]))) slots[i] = "";
            venelleStage = Mathf.Clamp(venelleStage, 0, 3);
            lu ??= new PartyMember(); venelle ??= new PartyMember();
            if (string.IsNullOrEmpty(mapId)) { mapId = "P01"; spawnId = "start"; hasPos = false; }
            if (string.IsNullOrEmpty(venelleState)) venelleState = "pending";
            if (string.IsNullOrEmpty(venelleDeathScene)) venelleDeathScene = "none";
            if (string.IsNullOrEmpty(difficulty)) difficulty = "story";
            venelleTrust = Mathf.Clamp(venelleTrust, 0, 3);
            lu.level = Mathf.Clamp(lu.level, 1, 10); venelle.level = Mathf.Clamp(venelle.level, 1, 10);
            lu.exp = Mathf.Max(0, lu.exp); lu.charge = Mathf.Max(0, lu.charge);
            if (float.IsNaN(posX) || float.IsNaN(posY) || float.IsInfinity(posX) || float.IsInfinity(posY)) hasPos = false;
            items.RemoveAll(i => i == null || string.IsNullOrEmpty(i.k));
            foreach (var i in items) i.v = Mathf.Clamp(i.v, 0, 99);
            vars.RemoveAll(v => v == null || string.IsNullOrEmpty(v.k));
            devices.RemoveAll(v => v == null || string.IsNullOrEmpty(v.id));
            foreach (var dv in devices) { dv.battery = Mathf.Clamp(dv.battery, 0, 100); if (dv.power < 0 || dv.power > 2) dv.power = 0; }
        }
    }
}
