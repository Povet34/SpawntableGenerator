using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 몬스터 공격 실행기. 근접(120° 부채꼴) / 원거리(낮은 명중률 레이저, 120° 조준각).
    /// PackFactory 에서 AttackProfile 기반으로 각 멤버 GO에 부착.
    /// </summary>
    public class MonsterAttack : MonoBehaviour
    {
        public AttackProfile profile;

        [Tooltip("원거리 공격 탄 퍼짐 (0=완벽, 0.4=낮은 명중률)")]
        public float rangedInaccuracy = 0.38f;

        Transform _player;
        Health _playerHealth;
        Player.PlayerController _playerCtrl;
        float[] _cooldowns;

        const float AttackConeHalfDeg = 60f; // 120° 부채꼴의 반각
        const float MeleeArcDeg = 120f;
        const float ArcDuration = 0.18f;

        public void Init(AttackProfile p, Transform player)
        {
            profile = p;
            _player = player;
            if (player != null)
            {
                _playerHealth = player.GetComponent<Health>();
                _playerCtrl = player.GetComponent<Player.PlayerController>();
            }
            _cooldowns = p != null ? new float[p.attacks.Length] : new float[0];
        }

        void Update()
        {
            if (_player == null || profile == null) return;

            var monster = GetComponent<Monster>();
            if (monster != null && monster.Pack != null && monster.Pack.State != PackState.Engage)
                return;

            for (int i = 0; i < profile.attacks.Length; i++)
            {
                if (_cooldowns[i] > 0f) { _cooldowns[i] -= Time.deltaTime; continue; }

                AttackDef atk = profile.attacks[i];
                Vector3 toPlayer = _player.position - transform.position;
                toPlayer.y = 0f;
                float dist = toPlayer.magnitude;
                if (dist > atk.range) continue;

                // 공격 방향 기준 120° 전방 콘 밖이면 공격 불가
                float cosLimit = Mathf.Cos(AttackConeHalfDeg * Mathf.Deg2Rad);
                if (dist > 0.1f && Vector3.Dot(transform.forward, toPlayer.normalized) < cosLimit)
                    continue;

                switch (atk.kind)
                {
                    case AttackKind.Melee:
                        ExecuteMelee(atk, toPlayer.normalized);
                        _cooldowns[i] = atk.cooldown;
                        break;
                    case AttackKind.Projectile:
                        ExecuteRanged(atk, toPlayer.normalized);
                        _cooldowns[i] = atk.cooldown;
                        break;
                }
            }
        }

        void ExecuteMelee(AttackDef atk, Vector3 toPlayerDir)
        {
            if (_playerHealth != null && !_playerHealth.IsDead)
                _playerHealth.TakeDamage(atk.damage, atk.damageType);

            if (_playerCtrl != null)
                _playerCtrl.AddKnockback(toPlayerDir * 4f);

            MeleeArcVfx.Spawn(transform.position, transform.forward,
                               atk.range, MeleeArcDeg, ArcDuration,
                               new Color(1f, 0.15f, 0.1f));
        }

        void ExecuteRanged(AttackDef atk, Vector3 perfectDir)
        {
            // 퍼짐(inaccuracy) 적용 — XZ 평면 안에서 무작위 편향
            Vector2 spread = Random.insideUnitCircle * rangedInaccuracy;
            Vector3 aimDir = new Vector3(
                perfectDir.x + spread.x,
                0f,
                perfectDir.z + spread.y).normalized;

            // 자기 콜라이더를 벗어난 위치에서 발사 (자기 hit 방지)
            Vector3 origin = transform.position + Vector3.up * 1f + aimDir * 1.6f;
            float speed = atk.projectileSpeed > 0f ? atk.projectileSpeed : 18f;

            LaserProjectile.SpawnRaw(origin, aimDir, speed, atk.range,
                                     atk.damage, atk.damageType,
                                     new Color(1f, 0.45f, 0.1f), hitsPlayer: true);
        }
    }
}
