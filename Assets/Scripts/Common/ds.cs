using System;
using System.Collections.Generic;

namespace Common
{
    public struct Pair<Key, Value>
    {
        public Key key;
        public Value value;
    }

    public class MapArrayCombo<Key, Value>
    {
        readonly Dictionary<Key, int> key2Index_; // key映射到索引
        Pair<Key, Value>[] pairList_; // key-value队列

        public MapArrayCombo()
        {
            key2Index_ = new Dictionary<Key, int>();
            pairList_ = new Pair<Key, Value>[32];
        }

        public void Add(Key key, Value value)
        {
            if (key2Index_.ContainsKey(key))
                return;
            add(key, value);
        }

        public bool Exists(Key key)
        {
            return key2Index_.ContainsKey(key);
        }

        public void Set(Key key, Value value)
        {
            if (key2Index_.ContainsKey(key))
            {
                var index = key2Index_[key];
                pairList_[index] = new Pair<Key, Value>(){key=key, value=value};
            }
            else
            {
                add(key, value);
            }
        }

        public bool Get(Key key, ref Value value)
        {
            if (!key2Index_.TryGetValue(key, out var index))
            {
                return false;
            }
            value = pairList_[index].value;
            return true;
        }

        public bool Remove(Key key, ref Value value)
        {
            // 删除key与index的索引映射
            if (!key2Index_.Remove(key, out var index))
            {
                return false;
            }

            /// 队列中的kv对删除需要一些特殊处理
            value = pairList_[index].value;

            // 最后一个索引
            var lastIndex = pairList_.Length - 1;
            // 如果被删除的不是队列最后一个，把最后一个pair挪到删除掉的pair的位置上
            if (index != lastIndex)
            {
                var lastIndexPair = pairList_[lastIndex];
                // 挪到需要删除的那个值的位置上
                pairList_[index] = lastIndexPair;
                // 更新索引
                key2Index_[lastIndexPair.key] = index;
            }
            
            return true;
        }

        public bool GetByIndex(int index, ref Key key, ref Value value)
        {
            int count = key2Index_.Count;
            if (index >= count)
            {
                return false;
            }
            key = pairList_[index].key;
            value = pairList_[index].value;
            return true;
        }

        public List<Key> GetKeyList()
        {
            int count = key2Index_.Count;
            if (count == 0) return null;

            var keyList = new List<Key>();
            for (int i=0; i<count; i++)
            {
                keyList.Add(pairList_[i].key);
            }
            return keyList;
        }

        public List<Value> GetValueList()
        {
            int count = key2Index_.Count;
            if (count == 0) return null;

            var valueList = new List<Value>();
            for (int i=0; i<count; i++)
            {
                valueList.Add(pairList_[i].value);
            }
            return valueList;
        }

        public void Clear()
        {
            key2Index_.Clear();
            Array.Clear(pairList_, 0, pairList_.Length);
        }

        private void add(Key key, Value value)
        {
            int count = key2Index_.Count;
            // 重新分配空间
            if (pairList_.Length <= count) {
                Array.Resize(ref pairList_, count+32);
            }
            // 把值追加到队列尾部
            pairList_[count] = new Pair<Key, Value>(){key=key, value=value};
            // 建立key跟索引的映射
            key2Index_[key] = count;
        }
    }

    public class MapListCombo<Key, Value>
    {
        readonly Dictionary<Key, LinkedListNode<Pair<Key, Value>>> key2Node_; // key映射到链表节点
        readonly LinkedList<Pair<Key, Value>> pairList_; // key-value链表

        public MapListCombo()
        {
            key2Node_ = new Dictionary<Key, LinkedListNode<Pair<Key, Value>>>();
            pairList_ = new LinkedList<Pair<Key, Value>>();
        }

        public void Add(Key key, Value value)
        {
            if (key2Node_.ContainsKey(key))
                return;

            add(key, value);
        }

        public bool Exists(Key key)
        {
            return key2Node_.ContainsKey(key);
        }

        public void Set(Key key, Value value)
        {
            if (key2Node_.ContainsKey(key))
            {
                var node = key2Node_[key];
                node.Value = new Pair<Key, Value>{key=key, value=value};
            }
            else
            {
                add(key, value);
            }
        }

        public bool Get(Key key, ref Value value)
        {
            if (!key2Node_.TryGetValue(key, out var node))
            {
                return false;
            }
            value = node.Value.value;
            return true;
        }

        public bool Remove(Key key)
        {
            // 删除key与index的索引映射
            if (!key2Node_.Remove(key, out var node))
            {
                return false;
            }

            /// 队列中的kv对删除需要一些特殊处理
            pairList_.Remove(node);
            
            return true;
        }

        public List<Key> GetKeyList()
        {
            int count = key2Node_.Count;
            if (count == 0) return null;

            var keyList = new List<Key>();
            foreach (var node in pairList_)
            {
                keyList.Add(node.key);
            }
            return keyList;
        }

        public List<Value> GetValueList()
        {
            int count = key2Node_.Count;
            if (count == 0) return null;

            var valueList = new List<Value>();
            foreach (var node in pairList_)
            {
                valueList.Add(node.value);
            }
            return valueList;
        }

        public List<Pair<Key, Value>> GetKeyValueList()
        {
            if (key2Node_.Count == 0) return null;
            var keyValueList = new List<Pair<Key, Value>>();
            foreach (var node in pairList_)
            {
                keyValueList.Add(node);
            }
            return keyValueList;
        }

        public void Clear()
        {
            key2Node_.Clear();
            pairList_.Clear();
        }

        public void Foreach(Action<Key, Value> action)
        {
            foreach (var n in pairList_)
            {
                action(n.key, n.value);
            }
        }

        private void add(Key key, Value value)
        {
            // 把值追加到链表尾部
            var node = pairList_.AddLast(new Pair<Key, Value>(){key=key, value=value});
            // 建立key跟节点的映射
            key2Node_[key] = node;
        }       
    }
}