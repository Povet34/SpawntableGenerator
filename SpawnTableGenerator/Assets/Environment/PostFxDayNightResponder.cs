using SpawnSystem.Environment;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 맵 전체의 색조(Post-processing Color Grading)를 낮/밤에 맞춘다.
/// 핵심(SpawnSystem)의 <see cref="DayNightResponderBehaviour"/>를 Assembly-CSharp에서 확장 —
/// URP Volume에 의존하므로 핵심 어셈블리를 건드리지 않고 추가(OCP, 경계 넘는 확장).
/// 낮: 따뜻하고 밝은 중성 톤 / 밤: 차갑고 어두운 청색 톤.
/// </summary>
public class PostFxDayNightResponder : DayNightResponderBehaviour
{
    [Tooltip("비우면 씬의 전역 Volume을 자동 탐색")]
    public Volume volume;

    [Header("색 필터(ColorAdjustments)")]
    public Color dayColorFilter = Color.white;
    public Color nightColorFilter = new Color(0.55f, 0.62f, 0.95f);

    [Header("노출(밤일수록 어둡게)")]
    public float dayPostExposure = 0f;
    public float nightPostExposure = -0.7f;

    [Header("채도(밤일수록 탈색)")]
    public float daySaturation = 0f;
    public float nightSaturation = -22f;

    ColorAdjustments _ca;

    protected override void OnEnable()
    {
        EnsureColorAdjustments();
        base.OnEnable();
    }

    void EnsureColorAdjustments()
    {
        if (volume == null)
        {
            foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsInactive.Exclude))
            {
                if (v.isGlobal) { volume = v; break; }
                if (volume == null) volume = v;
            }
        }
        if (volume == null) return;

        // sharedProfile가 아닌 profile을 쓰면 런타임 인스턴스 복제 → 에셋 원본을 건드리지 않음.
        var profile = volume.profile;
        if (!profile.TryGet(out _ca))
            _ca = profile.Add<ColorAdjustments>(true);

        _ca.colorFilter.overrideState = true;
        _ca.postExposure.overrideState = true;
        _ca.saturation.overrideState = true;
    }

    public override void OnDayNight(in DayNightState state)
    {
        if (_ca == null) return;
        float day = state.Daylight01;
        _ca.colorFilter.value = Color.Lerp(nightColorFilter, dayColorFilter, day);
        _ca.postExposure.value = Mathf.Lerp(nightPostExposure, dayPostExposure, day);
        _ca.saturation.value = Mathf.Lerp(nightSaturation, daySaturation, day);
    }
}
