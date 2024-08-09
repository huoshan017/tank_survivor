using System.Collections.Generic;

namespace Logic.Base
{
    public class Funcs
    {
        public static bool HasElement<Base, T>(IEnumerable<Base> enumerable) where T : class, Base
        {
            foreach (var e in enumerable)
            {
                if (e.GetType() == typeof(T))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasElements<Base, T, U>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
        {
            int c = 0;
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T) || type == typeof(U))
                {
                    c += 1;
                }
                if (c >= 2) return true;
            }
            return false;
        }

        public static bool HasElements<Base, T, U, V>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
        {
            int c = 0;
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T) || type == typeof(U) || type == typeof(V))
                {
                    c += 1;
                }
                if (c >= 3) return true;
            }
            return false;
        }

        public static bool HasElements<Base, T, U, V, W>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
        {
            int c = 0;
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T) || type == typeof(U) || type == typeof(V) || type == typeof(W))
                {
                    c += 1;
                }
                if (c >= 4) return true;
            }
            return false;
        }

        public static bool HasElements<Base, T, U, V, W, X>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
            where X : class, Base
        {
            int c = 0;
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T) || type == typeof(U) || type == typeof(V) || type == typeof(W) || type == typeof(X))
                {
                    c += 1;
                }
                if (c >= 5) return true;
            }
            return false;
        }

        public static bool HasElements<Base, T, U, V, W, X, Y>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
            where X : class, Base
            where Y : class, Base
        {
            int c = 0;
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T) || type == typeof(U) || type == typeof(V) || type == typeof(W) || type == typeof(X) || type == typeof(Y))
                {
                    c += 1;
                }
                if (c >= 6) return true;
            }
            return false;
        }

        public static bool HasElements<Base, T, U, V, W, X, Y, Z>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
            where X : class, Base
            where Y : class, Base
            where Z : class, Base
        {
            int c = 0;
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T) || type == typeof(U) || type == typeof(V) || type == typeof(W) || type == typeof(X) || type == typeof(Y) || type == typeof(Z))
                {
                    c += 1;
                }
                if (c >= 7) return true;
            }
            return false;
        }

        public static T GetElement<Base, T>(IEnumerable<Base> enumerable) where T : class, Base
        {
            foreach (var e in enumerable)
            {
                if (e.GetType() == typeof(T))
                {
                    return (T)e;
                }
            }
            return null;
        }

        public static (T, U) GetElements<Base, T, U>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
        {
            int c = 0;
            (T, U) comps = new();
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T))
                {
                    comps.Item1 = (T)e;
                    c += 1;
                }
                else if (type == typeof(U))
                {
                    comps.Item2 = (U)e;
                    c += 1;
                }
                if (c >= 2) break;
            }
            return comps;
        }

        public static (T, U, V) GetElements<Base, T, U, V>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
        {
            int c = 0;
            (T, U, V) comps = new();
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T))
                {
                    comps.Item1 = (T)e;
                    c += 1;
                }
                else if (type == typeof(U))
                {
                    comps.Item2 = (U)e;
                    c += 1;
                }
                else if (type == typeof(V))
                {
                    comps.Item3 = (V)e;
                    c += 1;
                }
                if (c >= 3) break;
            }
            return comps;
        }

        public static (T, U, V, W) GetElements<Base, T, U, V, W>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
        {
            int c = 0;
            (T, U, V, W) comps = new();
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T))
                {
                    comps.Item1 = (T)e;
                    c += 1;
                }
                else if (type == typeof(U))
                {
                    comps.Item2 = (U)e;
                    c += 1;
                }
                else if (type == typeof(V))
                {
                    comps.Item3 = (V)e;
                    c += 1;
                }
                else if (type == typeof(W))
                {
                    comps.Item4 = (W)e;
                    c += 1;
                }
                if (c >= 4) break;
            }
            return comps;
        }

        public static (T, U, V, W, X) GetElements<Base, T, U, V, W, X>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
            where X : class, Base
        {
            int c = 0;
            (T, U, V, W, X) comps = new();
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T))
                {
                    comps.Item1 = (T)e;
                    c += 1;
                }
                else if (type == typeof(U))
                {
                    comps.Item2 = (U)e;
                    c += 1;
                }
                else if (type == typeof(V))
                {
                    comps.Item3 = (V)e;
                    c += 1;
                }
                else if (type == typeof(W))
                {
                    comps.Item4 = (W)e;
                    c += 1;
                }
                else if (type == typeof(X))
                {
                    comps.Item5 = (X)e;
                    c += 1;
                }
                if (c >= 5) break;
            }
            return comps;
        }

        public static (T, U, V, W, X, Y) GetElements<Base, T, U, V, W, X, Y>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
            where X : class, Base
            where Y : class, Base
        {
            int c = 0;
            (T, U, V, W, X, Y) comps = new();
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T))
                {
                    comps.Item1 = (T)e;
                    c += 1;
                }
                else if (type == typeof(U))
                {
                    comps.Item2 = (U)e;
                    c += 1;
                }
                else if (type == typeof(V))
                {
                    comps.Item3 = (V)e;
                    c += 1;
                }
                else if (type == typeof(W))
                {
                    comps.Item4 = (W)e;
                    c += 1;
                }
                else if (type == typeof(X))
                {
                    comps.Item5 = (X)e;
                    c += 1;
                }
                else if (type == typeof(Y))
                {
                    comps.Item6 = (Y)e;
                    c += 1;
                }
                if (c >= 6) break;
            }
            return comps;
        }

        public static (T, U, V, W, X, Y, Z) GetElements<Base, T, U, V, W, X, Y, Z>(IEnumerable<Base> enumerable)
            where T : class, Base
            where U : class, Base
            where V : class, Base
            where W : class, Base
            where X : class, Base
            where Y : class, Base
            where Z : class, Base
        {
            int c = 0;
            (T, U, V, W, X, Y, Z) comps = new();
            foreach (var e in enumerable)
            {
                var type = e.GetType();
                if (type == typeof(T))
                {
                    comps.Item1 = (T)e;
                    c += 1;
                }
                else if (type == typeof(U))
                {
                    comps.Item2 = (U)e;
                    c += 1;
                }
                else if (type == typeof(V))
                {
                    comps.Item3 = (V)e;
                    c += 1;
                }
                else if (type == typeof(W))
                {
                    comps.Item4 = (W)e;
                    c += 1;
                }
                else if (type == typeof(X))
                {
                    comps.Item5 = (X)e;
                    c += 1;
                }
                else if (type == typeof(Y))
                {
                    comps.Item6 = (Y)e;
                    c += 1;
                }
                else if (type == typeof(Z))
                {
                    comps.Item7 = (Z)e;
                    c += 1;
                }
                if (c >= 7) break;
            }
            return comps;
        }

        public static bool HasElementDerivedFromT<Base, T>(IEnumerable<Base> enumerable) where T : class, Base
        {
            foreach (var e in enumerable)
            {
                if (e.GetType().IsSubclassOf(typeof(T)))
                {
                    return true;
                }
            }
            return false;
        }

        public static T GetElementDerivedFromT<Base, T>(IEnumerable<Base> enumerable) where T : class, Base
        {
            foreach (var e in enumerable)
            {
                if (e.GetType().IsSubclassOf(typeof(T)))
                {
                    return (T)e;
                }
            }
            return null;
        }
    }
}