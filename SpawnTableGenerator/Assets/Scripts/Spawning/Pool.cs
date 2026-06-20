using System;
using System.Collections.Generic;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 범용 오브젝트 풀(설계 §3 — Instantiate/Destroy 금지, 재사용). 비어 있을 때만 factory 로 생성하고,
    /// Release 된 항목을 우선 재사용한다. onGet/onRelease 로 활성/비활성 등 부수효과 처리.
    /// </summary>
    public class Pool<T>
    {
        readonly Func<T> _factory;
        readonly Action<T> _onGet;
        readonly Action<T> _onRelease;
        readonly Stack<T> _free = new Stack<T>();
        int _created;

        public int CreatedCount => _created;       // 지금까지 만든 총 개수(메모리 발자국)
        public int FreeCount => _free.Count;        // 재사용 대기 중
        public int ActiveCount => _created - _free.Count;

        public Pool(Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null, int prewarm = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet;
            _onRelease = onRelease;
            for (int i = 0; i < prewarm; i++)
                _free.Push(CreateNew());
        }

        T CreateNew()
        {
            _created++;
            return _factory();
        }

        public T Get()
        {
            T item = _free.Count > 0 ? _free.Pop() : CreateNew();
            _onGet?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {
            _onRelease?.Invoke(item);
            _free.Push(item);
        }
    }
}
