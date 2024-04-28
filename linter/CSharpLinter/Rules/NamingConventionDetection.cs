using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpLinter
{
    public static class NamingConventionDetection
    {
        public static void DetectNamingConventions(SyntaxTree tree, List<Issue> issues)
        {
            var root = tree.GetRoot();

            foreach (var variable in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                CheckCamelCase(
                    variable.Identifier.ValueText,
                    "変数",
                    variable.Identifier.GetLocation(),
                    issues
                );
            }

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                CheckPascalCase(
                    method.Identifier.ValueText,
                    "メソッド",
                    method.Identifier.GetLocation(),
                    issues
                );
            }

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                CheckPascalCase(
                    classDecl.Identifier.ValueText,
                    "クラス",
                    classDecl.Identifier.GetLocation(),
                    issues
                );
            }
        }

        private static void CheckCamelCase(
            string name,
            string type,
            Location location,
            List<Issue> issues
        )
        {
            if (!Regex.IsMatch(name, "^[a-z]+[a-zA-Z0-9]*$"))
            {
                var lineSpan = location.GetLineSpan();
                issues.Add(
                    new Issue
                    {
                        Severity = "Warning",
                        Message =
                            $"{name}という{type}名がキャメルケースで記述されていないね！{type}は小文字から始めて、単語の区切りを大文字にするのが一般的だよ！。",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        EndLine = lineSpan.EndLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        EndColumn = lineSpan.EndLinePosition.Character + 1
                    }
                );
            }
        }

        private static void CheckPascalCase(
            string name,
            string type,
            Location location,
            List<Issue> issues
        )
        {
            if (!Regex.IsMatch(name, "^[A-Z][a-zA-Z0-9]*$"))
            {
                var lineSpan = location.GetLineSpan();
                issues.Add(
                    new Issue
                    {
                        Severity = "Warning",
                        Message =
                            $"{name}という{type}名がパスカルケースで記述されてないね！{type}は大文字から始めて、単語の区切りも大文字にするのが一般的だよ！",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        EndLine = lineSpan.EndLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        EndColumn = lineSpan.EndLinePosition.Character + 1
                    }
                );
            }
        }
    }
}
