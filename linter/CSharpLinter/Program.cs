using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace CSharpLinter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: dotnet run <codePath> <userId>");
                return;
            }

            string codePath = args[0];
            if (!File.Exists(codePath))
            {
                Console.WriteLine($"Error: File not found - {codePath}");
                return;
            }

            string code = await File.ReadAllTextAsync(codePath);
            if (!int.TryParse(args[1], out int ur_id))
            {
                Console.WriteLine("Error: Invalid user ID");
                return;
            }

            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            var issues = new List<Issue>();

            string projectFilePath = FindCsprojPath(Path.GetDirectoryName(codePath)!);
            if (projectFilePath == null)
            {
                Console.WriteLine("No .csproj file found.");
                return;
            }

            var codeReferences = LoadReferencesFromProject(projectFilePath);
            var compilation = CSharpCompilation.Create(
                "MyCompilation",
                syntaxTrees: new[] { tree },
                references: codeReferences
            );

            AnalizeCodes.AnalyzeDiagnostics(compilation, issues);

            if (issues.ToArray().Length > 0)
            {
                Console.WriteLine("err");
                return;
            }
            NamingConventionAnalyzer.AnalyzeClassNames(tree, codePath, issues);

            MagicNumberDetection.DetectMagicNumbers(tree, issues);

            NamingConventionDetection.DetectNamingConventions(tree, issues);

            // NestingDepthDetector.DetectNestingDepth(tree, issues);

            // UnusedVariableDetector.DetectUnusedVariables(tree, issues, compilation);

            await ChatGptFeedbackFetcher.FetchFeedbackAndAddToIssues(tree, issues, ur_id);

            string json = JsonConvert.SerializeObject(issues, Formatting.Indented);
            Console.WriteLine(json);
        }

        static string FindCsprojPath(string directoryPath)
        {
            while (directoryPath != null)
            {
                var projectFiles = Directory.GetFiles(directoryPath, "*.csproj");
                if (projectFiles.Length > 0)
                    return projectFiles[0];

                directoryPath = Directory.GetParent(directoryPath)?.FullName!;
            }

            return null!;
        }

        static List<MetadataReference> LoadReferencesFromProject(string projectFilePath)
        {
            var references = new List<MetadataReference>();
            var projectRoot = ProjectRootElement.Open(projectFilePath);

            foreach (var item in projectRoot.Items)
            {
                if (item.ItemType == "Reference" || item.ItemType == "PackageReference")
                {
                    var hintPath = item.Metadata.FirstOrDefault(m => m.Name == "HintPath")?.Value;
                    if (hintPath != null)
                    {
                        string fullPath = Path.GetFullPath(
                            hintPath,
                            Path.GetDirectoryName(projectFilePath)!
                        );
                        if (File.Exists(fullPath))
                        {
                            references.Add(MetadataReference.CreateFromFile(fullPath));
                        }
                    }
                }
            }
            string unityAssembliesPath = Path.Combine(
                Path.GetDirectoryName(projectFilePath)!,
                "Library",
                "ScriptAssemblies"
            );
            if (Directory.Exists(unityAssembliesPath))
            {
                foreach (var dllPath in Directory.GetFiles(unityAssembliesPath, "*.dll"))
                {
                    references.Add(MetadataReference.CreateFromFile(dllPath));
                }
            }

            return references;
        }
    }

    public class Issue
    {
        [JsonProperty("severity")]
        public string? Severity { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("end_line")]
        public int EndLine { get; set; }

        [JsonProperty("column")]
        public int Column { get; set; }

        [JsonProperty("end_column")]
        public int EndColumn { get; set; }
        public string Description =>
            $"{Severity}: {Message} (Line: {Line}, Column: {Column} to Line: {EndLine}, Column: {EndColumn})";
    }
}
