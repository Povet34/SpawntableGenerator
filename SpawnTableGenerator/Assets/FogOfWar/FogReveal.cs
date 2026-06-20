using UnityEngine;

/// <summary>
/// 플레이어에 부착 — VolumetricFogAndMist2의 Fog of War에서
/// 플레이어 주변을 걷어냄. Awake에서 먼저 초기화해 LateUpdate보다
/// 빠른 실행을 보장하고, 이동 임계값 기반 업데이트로 복구 아티팩트를 줄인다.
/// </summary>
public class FogReveal : MonoBehaviour
{
    public float revealRadius = 15f;

    [Tooltip("이 거리 이상 이동했을 때만 FoW를 업데이트 (잦은 호출로 인한 엣지 아티팩트 방지)")]
    public float updateThreshold = 0.5f;

    VolumetricFogAndMist2.VolumetricFog _fog;
    Vector3 _lastRevealPos = Vector3.one * float.MaxValue;

    void Awake()
    {
        // LateUpdate보다 먼저 실행되도록 Awake에서 초기화
        _fog = UnityEngine.Object.FindAnyObjectByType<VolumetricFogAndMist2.VolumetricFog>();
        if (_fog == null) return;

        _fog.enableFogOfWar = true;
        _fog.fogOfWarRestoreDelay = 4f;
        _fog.fogOfWarRestoreDuration = 2.5f;
        _fog.fogOfWarBlur = true;
        _fog.fogOfWarTextureWidth = 512;
        // ReloadFogOfWarTexture → FogOfWarInit → fowTransitionList 초기화
        // (도메인 리로드 후 null 상태로 LateUpdate가 실행되는 버그 방지)
        _fog.ReloadFogOfWarTexture();

        RevealAt(transform.position);
    }

    void Start()
    {
        if (_fog != null)
            Debug.Log("[FogReveal] FoW 활성화 (threshold=" + updateThreshold + " blur=true tex=512)");
        else
            Debug.LogWarning("[FogReveal] VolumetricFog 컴포넌트를 씬에서 찾지 못했습니다.");
    }

    void Update()
    {
        if (_fog == null) return;
        Vector3 pos = transform.position;
        if (Vector3.Distance(pos, _lastRevealPos) >= updateThreshold)
            RevealAt(pos);
    }

    void RevealAt(Vector3 pos)
    {
        _fog.SetFogOfWarAlpha(pos, revealRadius, 0f);
        _lastRevealPos = pos;
    }
}
