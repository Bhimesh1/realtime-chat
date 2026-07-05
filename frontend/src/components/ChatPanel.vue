<script setup>
defineProps({
  receiverId: {
    type: String,
    default: ''
  },
  availableReceivers: {
    type: Array,
    required: true
  },
  errorMessage: {
    type: String,
    default: ''
  },
  typingIndicator: {
    type: String,
    default: ''
  },
  messages: {
    type: Array,
    required: true
  },
  userId: {
    type: String,
    default: ''
  },
  messageText: {
    type: String,
    default: ''
  }
})

const emit = defineEmits([
  'update:receiverId',
  'update:messageText',
  'send-message',
  'send-typing'
])

function updateReceiver(event) {
  emit('update:receiverId', event.target.value)
}

function updateMessageText(event) {
  emit('update:messageText', event.target.value)
  emit('send-typing')
}

function formatTime(sentAt) {
  return new Date(sentAt).toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<template>
  <section class="chat-panel">
    <div class="chat-header">
      <div>
        <h2>Messages</h2>
        <p>Send a message to another connected user.</p>
      </div>

      <select :value="receiverId" @change="updateReceiver">
        <option value="">Select receiver</option>
        <option
          v-for="receiver in availableReceivers"
          :key="receiver"
          :value="receiver"
        >
          {{ receiver }}
        </option>
      </select>
    </div>

    <p v-if="errorMessage" class="error">
      {{ errorMessage }}
    </p>

    <p v-if="typingIndicator" class="typing-indicator">
      {{ typingIndicator }}
    </p>

    <div class="message-list">
      <p v-if="messages.length === 0" class="empty-text">
        No messages yet.
      </p>

      <article
        v-for="message in messages"
        :key="`${message.senderId}-${message.receiverId}-${message.sentAt}-${message.data}`"
        class="message"
        :class="{ own: message.senderId === userId.trim() }"
      >
        <div class="message-meta">
          <strong>{{ message.senderId }}</strong>
          <span>to {{ message.receiverId }}</span>
          <span>{{ formatTime(message.sentAt) }}</span>
        </div>

        <p>{{ message.data }}</p>
      </article>
    </div>

    <form class="message-form" @submit.prevent="emit('send-message')">
      <input
        :value="messageText"
        type="text"
        placeholder="Type your message"
        @input="updateMessageText"
      />

      <button type="submit">
        Send
      </button>
    </form>
  </section>
</template>

