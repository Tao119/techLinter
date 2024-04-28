import * as vscode from 'vscode';
import { Axios } from './service/axios';
import { convertToCamelCase } from './service/convert';
export type User = {
    id: number;
    name: string;
    password: string;
    token: number;
    isAdmin: boolean;
};

export async function authenticateUser(ur_name: string, password: string): Promise<User | undefined> {
    const body = { name: ur_name, password: password };
    try {
        const res = await Axios.post("https://techlinter-server.onrender.com/login", body);
        if (res.status === 200) {
            const user = res.data;
            return convertToCamelCase(user);
        }
    } catch (error) {
        console.error('Error during authentication:', error);
    }
    return;
}


export async function validateSession(ur_name?: string, password?: string): Promise<User | undefined> {
    if (!ur_name || !password) {
        return;
    }
    return authenticateUser(ur_name, password);
}

export function saveSessionToken(context: vscode.ExtensionContext, token: string): void {
    context.globalState.update('sessionToken', token);
}

export async function getSessionToken(context: vscode.ExtensionContext): Promise<string | undefined> {
    return context.globalState.get('sessionToken');
}
