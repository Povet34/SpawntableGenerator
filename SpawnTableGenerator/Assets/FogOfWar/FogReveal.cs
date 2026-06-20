using UnityEngine;

/// <summary>
/// 플레이어에 부착 — VolumetricFog2 의 Fog of War 에서 플레이어 주변을 매 프레임 걷어냄.
/// VolumetricFog 컴포넌트가 씬에 있어야 동작. URP Renderer에 VolumetricFogRenderFeature 등록 필요.
/// Assembly-CSharp 에 컴파일돼 VolumetricFog2 + SpawnSystem 모두 접근 가능.
/// </summary>
public class FogReveal : MonoBehaviour
{
    [Tooltip("플레이어 주변 시야 반경")]
    public float revealRadius = 15f;

    [Tooltip("플레이어가 벗어난 뒤 안개가 복구되기까지의 지연(초)")]
    public float restoreDelay = 4f;

    [Tooltip("안개 복구 소요 시간(초)")]
    public float restoreDuration = 2f;

    object _fog; // VolumetricFog2.VolumetricFog — 직접 타입 참조 대신 reflection 사용
    System.Reflection.MethodInfo _setAlpha;

    void Start()
    {
        // VolumetricFog2 네임스페이스 없이 안전하게 찾기
        var fogType = System.Type.GetType("VolumetricFog2.VolumetricFog, VolumetricFog2");
        if (fogType == null)
            fogType = FindFogType();

        if (fogType != null)
        {
            _fog = Object.FindAnyObjectByType(fogType);
            if (_fog != null)
            {
                // enableFogOfWar = true
                var field = fogType.GetField("enableFogOfWar",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                field?.SetValue(_fog, true);

                // ReloadFogOfWarTexture()
                var reload = fogType.GetMethod("ReloadFogOfWarTexture");
                reload?.Invoke(_fog, null);

                // SetFogOfWarAlpha(Vector3, float, float, float, float, float, float) 시그니처 찾기
                _setAlpha = fogType.GetMethod("SetFogOfWarAlpha", new[] {
                    typeof(Vector3), typeof(float), typeof(float),
                    typeof(float), typeof(float), typeof(float), typeof(float)
                });

                Debug.Log($"[FogReveal] VolumetricFog found: {_fog}  setAlpha={_setAlpha != null}");
            }
            else
                Debug.LogWarning("[FogReveal] VolumetricFog 컴포넌트를 씬에서 찾지 못했습니다.");
        }
        else
            Debug.LogWarning("[FogReveal] VolumetricFog2 타입을 찾지 못했습니다. 에셋이 임포트됐는지 확인하세요.");
    }

    void Update()
    {
        if (_fog == null || _setAlpha == null) return;
        // SetFogOfWarAlpha(pos, radius, alpha=0, duration=0, smoothness=0, restoreDelay, restoreDuration)
        _setAlpha.Invoke(_fog, new object[] {
            transform.position, revealRadius, 0f,
            0f, 0f, restoreDelay, restoreDuration
        });
    }

    static System.Type FindFogType()
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType("VolumetricFog2.VolumetricFog");
            if (t != null) return t;
        }
        return null;
    }
}
