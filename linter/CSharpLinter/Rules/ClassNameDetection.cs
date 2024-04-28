using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpLinter
{
    public static class NamingConventionAnalyzer
    {
        public static void AnalyzeClassNames(SyntaxTree tree, string filePath, List<Issue> issues)
        {
            var rootNode = tree.GetRoot();
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            foreach (
                var classDeclaration in rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>()
            )
            {
                var className = classDeclaration.Identifier.Text;
                var baseTypeList = classDeclaration.BaseList;

                if (
                    baseTypeList != null
                    && baseTypeList.Types.Any(t => t.Type.ToString() == "MonoBehaviour")
                )
                {
                    if (className != fileName)
                    {
                        var lineSpan = classDeclaration
                            .Identifier.GetLocation()
                            .GetLineSpan()
                            .StartLinePosition;
                        issues.Add(
                            new Issue
                            {
                                Severity = "Warning",
                                Message =
                                    $"「{className}」というクラス名は、ファイル名「{fileName}.cs」に合わせる必要があるよ。Unityでは、このルールに従わないと、作成したクラスがGameObjectに正しくアタッチされないことがあるんだ。クラス名とファイル名が一致していると、Unityがスクリプトを見つけやすくなり、エラーなくゲームオブジェクトにスクリプトを適用できるから、しっかりと一致させるようにしようね！",
                                Line = lineSpan.Line + 1,
                                Column = lineSpan.Character + 1,
                                EndLine = lineSpan.Line + 1,
                                EndColumn = lineSpan.Character + className.Length + 1
                            }
                        );
                    }
                }
            }
        }
    }
}
