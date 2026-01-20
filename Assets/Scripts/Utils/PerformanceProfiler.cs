using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Performance profiler helper
    /// </summary>
    public class PerformanceProfiler : MonoBehaviour
    {
        private static PerformanceProfiler instance;
        public static PerformanceProfiler Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("PerformanceProfiler");
                    instance = go.AddComponent<PerformanceProfiler>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Display")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private bool showMemory = true;
        [SerializeField] private bool showFrameTime = true;

        private float deltaTime = 0f;
        private float fps = 0f;
        private GUIStyle style;
        private Rect fpsRect;
        private Rect memoryRect;
        private Rect frameTimeRect;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGUI();
        }

        private void InitializeGUI()
        {
            int w = Screen.width, h = Screen.height;

            fpsRect = new Rect(10, 10, 200, 50);
            memoryRect = new Rect(10, 70, 200, 50);
            frameTimeRect = new Rect(10, 130, 200, 50);

            style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 24;
            style.normal.textColor = Color.white;
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;
        }

        private void OnGUI()
        {
            if (style == null) InitializeGUI();

            if (showFPS)
            {
                float msec = deltaTime * 1000.0f;
                fps = 1.0f / deltaTime;
                string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, msec);

                // Color code based on FPS
                if (fps >= 50)
                    style.normal.textColor = Color.green;
                else if (fps >= 30)
                    style.normal.textColor = Color.yellow;
                else
                    style.normal.textColor = Color.red;

                GUI.Label(fpsRect, text, style);
            }

            if (showMemory)
            {
                long memory = System.GC.GetTotalMemory(false);
                string memoryText = $"Memory: {memory / 1024 / 1024} MB";
                style.normal.textColor = Color.white;
                GUI.Label(memoryRect, memoryText, style);
            }

            if (showFrameTime)
            {
                float frameTime = deltaTime * 1000f;
                string frameText = $"Frame: {frameTime:F2} ms";
                style.normal.textColor = frameTime < 16.67f ? Color.green : Color.red;
                GUI.Label(frameTimeRect, frameText, style);
            }
        }

        /// <summary>
        /// Profile một async operation
        /// </summary>
        public static async UniTask<T> ProfileAsync<T>(string operationName, UniTask<T> operation)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            try
            {
                T result = await operation;
                sw.Stop();
                Debug.Log($"[Performance] {operationName}: {sw.ElapsedMilliseconds}ms");
                return result;
            }
            catch (System.Exception e)
            {
                sw.Stop();
                Debug.LogError($"[Performance] {operationName} failed after {sw.ElapsedMilliseconds}ms: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Profile một sync operation
        /// </summary>
        public static T Profile<T>(string operationName, System.Func<T> operation)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            try
            {
                T result = operation();
                sw.Stop();
                Debug.Log($"[Performance] {operationName}: {sw.ElapsedMilliseconds}ms");
                return result;
            }
            catch (System.Exception e)
            {
                sw.Stop();
                Debug.LogError($"[Performance] {operationName} failed after {sw.ElapsedMilliseconds}ms: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Memory snapshot
        /// </summary>
        public static void LogMemorySnapshot(string label = "")
        {
            long memory = System.GC.GetTotalMemory(false);
            Debug.Log($"[Memory] {label}: {memory / 1024 / 1024} MB");
        }

        /// <summary>
        /// Force GC và log
        /// </summary>
        public static void ForceGC()
        {
            long before = System.GC.GetTotalMemory(false);
            System.GC.Collect();
            long after = System.GC.GetTotalMemory(true);
            Debug.Log($"[GC] Freed {(before - after) / 1024 / 1024} MB");
        }
    }

    /// <summary>
    /// Extension methods cho profiling
    /// </summary>
    public static class ProfilingExtensions
    {
        public static async UniTask<T> WithProfiling<T>(this UniTask<T> task, string operationName)
        {
            return await PerformanceProfiler.ProfileAsync(operationName, task);
        }
    }
}
