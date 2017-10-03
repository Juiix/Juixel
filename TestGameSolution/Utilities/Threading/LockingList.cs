using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Tools;

namespace Utilities.Threading
{
    public class LockingList<T>
    {
        private object _Sync = new object();
        private List<T> _List = new List<T>();

        public int Count => _List.Count;

        public T this[int Index]
        {
            get
            {
                lock (_Sync)
                    return _List[Index];
            }
        }

        public void Add(T Object)
        {
            lock (_Sync)
            {
                _List.Add(Object);
                CachedArray = null;
            }
        }

        public void Remove(T Object)
        {
            lock (_Sync)
            {
                _List.Remove(Object);
                CachedArray = null;
            }
        }

        private T[] CachedArray;

        public T[] ToArray()
        {
            if (CachedArray != null)
                return CachedArray;

            lock (_Sync)
            {
                CachedArray = _List.ToArray();
                return CachedArray;
            }
        }

        public T[] ToArrayAndClear()
        {
            lock (_Sync)
            {
                T[] Array = _List.ToArray();
                _List = new List<T>();
                CachedArray = null;
                return Array;
            }
        }

        public T TakeFirst()
        {
            lock (_Sync)
            {
                if (Count > 0)
                {
                    T Value = _List[0];
                    _List.RemoveAt(0);
                    return Value;
                }
                return default(T);
            }
        }
    }
}
