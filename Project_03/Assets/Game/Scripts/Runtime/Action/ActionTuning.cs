using System;
using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    /// <summary>
    /// 실시간 잠입·액션 수치. Resources/EW/action_tuning.json 에서 읽는다 (코드 수정 없이 조정).
    /// 시간은 모두 초 단위. 턴제 비용·지속 턴은 쓰지 않는다. 설명: Docs/STEALTH_COMBAT_SPEC.md
    /// </summary>
    [Serializable]
    public class ActionTuning
    {
        [Serializable]
        public class PlayerT
        {
            public float walk = 4.0f, crouchWalk = 1.7f, carry = 2.3f, climb = 3.2f;
            public float jumpVelocity = 12.6f, gravity = 38f;
            public float hurtInvuln = 1.1f, hurtStun = 0.25f, hurtKnockback = 3.2f;
            public float dodgeDistance = 2.4f, dodgeTime = 0.15f, dodgeInvuln = 0.15f;
            public float doubleTapWindow = 0.25f;
            /// <summary>대시가 끝난 뒤 다시 쓸 수 있을 때까지 (시작 시점이 아니라 종료 시점부터 센다 — 이중 대기 방지).</summary>
            public float dashCooldownFromEnd = 0.15f;
            /// <summary>대시 무적은 이 간격보다 자주 다시 주지 않는다 (연속 대시로 무한 무적 방지). 이동 대시 자체는 막지 않는다.</summary>
            public float dashInvulnRearm = 0.55f;
            public int chargeMax = 4;
            public float chargeRegenSeconds = 2.2f;
            public float footstepNoise = 1.6f, landNoise = 2.4f;
            public int baseStability = 40, stabilityPerLevel = 6;
        }

        [Serializable]
        public class SkillT
        {
            public string id, name, desc;
            /// <summary>슬롯·강조 표시용 짧은 이름. 안내서: target(적에게/기계에게), example(실전 사용 예).</summary>
            public string shortName, target, example;
            /// <summary>attack(즉시 근접) / projectile(즉시 원거리) / targeted(자동 대상 간섭) / source(전원+목표 자동 연결) / wire(전원선 이동)</summary>
            public string kind = "attack";
            public int cost;
            public float cooldown = 0.4f, windup = 0.1f, active = 0.1f, recovery = 0.2f;
            public int damage = 1;
            public float range = 1.1f, height = 1.0f, speed = 12f;
            public float stagger = 0.4f;
            /// <summary>대상에 주는 효과 시간 (간섭: CCTV 정지 시간 등).</summary>
            public float effectTime;
            /// <summary>소음 반경 (유닛). 경비가 이 안에 있으면 소리 쪽을 확인하러 온다.</summary>
            public float noise = 2f;
            /// <summary>전원 끌어오기: 전원과 대상 사이 최대 거리.</summary>
            public float sourceRange = 8f;
            public string anim = "Flick", fx = "Spark", sfx = "sfx_zap";
        }

        [Serializable]
        public class EnemyT
        {
            public string id;
            public int hp = 3;
            public float walk = 1.3f, chase = 3.0f, investigate = 1.8f;
            public float visionRange = 6f, visionHalfAngle = 26f, eyeHeight = 1.1f;
            /// <summary>초당 의심 증가 (가까이 2배, 먼 끝 0.6배, 웅크림 0.6배). 1 이 되면 발견.</summary>
            public float suspicionRate = 0.9f, suspicionDecay = 0.45f, suspicionGrace = 0.6f;
            /// <summary>의심 이 값 이상이면 확인하러 다가간다.</summary>
            public float checkThreshold = 0.45f;
            public float loseSightTime = 1.8f, searchTime = 5f, lookAroundTime = 1.2f;
            public float hearing = 1f;
            public float attackRange = 1.2f, attackWindup = 0.7f, attackActive = 0.16f, attackRecovery = 0.8f, attackCooldown = 1.3f;
            public int attackDamage = 8;
            public float attackLunge = 0.6f;
            /// <summary>경직 잠금 방지: window 초 안에 maxStaggers 번 경직되면 immune 초 동안 경직되지 않는다.</summary>
            public float staggerWindow = 3f, staggerImmune = 1.6f; public int maxStaggers = 2;
            /// <summary>쓰러진 기계가 다시 켜질 때까지 (일시 무력화).</summary>
            public float rebootTime = 28f;
            public float jamTime = 3.5f;
            public float alertRadius = 12f;
        }

        [Serializable]
        public class CameraT
        {
            public float sweepSpeed = 16f, endPause = 1.4f, range = 7.5f, halfAngle = 17f;
            public float suspicionRate = 0.6f, suspicionDecay = 0.55f, suspicionGrace = 0.5f;
            public float jamTime = 8f, alertRadius = 14f, realertInterval = 3f;
        }

        [Serializable]
        public class BossT
        {
            public int hp = 30;
            public float stalk = 1.1f, dashSpeed = 9f, dashWindup = 1.1f, firstDashWindup = 1.6f, dashDistance = 9f;
            public float recoverTime = 3.2f, talkWindow = 3.2f, resumeGrace = 1.6f;
            public int dashDamage = 9, acidDamage = 6, swipeDamage = 7;
            public float throwWindup = 0.8f, acidPuddleTime = 3.5f, swipeWindup = 0.55f, swipeRange = 1.3f;
            public float idleGap = 1.2f;
            // 2026-11-15 화학 패턴
            public float sprayWindup = 0.7f, sprayRange = 4.2f; public int sprayDamage = 6;
            public float grabWindup = 0.75f, grabLunge = 1.5f; public int grabDamage = 6;
            public float remixTime = 2.6f, puddleWidth = 1.6f, thrownInvuln = 1.3f; public int puddleMax = 2;
        }

        [Serializable]
        public class DifficultyT
        {
            public string id = "story";
            public float enemyDamage = 0.65f, suspicionMul = 0.75f, targetingTimeScale = 0.08f, windupMul = 1.15f;
        }

        public PlayerT player = new PlayerT();
        public List<SkillT> skills = new List<SkillT>();
        public List<EnemyT> enemies = new List<EnemyT>();
        public CameraT cctv = new CameraT();
        public BossT venelle = new BossT();
        public List<DifficultyT> difficulties = new List<DifficultyT>();
        /// <summary>진행 보상 (경험치). 적을 쓰러뜨려 얻는 경험치는 없다 — 생존·탐색·목표로 성장한다.</summary>
        public int expClue = 8, expSection = 20, expQuiet = 12, expNegotiate = 30;

        static ActionTuning cached;
        public static ActionTuning I
        {
            get
            {
                if (cached != null) return cached;
                var ta = Resources.Load<TextAsset>("EW/action_tuning");
                cached = ta != null ? Parse(ta.text) : null;
                if (cached == null) { Debug.LogWarning("[EW] action_tuning.json 이 없어 기본값을 씁니다."); cached = new ActionTuning(); }
                return cached;
            }
        }

        public static ActionTuning Parse(string json)
        {
            try { return JsonUtility.FromJson<ActionTuning>(json); }
            catch (Exception e) { Debug.LogError("[EW] action_tuning.json 읽기 실패: " + e.Message); return null; }
        }

        public static void Reload() { cached = null; }

        public SkillT Skill(string id) { foreach (var s in skills) if (s.id == id) return s; return null; }
        public EnemyT Enemy(string id) { foreach (var e in enemies) if (e.id == id) return e; return enemies.Count > 0 ? enemies[0] : new EnemyT(); }
        public DifficultyT Difficulty(string id) { foreach (var d in difficulties) if (d.id == id) return d; return difficulties.Count > 0 ? difficulties[0] : new DifficultyT(); }
        public static DifficultyT CurrentDifficulty => I.Difficulty(Game.I != null && Game.I.State != null ? Game.I.State.D.difficulty : "story");
    }
}
