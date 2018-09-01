using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DictionaryExtensions {
    public static TValue Value<TKey, TValue> (this IDictionary<TKey, TValue> dict, TKey key) {
        return dict.ContainsKey(key) ? dict[key] : default(TValue);
    }
}

public class FloatSections {
    public float[] sections;

    public FloatSections (float[] inSections) {
        sections = inSections;
    }

    public int length {
        get {
            return sections.Length;
        }
    }

    public int Index (float value) {
        for (int i = 0; i < length; ++i) {
            if (value < sections[i]) return i;
        }
        return length;
    }
}
