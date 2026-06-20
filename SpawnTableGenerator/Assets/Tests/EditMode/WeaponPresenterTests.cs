using NUnit.Framework;
using SpawnSystem.UI;

namespace SpawnSystem.Tests
{
    /// <summary>무기 슬롯 MVP Presenter EditMode 테스트.</summary>
    public class WeaponPresenterTests
    {
        [Test]
        public void Initialize_RendersInitialSlot()
        {
            var model = new FakeWeaponModel { ActiveSlot = 0, ActiveName = "근접" };
            var view = new FakeWeaponView();
            var p = new WeaponPresenter(model, view);

            p.Initialize();

            Assert.AreEqual(1, view.RenderCount);
            Assert.AreEqual(0, view.LastSlot);
            Assert.AreEqual("근접", view.LastName);
            p.Dispose();
        }

        [Test]
        public void SlotChange_ReRenders()
        {
            var model = new FakeWeaponModel { ActiveSlot = 0, ActiveName = "근접" };
            var view = new FakeWeaponView();
            var p = new WeaponPresenter(model, view);
            p.Initialize();

            model.ActiveSlot = 1;
            model.ActiveName = "원거리";
            model.Raise();

            Assert.AreEqual(2, view.RenderCount);
            Assert.AreEqual(1, view.LastSlot);
            Assert.AreEqual("원거리", view.LastName);
            p.Dispose();
        }
    }
}
