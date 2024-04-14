import subprocess
import json

def lint_csharp(code):
  try:
    # C# Linter プログラムを実行し、標準入力を通じてコードを渡し、出力を受け取る
    process = subprocess.Popen(["dotnet", "run", "--project", "CSharpLinter"],
                               stdin=subprocess.PIPE, stdout=subprocess.PIPE, 
                               stderr=subprocess.PIPE, text=True)
    stdout, stderr = process.communicate(code)

    if process.returncode == 0:
        print("Received JSON from C#: ")
        json_start_pos = stdout.index('[')
        json_data = stdout[json_start_pos:]
        issues = json.loads(json_data.strip())
        print(issues)
        with open("output.json","w") as f:
            json.dump(issues, f, indent=2)
    else:
        print(stderr)

  except Exception as e:
        print("Error from C# Linter: ")
        print(e)

