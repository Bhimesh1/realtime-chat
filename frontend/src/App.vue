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
  visibleMessages,
  errorMessage,
  typingIndicator,
  availableReceivers,
  conversationPreviews,
  selectReceiver,
  connectUser,
  disconnectUser,
  sendMessage,
  sendTyping
} = useChat()
</script>

<template>
  <LoginPanel
    v-if="connectionStatus === 'Disconnected'"
    :user-id="userId"
    :connection-status="connectionStatus"
    :error-message="errorMessage"
    @update:user-id="userId = $event"
    @connect="connectUser"
  />

  <main v-else class="layout">
    <div v-if="connectionStatus === 'Reconnecting'" class="banner reconnecting">
      Connection lost — reconnecting…
    </div>

    <div v-if="errorMessage" class="banner error">
      {{ errorMessage }}
    </div>

    <div class="main" :class="{ 'has-selected-chat': receiverId }">
      <UserSidebar
        :user-id="userId"
        :conversations="conversationPreviews"
        :selected-receiver-id="receiverId"
        @select-user="selectReceiver"
        @disconnect="disconnectUser"
      />

      <ChatPanel
        :receiver-id="receiverId"
        :available-receivers="availableReceivers"
        :typing-indicator="typingIndicator"
        :messages="visibleMessages"
        :user-id="userId"
        :message-text="messageText"
        :connection-status="connectionStatus"
        @select-receiver="selectReceiver"
        @update:message-text="messageText = $event"
        @send-message="sendMessage"
        @send-typing="sendTyping"
      />
    </div>
  </main>
</template>

<style scoped>
.layout {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.banner {
  padding: 8px 16px;
  text-align: center;
  font-size: 13px;
  font-weight: 500;
}

.banner.reconnecting {
  background: #4a3b12;
  color: #ffd166;
}

.banner.error {
  background: #451820;
  color: var(--red);
}

.main {
  flex: 1;
  display: flex;
  min-height: 0;
}

@media (max-width: 720px) {
  .main {
    position: relative;
  }

  .main:not(.has-selected-chat) :deep(.chat) {
    display: none;
  }

  .main.has-selected-chat :deep(.sidebar) {
    display: none;
  }
}

@media (max-width: 720px) {
  .main {
    flex-direction: column;
  }
}
</style>
