using System;
using NUnit.Framework;
using SpawnSystem.UI;

namespace SpawnSystem.Tests
{
    /// <summary>HP MVP Presenter EditMode 테스트(가짜 Model/View).</summary>
    public class HealthPresenterTests
    {
        const float Eps = 1e-4f;

        [Test]
        public void Initialize_RendersOnce_WithCurrentValues()
        {
            var model = new FakeHealthModel { Current = 70f, Max = 100f, Normalized = 0.7f };
            var view = new FakeHealthView();
            var p = new HealthPresenter(model, view);

            p.Initialize();

            Assert.AreEqual(1, view.RenderCount);
            Assert.AreEqual(0.7f, view.LastNormalized, Eps);
            Assert.AreEqual(70f, view.LastCurrent, Eps);
            Assert.AreEqual(100f, view.LastMax, Eps);
            p.Dispose();
        }

        [Test]
        public void ModelChanged_ReRendersWithNewValues()
        {
            var model = new FakeHealthModel { Current = 100f, Max = 100f, Normalized = 1f };
            var view = new FakeHealthView();
            var p = new HealthPresenter(model, view);
            p.Initialize();

            model.Current = 40f;
            model.Normalized = 0.4f;
            model.Raise();

            Assert.AreEqual(2, view.RenderCount);
            Assert.AreEqual(0.4f, view.LastNormalized, Eps);
            p.Dispose();
        }

        [Test]
        public void Dispose_StopsReceivingUpdates()
        {
            var model = new FakeHealthModel { Normalized = 1f };
            var view = new FakeHealthView();
            var p = new HealthPresenter(model, view);
            p.Initialize();
            p.Dispose();

            model.Raise();

            Assert.AreEqual(1, view.RenderCount); // Initialize 1회만
        }

        [Test]
        public void NullArguments_Throw()
        {
            var view = new FakeHealthView();
            var model = new FakeHealthModel();
            Assert.Throws<ArgumentNullException>(() => new HealthPresenter(null, view));
            Assert.Throws<ArgumentNullException>(() => new HealthPresenter(model, null));
        }
    }
}
