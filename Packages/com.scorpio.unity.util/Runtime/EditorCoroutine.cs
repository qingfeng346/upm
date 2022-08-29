#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Scorpio.Unity.Util {
    public static class EditorCoroutineUtility {
        private static List<EditorCoroutine> m_Coroutines = new List<EditorCoroutine>();
        public static void Start(IEnumerator enumerator) {
            m_Coroutines.Add(new EditorCoroutine(enumerator));
        }
        public static void StopAll() {
            m_Coroutines.Clear();
        }
        public static void Wait() {
            while (m_Coroutines.Count > 0) {
                for (var i = 0; i < m_Coroutines.Count;) {
                    if (m_Coroutines[i].MoveNext()) {
                        m_Coroutines.RemoveAt(i);
                    } else {
                        ++i;
                    }
                }
            }
        }
    }
    /// <summary>
    /// A handle to an EditorCoroutine, can be passed to <see cref="EditorCoroutineUtility">EditorCoroutineUtility</see> methods to control lifetime.
    /// </summary>
    internal class EditorCoroutine {
        private struct YieldProcessor {
            enum DataType : byte {
                None = 0,
                WaitForSeconds = 1,
                EditorCoroutine = 2,
                AsyncOP = 3,
            }
            struct ProcessorData {
                public DataType type;
                public double targetTime;
                public object current;
            }

            ProcessorData data;

            public void Set(object yield) {
                if (yield == data.current)
                    return;

                var type = yield.GetType();
                var dataType = DataType.None;
                double targetTime = -1;

                if (type == typeof(EditorCoroutine)) {
                    dataType = DataType.EditorCoroutine;
                } else if (type == typeof(AsyncOperation) || type.IsSubclassOf(typeof(AsyncOperation))) {
                    dataType = DataType.AsyncOP;
                }

                data = new ProcessorData { current = yield, targetTime = targetTime, type = dataType };
            }

            public bool MoveNext(IEnumerator enumerator) {
                bool advance = false;
                switch (data.type) {
                    case DataType.WaitForSeconds:
                        advance = data.targetTime <= EditorApplication.timeSinceStartup;
                        break;
                    case DataType.EditorCoroutine:
                        advance = (data.current as EditorCoroutine).m_IsDone;
                        break;
                    case DataType.AsyncOP:
                        advance = (data.current as AsyncOperation).isDone;
                        break;
                    default:
                        advance = data.current == enumerator.Current; //a IEnumerator or a plain object was passed to the implementation
                        break;
                }

                if (advance) {
                    data = default(ProcessorData);
                    return enumerator.MoveNext();
                }
                return true;
            }
        }

        IEnumerator m_Routine;
        YieldProcessor m_Processor;

        bool m_IsDone;

        internal EditorCoroutine(IEnumerator routine) {
            m_Routine = routine;
        }
        internal bool MoveNext() {
            bool done = ProcessIEnumeratorRecursive(m_Routine);
            m_IsDone = !done;
            return m_IsDone;
        }

        static Stack<IEnumerator> kIEnumeratorProcessingStack = new Stack<IEnumerator>(32);
        private bool ProcessIEnumeratorRecursive(IEnumerator enumerator) {
            var root = enumerator;
            while (enumerator.Current as IEnumerator != null) {
                kIEnumeratorProcessingStack.Push(enumerator);
                enumerator = enumerator.Current as IEnumerator;
            }

            //process leaf
            m_Processor.Set(enumerator.Current);
            var result = m_Processor.MoveNext(enumerator);

            while (kIEnumeratorProcessingStack.Count > 1) {
                if (!result) {
                    result = kIEnumeratorProcessingStack.Pop().MoveNext();
                } else
                    kIEnumeratorProcessingStack.Clear();
            }

            if (kIEnumeratorProcessingStack.Count > 0 && !result && root == kIEnumeratorProcessingStack.Pop()) {
                result = root.MoveNext();
            }

            return result;
        }

        internal void Stop() {
            m_Routine = null;
        }
    }
}
#endif