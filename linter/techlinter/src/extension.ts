const { exec } = require('child_process');

function lintCode() {
    let filePath = vscode.window.activeTextEditor.document.fileName;
    exec(`python lint_csharp.py ${filePath}`, (err, stdout, stderr) => {
        if (err) {
            vscode.window.showErrorMessage("Error: " + stderr);
            return;
        }
        vscode.window.showInformationMessage(stdout);
    });
}
