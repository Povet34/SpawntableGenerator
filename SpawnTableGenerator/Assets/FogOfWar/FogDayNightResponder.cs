using SpawnSystem.Environment;
using UnityEngine;

/// <summary>
/// 낮/밤에 따라 FogReveal의 시야 반경을 조절한다(낮=넓게, 밤=좁게).
/// VolumetricFog 패키지에 의존하므로 Assembly-CSharp(Assets/FogOfWar)에 두지만,
/// 핵심 어셈블리의 <see cref="DayNightResponderBehaviour"/>를 상속해 자가 등록한다.
/// → 핵심 코드는 VolumetricFog를 전혀 모른다(OCP, 어셈블리 경계 너머 확장).
/// </summary>
public class FogDayNightResponder : DayNightResponderBehaviour
{
    [Tooltip("비우면 씬에서 FogReveal 자동 탐색")]
    public FogReveal fogReveal;

    protected override void OnEnable()
    {
        if (fogReveal == null)
            fogReveal = Object.FindAnyObjectByType<FogReveal>();
        base.OnEnable();
    }

    public override void OnDayNight(in DayNightState state)
    {
        if (fogReveal != null)
            fogReveal.revealRadius = state.ViewRadius;
    }
}
