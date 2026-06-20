using SpawnSystem.Monsters;
using SpawnSystem.Spawning;
using UnityEditor;
using UnityEngine;

namespace SpawnSystem.EditorTools
{
    /// <summary>
    /// Monsters.md 로스터(근접 5종 + 원거리 3종)를 3-프로필 조립(이동/방어/공격)으로 author링하는 메뉴.
    /// Tools/Spawn System/Create Sample Monster Defs → Assets/GameData/Monsters/. 재실행하면 갱신.
    /// </summary>
    public static class SampleMonsterContent
    {
        const string Dir = "Assets/GameData/Monsters";

        [MenuItem("Tools/Spawn System/Create Sample Monster Defs")]
        public static void Create()
        {
            EnsureFolder();

            // --- 이동 성격(재사용) ---
            var aggressive = Movement("MP_Aggressive", viewAvoid: 0.6f, react: 0.5f, repos: new Vector2(0.7f, 1.6f), act: 0.45f, strafeCost: 1f, backCost: 1f);
            var skittish = Movement("MP_Skittish", viewAvoid: 2.0f, react: 0.25f, repos: new Vector2(0.3f, 0.9f), act: 0.7f, strafeCost: 0.25f, backCost: 0.5f);

            // --- 방어(재사용) ---
            var dpNone = Defense("DP_None", MonsterArmor.None, DamageType.Normal | DamageType.Piercing | DamageType.Explosive);
            var dpLight = Defense("DP_Light", MonsterArmor.Light, DamageType.Normal | DamageType.Piercing | DamageType.Explosive);
            var dpHeavy = Defense("DP_Heavy", MonsterArmor.Heavy, DamageType.Piercing | DamageType.WeakPoint, wpMult: 2.5f);

            // --- 공격(재사용) ---
            var apClaw = Attack("AP_MeleeClaw",
                new AttackDef { name = "할퀴기", kind = AttackKind.Melee, damage = 6f, damageType = DamageType.Normal, range = 1.5f, cooldown = 1.0f, telegraph = 0.3f });
            var apLeap = Attack("AP_LeapStrike",
                new AttackDef { name = "도약 강타", kind = AttackKind.Leap, damage = 10f, damageType = DamageType.Normal, range = 8f, cooldown = 4f, telegraph = 0.6f });
            var apShot = Attack("AP_RangedShot",
                new AttackDef { name = "사격", kind = AttackKind.Projectile, damage = 5f, damageType = DamageType.Normal, range = 14f, cooldown = 1.2f, telegraph = 0.4f, projectileSpeed = 20f });
            var apExplosive = Attack("AP_Explosive",
                new AttackDef { name = "폭발탄", kind = AttackKind.AoE, damage = 18f, damageType = DamageType.Explosive, range = 16f, cooldown = 3f, telegraph = 0.8f, aoeRadius = 3f });
            var apArtillery = Attack("AP_Artillery",
                new AttackDef { name = "폭발탄", kind = AttackKind.AoE, damage = 18f, damageType = DamageType.Explosive, range = 18f, cooldown = 3.5f, telegraph = 0.9f, aoeRadius = 3.5f },
                new AttackDef { name = "기관총", kind = AttackKind.Sustained, damage = 2f, damageType = DamageType.Normal, range = 20f, cooldown = 0.12f, telegraph = 0.5f, projectileSpeed = 30f });

            var orange = new Color(0.90f, 0.40f, 0.20f);
            var red = new Color(0.85f, 0.20f, 0.20f);
            var yellow = new Color(0.90f, 0.80f, 0.20f);
            var purple = new Color(0.50f, 0.15f, 0.50f);
            var teal = new Color(0.25f, 0.65f, 0.70f);

            // --- 근접 ---
            Monster("MD_Melee_Small", "작은놈", MonsterTag.Swarm | MonsterTag.Melee, 0.5f, 6f, 4f, true, false, true, MonsterAbility.None, new Vector2(0f, 2f), skittish, dpNone, apClaw, orange);
            Monster("MD_Melee_SmallJumper", "작은놈 점프형", MonsterTag.Swarm | MonsterTag.Melee, 0.5f, 6f, 5f, true, false, true, MonsterAbility.Leap, new Vector2(0f, 2f), skittish, dpNone, apLeap, orange);
            Monster("MD_Melee_Medium", "중간놈", MonsterTag.Melee, 1.0f, 4f, 12f, false, false, true, MonsterAbility.None, new Vector2(0f, 2f), aggressive, dpLight, apClaw, red);
            Monster("MD_Melee_MediumBurrower", "중간놈 잠복형", MonsterTag.Melee, 1.0f, 4f, 12f, false, false, true, MonsterAbility.Burrow, new Vector2(0f, 2f), aggressive, dpLight, apClaw, red);
            Monster("MD_Melee_LargeHeavy", "큰놈", MonsterTag.Melee | MonsterTag.Elite, 1.6f, 2.0f, 60f, false, false, false, MonsterAbility.None, new Vector2(0f, 2f), aggressive, dpHeavy, apClaw, purple);

            // --- 원거리 ---
            Monster("MD_Ranged_Small", "작은놈(원거리)", MonsterTag.Ranged, 0.5f, 5f, 4f, true, true, true, MonsterAbility.None, new Vector2(8f, 14f), skittish, dpNone, apShot, yellow);
            Monster("MD_Ranged_MediumExplosive", "중간놈 폭발형", MonsterTag.Ranged | MonsterTag.Elite, 1.1f, 2.5f, 40f, false, false, false, MonsterAbility.None, new Vector2(10f, 16f), aggressive, dpHeavy, apExplosive, teal);
            Monster("MD_Ranged_LargeArtillery", "큰놈 포대형", MonsterTag.Ranged | MonsterTag.Elite, 1.8f, 2.0f, 80f, false, false, false, MonsterAbility.None, new Vector2(12f, 20f), aggressive, dpHeavy, apArtillery, teal);

            // --- 샘플 스폰 테이블 + 디렉터 프로필 ---
            var st = LoadOrCreate<SpawnTable>("ST_Sample", out bool stNew);
            st.mode = SpawnTable.SelectionMode.BudgetFill;
            st.entries = new[]
            {
                new SpawnEntry { monster = LoadDef("MD_Melee_Small"), weight = 3f, cost = 2f, groupSize = new Vector2Int(4, 7) },
                new SpawnEntry { monster = LoadDef("MD_Melee_SmallJumper"), weight = 1f, cost = 3f, groupSize = new Vector2Int(2, 4), minDifficulty = 0.2f },
                new SpawnEntry { monster = LoadDef("MD_Melee_Medium"), weight = 2f, cost = 4f, groupSize = new Vector2Int(2, 4) },
                new SpawnEntry { monster = LoadDef("MD_Melee_LargeHeavy"), weight = 1f, cost = 10f, groupSize = new Vector2Int(1, 1), minDifficulty = 0.5f },
            };
            Save(st, stNew, "ST_Sample");

            var dp = LoadOrCreate<DirectorProfile>("Dir_Sample", out bool dpNew);
            Save(dp, dpNew, "Dir_Sample");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SampleMonsterContent] 로스터 + 스폰테이블/디렉터 생성/갱신 → {Dir} (몬스터 8 + ST_Sample + Dir_Sample)");
        }

