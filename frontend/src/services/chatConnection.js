import * as signalR from '@microsoft/signalr'

// Vite exposes VITE_* variables to browser code.
// The fallback keeps local development working without a .env file.
const hubUrl = import.meta.env.VITE_CHAT_HUB_URL || 'http://localhost:5046/chatHub'

export function createChatConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build()
}
