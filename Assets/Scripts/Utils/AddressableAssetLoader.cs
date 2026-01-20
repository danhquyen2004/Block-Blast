using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Quản lý load assets sử dụng Addressables
    /// </summary>
    public class AddressableAssetLoader : MonoBehaviour
    {
        private static AddressableAssetLoader instance;
        public static AddressableAssetLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AddressableAssetLoader");
                    instance = go.AddComponent<AddressableAssetLoader>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();

        /// <summary>
        /// Load một sprite bằng Addressable key
        /// </summary>
        public async UniTask<Sprite> LoadSpriteAsync(string key, CancellationToken ct = default)
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(key);
                await handle.WithCancellation(ct);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedAssets[key] = handle;
                    return handle.Result;
                }
                else
                {
                    Debug.LogError($"Failed to load sprite: {key}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading sprite {key}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load nhiều sprites cùng lúc
        /// </summary>
        public async UniTask<Sprite[]> LoadSpritesAsync(string[] keys, CancellationToken ct = default)
        {
            List<UniTask<Sprite>> tasks = new List<UniTask<Sprite>>();
            
            foreach (string key in keys)
            {
                tasks.Add(LoadSpriteAsync(key, ct));
            }

            return await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// Load sprites theo label (e.g., "BlockStones")
        /// </summary>
        public async UniTask<IList<Sprite>> LoadSpritesByLabelAsync(string label, CancellationToken ct = default)
        {
            try
            {
                var handle = Addressables.LoadAssetsAsync<Sprite>(label, null);
                await handle.WithCancellation(ct);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedAssets[label] = handle;
                    return handle.Result;
                }
                else
                {
                    Debug.LogError($"Failed to load sprites with label: {label}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading sprites by label {label}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load một GameObject prefab
        /// </summary>
        public async UniTask<GameObject> LoadPrefabAsync(string key, CancellationToken ct = default)
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle.WithCancellation(ct);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedAssets[key] = handle;
                    return handle.Result;
                }
                else
                {
                    Debug.LogError($"Failed to load prefab: {key}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading prefab {key}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Instantiate prefab từ Addressables
        /// </summary>
        public async UniTask<GameObject> InstantiatePrefabAsync(string key, Transform parent = null, CancellationToken ct = default)
        {
            try
            {
                var handle = Addressables.InstantiateAsync(key, parent);
                await handle.WithCancellation(ct);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }
                else
                {
                    Debug.LogError($"Failed to instantiate prefab: {key}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error instantiating prefab {key}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Release một asset đã load
        /// </summary>
        public void ReleaseAsset(string key)
        {
            if (loadedAssets.TryGetValue(key, out AsyncOperationHandle handle))
            {
                Addressables.Release(handle);
                loadedAssets.Remove(key);
            }
        }

        /// <summary>
        /// Release tất cả assets
        /// </summary>
        public void ReleaseAllAssets()
        {
            foreach (var handle in loadedAssets.Values)
            {
                Addressables.Release(handle);
            }
            loadedAssets.Clear();
        }

        private void OnDestroy()
        {
            ReleaseAllAssets();
        }
    }
}
