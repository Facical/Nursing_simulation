using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace NursingSim.EditorTools
{
    public static class Phase0FontAtlasTool
    {
        private const string FontDir = "Assets/_Project/Art/Fonts";
        private const int SamplingPointSize = 90;
        private const int AtlasPadding = 9;
        private const int AtlasSize = 1024;

        [MenuItem("Tools/Nursing Sim/Phase 0/Rebuild SDF Font Atlases (Pretendard)")]
        public static void RebuildAll()
        {
            RebuildOne($"{FontDir}/Pretendard-Regular.otf", $"{FontDir}/SDF_Regular.asset");
            RebuildOne($"{FontDir}/Pretendard-Bold.otf",    $"{FontDir}/SDF_Bold.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase0] SDF atlases rebuilt for Pretendard (Regular + Bold).");
            EditorUtility.DisplayDialog(
                "SDF Atlas Rebuild",
                "Pretendard SDF Regular/Bold atlas를 재생성했습니다.\nDynamic 모드라 처음 사용하는 글리프는 런타임에 자동 추가됩니다.",
                "확인");
        }

        private static void RebuildOne(string otfPath, string sdfAssetPath)
        {
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(otfPath);
            if (sourceFont == null)
            {
                Debug.LogError($"[Phase0] Source OTF not found at {otfPath}");
                return;
            }

            var assetName = Path.GetFileNameWithoutExtension(sdfAssetPath);

            var fresh = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                SamplingPointSize,
                AtlasPadding,
                GlyphRenderMode.SDFAA,
                AtlasSize,
                AtlasSize,
                AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            if (fresh == null)
            {
                Debug.LogError($"[Phase0] CreateFontAsset failed for {otfPath}");
                return;
            }
            fresh.name = assetName;
            foreach (var tex in fresh.atlasTextures)
            {
                if (tex != null) tex.name = assetName + "_Atlas";
            }
            if (fresh.material != null) fresh.material.name = assetName + "_Material";

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfAssetPath);
            if (existing != null)
            {
                foreach (var sub in AssetDatabase.LoadAllAssetRepresentationsAtPath(sdfAssetPath))
                {
                    if (sub is Texture2D || sub is Material)
                    {
                        AssetDatabase.RemoveObjectFromAsset(sub);
                    }
                }

                EditorUtility.CopySerialized(fresh, existing);

                foreach (var tex in fresh.atlasTextures)
                {
                    if (tex != null) AssetDatabase.AddObjectToAsset(tex, existing);
                }
                if (fresh.material != null) AssetDatabase.AddObjectToAsset(fresh.material, existing);

                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(sdfAssetPath);
                Debug.Log($"[Phase0] Updated atlas asset in place (GUID preserved): {sdfAssetPath}");
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sdfAssetPath));
                AssetDatabase.CreateAsset(fresh, sdfAssetPath);
                foreach (var tex in fresh.atlasTextures)
                {
                    if (tex != null) AssetDatabase.AddObjectToAsset(tex, fresh);
                }
                if (fresh.material != null) AssetDatabase.AddObjectToAsset(fresh.material, fresh);
                EditorUtility.SetDirty(fresh);
                Debug.Log($"[Phase0] Created atlas asset: {sdfAssetPath}");
            }
        }
    }
}
