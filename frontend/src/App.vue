<script setup>
import LoginPanel from './components/LoginPanel.vue'
import UserSidebar from './components/UserSidebar.vue'
import ChatPanel from './components/ChatPanel.vue'
import { useChat } from './composables/useChat'

const {
  userId,
  receiverId,
  messageText,
  connectionStatus,
  onlineUsers,
  messages,
  errorMessage,
  typingIndicator,
  availableReceivers,
  connectUser,
  disconnectUser,
  sendMessage,
  sendTyping
} = useChat()
</script>

<template>
  <main class="page">
    <section class="chat-shell">
      <aside class="sidebar">
        <LoginPanel
          :user-id="userId"
          :connection-status="connectionStatus"
          @update:user-id="userId = $event"
          @connect="connectUser"
          @disconnect="disconnectUser"
        />

        <UserSidebar :online-users="onlineUsers" />
      </aside>

      <ChatPanel
        :receiver-id="receiverId"
        :available-receivers="availableReceivers"
        :error-message="errorMessage"
        :typing-indicator="typingIndicator"
        :messages="messages"
        :user-id="userId"
        :message-text="messageText"
        @update:receiver-id="receiverId = $event"
        @update:message-text="messageText = $event"
        @send-message="sendMessage"
        @send-typing="sendTyping"
      />
    </section>
  </main>
</template>

<style>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #f3f4f6;
  font-family: Arial, sans-serif;
}

.chat-shell {
  width: min(1000px, 94vw);
  min-height: 620px;
  display: grid;
  grid-template-columns: 320px 1fr;
  overflow: hidden;
  border-radius: 18px;
  background: white;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.12);
}

.sidebar {
  padding: 24px;
  background: #0f172a;
  color: white;
}

.sidebar h1 {
  margin: 0;
  font-size: 28px;
}

.subtitle {
  margin-top: 8px;
  color: #cbd5e1;
}

.connect-box {
  margin-top: 28px;
}

label {
  display: block;
  margin-bottom: 8px;
  font-weight: 700;
}

.form-row {
  display: flex;
  gap: 10px;
}

input,
select {
  width: 100%;
  padding: 12px;
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  font-size: 15px;
}

button {
  padding: 12px 18px;
  border: 0;
  border-radius: 10px;
  background: #2563eb;
  color: white;
  font-weight: 700;
  cursor: pointer;
}

button:disabled,
input:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.secondary-button {
  background: #475569;
}

.status {
  margin-top: 18px;
}

.online-users {
  margin-top: 28px;
}

.online-users h2 {
  font-size: 18px;
}

.online-users ul {
  padding: 0;
  list-style: none;
}

.online-users li {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 0;
}

.presence-dot {
  width: 9px;
  height: 9px;
  border-radius: 999px;
  background: #22c55e;
}

.chat-panel {
  display: flex;
  flex-direction: column;
  min-height: 620px;
  padding: 24px;
}

.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.chat-header h2 {
  margin: 0;
}

.chat-header p {
  margin: 6px 0 0;
  color: #64748b;
}

.chat-header select {
  max-width: 220px;
}

.error {
  margin-top: 16px;
  padding: 12px;
  border-radius: 10px;
  background: #fee2e2;
  color: #991b1b;
}

.typing-indicator {
  margin: 14px 0 0;
  color: #64748b;
  font-size: 14px;
  font-style: italic;
}

.message-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 20px;
  padding: 16px;
  overflow-y: auto;
  border-radius: 14px;
  background: #f8fafc;
}

.message {
  max-width: 75%;
  padding: 12px;
  border-radius: 14px;
  background: white;
  border: 1px solid #e2e8f0;
}

.message.own {
  align-self: flex-end;
  background: #dbeafe;
}

.message-meta {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  font-size: 12px;
  color: #64748b;
}

.message p {
  margin: 8px 0 0;
}

.message-form {
  display: flex;
  gap: 12px;
  margin-top: 18px;
}

.empty-text {
  color: #94a3b8;
}

@media (max-width: 760px) {
  .chat-shell {
    grid-template-columns: 1fr;
  }

  .chat-header {
    align-items: stretch;
    flex-direction: column;
  }

  .chat-header select {
    max-width: none;
  }
}
</style>
