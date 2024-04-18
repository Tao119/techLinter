"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.deactivate = exports.activate = void 0;
var vscode = require("vscode");
var childProcess = require("child_process");
var path = require("path");
function activate(context) {
    var disposable = vscode.commands.registerCommand('extension.runLinter', function () {
        var _a;
        var pythonPath = path.join(__dirname, '../linter/main.py');
        var filePath = (_a = vscode.window.activeTextEditor) === null || _a === void 0 ? void 0 : _a.document.fileName;
        childProcess.exec("python \"".concat(pythonPath, "\" \"").concat(filePath, "\""), function (error, stdout, stderr) {
            if (error) {
                vscode.window.showErrorMessage("Error: ".concat(stderr));
            }
            else {
                vscode.window.showInformationMessage(stdout);
            }
        });
    });
    context.subscriptions.push(disposable);
}
exports.activate = activate;
function deactivate() { }
exports.deactivate = deactivate;
