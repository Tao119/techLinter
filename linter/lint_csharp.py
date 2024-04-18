import subprocess
import json
import sys
from pathlib import Path


def lint_csharp(code_path):
    try:
        # C# リンタープロジェクトのパスを正しく設定
        base_path = Path(__file__).parent.resolve()
        project_path = base_path / "../linter/CSharpLinter"

        # subprocess.Popen を使用して外部プロセスを起動
        process = subprocess.Popen(
            ["dotnet", "run", "--project", project_path],
            stdin=subprocess.PIPE, stdout=subprocess.PIPE,
            stderr=subprocess.PIPE, text=True, universal_newlines=True)

        # 標準入力を通じてコードを送信し、プロセスを閉じる
        stdout, stderr = process.communicate(input=code_path)
        print(stdout)

        # エラーがあれば表示
        if process.returncode != 0:
            print(f"Error: {stderr}", file=sys.stderr)
            return None

        # JSON出力を解析して返す
        return json.loads(stdout)

    except Exception as e:
        print(f"An error occurred: {str(e)}", file=sys.stderr)
        return None
