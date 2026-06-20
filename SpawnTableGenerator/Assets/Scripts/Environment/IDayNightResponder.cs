namespace SpawnSystem.Environment
{
    /// <summary>
    /// 낮/밤 상태 변화를 받아 자기 책임 영역(빛/안개/스폰 등)만 반영하는 구독자.
    /// <see cref="DayNightController"/>는 이 추상에만 의존한다(DIP). 새 반응자를
    /// 추가할 때 컨트롤러를 수정할 필요가 없다(OCP).
    /// </summary>
    public interface IDayNightResponder
    {
        void OnDayNight(in DayNightState state);
    }
}
