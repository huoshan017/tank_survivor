using System;
using System.Collections.Generic;

namespace Common
{
    // 二叉堆
    public class BinaryHeap<T> where T : IComparable<T>
    {
        readonly bool maxHeap_;
        readonly List<T> array_;

        public BinaryHeap(bool maxHeap = true)
        {
            maxHeap_ = maxHeap;
            array_ = new();
        }

        public void Clear()
        {
            if (array_.Count > 0)
            {
                array_.Clear();
            }
        }

        public void Set(T v)
        {
            var l = array_.Count;
            array_.Add(v);
            AdjustUp(l);
        }

        public bool Get(out T v)
        {
            var l = array_.Count;
            if (l <= 0)
            {
                v = default;
                return false;
            }
            v = array_[0];
            array_[0] = array_[l-1];
            array_.RemoveAt(l-1);
            l -= 1;
            AdjustDown(l-1);
            return true;
        }

        public bool Peek(out T v)
        {
            if (array_.Count <= 0)
            {
                v = default;
                return false;
            }
            v = array_[0];
            return true;
        }

        public int Length()
        {
            return array_.Count;
        }

        void AdjustUp(int n)
        {
            var p = (n - 1) / 2;
            while (p >= 0)
            {
                if (maxHeap_)
                {
                    if (array_[n].CompareTo(array_[p]) <= 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (array_[n].CompareTo(array_[p]) >= 0)
                    {
                        break;
                    }
                }
                (array_[n], array_[p]) = (array_[p], array_[n]);
                n = p;
                p = (p - 1) / 2;
            }
        }

        void AdjustDown(int n)
        {
            int c = 0, m;
            var l = 2 * c + 1;
            var r = 2 * c + 1;

            while (l <= n)
            {
                m = l;
                if (maxHeap_)
                {
                    if (r <= n && array_[m].CompareTo(array_[r]) < 0)
                    {
                        m = r;
                    }
                    if (array_[c].CompareTo(array_[m]) >= 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (r <= n && array_[m].CompareTo(array_[r]) > 0)
                    {
                        m = r;
                    }
                    if (array_[c].CompareTo(array_[m]) <= 0)
                    {
                        break;
                    }
                }
                (array_[c], array_[m]) = (array_[m], array_[c]);
                c = m;
                l = 2 * c + 1;
                r = 2 * c + 2;
            }
        }
    }

    // 大顶二叉堆
    public class MaxBinaryHeap<T> : BinaryHeap<T> where T : IComparable<T>
    {
        public MaxBinaryHeap() : base(true)
        {

        }
    }

    // 小顶二叉堆
    public class MinBinaryHeap<T> : BinaryHeap<T> where T : IComparable<T>
    {
        public MinBinaryHeap() : base(false)
        {

        }
    }

    struct KV<K, V> where K : IEquatable<K> where V : IComparable<V>
    {
        public K k;
        public V v;
    }

    // 二叉KV堆
    public class BinaryHeapKV<K, V> where K : IEquatable<K> where V : IComparable<V>
    {
        readonly bool maxHeap_;
        readonly List<KV<K, V>> array_;
        readonly Dictionary<K, int> k2n_;
        
        public BinaryHeapKV(bool maxHeap = true)
        {
            maxHeap_ = maxHeap;
            array_ = new();
            k2n_ = new();
        }

        public void Clear()
        {
            k2n_.Clear();
            array_.Clear();
        }

        public void Set(K k, V v)
        {
            var l = array_.Count;
            if (!k2n_.TryGetValue(k, out var idx))
            {
                array_.Add(new KV<K, V>(){k=k,v=v});
                l += 1;
                k2n_[k] = l - 1;
                AdjustUp(l - 1);
            }
            else
            {
                if (array_[idx].v.CompareTo(v) == 0)
                {
                    return;
                }

                var kv = array_[idx];
                kv.v = v;
                array_[idx] = kv;

                // 与原来的比较大小
                if (v.CompareTo(array_[idx].v) > 0)
                {
                    if (maxHeap_)
                    {
                        AdjustUp(idx);
                    }
                    else
                    {
                        AdjustDown(idx, l - 1);
                    }
                }
                else
                {
                    if (!maxHeap_)
                    {
                        AdjustUp(idx);
                    }
                    else
                    {
                        AdjustDown(idx, l - 1);
                    }
                }
            }
        }

        public bool Get(out (K, V) kv)
        {
            var l = array_.Count;
            if (l <= 0)
            {
                kv = new();
                return false;
            }

            var pkv = array_[0];
            array_[0] = array_[l-1];
            array_.RemoveAt(l-1);
            l -= 1;
            k2n_.Remove(pkv.k);
            if (l > 0)
            {
                k2n_[array_[0].k] = 0;
            }
            AdjustDown(0, l-1);
            kv.Item1 = pkv.k;
            kv.Item2 = pkv.v;
            return true;
        }

        public bool Peek(out (K, V) kv)
        {
            if (array_.Count <= 0)
            {
                kv = new();
                return false;
            }
            kv.Item1 = array_[0].k;
            kv.Item2 = array_[0].v;
            return true;
        }

        public int Length()
        {
            return array_.Count;
        }

        public bool Delete(K k, out V v)
        {
            if (!k2n_.TryGetValue(k, out var n))
            {
                v = default;
                return false;
            }
            v = array_[n].v;
            var l = array_.Count;
            if (n != l - 1)
            {
                array_[n] = array_[l-1];
                k2n_[array_[n].k] = n;
            }

            k2n_.Remove(k);
            array_.RemoveAt(l-1);
            l -= 1;
            AdjustDown(n, l-1);
            return true;
        }

        void AdjustUp(int n)
        {
            var p = (n - 1) / 2;
            while (p >= 0)
            {
                if (maxHeap_)
                {
                    if (array_[n].v.CompareTo(array_[p].v) <= 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (array_[n].v.CompareTo(array_[p].v) >= 0)
                    {
                        break;
                    }
                }

                (array_[p], array_[n]) = (array_[n], array_[p]);
                var kn = array_[n].k;
                var kp = array_[p].k;
                k2n_[kn] = n;
                k2n_[kp] = p;
                n = p;
                p = (p - 1) / 2;
            }
        }

        void AdjustDown(int c, int n)
        {
            int m;
            var l = 2*c + 1; // left child
            var r = 2*c + 2; // right child

            while (l <= n)
            {
                m = l;
                if (maxHeap_)
                {
                    if (r <= n && array_[m].v.CompareTo(array_[r].v) < 0)
                    {
                        m = r;
                    }
                    if (array_[c].v.CompareTo(array_[m].v) >= 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (r <= n && array_[m].v.CompareTo(array_[r].v) > 0)
                    {
                        m = r;
                    }
                    if (array_[c].v.CompareTo(array_[m].v) <= 0)
                    {
                        break;
                    }
                }
                (array_[c], array_[m]) = (array_[m], array_[c]);
                var kc = array_[c].k;
                var km = array_[m].k;
                k2n_[kc] = c;
                k2n_[km] = m;
                c = m;
                l = 2 * c + 1;
                r = 2 * c + 2;
            }
        }
    }

    // 二叉大顶kv堆
    public class MaxBinaryHeapKV<K, V> : BinaryHeapKV<K, V> where K : IEquatable<K> where V : IComparable<V>
    {
        public MaxBinaryHeapKV() : base(true)
        {
        }
    }

    // 二叉小顶kv堆
    public class MinBinaryHeapKV<K, V> : BinaryHeapKV<K, V> where K : IEquatable<K> where V : IComparable<V>
    {
        public MinBinaryHeapKV() : base(false)
        {
        }
    }

    // 四叉堆
    public class QuadHeap<T> where T : IComparable<T>
    {
        readonly bool maxHeap_;
        readonly List<T> array_;

        public QuadHeap(bool maxHeap = true)
        {
            maxHeap_ = maxHeap;
            array_ = new();
        }

        public void Clear()
        {
            array_.Clear();
        }

        public void Set(T v)
        {
            var l = array_.Count;
            array_.Add(v);
            AdjustUp(l);
        }

        public bool Get(out T v)
        {
            var l = array_.Count;
            if (l <= 0)
            {
                v = default;
                return false;
            }
            v = array_[0];
            array_[0] = array_[l-1];
            array_.RemoveAt(l-1);
            l -= 1;
            AdjustDown(l-1);
            return true;
        }

        public bool Peek(out T v)
        {
            if (array_.Count <= 0)
            {
                v = default;
                return false;
            }
            v = array_[0];
            return true;
        }

        public int Length()
        {
            return array_.Count;
        }

        void AdjustUp(int n)
        {
            var p = (n - 1) / 4;
            while (p >= 0)
            {
                if (maxHeap_)
                {
                    if (array_[n].CompareTo(array_[p]) <= 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (array_[n].CompareTo(array_[p]) >= 0)
                    {
                        break;
                    }
                }
                (array_[n], array_[p]) = (array_[p], array_[n]);
                n = p;
                p = (p - 1) / 4;
            }
        }

        void AdjustDown(int n)
        {
            int s = 0, m = 0;
            int c0 = 4*s + 1;
            int c1 = 4*s + 2;
            int c2 = 4*s + 3;
            int c3 = 4*s + 4;

            while (c0 <= n)
            {
                s = c0;
                if (maxHeap_)
                {
                    if (c1 <= n && array_[m].CompareTo(array_[c1]) < 0)
                    {
                        m = c1;
                    }
                    if (c2 <= n && array_[m].CompareTo(array_[c2]) < 0)
                    {
                        m = c2;
                    }
                    if (c3 <= n && array_[m].CompareTo(array_[c3]) < 0)
                    {
                        m = c3;
                    }
                    if (array_[s].CompareTo(array_[m]) >= 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (c1 <= n && array_[m].CompareTo(array_[c1]) > 0)
                    {
                        m = c1;
                    }
                    if (c2 <= n && array_[m].CompareTo(array_[c2]) > 0)
                    {
                        m = c2;
                    }
                    if (c3 <= n && array_[m].CompareTo(array_[c3]) > 0)
                    {
                        m = c3;
                    }
                    if (array_[s].CompareTo(array_[m]) >= 0)
                    {
                        break;
                    }
                }
                (array_[s], array_[m]) = (array_[m], array_[s]);
                s = m;
                c0 = 4*s + 1;
                c1 = 4*s + 2;
                c2 = 4*s + 3;
                c3 = 4*s + 4;
            }
        }
    }

    // 大顶四叉堆
    public class MaxQuadHeap<T> : QuadHeap<T> where T : IComparable<T>
    {
        public MaxQuadHeap() : base(true)
        {

        }
    }

    // 小顶四叉堆
    public class MinQuadHeap<T> : QuadHeap<T> where T : IComparable<T>
    {
        public MinQuadHeap() : base(false)
        {

        }
    }
}