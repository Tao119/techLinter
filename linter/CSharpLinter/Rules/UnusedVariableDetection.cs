using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpLinter
{
    public static class UnusedVariableDetector
    {
        public static void DetectUnusedVariables(
            SyntaxTree tree,
            List<Issue> issues,
            Compilation compilation
        )
        {
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();
            var allIdentifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();

            var variableUsage = new Dictionary<ISymbol, bool>(SymbolEqualityComparer.Default);

            foreach (var declaration in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(declaration);
                if (symbol != null)
                {
                    variableUsage[symbol] = false;
                }
            }

            foreach (var identifier in allIdentifiers)
            {
                var symbol = model.GetSymbolInfo(identifier).Symbol;
                if (symbol != null && variableUsage.ContainsKey(symbol))
                {
                    variableUsage[symbol] = true;
                }
            }

            foreach (var kvp in variableUsage)
            {
                if (!kvp.Value)
                {
                    var location = kvp.Key.Locations[0];
                    var lineSpan = location.GetLineSpan();
                    issues.Add(
                        new Issue
                        {
                            Severity = "Warning",
                            Message =
                                $"Variable '{kvp.Key.Name}' is declared and initialized but never used as an argument.",
                            Line = lineSpan.StartLinePosition.Line + 1,
                            EndLine = lineSpan.EndLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            EndColumn = lineSpan.EndLinePosition.Character + 1,
                        }
                    );
                }
            }
        }
    }
}
