using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace CSharpLinter
{
    class Program
    {
        static void Main()
        {
            string codePath = Console.In.ReadToEnd().Trim();
            string code = File.ReadAllText(codePath);

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

            MagicNumberDetection.DetectMagicNumbers(tree, issues);

            NamingConventionDetection.DetectNamingConventions(tree, issues);

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
        public string? Severity { get; set; }
        public string? Message { get; set; }
        public int Line { get; set; }
        public int EndLine { get; set; }
        public int Column { get; set; }
        public int EndColumn { get; set; }
        public string Description =>
            $"{Severity}: {Message} (Line: {Line}, Column: {Column} to Line: {EndLine}, Column: {EndColumn})";
    }
}
