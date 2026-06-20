using UnityEngine;
using SpawnSystem.Map;

/// <summary>
/// MapGenerator.onGenerated 이벤트를 구독해 VolumetricFog의
/// fogOfWarSize를 맵 크기에 맞게 자동 동기화한다.
/// MapGenerator GO에 같이 붙이거나 씬의 별도 GO에 붙여도 됨.
/// </summary>
public class FogMapSync : MonoBehaviour
{
    [Tooltip("MapGenerator 컴포넌트 (없으면 씬에서 자동 탐색)")]
    public MapGenerator mapGenerator;

    VolumetricFogAndMist2.VolumetricFog _fog;

    void Start()
    {
        if (mapGenerator == null)
            mapGenerator = UnityEngine.Object.FindAnyObjectByType<MapGenerator>();

        _fog = UnityEngine.Object.FindAnyObjectByType<VolumetricFogAndMist2.VolumetricFog>();

        if (mapGenerator != null)
            mapGenerator.onGenerated += OnMapGenerated;
    }

    void OnDestroy()
    {
        if (mapGenerator != null)
            mapGenerator.onGenerated -= OnMapGenerated;
    }

    void OnMapGenerated(float width, float length)
    {
        if (_fog == null) return;
        _fog.fogOfWarSize = new Vector3(width, 0f, length);
        _fog.ReloadFogOfWarTexture();
        Debug.Log($"[FogMapSync] fogOfWarSize → ({width}, 0, {length})");
    }
}