        static MonsterDef LoadDef(string name) => AssetDatabase.LoadAssetAtPath<MonsterDef>($"{Dir}/{name}.asset");

        static MovementProfile Movement(string name, float viewAvoid, float react, Vector2 repos, float act, float strafeCost, float backCost)
        {
            var p = LoadOrCreate<MovementProfile>(name, out bool isNew);
            p.wViewAvoid = viewAvoid;
            p.reactionThreshold = react;
            p.repositionInterval = repos;
            p.actChance = act;
            p.costStrafe = strafeCost;
            p.costBackstep = backCost;
            Save(p, isNew, name);
            return p;
        }

        static DefenseProfile Defense(string name, MonsterArmor armor, DamageType vuln, bool reqWeak = false, float wpMult = 2f)
        {
            var d = LoadOrCreate<DefenseProfile>(name, out bool isNew);
            d.armor = armor;
            d.vulnerableTo = vuln;
            d.requiresWeakPoint = reqWeak;
            d.weakPointMultiplier = wpMult;
            Save(d, isNew, name);
            return d;
        }

        static AttackProfile Attack(string name, params AttackDef[] attacks)
        {
            var a = LoadOrCreate<AttackProfile>(name, out bool isNew);
            a.attacks = attacks;
            Save(a, isNew, name);
            return a;
        }

        static void Monster(string id, string display, MonsterTag tags, float scale, float speed, float hp,
            bool canStrafe, bool canBackstep, bool canFlee, MonsterAbility abilities, Vector2 preferredRange,
            MovementProfile move, DefenseProfile def, AttackProfile atk, Color color)
        {
            var d = LoadOrCreate<MonsterDef>(id, out bool isNew);
            d.id = id;
            d.displayName = display;
            d.tags = tags;
            d.scale = scale;
            d.moveSpeed = speed;
            d.maxHP = hp;
            d.canStrafe = canStrafe;
            d.canBackstep = canBackstep;
            d.canFlee = canFlee;
            d.abilities = abilities;
            d.preferredRange = preferredRange;
            d.movement = move;
            d.defense = def;
            d.attack = atk;
            d.color = color;
            d.sizeClass = scale < 0.7f ? MonsterSizeClass.Small : (scale > 1.2f ? MonsterSizeClass.Large : MonsterSizeClass.Medium);
            Save(d, isNew, id);
        }

        static T LoadOrCreate<T>(string name, out bool isNew) where T : ScriptableObject
        {
            var path = $"{Dir}/{name}.asset";
            var obj = AssetDatabase.LoadAssetAtPath<T>(path);
            isNew = obj == null;
            if (isNew)
                obj = ScriptableObject.CreateInstance<T>();
            return obj;
        }

        static void Save(ScriptableObject obj, bool isNew, string name)
        {
            if (isNew)
                AssetDatabase.CreateAsset(obj, $"{Dir}/{name}.asset");
            else
                EditorUtility.SetDirty(obj);
        }

        static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameData"))
                AssetDatabase.CreateFolder("Assets", "GameData");
            if (!AssetDatabase.IsValidFolder(Dir))
                AssetDatabase.CreateFolder("Assets/GameData", "Monsters");
        }
    }
}
