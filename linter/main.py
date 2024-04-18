import os
from lint_csharp import lint_csharp

import sys


def main():
    if len(sys.argv) < 2:
        print("Error: No file path provided.")
        sys.exit(1)

    file_path = sys.argv[1]

    if not os.path.exists(file_path):
        print(f"Error: File does not exist - {file_path}")
        sys.exit(1)

    file_extension = os.path.splitext(file_path)[1]

    if file_extension.lower() != '.cs':
        print(f"No linter available for files with extension {file_extension}")
        sys.exit(0)

    # with open(file_path, 'r', encoding='utf-8') as file:
        # code = file.read()

    issues = lint_csharp(file_path)
    if issues:
        return issues
    else:
        print("No issues found.")
        return []


if __name__ == "__main__":
    main()
