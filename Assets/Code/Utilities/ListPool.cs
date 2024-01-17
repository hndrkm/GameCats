namespace CatGame
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    public partial class ListPool<T>
    {
        private const int POOL_CAPACITY = 4;
        private const int LIST_CAPACITY = 16;

        public static readonly ListPool<T> Shared = new ListPool<T>();

        private List<List<T>> _pool = new List<List<T>>(POOL_CAPACITY);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> Get(int capacitty)
        {
            lock (_pool)
            { 
                int poolCount = _pool.Count;
                if (poolCount == 0)
                    return new List<T>(capacitty>0?capacitty:LIST_CAPACITY);
                for (int i = 0; i < poolCount; i++)
                { 
                    List<T> list = _pool[i];
                    if (list.Capacity < capacitty)
                        continue;
                    _pool[i] = _pool[_pool.Count -1];
                    _pool.RemoveAt(_pool.Count - 1);
                    return list;
                }
                int lastListIndex = poolCount -1;
                List<T> lastList = _pool[lastListIndex];
                lastList.Capacity = capacitty;
                _pool.RemoveAt(lastListIndex);
                return lastList;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(List<T> list)
        {
            if (list == null)
                return;

            list.Clear();

            lock (_pool)
            {
                _pool.Add(list);
            }
        }
    }
    public static class ListPool 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Get<T>(int capacity) 
        {
            return ListPool<T>.Shared.Get(capacity);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(List<T> list)
        {
            ListPool<T>.Shared.Return(list);
        }
    }
}
