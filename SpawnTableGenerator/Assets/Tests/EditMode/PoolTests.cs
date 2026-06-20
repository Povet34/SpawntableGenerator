using NUnit.Framework;
using SpawnSystem.Spawning;

namespace SpawnSystem.Tests
{
    /// <summary>범용 풀(<see cref="Pool{T}"/>) EditMode 테스트. 설계 §3 재사용.</summary>
    public class PoolTests
    {
        [Test]
        public void Get_CreatesWhenEmpty()
        {
            int created = 0;
            var pool = new Pool<int>(() => ++created);
            pool.Get();
            Assert.AreEqual(1, pool.CreatedCount);
            Assert.AreEqual(1, pool.ActiveCount);
        }

        [Test]
        public void Get_ReusesReleased_WithoutCreatingNew()
        {
            int created = 0;
            var pool = new Pool<int>(() => ++created);
            var a = pool.Get();   // 생성 1
            pool.Release(a);
            pool.Get();           // 재사용 → 생성 그대로
            Assert.AreEqual(1, pool.CreatedCount);
            Assert.AreEqual(1, created);
        }

        [Test]
        public void Counts_TrackActiveAndFree()
        {
            int created = 0;
            var pool = new Pool<int>(() => ++created);
            pool.Get();
            pool.Get();
            var c = pool.Get();   // active 3, created 3
            Assert.AreEqual(3, pool.ActiveCount);
            Assert.AreEqual(0, pool.FreeCount);

            pool.Release(c);      // active 2, free 1
            Assert.AreEqual(2, pool.ActiveCount);
            Assert.AreEqual(1, pool.FreeCount);

            pool.Get();           // 재사용 → active 3, created 여전히 3
            Assert.AreEqual(3, pool.ActiveCount);
            Assert.AreEqual(3, pool.CreatedCount);
        }

        [Test]
        public void Prewarm_CreatesUpfront_AllFree()
        {
            int created = 0;
            var pool = new Pool<int>(() => ++created, prewarm: 5);
            Assert.AreEqual(5, pool.CreatedCount);
            Assert.AreEqual(5, pool.FreeCount);
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void OnGet_OnRelease_AreInvoked()
        {
            int gets = 0, releases = 0;
            var pool = new Pool<int>(() => 1, onGet: _ => gets++, onRelease: _ => releases++);
            var a = pool.Get();
            pool.Release(a);
            Assert.AreEqual(1, gets);
            Assert.AreEqual(1, releases);
        }
    }
}
