using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpLinter
{
    public static class NestingDepthDetector
    {
        private static int _currentDepth = 0;
        private const int MaxDepth = 4;

        public static void DetectNestingDepth(SyntaxTree tree, List<Issue> issues)
        {
            var root = tree.GetRoot();
            VisitNode(root, issues);
        }

        private static void VisitNode(SyntaxNode node, List<Issue> issues)
        {
            foreach (var childNode in node.ChildNodes())
            {
                if (
                    childNode is IfStatementSyntax
                    || childNode is WhileStatementSyntax
                    || childNode is ForStatementSyntax
                    || childNode is ForEachStatementSyntax
                    || childNode is SwitchStatementSyntax
                )
                {
                    _currentDepth++;
                    if (_currentDepth > MaxDepth)
                    {
                        var lineSpan = childNode.GetLocation().GetLineSpan();
                        issues.Add(
                            new Issue
                            {
                                Severity = "Warning",
                                Message = $"ネストしすぎです。",
                                Line = lineSpan.StartLinePosition.Line + 1,
                                EndLine = lineSpan.EndLinePosition.Line + 1,
                                Column = lineSpan.StartLinePosition.Character + 1,
                                EndColumn = lineSpan.EndLinePosition.Character + 1
                            }
                        );
                    }

                    VisitNode(childNode, issues);
                    _currentDepth--;
                }
                else
                {
                    VisitNode(childNode, issues);
                }
            }
        }
    }
}
