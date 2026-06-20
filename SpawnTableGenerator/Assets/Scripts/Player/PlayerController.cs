using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace SpawnSystem.Player
{
    /// <summary>
    /// MOBA식 탑다운 조작. 마우스 우클릭 = 이동, 좌클릭 = 공격(미구현, 자리만 확보).
    /// 커서 아래 실제 지오메트리(Physics 레이)를 찍고, 그 지점에서 갈 수 있는 가장 가까운
    /// NavMesh 위치를 목적지로 준다. 갈 수 있는 곳을 찍으면 NavMeshAgent가 벽/장애물을
    /// 우회해서 가고, 못 가는 곳(벽 위 등)을 찍으면 가장 가까운 가장자리까지 가서 멈춘다.
    /// 버튼을 꾹 누르고 있으면 매 프레임 목적지를 갱신한다(홀드 추적).
    /// 신형 Input System 전용(프로젝트 activeInputHandler = 1).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("클릭 레이를 투영할 바닥 높이 (월드 y, Physics 레이 실패 시 폴백용)")]
        public float groundHeight = 0f;

        [Tooltip("못 가는 곳을 찍었을 때 가장 가까운 NavMesh를 찾는 최대 반경")]
        public float sampleRadius = 12f;

        [Tooltip("목적지 표식을 그릴지 여부 (기즈모)")]
        public bool drawDestinationGizmo = true;

        NavMeshAgent _agent;
        Camera _camera;
        Vector3 _destination;
        bool _hasDestination;

        // 넉백
        Vector3 _knockback;
        const float KnockbackDecay = 8f;

        // 수동 회전 (공격 시 snap, 그 외 이동방향)
        Vector3 _overrideRotDir;
        float _overrideTimer;

        readonly Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false; // 회전 직접 제어
            _camera = Camera.main;
        }

        /// <summary>근접/폭발 피격 시 방향 충격량 추가 (MonsterAttack, LaserProjectile 에서 호출).</summary>
        public void AddKnockback(Vector3 impulse) => _knockback += impulse;

        /// <summary>공격 시 잠깐 해당 방향을 바라보도록 오버라이드 (PlayerCombat 에서 호출).</summary>
        public void OverrideRotation(Vector3 dir) { _overrideRotDir = dir; _overrideTimer = 0.3f; }

        void Update()
        {
            if (_camera == null)
                _camera = Camera.main;

            var mouse = Mouse.current;
            if (mouse == null || _camera == null)
                return;

            // 우클릭 = 이동. isPressed 라서 꾹 누르고 있으면 매 프레임 목적지를 갱신한다.
            if (mouse.rightButton.isPressed)
                TryMoveToCursor(mouse.position.ReadValue());

            // 넉백 적용
            if (_knockback.sqrMagnitude > 0.001f)
            {
                _agent.Move(_knockback * Time.deltaTime);
                _knockback = Vector3.Lerp(_knockback, Vector3.zero, KnockbackDecay * Time.deltaTime);
            }

            // 회전 처리
            _overrideTimer -= Time.deltaTime;
            Vector3 vel = _agent.velocity;
            vel.y = 0f;
            if (_overrideTimer > 0f && _overrideRotDir.sqrMagnitude > 1e-4f)
                transform.rotation = Quaternion.LookRotation(_overrideRotDir);
            else if (vel.sqrMagnitude > 0.05f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(vel.normalized),
                                                      Time.deltaTime * 12f);
        }

        void TryMoveToCursor(Vector2 screenPos)
        {
            Ray ray = _camera.ScreenPointToRay(screenPos);

            // 커서 아래의 실제 지오메트리(바닥/벽/장애물 콜라이더)를 찍는다.
            // 벽 '위'를 클릭하면 벽 표면 지점이 잡혀서 "못 가는 곳"으로 올바르게 해석됨.
            Vector3 clicked;
            if (Physics.Raycast(ray, out RaycastHit physHit, 500f))
            {
                // 순수 로직(MovementResolver)으로 위임 — 수직 벽면이면 바닥 투영 + 법선 밀기로 밀착.
                clicked = MovementResolver.ResolveClickTarget(
                    physHit.point, physHit.normal, groundHeight, _agent.radius + 0.1f);
            }
            else
            {
                // 콜라이더를 못 맞추면 y=groundHeight 평면으로 폴백.
                var plane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
                if (!plane.Raycast(ray, out float enter))
                    return;
                clicked = ray.GetPoint(enter);
            }

            // 클릭 지점에서 '갈 수 있는' 가장 가까운 NavMesh 위치로 스냅.
            // - 갈 수 있는 바닥(벽 너머 포함)을 찍으면 → 그 지점이 잡히고, SetDestination 이 우회 경로를 계산.
            // - 못 가는 곳(벽 위 등)을 찍으면 → 가장 가까운 가장자리가 잡혀 벽에 붙어 멈춤.
            if (NavMesh.SamplePosition(clicked, out NavMeshHit navHit, sampleRadius, NavMesh.AllAreas))
            {
                _destination = navHit.position;
                _hasDestination = true;
                _agent.SetDestination(_destination);
            }
        }

        void OnDrawGizmos()
        {
            if (!drawDestinationGizmo || !_hasDestination)
                return;
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
            Gizmos.DrawWireSphere(_destination, 0.5f);
            Gizmos.DrawLine(_destination, _destination + Vector3.up * 2f);
        }
    }
}
