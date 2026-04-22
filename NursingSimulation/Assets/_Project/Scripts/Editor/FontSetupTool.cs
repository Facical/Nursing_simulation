using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace NursingSim.EditorTools
{
    /// <summary>
    /// Pretendard OTF → TextMeshPro SDF Font Asset 변환 툴.
    /// 한국어 KS X 1001 + Latin + Jamo + 기호 범위를 동적 아틀라스로 준비.
    /// </summary>
    public static class FontSetupTool
    {
        private const string FontFolder = "Assets/_Project/Art/Fonts";

        [MenuItem("Tools/Nursing Sim/Fonts/Generate Korean SDF Atlas (Regular + Bold)")]
        public static void GenerateKoreanSDFAtlases()
        {
            int ok = 0, fail = 0;
            if (TryGenerate("Pretendard-Regular")) ok++; else fail++;
            if (TryGenerate("Pretendard-Bold")) ok++; else fail++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "SDF Atlas",
                $"한글 SDF Atlas 생성 완료\n- 성공: {ok}개\n- 실패: {fail}개\n\n경로: {FontFolder}/SDF_*.asset",
                "확인");
        }

        private static bool TryGenerate(string baseName)
        {
            string otfPath = $"{FontFolder}/{baseName}.otf";
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(otfPath);
            if (sourceFont == null)
            {
                Debug.LogError($"[Font] OTF not found: {otfPath}");
                return false;
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                samplingPointSize: 90,
                atlasPadding: 9,
                renderMode: GlyphRenderMode.SDFAA,
                atlasWidth: 4096,
                atlasHeight: 4096,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            if (fontAsset == null)
            {
                Debug.LogError($"[Font] TMP_FontAsset.CreateFontAsset returned null for {baseName}");
                return false;
            }

            fontAsset.TryAddCharacters(BuildKoreanPlusLatinCharset(), out string missing);
            if (!string.IsNullOrEmpty(missing))
            {
                Debug.LogWarning($"[Font] {missing.Length} chars missing in {baseName} (normal for Latin-1/Jamo edge cases)");
            }

            string outPath = $"{FontFolder}/SDF_{baseName.Replace("Pretendard-", "")}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath);
            if (existing != null) AssetDatabase.DeleteAsset(outPath);

            AssetDatabase.CreateAsset(fontAsset, outPath);
            Debug.Log($"[Font] SDF asset saved: {outPath}");
            return true;
        }

        private static string BuildKoreanPlusLatinCharset()
        {
            var sb = new StringBuilder(12000);
            AppendRange(sb, 0x0020, 0x007E);   // Basic Latin (ASCII printable)
            AppendRange(sb, 0x00A0, 0x00FF);   // Latin-1 Supplement
            AppendRange(sb, 0x2000, 0x206F);   // General Punctuation
            AppendRange(sb, 0x3000, 0x303F);   // CJK Symbols and Punctuation
            AppendRange(sb, 0x3131, 0x318E);   // Hangul Compatibility Jamo
            AppendRange(sb, 0xAC00, 0xD7A3);   // Hangul Syllables (11,172자; KS X 1001 2,350 포함)
            AppendRange(sb, 0xFF00, 0xFFEF);   // Halfwidth and Fullwidth Forms
            return sb.ToString();
        }

        private static void AppendRange(StringBuilder sb, int start, int end)
        {
            for (int i = start; i <= end; i++) sb.Append((char)i);
        }

        [MenuItem("Tools/Nursing Sim/Fonts/Verify Korean Rendering (quick test)")]
        public static void VerifyKoreanRendering()
        {
            string path = $"{FontFolder}/SDF_Regular.asset";
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset == null)
            {
                EditorUtility.DisplayDialog("SDF Atlas", $"SDF_Regular.asset 없음. 먼저 Generate 실행.\n{path}", "확인");
                return;
            }

            string testText = "근육주사 시뮬레이션 가나다라 ABC 123 — 한글 SDF OK?";
            var missing = new List<char>();
            foreach (var c in testText)
            {
                if (c == ' ') continue;
                if (!asset.HasCharacter(c)) missing.Add(c);
            }

            string msg = missing.Count == 0
                ? $"테스트 문자열 전부 렌더 가능.\n\"{testText}\""
                : $"누락 {missing.Count}자: {string.Join(",", missing)}";
            EditorUtility.DisplayDialog("Korean Rendering Verify", msg, "확인");
        }
    }
}
