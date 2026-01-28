using System;
using System.Collections.Generic;
using UnityEngine;

namespace LowoUN.Util {
    public class Pair<T, U> {
        public T First { get; }
        public U Second { get; }

        public Pair (T first, U second) {
            First = first;
            Second = second;
        }
    }

    public static class Extensions {
        // MonoBehaviour类型的对象 的完全安全形态
        public static bool IsMonoValid (this MonoBehaviour monoObj) {
            return monoObj != null && monoObj.gameObject != null;
        }
        // MonoBehaviour类型的对象完全安全，且处于可视状态，才执行的逻辑（战斗中多为此类情况）
        public static bool IsValid (this MonoBehaviour monoObj) {
            return monoObj != null && monoObj.gameObject != null && monoObj.gameObject.activeInHierarchy;
        }

        public static bool IsValid (this string str) {
            return !string.IsNullOrEmpty (str);
        }
        public static bool IsValid<T> (this List<T> lst) {
            return lst != null && lst.Count > 0;
            // return lst?.Count > 0;
        }
        public static bool IsInValid<T> (this List<T> lst) {
            return lst == null || lst.Count <= 0;
            // return lst?.Count > 0;
        }
        public static bool IsInValid<T, K> (this Dictionary<T, K> dict) {
            return dict == null || dict.Count <= 0;
        }
        public static bool IsValid<T, K> (this Dictionary<T, K> dict) {
            return dict != null && dict.Count > 0;
        }
        public static void AddRange<TKey, TValue> (this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source) {
            foreach (var kvp in source) {
                target[kvp.Key] = kvp.Value;
            }
        }
        public static bool ApproximatelyEqual (this Vector3 a, Vector3 b, float tolerance = 0.001f) {
            return (a - b).sqrMagnitude < tolerance * tolerance;
        }
        public static bool ApproximatelyEqual_V2 (this Vector2 a, Vector2 b, float tolerance = 0.001f) {
            return (a - b).sqrMagnitude < tolerance * tolerance;
        }

        public static bool ApproximatelyEqual (this float a, float b, float tolerance = 0.001f) {
            return Math.Abs (a - b) < tolerance;
        }
    }
}