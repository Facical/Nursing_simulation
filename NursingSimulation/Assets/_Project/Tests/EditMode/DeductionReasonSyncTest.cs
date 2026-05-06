using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NursingSim.Data;
using UnityEngine;

namespace NursingSim.Tests.EditMode
{
    // 본 테스트는 docs/05-data-model.md의 DeductionReason enum 코드 블록과
    // 실제 NursingSim.Data.DeductionReason 값 집합이 동일한지 검증한다.
    // 두 곳 중 한 쪽만 수정하면 이 테스트가 실패해 갱신 누락을 잡는다.
    public class DeductionReasonSyncTest
    {
        [Test]
        public void DocsAndEnum_HaveIdenticalSets()
        {
            var docPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "docs", "05-data-model.md"));
            Assert.IsTrue(File.Exists(docPath), $"docs/05-data-model.md not found at expected path: {docPath}");

            var docNames = ExtractEnumValuesFromDoc(docPath);
            Assert.Greater(docNames.Count, 0, "Failed to parse any enum values from docs/05-data-model.md");

            var codeNames = Enum.GetNames(typeof(DeductionReason)).ToHashSet();

            var missingInCode = docNames.Except(codeNames).ToList();
            var missingInDoc = codeNames.Except(docNames).ToList();

            if (missingInCode.Count > 0 || missingInDoc.Count > 0) {
                var msg = "DeductionReason drift detected.\n";
                if (missingInCode.Count > 0) msg += $"  In docs but not in DeductionReason.cs: [{string.Join(", ", missingInCode)}]\n";
                if (missingInDoc.Count > 0) msg += $"  In DeductionReason.cs but not in docs: [{string.Join(", ", missingInDoc)}]\n";
                Assert.Fail(msg);
            }
        }

        private static HashSet<string> ExtractEnumValuesFromDoc(string path)
        {
            var text = File.ReadAllText(path);
            // docs/05의 DeductionReason enum 코드 블록 추출
            var blockMatch = Regex.Match(text, @"public enum DeductionReason\s*\{(?<body>[^}]*)\}", RegexOptions.Singleline);
            if (!blockMatch.Success) return new HashSet<string>();

            var body = blockMatch.Groups["body"].Value;
            var lines = body.Split('\n');
            var names = new HashSet<string>();
            foreach (var raw in lines) {
                var line = raw;
                var commentIdx = line.IndexOf("//", StringComparison.Ordinal);
                if (commentIdx >= 0) line = line.Substring(0, commentIdx);
                line = line.Trim().TrimEnd(',').Trim();
                if (line.Length == 0) continue;
                if (Regex.IsMatch(line, @"^[A-Za-z_][A-Za-z0-9_]*$")) names.Add(line);
            }
            return names;
        }
    }
}
