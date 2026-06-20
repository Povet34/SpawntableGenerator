using System.Collections.Generic;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 몬스터 공격 실행기. AttackProfile 기반으로 근접(부채꼴)/원거리(레이저 투사체) 공격.
    /// PackFactory 에서 각 몬스터 GO에 붙임. PackState.Engage 상태에서만 동작.
    /// </summary>
    public class MonsterAttack : MonoBehaviour
    {
        public AttackProfile profile;

        Transform _player;
        Health _playerHealth;
        Player.PlayerController _playerCtrl;
        float[] _cooldowns;

        // 시각 쿨 — 슬래시 아크
        const float ArcDuration = 0.25f;
        const float MeleeArcDeg = 110f;

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

            // Engage 상태에서만 공격
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

                switch (atk.kind)
                {
                    case AttackKind.Melee:
                        if (TryMelee(atk, toPlayer.normalized))
                            _cooldowns[i] = atk.cooldown;
                        break;
                    case AttackKind.Projectile:
                        TryRanged(atk, toPlayer.normalized);
                        _cooldowns[i] = atk.cooldown;
                        break;
                }
            }
        }

        bool TryMelee(AttackDef atk, Vector3 toPlayerDir)
        {
            // 몬스터의 전방 기준 부채꼴 (Monster.Step이 transform.forward를 이동방향으로 유지)
            float halfAng = MeleeArcDeg * 0.5f;
            float dot = Vector3.Dot(transform.forward, toPlayerDir);
            if (dot < Mathf.Cos(halfAng * Mathf.Deg2Rad)) return false;

            // 데미지
            if (_playerHealth != null && !_playerHealth.IsDead)
                _playerHealth.TakeDamage(atk.damage, atk.damageType);

            // 근접 넉백
            if (_playerCtrl != null)
                _playerCtrl.AddKnockback(toPlayerDir * 4f);

            // 시각 아크 (몬스터 위치 기준)
            MeleeArcVfx.Spawn(transform.position, transform.forward, atk.range, MeleeArcDeg,
                              ArcDuration, new Color(1f, 0.15f, 0.1f)); // 적색
            return true;
        }

        void TryRanged(AttackDef atk, Vector3 dir)
        {
            Vector3 origin = transform.position + Vector3.up * 1f;
            float speed = atk.projectileSpeed > 0f ? atk.projectileSpeed : 18f;
            LaserProjectile.SpawnRaw(origin, dir, speed, atk.range, atk.damage,
                                     atk.damageType, new Color(1f, 0.5f, 0.1f), hitsPlayer: true);
        }
    }
}
