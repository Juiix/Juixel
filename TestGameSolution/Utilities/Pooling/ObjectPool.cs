using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Tools;

namespace Utilities.Pooling
{
    /*public static class ObjectPool
    {
        private static ConcurrentDictionary<Type, ConcurrentDictionary<IRecyclable, bool>> _Objects = new ConcurrentDictionary<Type, ConcurrentDictionary<IRecyclable, bool>>();

        public static T Get<T>() where T : IRecyclable
        {
            Type Type = typeof(T);
            var Dict = GetDict(Type);
            var Pairs = Dict.ToArray();
            for (int i = 0; i < Pairs.Length; i++)
                if (!Pairs[i].Value)
                    if (Dict.TryUpdate(Pairs[i].Key, true, false))
                        return (T)Pairs[i].Key;
            T Obj = (T)Activator.CreateInstance(Type);
            Dict.TryAdd(Obj, true);
            Obj.Initialize();
            return Obj;
        }

        public static void Return(IRecyclable Obj)
        {
            Type Type = Obj.GetType();
            var Dict = GetDict(Type);
            Obj.Recycle();
            Dict.TryUpdate(Obj, false, true);
        }

        private static ConcurrentDictionary<IRecyclable, bool> GetDict(Type T)
        {
            ConcurrentDictionary<IRecyclable, bool> Dict;
            if (_Objects.TryGetValue(T, out Dict))
                return Dict;
            Dict = new ConcurrentDictionary<IRecyclable, bool>();
            if (_Objects.TryAdd(T, Dict))
                return Dict;
            else
                return GetDict(T);
        }
    }*/
}
