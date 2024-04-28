using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpLinter
{
    public static class MagicNumberDetection
    {
        public static void DetectMagicNumbers(SyntaxTree tree, List<Issue> issues)
        {
            var literals = tree.GetRoot().DescendantNodes().OfType<LiteralExpressionSyntax>();

            foreach (
                var literal in literals.Where(l => l.IsKind(SyntaxKind.NumericLiteralExpression))
            )
            {
                if (IsPartOfVectorInitialization(literal))
                    continue;

                var value = literal.Token.ValueText;

                if (value == "0")
                    continue;

                if (
                    literal.Parent is ArgumentSyntax
                    || literal.Parent is BinaryExpressionSyntax
                    || literal.Parent is ForStatementSyntax
                )
                {
                    var lineSpan = literal.GetLocation().GetLineSpan().StartLinePosition;
                    var endLineSpan = literal.GetLocation().GetLineSpan().EndLinePosition;
                    issues.Add(
                        new Issue
                        {
                            Severity = "Info",
                            Message =
                                $"{value}という値が直接使用されているね！これだけだと意味が分かりにくいからこの値に具体的な名前を付ける定数を定義して、その定数を使用するようにするとプログラムの可読性上がるよ！",
                            Line = lineSpan.Line + 1,
                            EndLine = endLineSpan.Line + 1,
                            Column = lineSpan.Character + 1,
                            EndColumn = endLineSpan.Character + 1
                        }
                    );
                }
            }
        }

        private static bool IsPartOfVectorInitialization(LiteralExpressionSyntax literal)
        {
            return literal.Parent is ArgumentSyntax argument
                && argument.Parent is ArgumentListSyntax argumentList
                && argumentList.Parent is ObjectCreationExpressionSyntax;
        }
    }
}
