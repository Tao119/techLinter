// MagicNumberDetection.cs
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpLinter
{
    public static class MagicNumberDetection
    {
        public static List<object> DetectMagicNumbers(SyntaxTree tree)
        {
            var issues = new List<object>();
            var literals = tree.GetRoot().DescendantNodes().OfType<LiteralExpressionSyntax>();

            foreach (
                var literal in literals.Where(l => l.IsKind(SyntaxKind.NumericLiteralExpression))
            )
            {
                if (
                    literal.Parent is ArgumentSyntax
                    || literal.Parent is BinaryExpressionSyntax
                    || literal.Parent is ForStatementSyntax
                )
                {
                    var lineSpan = literal.GetLocation().GetLineSpan().StartLinePosition;
                    issues.Add(
                        new
                        {
                            severity = "Info",
                            message = $"Magic number detected: {literal.Token.ValueText} used in context",
                            line = lineSpan.Line + 1,
                            column = lineSpan.Character + 1
                        }
                    );
                }
            }

            return issues;
        }
    }
}
