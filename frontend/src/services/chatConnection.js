import * as signalR from '@microsoft/signalr'

const hubUrl = import.meta.env.VITE_CHAT_HUB_URL || 'http://localhost:5046/chatHub'

export function createChatConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build()
}
