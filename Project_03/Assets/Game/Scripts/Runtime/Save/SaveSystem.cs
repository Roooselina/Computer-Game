using System;
using System.IO;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// JSON 저장. 쓰기는 임시 파일 → 교체 순서로 진행하고 직전 저장을 .bak 으로 남긴다.
    /// 읽기 실패 시 백업으로 복구하며, 둘 다 실패하면 이어하기를 막고 이유를 알려준다.
    /// </summary>
    public static class SaveSystem
    {
        public const int CurrentVersion = 4;
        public const string FileName = "ew_lastlight_save.json";

        public enum LoadResult { Ok, RecoveredFromBackup, NoSave, Corrupt, TooNew }

        /// <summary>테스트에서 저장 위치를 바꿀 때 사용.</summary>
        public static string OverrideDirectory;

        public static string Directory => string.IsNullOrEmpty(OverrideDirectory) ? Application.persistentDataPath : OverrideDirectory;
        public static string MainPath => Path.Combine(Directory, FileName);
        public static string BackupPath => MainPath + ".bak";
        static string TempPath => MainPath + ".tmp";

        public static bool HasAnySave => File.Exists(MainPath) || File.Exists(BackupPath);

        public static bool Write(SaveData data, out string error)
        {
            error = null;
            try
            {
                System.IO.Directory.CreateDirectory(Directory);
                data.version = CurrentVersion;
                data.savedAtUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(TempPath, json);
                if (File.Exists(MainPath))
                {
                    // 정상적으로 읽히는 기존 저장만 백업으로 남긴다.
                    if (TryParse(MainPath, out _, out _, out _)) File.Copy(MainPath, BackupPath, true);
                    File.Delete(MainPath);
                }
                File.Move(TempPath, MainPath);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                Debug.LogError("[EW] 저장 실패: " + e);
                return false;
            }
        }

        public static LoadResult TryLoad(out SaveData data, out string message)
        {
            data = null; message = null;
            bool mainExists = File.Exists(MainPath), bakExists = File.Exists(BackupPath);
            if (!mainExists && !bakExists) { message = "저장 데이터가 없습니다."; return LoadResult.NoSave; }
            bool tooNew = false; string why = null;
            if (mainExists && TryParse(MainPath, out data, out tooNew, out why)) return LoadResult.Ok;
            if (mainExists && tooNew) { message = "이 버전보다 새로운 저장 데이터입니다. 게임을 업데이트해 주세요."; data = null; return LoadResult.TooNew; }
            if (bakExists && TryParse(BackupPath, out data, out _, out _))
            {
                message = "저장 파일이 손상되어 직전 백업에서 불러왔습니다." + (why != null ? " (" + why + ")" : "");
                return LoadResult.RecoveredFromBackup;
            }
            data = null;
            message = "저장 파일이 손상되어 불러올 수 없습니다. 새 게임을 시작해 주세요.";
            return LoadResult.Corrupt;
        }

        static bool TryParse(string path, out SaveData data, out bool tooNew, out string why)
        {
            data = null; tooNew = false; why = null;
            try
            {
                string text = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(text)) { why = "빈 파일"; return false; }
                var d = JsonUtility.FromJson<SaveData>(text);
                if (d == null) { why = "형식 오류"; return false; }
                if (d.version <= 0) { why = "버전 정보 없음"; return false; }
                if (d.version > CurrentVersion) { tooNew = true; why = "새 버전"; return false; }
                // 옛 버전 저장은 변환 전에 원본을 따로 보존한다 (지우지 않음)
                if (d.version < CurrentVersion)
                {
                    string keep = path + ".v" + d.version + "_original";
                    try { if (!File.Exists(keep)) File.Copy(path, keep); } catch (Exception) { }
                    LastMigratedFrom = d.version;
                }
                Migrate(d);
                d.Sanitize();
                data = d;
                return true;
            }
            catch (Exception e)
            {
                why = "읽기 오류: " + e.GetType().Name;
                return false;
            }
        }

        /// <summary>
        /// 이전 버전 저장을 현재 구조로 옮긴다.
        /// v1 → v2: 오프닝 교체·맵 통합으로 맵 ID 가 바뀌었다. 옛 맵은 가장 가까운 새 맵의 시작 지점으로 옮기고
        /// (저장 위치 좌표는 버린다), 이미 지난 오프닝 장면의 완료 플래그를 채운다.
        /// </summary>
        /// <summary>마지막으로 읽은 저장이 옛 버전에서 변환되었으면 그 버전 (안내용, 0 = 변환 없음).</summary>
        public static int LastMigratedFrom;

        public static void Migrate(SaveData d)
        {
            if (d.version < 2)
            {
                string old = d.mapId ?? "";
                string map = null, spawn = "start";
                switch (old)
                {
                    case "M00": case "M01": case "M02": case "M03": map = "P01"; break;          // 옛 프롤로그 → 새 오프닝 처음부터
                    case "M04": map = "C01"; spawn = "tank"; break;
                    case "M05": map = "C02"; spawn = "from_lab"; break;
                    case "M06": map = "C02"; spawn = "rest"; break;
                    case "M07": case "M08": map = "C03"; break;
                    case "M10": map = "C04"; break;
                    case "M12": map = "C05"; break;
                }
                if (map != null) { d.mapId = map; d.spawnId = spawn; d.hasPos = false; }
                bool pastOpening = map != "P01";
                if (pastOpening)
                    foreach (var f in new[] { "op_village_done", "op_house_done", "op_desktop_done" }) if (!d.flags.Contains(f)) d.flags.Add(f);
                if (d.flags.Contains("c1_tank_broken"))
                    foreach (var f in new[] { "op_tank_done", "op_r_in", "c1_researchers_down", "c1_left_lab" }) if (!d.flags.Contains(f)) d.flags.Add(f);
                if (map == "C01" && !d.flags.Contains("c1_tank_broken")) { d.mapId = "P03"; d.spawnId = "start"; d.flags.Remove("op_desktop_done"); }
                // 옛 1장 전투를 끝낸 저장이면 지름길도 열린 상태
                if (d.clearedEncounters.Contains("M05_e1")) { if (!d.clearedEncounters.Contains("C02_guard")) d.clearedEncounters.Add("C02_guard"); if (!d.flags.Contains("c1_shortcut_open")) d.flags.Add("c1_shortcut_open"); }
                // 탈출 구역 이후 저장이면 그 구역의 첫 관찰 장면은 이미 지난 것으로
                if (d.mapId != "P01" && d.mapId != "P03" && d.mapId != "C01" && !d.flags.Contains("c1_escape_view")) d.flags.Add("c1_escape_view");
            }
            if (d.version < 3)
            {
                // v2 → v3: 턴제 전투를 실시간 잠입 액션으로 교체.
                // - 맵 ID 는 그대로. 탈출 구역 이후라면 정체 깨달음·경보 사건을 이미 지난 것으로 둔다.
                // - 베넬 만남 장소가 혼합실(M13) 보스전 → 새 복도(C06)로 바뀌었다: 만남 전 저장은 복도 입구로 옮긴다.
                // - 처치 기록(clearedEncounters)·기물 등록은 더 이상 쓰지 않지만 지우지 않는다 (보상 중복 방지 기록과 함께 보존).
                d.Sanitize();
                bool leftLab = d.flags.Contains("c1_left_lab") || (d.mapId != "P01" && d.mapId != "P02" && d.mapId != "P03" && d.mapId != "C01");
                if (leftLab)
                    foreach (var f in new[] { "lab_woke", "lab_realized", "lab_alarm", "op_tank_done" }) if (!d.flags.Contains(f)) d.flags.Add(f);
                if (d.flags.Contains("op_tank_done") && !d.flags.Contains("lab_woke")) d.flags.Add("lab_woke");
                if (d.mapId == "M13" && !d.flags.Contains("c3_boss_done")) { d.mapId = "C06"; d.spawnId = "start"; d.hasPos = false; }
                if (d.mapId == "C02" || d.mapId == "C03" || d.mapId == "C04" || d.mapId == "C05") { d.hasPos = false; if (string.IsNullOrEmpty(d.spawnId)) d.spawnId = "start"; }
                if (d.flags.Contains("c3_boss_done")) d.venelleStage = 3;
                if (!d.skills.Contains("pull") && leftLab && d.mapId != "C02") d.skills.Add("pull");
            }
            if (d.version < 4)
            {
                // v3 → v4: Q/E/R/F 고정 슬롯, 전원선 이동(R) 기술, 컴퓨터 안 탐색·베넬 분기 개편.
                // - 가진 기술은 지우지 않고 기본 네 기술을 더한 뒤 기본 배치로 슬롯을 채운다.
                // - 잠입 구역(C02~C05) 구조가 바뀌어 저장 좌표는 버리고 그 맵 시작 지점에서 이어 한다.
                // - 컴퓨터 안(P03) 저장은 착지 지점에서 다시 탐색한다.
                d.Sanitize();
                foreach (var sk in PlayerSkills.DefaultSkills) if (!d.skills.Contains(sk)) d.skills.Add(sk);
                d.slots = new System.Collections.Generic.List<string>(PlayerSkills.DefaultSlots);
                if (d.mapId == "P03" || d.mapId == "C02" || d.mapId == "C03" || d.mapId == "C04" || d.mapId == "C05" || d.mapId == "C06") { d.hasPos = false; d.spawnId = "start"; if (d.mapId == "C02") d.spawnId = "from_lab"; }
                if (d.flags.Contains("op_desktop_done")) { if (!d.flags.Contains("pc_guide_read")) d.flags.Add("pc_guide_read"); }
                if (d.flags.Contains("c3_joined") && d.venelleState == "pending") d.venelleState = "ally";
                if (d.flags.Contains("c3_boss_done") && !d.flags.Contains("venelle_spared")) d.flags.Add("venelle_spared");
                // 시설 경보는 베넬 만남에서 끝난다 (v3 변환이 붙인 경보 플래그를 3장 결말 뒤 저장에서는 지운다)
                if (d.flags.Contains("c3_boss_done")) d.flags.Remove("lab_alarm");
            }
            d.version = CurrentVersion;
        }

        public static void DeleteAll()
        {
            foreach (var p in new[] { MainPath, BackupPath, TempPath })
                if (File.Exists(p)) File.Delete(p);
        }
    }
}
