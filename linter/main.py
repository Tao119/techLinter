import os
from lint_csharp import lint_csharp

import sys


def main():
    if len(sys.argv) < 3:
        print("Error: No file path provided.")
        sys.exit(1)

    file_path = sys.argv[1]
    ur_id = sys.argv[2]

    if not os.path.exists(file_path):
        print(f"Error: File does not exist - {file_path}")
        sys.exit(1)

    file_extension = os.path.splitext(file_path)[1]

    if file_extension.lower() != '.cs':
        print(f"No linter available for files with extension {file_extension}")
        sys.exit(0)

    issues = lint_csharp(file_path, ur_id)
    if issues:
        return issues
    else:
        return []


if __name__ == "__main__":
    main()
