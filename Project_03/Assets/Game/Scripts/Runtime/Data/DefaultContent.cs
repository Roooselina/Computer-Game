using UnityEngine;

namespace EW
{
    /// <summary>
    /// GameDatabase 기본값. 에디터 구성 도구가 GameDatabase.asset 이 없을 때 한 번만 이 값으로 만든다.
    /// 이후 수치 조정은 GameDatabase.asset 에서 한다(이 파일을 고쳐도 기존 에셋은 바뀌지 않는다).
    /// 수치 출처: GDD 4.2 / 4.4 / 4.5 / 4.6 / 4.7 / 5장.
    /// </summary>
    public static class DefaultContent
    {
        public static GameDatabase Create()
        {
            var db = ScriptableObject.CreateInstance<GameDatabase>();
            Fill(db);
            return db;
        }

        public static void Fill(GameDatabase db)
        {
            db.characters.Clear();
            db.characters.Add(new CharacterGrowth { id = "lu", name = "루", baseHp = 48, hpPerLevel = 8, baseAtk = 8, atkPerLevel = 2, baseDef = 3, defPerLevel = 1, baseCharge = 6 });
            db.characters.Add(new CharacterGrowth { id = "venelle", name = "베넬", baseHp = 46, hpPerLevel = 7, baseAtk = 7, atkPerLevel = 2, baseDef = 3, defPerLevel = 1, baseCharge = 0 });

            db.skills.Clear();
            // 시작 기술 (GDD 4.5): 톡!, 스파크 샷, 전하 모으기. 이후 Lv.2 아크 채찍, Lv.3 번개 발차기.
            db.skills.Add(new SkillDef { id = "tok", name = "톡!", cost = 0, target = SkillTarget.Single, coef = 1.0f, unlockLevel = 1, anim = "Flick", fx = "Spark", desc = "단일 1.0. 전하 없이 언제나 사용 가능. 절연 상대에게도 최소 피해." });
            db.skills.Add(new SkillDef { id = "spark_shot", name = "스파크 샷", cost = 1, target = SkillTarget.Single, coef = 0.75f, hits = 2, unlockLevel = 1, anim = "Point", fx = "Bolt", desc = "원거리 단일 1.5 (작은 전기탄 두 발)." });
            db.skills.Add(new SkillDef { id = "gather", name = "전하 모으기", cost = 0, target = SkillTarget.Self, coef = 0, chargeGain = 3, guard = 0.3f, unlockLevel = 1, anim = "Charge", fx = "", desc = "전하 +3. 이번 턴 받는 피해 30% 감소." });
            db.skills.Add(new SkillDef { id = "arc_whip", name = "아크 채찍", cost = 2, target = SkillTarget.UpToTwo, coef = 1.2f, unlockLevel = 2, anim = "Swing", fx = "Arc", desc = "적 최대 2명에게 1.2씩." });
            db.skills.Add(new SkillDef { id = "thunder_kick", name = "번개 발차기", cost = 2, target = SkillTarget.Single, coef = 1.8f, unlockLevel = 3, staggerChance = 0.35f, anim = "Kick", fx = "Spark", desc = "단일 1.8. 기계의 자세를 35% 확률로 무너뜨림(다음 행동 불가). 최소 피해 보장." });

            db.enemies.Clear();
            db.enemies.Add(new EnemyDef { id = "patrol_guard", name = "순찰 경비", artId = "droid03", hp = 26, atk = 7, def = 2, exp = 12, ai = EnemyAI.Guard, dropItem = "bandage", dropChance = 0.35f, shield = 0.35f, note = "다음 턴 제압봉 예고. 몸통이 느림. 방어 자세(피해 35% 감소)는 노출 전선으로 무너진다. (임시 그래픽: Foozle Droid03)" });
            db.enemies.Add(new EnemyDef { id = "capture_drone", name = "포획 드론", artId = "droid01", hp = 14, atk = 5, def = 1, exp = 9, ai = EnemyAI.Drone, flying = true, dropItem = "bandage", dropChance = 0.25f, note = "그물 발사, 체력 낮음. 제어 단말이 꺼지면 지원 행동 중단. 자체 배터리로 이동은 계속. (임시 그래픽: Foozle Droid01)" });
            db.enemies.Add(new EnemyDef { id = "carrier_bot", name = "운반 로봇", artId = "droid02", hp = 28, atk = 7, def = 2, exp = 14, ai = EnemyAI.Carrier, flying = true, dropItem = "snack", dropChance = 0.2f, note = "짐을 던지고 회수하느라 한 턴 쉼. (임시 그래픽: Foozle Droid02 — 전용 외형 없음)" });
            db.enemies.Add(new EnemyDef { id = "venelle_boss", name = "베넬", artId = "venelle", hp = 84, atk = 8, def = 3, exp = 40, ai = EnemyAI.VenelleBoss, machine = false, note = "3장 보스. 대화로 협상 가능." });

            db.items.Clear();
            db.items.Add(new ItemDef { id = "snack", name = "과자", healPct = 0.3f, charge = 2, desc = "안정도 30%·전하 2 회복.", iconSprite = "Prop_Snack" });
            db.items.Add(new ItemDef { id = "chocolate", name = "초콜릿", healPct = 0f, charge = 0, fieldUse = false, desc = "압수품 바구니의 겉옷 주머니에 남아 있던 초콜릿 한 개. 누군가에게 건넬 수 있다.", iconSprite = "Prop_Snack" });
            db.items.Add(new ItemDef { id = "pump_part", name = "이음쇠 부품", healPct = 0f, charge = 0, battleUse = false, fieldUse = false, desc = "물 펌프의 깨진 연결 부품을 바꿀 새 이음쇠. 펌프실 선반에서 찾았다.", iconSprite = "Prop_Snack" });
            db.items.Add(new ItemDef { id = "staff_card", name = "직원 카드", healPct = 0f, charge = 0, battleUse = false, fieldUse = false, desc = "수용·접수동 직원 카드. 영상실 문이 열린다. (경비 책상 / 상담 부스 분실물 상자)", iconSprite = "Prop_Doc" });
            db.items.Add(new ItemDef { id = "edward_tape", name = "녹음 테이프", healPct = 0f, charge = 0, battleUse = false, fieldUse = false, desc = "녹음 보관실의 오래된 테이프. 이름표에 'E.' 라고만 적혀 있다.", iconSprite = "Prop_Doc" });
            db.items.Add(new ItemDef { id = "bandage", name = "회복 붕대", healPct = 0.45f, charge = 0, desc = "안정도 45% 회복.", iconSprite = "medkit_item" });

            db.props.Clear();
            db.props.Add(new PropDef { kind = PropKind.ExposedWire, name = "끊어진 노출 전선", cost = 1, coef = 1.3f, maxTargets = 2, desc = "전선 끝에서 아크를 일으켜 근처 적 2명을 공격.", firstLine = "이쪽이야! 반짝반짝!" });
            db.props.Add(new PropDef { kind = PropKind.ControlTerminal, name = "제어 단말", cost = 2, coef = 0f, maxTargets = 0, desc = "과한 방전으로 단말 전원을 멈춘다. 지원 로봇의 지시 기능 중단. 같은 단말로 여는 문도 멈춘다.", firstLine = "...문도 같이 꺼졌네." });
            db.props.Add(new PropDef { kind = PropKind.ChemicalPuddle, name = "전도성 약품 웅덩이", cost = 2, coef = 1.6f, maxTargets = 3, desc = "웅덩이를 따라 방전. 웅덩이 위의 적을 공격. 절연 발판의 아군은 안전.", firstLine = "" });

            db.difficulties.Clear();
            db.difficulties.Add(new DifficultyDef { id = "story", name = "이야기", enemyDamageMul = 0.7f, healMul = 1f, fieldChargeSeconds = 3f, critChance = 0.15f, critMul = 1.5f, talkBaseChance = 0.7f, retryHeal = true });
            db.difficulties.Add(new DifficultyDef { id = "normal", name = "기본", enemyDamageMul = 1f, healMul = 1f, fieldChargeSeconds = 3f, critChance = 0.15f, critMul = 1.5f, talkBaseChance = 0.7f });
        }
    }
}
