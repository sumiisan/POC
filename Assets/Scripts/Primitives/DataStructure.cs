using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DictionaryExtensions {
    public static TValue Value<TKey, TValue> (this IDictionary<TKey, TValue> dict, TKey key) {
        return dict.ContainsKey(key) ? dict[key] : default(TValue);
    }
}