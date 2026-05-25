using NursingSim.Core;
using UnityEditor;
using UnityEngine;

namespace NursingSim.EditorTools
{
    /// <summary>
    /// Shared lookup used by every wiring tool. Tries the AssetCatalog first; if the role is
    /// missing or no prefab assigned, falls back to the legacy primitive so wiring keeps working
    /// before assets are acquired.
    /// </summary>
    internal static class AssetCatalogHelper
    {
        private const string CatalogPath = "Assets/_Project/Data/AssetCatalog.asset";

        public static AssetCatalog GetOrCreate()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(CatalogPath);
            if (catalog != null) return catalog;

            var dir = System.IO.Path.GetDirectoryName(CatalogPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
            catalog = ScriptableObject.CreateInstance<AssetCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        /// <summary>
        /// Spawn a GameObject for the role. Catalog hit → prefab instance with default scale.
        /// Catalog miss → legacy primitive built with caller-supplied fallback. The return is
        /// always a fresh GameObject the caller can reparent and reposition.
        /// </summary>
        public static GameObject SpawnRole(string role, string nameOverride, PrimitiveType fallbackPrimitive, Vector3? fallbackScale = null)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(CatalogPath);
            var slot = catalog?.Find(role);
            if (slot != null && slot.prefab != null)
            {
                var prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(slot.prefab);
                if (!string.IsNullOrEmpty(nameOverride)) prefabInstance.name = nameOverride;
                return prefabInstance;
            }

            var go = GameObject.CreatePrimitive(fallbackPrimitive);
            if (!string.IsNullOrEmpty(nameOverride)) go.name = nameOverride;
            if (fallbackScale.HasValue) go.transform.localScale = fallbackScale.Value;
            return go;
        }
    }
}
