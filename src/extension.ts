import * as vscode from 'vscode';
import * as childProcess from 'child_process';
import * as path from 'path';

const severity: { [_: string]: vscode.DiagnosticSeverity } = {
	"Info": vscode.DiagnosticSeverity.Information,
	"Error": vscode.DiagnosticSeverity.Error,
	"Warning": vscode.DiagnosticSeverity.Warning,
	"Hint": vscode.DiagnosticSeverity.Hint
};

type Issue = {
	Severity?: string,
	Message?: string,
	Line: number,
	EndLine: number,
	Column: number,
	EndColumn: number
}

export function activate(context: vscode.ExtensionContext) {
	const diagnosticsCollection = vscode.languages.createDiagnosticCollection('linter');
	context.subscriptions.push(diagnosticsCollection);

	let disposable = vscode.commands.registerCommand('extension.runLinter', async () => {
		const editor = vscode.window.activeTextEditor;
		if (!editor) {
			vscode.window.showErrorMessage("No file is open.");
			return;
		}

		const filePath = editor.document.fileName;
		if (!filePath) {
			vscode.window.showErrorMessage("No file is open.");
			return;
		}

		const pythonCommand = getPythonCommand();
		const pythonPath = path.join(__dirname, '../linter/main.py');
		const command = `${pythonCommand} "${pythonPath}" "${filePath}"`;

		childProcess.exec(command, (error, stdout, stderr) => {
			if (error) {
				vscode.window.showErrorMessage(`Error: ${stderr}`);
				return;
			}

			try {
				const lintResults: Issue[] = JSON.parse(stdout);

				const diagnostics: vscode.Diagnostic[] = lintResults.map((result) => {
					const range = new vscode.Range(
						new vscode.Position(result.Line - 1, result.Column - 1),
						new vscode.Position(result.EndLine - 1, result.EndColumn - 1)
					);
					console.log(result);

					return new vscode.Diagnostic(
						range,
						`${result.Message} ${result.Message ? "(from tech linter)" : ""}`,
						severity[result.Severity ?? vscode.DiagnosticSeverity.Information]
					);
				});

				diagnosticsCollection.set(editor.document.uri, diagnostics);
				vscode.window.showInformationMessage("Linting completed.");
			} catch (e) {
				vscode.window.showErrorMessage(`Failed to parse lint results: ${e}`);
			}
		});

	});

	context.subscriptions.push(disposable);
}

function getPythonCommand(): string {
	return 'python3';
}

export function deactivate() {
}
