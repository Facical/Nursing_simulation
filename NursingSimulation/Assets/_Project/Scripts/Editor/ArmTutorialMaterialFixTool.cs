using UnityEditor;
using UnityEngine;

namespace NursingSim.EditorTools
{
    public static class ArmTutorialMaterialFixTool
    {
        private const string ArmTutorialRoot = "Assets/ThirdParty/ArmTutorial";
        private const string UrpLitShaderName = "Universal Render Pipeline/Lit";
        private const string Body1001DiffusePath = "Assets/ThirdParty/ArmTutorial/Scenes/TEXTURE/Ch16_1001_Diffuse.png";
        private const string Body1001NormalPath = "Assets/ThirdParty/ArmTutorial/Scenes/TEXTURE/Ch16_1001_Normal.png";
        private const string Body1002DiffusePath = "Assets/ThirdParty/ArmTutorial/Scenes/TEXTURE/Ch16_1002_Diffuse.png";
        private const string Body1002NormalPath = "Assets/ThirdParty/ArmTutorial/Scenes/TEXTURE/Ch16_1002_Normal.png";

        [MenuItem("Tools/Nursing Sim/Phase 3/Fix ArmTutorial Materials for URP")]
        public static void FixArmTutorialMaterials()
        {
            var fixedTextures = FixNormalTextureImports();
            var fixedMaterials = FixMaterials();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ArmTutorialMaterialFix] Fixed {fixedMaterials} materials and {fixedTextures} normal textures.");
        }

        private static int FixNormalTextureImports()
        {
            var fixedCount = 0;
            var textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArmTutorialRoot });
            foreach (var guid in textureGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("Normal")) continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                var changed = false;
                if (importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    changed = true;
                }

                if (importer.sRGBTexture)
                {
                    importer.sRGBTexture = false;
                    changed = true;
                }

                if (!changed) continue;

                importer.SaveAndReimport();
                fixedCount++;
            }

            return fixedCount;
        }

        private static int FixMaterials()
        {
            var urpLit = Shader.Find(UrpLitShaderName);
            if (urpLit == null)
            {
                Debug.LogError($"[ArmTutorialMaterialFix] Shader not found: {UrpLitShaderName}");
                return 0;
            }

            var fixedCount = 0;
            var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { ArmTutorialRoot });
            foreach (var guid in materialGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                if (TryUpgradeMaterial(mat, path, urpLit))
                {
                    EditorUtility.SetDirty(mat);
                    fixedCount++;
                }
            }

            return fixedCount;
        }

        private static bool TryUpgradeMaterial(Material mat, string path, Shader urpLit)
        {
            var mainTex = mat.HasProperty("_BaseMap")
                ? mat.GetTexture("_BaseMap")
                : mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            var color = mat.HasProperty("_BaseColor")
                ? mat.GetColor("_BaseColor")
                : mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
            var bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
            var bumpScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : 1f;
            var smoothness = mat.HasProperty("_Smoothness")
                ? mat.GetFloat("_Smoothness")
                : mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.35f;
            var metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
            if (mainTex == null) mainTex = ResolveFallbackTexture(mat.name, path, normal: false);
            if (bumpMap == null) bumpMap = ResolveFallbackTexture(mat.name, path, normal: true);

            mat.shader = urpLit;

            if (mat.HasProperty("_BaseMap") && mainTex != null) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_BumpMap") && bumpMap != null) mat.SetTexture("_BumpMap", bumpMap);
            if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", bumpScale);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
            if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 0f);

            SetKeyword(mat, "_NORMALMAP", bumpMap != null);
            SetKeyword(mat, "_ALPHATEST_ON", false);
            SetKeyword(mat, "_SURFACE_TYPE_TRANSPARENT", false);
            mat.renderQueue = -1;

            return true;
        }

        private static Texture ResolveFallbackTexture(string materialName, string path, bool normal)
        {
            var normalizedName = materialName.ToLowerInvariant();
            var normalizedPath = path.ToLowerInvariant();
            if (normalizedName.Contains("body1") || normalizedPath.Contains("ch16_body1"))
            {
                return AssetDatabase.LoadAssetAtPath<Texture>(normal ? Body1002NormalPath : Body1002DiffusePath);
            }

            if (normalizedName.Contains("ch16_body") || normalizedName.Contains("eyelashes"))
            {
                return AssetDatabase.LoadAssetAtPath<Texture>(normal ? Body1001NormalPath : Body1001DiffusePath);
            }

            return null;
        }

        private static void SetKeyword(Material mat, string keyword, bool enabled)
        {
            if (enabled) mat.EnableKeyword(keyword);
            else mat.DisableKeyword(keyword);
        }
    }
}
