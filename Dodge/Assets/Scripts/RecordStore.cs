using System.Collections.Generic;
using UnityEngine;

// 최고 기록 Top 3를 PlayerPrefs에 저장/로드 (게임을 다시 실행해도 유지)
// 점수는 GameManager의 기존 점수(생존 시간)를 그대로 사용한다.
public static class RecordStore
{
    public const int MaxRecords = 3; // 저장할 최대 기록 수
    const string CountKey = "TopRecordCount";
    const string RecordKeyPrefix = "TopRecord_";

    // 저장된 기록을 높은 점수 순으로 반환 (최대 MaxRecords개)
    public static List<float> Load()
    {
        List<float> records = new List<float>();

        if (!PlayerPrefs.HasKey(CountKey))
        {
            // Top 3 기능 이전에 저장된 최고 기록(BestTime)이 있으면 1위로 가져옴
            float legacyBest = PlayerPrefs.GetFloat(GameManager.HighScoreKey, 0f);
            if (legacyBest > 0f) records.Add(legacyBest);
            return records;
        }

        int count = Mathf.Clamp(PlayerPrefs.GetInt(CountKey, 0), 0, MaxRecords);
        for (int i = 0; i < count; i++)
        {
            records.Add(PlayerPrefs.GetFloat(RecordKeyPrefix + i, 0f));
        }
        records.Sort((a, b) => b.CompareTo(a)); // 저장 값이 꼬였더라도 항상 내림차순
        return records;
    }

    // 새 점수를 Top 3에 등록. 순위에 들면 0부터 시작하는 순위를, 들지 못하면 -1을 반환
    public static int Submit(float score)
    {
        if (score <= 0f) return -1;

        List<float> records = Load();

        // 같은 점수는 기존 기록이 먼저 (새 점수는 그 아래에 삽입)
        int rank = records.Count;
        for (int i = 0; i < records.Count; i++)
        {
            if (score > records[i])
            {
                rank = i;
                break;
            }
        }
        if (rank >= MaxRecords) return -1;

        records.Insert(rank, score);
        // 4위 이하 기록 제거
        if (records.Count > MaxRecords) records.RemoveRange(MaxRecords, records.Count - MaxRecords);

        Save(records);
        return rank;
    }

    static void Save(List<float> records)
    {
        PlayerPrefs.SetInt(CountKey, records.Count);
        for (int i = 0; i < MaxRecords; i++)
        {
            if (i < records.Count) PlayerPrefs.SetFloat(RecordKeyPrefix + i, records[i]);
            else PlayerPrefs.DeleteKey(RecordKeyPrefix + i);
        }
        PlayerPrefs.Save();
    }

    // 화면에 표시할 점수 문자열 (게임 중 Time/Score 표시와 동일하게 정수로 표시)
    public static string Format(float score)
    {
        return ((int)score).ToString();
    }
}
