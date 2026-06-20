using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace SpawnSystem.Player
{
    /// <summary>
    /// MOBA식 탑다운 조작. 마우스 우클릭 = 이동, 좌클릭 = 공격(미구현, 자리만 확보).
    /// 우클릭 지점을 바닥 평면(y=0)에 투영해 NavMeshAgent 목적지로 설정하며,
    /// 버튼을 꾹 누르고 있으면 매 프레임 커서 위치로 목적지를 갱신한다(홀드 추적).
    /// 신형 Input System 전용(프로젝트 activeInputHandler = 1).
    /// 높이/회전은 NavMeshAgent와 길찾기가 처리하므로 XZ 평면 이동만 다룬다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("클릭 레이를 투영할 바닥 높이 (월드 y)")]
        public float groundHeight = 0f;

        [Tooltip("목적지 표식을 그릴지 여부 (기즈모)")]
        public bool drawDestinationGizmo = true;

        NavMeshAgent _agent;
        Camera _camera;
        Vector3 _destination;
        bool _hasDestination;

        readonly Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.main;
        }

        void Update()
        {
            if (_camera == null)
                _camera = Camera.main;

            var mouse = Mouse.current;
            if (mouse == null || _camera == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
                TryMoveToCursor(mouse.position.ReadValue());
        }

        void TryMoveToCursor(Vector2 screenPos)
        {
            Ray ray = _camera.ScreenPointToRay(screenPos);

            // 콜라이더 의존 없이 수학 평면(y=groundHeight)과 교차.
            var plane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
            if (!plane.Raycast(ray, out float enter))
                return;

            Vector3 hit = ray.GetPoint(enter);

            // 클릭 지점에서 가장 가까운 NavMesh 위치로 스냅(벽 밖 클릭 방어).
            if (NavMesh.SamplePosition(hit, out NavMeshHit navHit, 4f, NavMesh.AllAreas))
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
