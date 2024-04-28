import subprocess
import json
import sys
from pathlib import Path


def lint_csharp(code_path, ur_id):
    try:
        base_path = Path(__file__).parent.resolve()
        project_path = base_path / "../linter/CSharpLinter"

        process = subprocess.Popen(
            ["dotnet", "run", "--project",
                project_path, code_path, str(ur_id)],
            stdin=subprocess.PIPE, stdout=subprocess.PIPE,
            stderr=subprocess.PIPE, text=True, universal_newlines=True)

        stdout, stderr = process.communicate()
        print(stdout)

        if process.returncode != 0:
            print(f"Error: {stderr}", file=sys.stderr)
            return None

        if (stdout.strip() == "err"):
            return "err"

        return json.loads(stdout)

    except Exception as e:
        print(f"An error occurred: {str(e)}", file=sys.stderr)
        return None
