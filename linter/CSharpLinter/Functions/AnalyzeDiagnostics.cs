// MagicNumberDetection.cs
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpLinter
{
    public static class AnalizeCodes
    {
        public static void AnalyzeDiagnostics(CSharpCompilation compilation, List<Issue> issues)
        {
            foreach (var diag in compilation.GetDiagnostics())
            {
                if (!diag.Descriptor.Id.Equals("CS5001"))
                {
                    var lineSpan = diag.Location.GetLineSpan().StartLinePosition;
                    var endLineSpan = diag.Location.GetLineSpan().EndLinePosition;
                    issues.Add(
                        new Issue
                        {
                            Severity = diag.Severity.ToString(),
                            Message = diag.GetMessage(),
                            Line = lineSpan.Line + 1,
                            EndLine = endLineSpan.Line + 1,
                            Column = lineSpan.Character + 1,
                            EndColumn = endLineSpan.Character + 1
                        }
                    );
                }
            }
        }
    }
}
