namespace SpawnSystem.Environment
{
    /// <summary>
    /// 하루 주기의 4구간. NormalizedTime 규약: 0=자정, 0.25=일출, 0.5=정오, 0.75=일몰.
    /// (GameDesign.md §3 낮/밤 사이클)
    /// </summary>
    public enum DayNightPhase
    {
        Night,
        Dawn,
        Day,
        Dusk
    }
}
