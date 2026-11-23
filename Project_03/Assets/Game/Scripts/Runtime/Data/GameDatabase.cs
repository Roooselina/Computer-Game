using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    public enum SkillTarget { Single, UpToTwo, AllEnemies, Self }
    public enum EnemyAI { Guard, Drone, Carrier, VenelleBoss }
    public enum PropKind { ExposedWire, ControlTerminal, ChemicalPuddle }

    [Serializable]
    public class SkillDef
    {
        public string id;
        public string name;
        [TextArea] public string desc;
        public int cost;
        public SkillTarget target;
        public float coef = 1f;
        public int hits = 1;
        public int unlockLevel = 1;
        [Range(0, 1)] public float staggerChance;
        public bool alwaysHits = true;
        public int minDamage = 1;
        public int chargeGain;
        [Range(0, 1)] public float guard;      // 이번 턴 받는 피해 감소율
        public string anim = "Flick";            // CharacterView 상태 이름
        public string fx = "Spark";
        public string line;                      // 사용할 때 짧은 대사(선택)
    }

    [Serializable]
    public class EnemyDef
    {
        public string id;
        public string name;
        public string artId;
        public int hp = 20, atk = 6, def = 1, exp = 8;
        public EnemyAI ai;
        public bool machine = true;
        public bool flying;
        public string dropItem;
        [Range(0, 1)] public float dropChance = 0.3f;
        [Tooltip("방어 자세 피해 감소 (0~1). 노출 전선·번개 발차기의 자세 무너뜨리기로 해제된다.")]
        [Range(0, 0.9f)] public float shield;
        [TextArea] public string note;
    }

    [Serializable]
    public class ItemDef
    {
        public string id;
        public string name;
        [TextArea] public string desc;
        [Range(0, 1)] public float healPct;
        public int charge;
        public bool battleUse = true;
        public bool fieldUse = true;
        public string iconSprite;
    }

    [Serializable]
    public class CharacterGrowth
    {
        public string id;
        public string name;
        public int baseHp = 40, hpPerLevel = 8;
        public int baseAtk = 8, atkPerLevel = 2;
        public int baseDef = 3, defPerLevel = 1;
        public int baseCharge = 6;
    }

    /// <summary>GDD 4.5: 난이도별 배율을 데이터로 분리. 기본은 '이야기'.</summary>
    [Serializable]
    public class DifficultyDef
    {
        public string id = "story";
        public string name = "이야기";
        public float enemyDamageMul = 0.7f;
        public float healMul = 1f;
        public float fieldChargeSeconds = 3f;
        [Range(0, 1)] public float critChance = 0.15f;
        public float critMul = 1.5f;
        [Range(0, 1)] public float talkBaseChance = 0.7f;
        [Tooltip("패배 후 '전투 직전부터 다시'를 고르면 안정도를 채운다 (이야기 난이도: 같은 패배 반복 방지).")]
        public bool retryHeal = true;
    }

    [Serializable]
    public class PropDef
    {
        public PropKind kind;
        public string name;
        [TextArea] public string desc;
        public int cost;
        public float coef;
        public int maxTargets;
        public string firstLine;   // 처음 쓸 때의 짧은 반응
    }

    /// <summary>캐릭터·기술·적·아이템 데이터. Inspector 에서 수치를 조정한다.</summary>
    [CreateAssetMenu(menuName = "EW/Game Database", fileName = "GameDatabase")]
    public class GameDatabase : ScriptableObject
    {
        public List<CharacterGrowth> characters = new List<CharacterGrowth>();
        public List<SkillDef> skills = new List<SkillDef>();
        public List<EnemyDef> enemies = new List<EnemyDef>();
        public List<ItemDef> items = new List<ItemDef>();
        public List<PropDef> props = new List<PropDef>();
        public List<DifficultyDef> difficulties = new List<DifficultyDef>();

        public SkillDef GetSkill(string id) { foreach (var s in skills) if (s.id == id) return s; return null; }
        public EnemyDef GetEnemy(string id) { foreach (var s in enemies) if (s.id == id) return s; return null; }
        public ItemDef GetItem(string id) { foreach (var s in items) if (s.id == id) return s; return null; }
        public PropDef GetProp(PropKind k) { foreach (var s in props) if (s.kind == k) return s; return null; }
        public CharacterGrowth GetGrowth(string id) { foreach (var s in characters) if (s.id == id) return s; return characters.Count > 0 ? characters[0] : new CharacterGrowth(); }
        public DifficultyDef GetDifficulty(string id) { foreach (var s in difficulties) if (s.id == id) return s; return difficulties.Count > 0 ? difficulties[0] : new DifficultyDef(); }

        public List<SkillDef> SkillsFor(int level)
        {
            var r = new List<SkillDef>();
            foreach (var s in skills) if (s.unlockLevel <= level && s.id != "tok") r.Add(s);
            return r;
        }
    }
}
