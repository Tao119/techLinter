import * as vscode from 'vscode';
import { User, authenticateUser, getSessionToken, saveSessionToken, validateSession } from './authentication';
import * as childProcess from 'child_process';
import * as path from 'path';
import { convertToCamelCase } from './service/convert';

const severity: { [_: string]: vscode.DiagnosticSeverity } = {
	"Info": vscode.DiagnosticSeverity.Information,
	"Error": vscode.DiagnosticSeverity.Error,
	"Warning": vscode.DiagnosticSeverity.Warning,
	"Hint": vscode.DiagnosticSeverity.Hint
};

type Issue = {
	severity: string,
	message: string,
	suggest?: string,
	line: number,
	endLine: number,
	column: number,
	endColumn: number
};

export function activate(context: vscode.ExtensionContext) {
	const diagnosticsCollection = vscode.languages.createDiagnosticCollection('linter');
	context.subscriptions.push(diagnosticsCollection);

	let disposable = vscode.commands.registerCommand('extension.runLinter', async () => {
		const editor = vscode.window.activeTextEditor;
		if (!editor) {
			vscode.window.showErrorMessage("ファイルが開かれていません。");
			return;
		}

		const filePath = editor.document.fileName;
		if (!filePath) {
			vscode.window.showErrorMessage("ファイルが開かれていません。");
			return;
		}

		const sessionToken = await getSessionToken(context);
		let sessionData;
		try {
			sessionData = JSON.parse(sessionToken || '{}');
		} catch {
			sessionData = {};
		}
		let user = await validateSession(sessionData.ur_name, sessionData.password);

		if (!user) {
			const newUser = await promptForLogin(context);
			if (!newUser) {
				vscode.window.showErrorMessage("実行にはログインが必要です。");
				return;
			}
			user = newUser;
		}
		vscode.window.showInformationMessage(`Hello, ${user.name}.`);



		if (!user.isAdmin && user.token < 1) {
			vscode.window.showErrorMessage("使用可能なトークンが残っていません。GPT部分を除いて実行します。");
		}

		const pythonPath = path.join(__dirname, '../linter/main.py');
		const pythonCommand = 'python3';
		const command = `${pythonCommand} "${pythonPath}" "${filePath}" "${user.id}"`;

		childProcess.exec(command, (error, stdout, stderr) => {
			if (error) {
				vscode.window.showErrorMessage(`Error: ${stderr}`);
				return;
			}
			console.log(stdout);

			try {
				if (stdout.trim() === "err") {
					vscode.window.showWarningMessage("コンパイルエラーがあるよ！まずは直してみよう。");
					return;
				}
				const lintResults: Issue[] = convertToCamelCase(JSON.parse(stdout));
				console.log(lintResults);
				if (lintResults.length > 0) {
					const diagnostics: vscode.Diagnostic[] = lintResults.map(issue => {
						const range = new vscode.Range(
							new vscode.Position(issue.line - 1, issue.column - 1),
							new vscode.Position(issue.endLine - 1, issue.endColumn - 1));
						return new vscode.Diagnostic(range, `${issue.message}`, severity[issue.severity]);
					});
					diagnosticsCollection.set(editor.document.uri, diagnostics);
					vscode.window.showInformationMessage("アドバイスを表示したよ！");
				} else {
					diagnosticsCollection.set(editor.document.uri, []);
					vscode.window.showInformationMessage("完璧！！");
				}
			} catch (e) {
				vscode.window.showErrorMessage(`失敗しました。`);
			}
		});
	});

	context.subscriptions.push(disposable);
}

export function deactivate() { }

async function promptForLogin(context: vscode.ExtensionContext): Promise<User | undefined> {
	const ur_name = await vscode.window.showInputBox({ prompt: "ユーザー名を入力してね" });
	if (!ur_name) {
		vscode.window.showErrorMessage("ユーザー名を入力してください。");
		return;
	}
	const password = await vscode.window.showInputBox({ prompt: "パスワードを入力してね", password: true });
	if (!password) {
		vscode.window.showErrorMessage("パスワードを入力してください。");
		return;
	}
	const user = await authenticateUser(ur_name, password);

	if (user) {
		await saveSessionToken(context, JSON.stringify({ ur_name, password }));

		return user;
	}
	vscode.window.showErrorMessage("認証に失敗しました。");
	return;
}
