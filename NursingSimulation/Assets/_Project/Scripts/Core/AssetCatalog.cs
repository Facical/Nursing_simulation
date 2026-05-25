using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Core
{
    /// <summary>
    /// Maps role keys (e.g. "PatientBody", "HandSanitizerPump") to either a real prefab or a
    /// primitive fallback. Wiring tools consult this catalog so the same script keeps working
    /// whether or not the user has acquired the actual asset yet.
    /// </summary>
    [CreateAssetMenu(menuName = "NursingSim/AssetCatalog", fileName = "AssetCatalog")]
    public class AssetCatalog : ScriptableObject
    {
        [Serializable]
        public class Slot
        {
            public string role;
            public GameObject prefab;
            public PrimitiveType fallbackPrimitive = PrimitiveType.Cube;
            public Vector3 fallbackScale = Vector3.one;
            public Color fallbackColor = Color.white;

            // Applied after instantiating `prefab` to compensate for model-specific pivot/scale/orientation.
            // Defaults are no-op so existing prefab Transform is preserved.
            public Vector3 instancePosition = Vector3.zero;
            public Vector3 instanceRotationEuler = Vector3.zero;
            public Vector3 instanceScale = Vector3.one;
        }

        public List<Slot> slots = new List<Slot>();

        public Slot Find(string role)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].role == role) return slots[i];
            }
            return null;
        }

        /// <summary>
        /// Spawn a GameObject for the role. Returns prefab instance if the catalog has one,
        /// otherwise builds the configured primitive. Caller is responsible for parenting and
        /// final transform.
        /// </summary>
        public GameObject Spawn(string role, string nameOverride = null)
        {
            var slot = Find(role);
            GameObject go;
            if (slot != null && slot.prefab != null)
            {
#if UNITY_EDITOR
                go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(slot.prefab);
#else
                go = Instantiate(slot.prefab);
#endif
                if (!string.IsNullOrEmpty(nameOverride)) go.name = nameOverride;
                return go;
            }

            var prim = slot != null ? slot.fallbackPrimitive : PrimitiveType.Cube;
            go = GameObject.CreatePrimitive(prim);
            go.name = nameOverride ?? role;
            if (slot != null)
            {
                go.transform.localScale = slot.fallbackScale;
                var rend = go.GetComponent<Renderer>();
                if (rend != null && rend.sharedMaterial != null)
                {
                    rend.sharedMaterial.color = slot.fallbackColor;
                }
            }
            return go;
        }
    }
}
