using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>군집이 멤버에게 매 프레임 넘기는 동적 컨텍스트(설계 §12).</summary>
    public readonly struct SteerContext
    {
        public readonly Vector3 PlayerPos;
        public readonly Vector3 PlayerForward;
        public readonly Vector3 LastAttackDir;
        public readonly float LastAttackAge;
        public readonly Vector3 AnchorPos;
        public readonly MovementProfile Profile;
        public readonly float MoveSpeed;
        public readonly Vector2 PreferredRange;
        public readonly float EngageRange;
        public readonly int DirCount;

        public SteerContext(Vector3 playerPos, Vector3 playerForward, Vector3 lastAttackDir, float lastAttackAge,
            Vector3 anchorPos, MovementProfile profile, float moveSpeed, Vector2 preferredRange, float engageRange, int dirCount)
        {
            PlayerPos = playerPos;
            PlayerForward = playerForward;
            LastAttackDir = lastAttackDir;
            LastAttackAge = lastAttackAge;
            AnchorPos = anchorPos;
            Profile = profile;
            MoveSpeed = moveSpeed;
            PreferredRange = preferredRange;
            EngageRange = engageRange;
            DirCount = dirCount;
        }
    }

    /// <summary>
    /// 군집 멤버 1마리(설계 §12). 길찾기는 안 함(앵커만). 거리에 따라:
    /// - 원거리: 앵커로 최고속 직진(앵커가 글로벌 길찾기로 데려옴).
    /// - 근접 교전권: 컨텍스트 스티어링으로 플레이어 시야를 피하며 선호 거리 유지(포위는 창발).
    /// 반응은 즉각이 아님 — repositionInterval 마다만 방향 재결정(관성 유지). 적용은 <see cref="IMemberMover"/>.
    /// </summary>
    public class Monster : MonoBehaviour
    {
        [System.NonSerialized] public MonsterPack Pack;
        [System.NonSerialized] public Health Health;

        IMemberMover _mover;
        Vector3 _heading;
        Vector3 _dir;
        float _decideTimer;
        float _groundY;
        bool _groundYSet;

        public IMemberMover Mover
        {
            get => _mover ??= new TransformMover();
            set => _mover = value;
        }

        /// <summary>이 게임은 XZ 평면(y축 없음). 멤버를 고정 높이에 묶는다(머리 위로 안 올라가게).</summary>
        public void SetGroundY(float y)
        {
            _groundY = y;
            _groundYSet = true;
        }

        /// <summary>풀에서 재사용될 때 이전 생애의 이동 상태를 초기화.</summary>
        public void ResetForReuse()
        {
            _heading = Vector3.zero;
            _dir = Vector3.zero;
            _decideTimer = 0f;
            _groundYSet = false;
        }

        public void Step(in SteerContext ctx, IReadOnlyList<Vector3> neighbors, float dt)
        {
            if (!_groundYSet)
            {
                _groundY = transform.position.y; // 스폰 높이 = 바닥 안착 높이
                _groundYSet = true;
            }

            Vector3 self = transform.position;
            float distToPlayer = Flat(ctx.PlayerPos - self).magnitude;

            if (distToPlayer > ctx.EngageRange)
            {
                // 원거리: 앵커로 직진(글로벌 접근).
                Vector3 toAnchor = Flat(ctx.AnchorPos - self);
                if (toAnchor.sqrMagnitude > 1e-4f)
                    _dir = toAnchor.normalized;
            }
            else
            {
                // 근접: 둔감하게(주기적) 컨텍스트 스티어링 재결정.
                _decideTimer -= dt;
                if (_decideTimer <= 0f)
                {
                    _decideTimer = Mathf.Max(0.05f, ctx.Profile.repositionInterval.x);
                    _dir = Decide(ctx, neighbors, self, distToPlayer);
                }
            }

            float speed = ctx.MoveSpeed * NavTerrain.SpeedMultiplier(self);
            Mover.MoveBy(transform, _dir * speed, dt);

            // XZ 평면 고정 — y축 이동 금지(머리 위로 올라가지 않게).
            var pos = transform.position;
            if (!Mathf.Approximately(pos.y, _groundY))
            {
                pos.y = _groundY;
                transform.position = pos;
            }

            if (_dir.sqrMagnitude > 1e-4f)
            {
                _heading = _dir;
                transform.forward = _dir;
            }
        }

        Vector3 Decide(SteerContext ctx, IReadOnlyList<Vector3> neighbors, Vector3 self, float distToPlayer)
        {
            float selfPressure = ViewPressure.Total(ctx.PlayerPos, ctx.PlayerForward, ctx.LastAttackDir, ctx.LastAttackAge,
                self, ctx.Profile.viewConeAngle, ctx.Profile.viewConeRange, ctx.Profile.lastAttackMemory);

            bool inBand = distToPlayer >= ctx.PreferredRange.x && distToPlayer <= ctx.PreferredRange.y;

            // 노려봐지지 않고 선호 거리면 만족 → 가만히(즉각/잦은 무빙 억제, §12.1).
            if (selfPressure < ctx.Profile.reactionThreshold && inBand)
                return Vector3.zero;

            Vector3 selfCopy = self;
            float Pressure(Vector3 p) => ViewPressure.Total(ctx.PlayerPos, ctx.PlayerForward, ctx.LastAttackDir, ctx.LastAttackAge,
                p, ctx.Profile.viewConeAngle, ctx.Profile.viewConeRange, ctx.Profile.lastAttackMemory);
            float Blocked(Vector3 p) => NavTerrain.PointPenalty(selfCopy, p);

            return ContextSteering.ChooseDirection(self, _heading, ctx.PlayerPos, ctx.PreferredRange,
                neighbors, ctx.Profile, Pressure, Blocked, ctx.DirCount);
        }

        static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }
    }
}
