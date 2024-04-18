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

            // Variables
            foreach (var variable in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                CheckNameConventions(
                    variable.Identifier.ValueText,
                    "Variable",
                    variable.Identifier.GetLocation(),
                    issues
                );
            }

            // Methods
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                CheckNameConventions(
                    method.Identifier.ValueText,
                    "Method",
                    method.Identifier.GetLocation(),
                    issues
                );
            }

            // Classes
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                CheckNameConventions(
                    classDecl.Identifier.ValueText,
                    "Class",
                    classDecl.Identifier.GetLocation(),
                    issues
                );
            }
        }

        private static void CheckNameConventions(
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
                        Message = $"{name} という{type}名はパスカルケースであるべきです。",
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
