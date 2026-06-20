using NUnit.Framework;
using SpawnSystem.Monsters;

namespace SpawnSystem.Tests
{
    /// <summary>군집 상태머신 전이(<see cref="PackFsm"/>) EditMode 테스트. 설계 §5 표.</summary>
    public class PackFsmTests
    {
        static readonly PackPerception P = PackPerception.Default; // investigate 5, loseSight 3

        static PackState Next(PackState s, bool sight, bool noise, float timeInState, float timeSinceSight)
            => PackFsm.Next(s, new PackSenses { SightContact = sight, NoiseHeard = noise, TimeInState = timeInState, TimeSinceSight = timeSinceSight }, P);

        [Test] public void Patrol_Sight_ToEngage()
            => Assert.AreEqual(PackState.Engage, Next(PackState.Patrol, true, false, 0, 0));

        [Test] public void Patrol_Noise_ToAlert()
            => Assert.AreEqual(PackState.Alert, Next(PackState.Patrol, false, true, 0, 0));

        [Test] public void Patrol_Nothing_StaysPatrol()
            => Assert.AreEqual(PackState.Patrol, Next(PackState.Patrol, false, false, 99, 99));

        [Test] public void Patrol_SightBeatsNoise()
            => Assert.AreEqual(PackState.Engage, Next(PackState.Patrol, true, true, 0, 0));

        [Test] public void Alert_Sight_ToEngage()
            => Assert.AreEqual(PackState.Engage, Next(PackState.Alert, true, false, 1f, 0));

        [Test] public void Alert_Timeout_ToPatrol()
            => Assert.AreEqual(PackState.Patrol, Next(PackState.Alert, false, false, 5f, 99));

        [Test] public void Alert_BeforeTimeout_StaysAlert()
            => Assert.AreEqual(PackState.Alert, Next(PackState.Alert, false, false, 2f, 99));

        [Test] public void Engage_LostSight_ToAlert()
            => Assert.AreEqual(PackState.Alert, Next(PackState.Engage, false, false, 1f, 3f));

        [Test] public void Engage_RecentSight_StaysEngage()
            => Assert.AreEqual(PackState.Engage, Next(PackState.Engage, false, false, 1f, 1f));

        [Test] public void Engage_SightResetsAndStays()
            => Assert.AreEqual(PackState.Engage, Next(PackState.Engage, true, false, 1f, 0f));
    }
}
