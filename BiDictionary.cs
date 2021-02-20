using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Collections;

namespace Tyfyter.Utils {
    [DebuggerDisplay("Count = {Count}")]
    public class BiDict<TKey, TVal> : IDictionary<TKey,TVal>{
        public readonly EqualityComparer<TKey> KeyComparer;
        public readonly EqualityComparer<TVal> ValueComparer;
        public readonly bool AddOnSet = false;

        public List<Entry> Entries;

        public ICollection<TKey> Keys => (ICollection<TKey>)Entries.Select((e)=>e.key);
        public ICollection<TVal> Values => (ICollection<TVal>)Entries.Select((e)=>e.val);

        public int Count => Entries.Count;
        public bool IsReadOnly => false;

        public BiDict(EqualityComparer<TKey> keyComparer = null, EqualityComparer<TVal> valueComparer = null, bool addOnSet = false) {
            Entries = new List<Entry>();
            KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            ValueComparer = valueComparer ?? EqualityComparer<TVal>.Default;
            AddOnSet = addOnSet;
        }
        public BiDict(List<Entry> input, EqualityComparer<TKey> keyComparer = null, EqualityComparer<TVal> valueComparer = null, bool addOnSet = false) : this(keyComparer, valueComparer, addOnSet){
            Entries = input;
        }

        public TVal this[TKey key] {
            get => Entries[FindEntryByKey(key)].val;
            set {
                int i = FindEntryByKey(key);
                if(i == -1 && AddOnSet) {
                    Add(key, value);
                }
                Entries[i] = (key, value);
            }
        }
        public TKey this[TVal val] {
            get => Entries[FindEntryByValue(val)].key;
            set {
                int i = FindEntryByValue(val);
                if(i == -1 && AddOnSet) {
                    Add(value, val);
                }
                Entries[i] = (value, val);
            }
        }

        public int FindEntryByKey(TKey key) {
            int keyHash = KeyComparer.GetHashCode(key);
            for(int entry = 0; entry < Entries.Count; entry++){
                if(KeyComparer.GetHashCode(Entries[entry].key)==keyHash && KeyComparer.Equals(Entries[entry].key, key))return entry;
            }
            return -1;
        }
        public int FindEntryByValue(TVal val) {
            int keyHash = ValueComparer.GetHashCode(val);
            for(int entry = 0; entry < Entries.Count; entry++){
                if(ValueComparer.GetHashCode(Entries[entry].val)==keyHash && ValueComparer.Equals(Entries[entry].val, val))return entry;
            }
            return -1;
        }

        public bool ContainsKey(TKey key) => FindEntryByKey(key) != -1;
        public bool ContainsValue(TVal value) => FindEntryByValue(value) != -1;

        public void Add(TKey key, TVal value) {
            if(ContainsKey(key)) {
                throw new ArgumentException("Cannot add duplicate key");
            }
            if(ContainsValue(value)) {
                throw new ArgumentException("Cannot add duplicate value");
            }
            Entries.Add((key, value));
        }

        [Obsolete("Use RemoveKey or RemoveValue instead", false)]
        public bool Remove(TKey key) {
            int index = FindEntryByKey(key);
            if(index != -1) {
                Entries.RemoveAt(index);
                return true;
            }
            return false;
        }
        public bool RemoveKey(TKey key) {
            int index = FindEntryByKey(key);
            if(index != -1) {
                Entries.RemoveAt(index);
                return true;
            }
            return false;
        }
        public bool RemoveValue(TVal value) {
            int index = FindEntryByValue(value);
            if(index != -1) {
                Entries.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TVal value) {
            int i = FindEntryByKey(key);
            if (i >= 0) {
                value = Entries[i].val;
                return true;
            }
            value = default;
            return false;
        }
        public bool TryGetKey(TVal value, out TKey key) {
            int i = FindEntryByValue(value);
            if (i >= 0) {
                key = Entries[i].key;
                return true;
            }
            key = default;
            return false;
        }

        public void Add(KeyValuePair<TKey, TVal> item) {
            Add(item.Key, item.Value);
        }
        public void Clear() {
            Entries.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TVal> item) {
            return TryGetValue(item.Key, out TVal val)&&val.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TVal>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<TKey, TVal>>)Entries.Select((e) => new KeyValuePair<TKey, TVal>(e.key, e.val))).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TVal> item) {
            int index = FindEntryByKey(item.Key);
            if(index != -1 && ValueComparer.Equals(Entries[index].val, item.Value)) {
                Entries.RemoveAt(index);
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TVal>> GetEnumerator() {
            return Entries.Select((e) => new KeyValuePair<TKey, TVal>(e.key, e.val)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Entries.GetEnumerator();
        }

        public override string ToString() {
            string o = "[";
            for(int i = 0; i < Entries.Count; i++) {
                o+=Entries[i].ToString();
                if(i<Entries.Count-1)o+=",";
            }
            return o+"]";
        }

        public string ToSwitch() {
            string o = "switch(){\n";
            for(int i = 0; i < Entries.Count; i++) {
                o+=$"case {Entries[i].key}:\nreturn {Entries[i].val}\n";
            }
            return o+"}";
        }

        public struct Entry {
            public TKey key;
            public TVal val;
            public Entry(TKey key, TVal val) {
                this.key=key;
                this.val=val;
            }
            public static implicit operator string(Entry input){
                return "{"+"Key:"+input.key.ToString()+", Value:"+input.val.ToString()+"}";
            }
            public static implicit operator (TKey,TVal)(Entry input){
                return (input.key,input.val);
            }
            public static implicit operator Entry((TKey,TVal) input){
                return new Entry(input.Item1,input.Item2);
            }
            public override string ToString() {
                return "{key:"+key.ToString()+",value:"+val.ToString()+"}";
            }
        }
    }
}