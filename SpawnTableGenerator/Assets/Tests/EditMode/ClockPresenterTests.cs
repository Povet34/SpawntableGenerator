using NUnit.Framework;
using SpawnSystem.Environment;
using SpawnSystem.UI;

namespace SpawnSystem.Tests
{
    /// <summary>낮/밤 시계 MVP Presenter + ClockModel 포맷 헬퍼 EditMode 테스트.</summary>
    public class ClockPresenterTests
    {
        [Test]
        public void Initialize_RendersInitialState()
        {
            var model = new FakeClockModel
            {
                Phase = DayNightPhase.Day,
                NormalizedTime = 0.5f,
                Daylight01 = 1f,
                Label = "낮 12:00"
            };
            var view = new FakeClockView();
            var p = new ClockPresenter(model, view);

            p.Initialize();

            Assert.AreEqual(1, view.RenderCount);
            Assert.AreEqual(DayNightPhase.Day, view.LastPhase);
            Assert.AreEqual("낮 12:00", view.LastLabel);
            p.Dispose();
        }

        [Test]
        public void StateChange_ReRenders()
        {
            var model = new FakeClockModel { Phase = DayNightPhase.Day, Daylight01 = 1f };
            var view = new FakeClockView();
            var p = new ClockPresenter(model, view);
            p.Initialize();

            model.Phase = DayNightPhase.Night;
            model.Daylight01 = 0f;
            model.Raise();

            Assert.AreEqual(2, view.RenderCount);
            Assert.AreEqual(DayNightPhase.Night, view.LastPhase);
            Assert.AreEqual(0f, view.LastDaylight, 1e-4f);
            p.Dispose();
        }

        [Test]
        public void ClockString_FormatsTwentyFourHour()
        {
            Assert.AreEqual("00:00", ClockModel.ClockString(0f));
            Assert.AreEqual("12:00", ClockModel.ClockString(0.5f));
            Assert.AreEqual("06:00", ClockModel.ClockString(0.25f));
            Assert.AreEqual("18:00", ClockModel.ClockString(0.75f));
        }

        [Test]
        public void PhaseLabel_KoreanLabels()
        {
            Assert.AreEqual("새벽", ClockModel.PhaseLabel(DayNightPhase.Dawn));
            Assert.AreEqual("낮", ClockModel.PhaseLabel(DayNightPhase.Day));
            Assert.AreEqual("황혼", ClockModel.PhaseLabel(DayNightPhase.Dusk));
            Assert.AreEqual("밤", ClockModel.PhaseLabel(DayNightPhase.Night));
        }
    }
}
