using System;
using System.Collections.Generic;
using System.IO;
using CSharpLinter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace CSharpLinter
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = Console.In.ReadToEnd();
            // @"
            // using System;
            // class MyProgram
            // {
            //     public static void Main(string[] args)
            //     {
            //         int vari = 42;
            //         for (var i = 0; i < 13; i++){
            //             Console.WriteLine('a');
            //         }
            //         Console.WriteLine(vari);
            //     }
            // }";

            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            var rootNode = tree.GetRoot();
            var usings = rootNode.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
            string? runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var issues = new List<object>();

            var codeReferences = new List<MetadataReference>(
                [
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(
                        Path.Combine(runtimePath, "System.Console.dll")
                    ),
                    MetadataReference.CreateFromFile(
                        Path.Combine(runtimePath, "System.Runtime.dll")
                    ),
                ]
            );

            foreach (var usingDirective in usings)
            {
                string namespaceName = usingDirective.Name.ToString();
                string assemblyPath = Path.Combine(runtimePath, $"{namespaceName}.dll");

                if (File.Exists(assemblyPath)) // ファイルが存在するか確認
                {
                    codeReferences.Add(MetadataReference.CreateFromFile(assemblyPath));
                }
            }

            var compilation = CSharpCompilation.Create(
                "MyCompilation",
                syntaxTrees: new[] { tree },
                references: codeReferences
            );

            var model = compilation.GetSemanticModel(tree);
            var diagnostics = compilation.GetDiagnostics();

            // コンパイルエラーと警告の処理
            foreach (var diagnostic in diagnostics)
            {
                var lineSpan = diagnostic.Location.GetLineSpan().StartLinePosition;
                issues.Add(
                    new
                    {
                        severity = diagnostic.Severity.ToString(),
                        message = diagnostic.GetMessage(),
                        line = lineSpan.Line + 1,
                        column = lineSpan.Character + 1
                    }
                );
            }

            //マジックナンバー
            issues.AddRange(MagicNumberDetection.DetectMagicNumbers(tree));

            // パブリックメソッドのXMLコメントの確認
            var methods = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                if (
                    method.Modifiers.Any(SyntaxKind.PublicKeyword)
                    && !method
                        .GetLeadingTrivia()
                        .Any(tr =>
                            tr.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                            || tr.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                        )
                )
                {
                    var lineSpan = method.GetLocation().GetLineSpan().StartLinePosition;
                    issues.Add(
                        new
                        {
                            severity = "Warning",
                            message = $"Public method '{method.Identifier.Text}' is missing XML documentation comments.",
                            line = lineSpan.Line + 1,
                            column = lineSpan.Character + 1
                        }
                    );
                }
            }

            // JSON形式で結果を出力
            string json = JsonConvert.SerializeObject(issues, Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    public class Issue
    {
        public string? Description { get; set; }
    }
}
