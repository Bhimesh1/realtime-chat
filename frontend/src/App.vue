<script setup>
import { computed, ref } from 'vue'
import { createChatConnection } from './services/chatConnection'

const userId = ref('')
const receiverId = ref('')
const messageText = ref('')
const connectionStatus = ref('Disconnected')
const onlineUsers = ref([])
const messages = ref([])
const errorMessage = ref('')

let connection = null

const availableReceivers = computed(() =>
  onlineUsers.value.filter(user => user !== userId.value.trim())
)

async function connectUser() {
  errorMessage.value = ''

  if (!userId.value.trim()) {
    errorMessage.value = 'Enter a user id before connecting.'
    return
  }

  try {
    connection = createChatConnection()

    connection.on('OnlineUsers', users => {
      onlineUsers.value = users
    })

    connection.on('UserPresenceChanged', (changedUserId, isOnline) => {
      if (isOnline && !onlineUsers.value.includes(changedUserId)) {
        onlineUsers.value = [...onlineUsers.value, changedUserId].sort()
      }

      if (!isOnline) {
        onlineUsers.value = onlineUsers.value.filter(user => user !== changedUserId)

        if (receiverId.value === changedUserId) {
          receiverId.value = ''
        }
      }
    })

    connection.on('ReceiveMessage', message => {
      messages.value = [...messages.value, message]
    })

    connection.on('ReceiveError', error => {
      errorMessage.value = error.data
    })

    connection.onreconnecting(() => {
      connectionStatus.value = 'Reconnecting'
    })

    connection.onreconnected(async () => {
      connectionStatus.value = 'Connected'

      await connection.invoke('RegisterUser', {
        type: 'connect',
        senderId: userId.value.trim(),
        receiverId: '',
        data: ''
      })
    })

    connection.onclose(() => {
      connectionStatus.value = 'Disconnected'
      onlineUsers.value = []
    })

    await connection.start()

    await connection.invoke('RegisterUser', {
      type: 'connect',
      senderId: userId.value.trim(),
      receiverId: '',
      data: ''
    })

    connectionStatus.value = 'Connected'
  } catch (error) {
    connectionStatus.value = 'Disconnected'
    errorMessage.value = 'Could not connect to chat server.'
    console.error(error)
  }
}

async function sendMessage() {
  errorMessage.value = ''

  if (!connection || connectionStatus.value !== 'Connected') {
    errorMessage.value = 'Connect before sending a message.'
    return
  }

  if (!receiverId.value.trim()) {
    errorMessage.value = 'Enter a receiver id.'
    return
  }

  if (!messageText.value.trim()) {
    errorMessage.value = 'Enter a message.'
    return
  }

  try {
    await connection.invoke('SendMessage', {
      type: 'chat',
      senderId: userId.value.trim(),
      receiverId: receiverId.value.trim(),
      data: messageText.value.trim()
    })

    messageText.value = ''
  } catch (error) {
    errorMessage.value = 'Message could not be sent.'
    console.error(error)
  }
}

function formatTime(sentAt) {
  return new Date(sentAt).toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<template>
  <main class="page">
    <section class="chat-shell">
      <aside class="sidebar">
        <h1>Real-Time Chat</h1>
        <p class="subtitle">ASP.NET Core SignalR with Vue.</p>

        <div class="connect-box">
          <label for="userId">Your user id</label>
          <div class="form-row">
            <input
              id="userId"
              v-model="userId"
              type="text"
              placeholder="user-1"
              :disabled="connectionStatus === 'Connected'"
            />

            <button
              type="button"
              :disabled="connectionStatus === 'Connected'"
              @click="connectUser"
            >
              Connect
            </button>
          </div>
        </div>

        <p class="status">
          Status: <strong>{{ connectionStatus }}</strong>
        </p>

        <section class="online-users">
          <h2>Online users</h2>

          <p v-if="onlineUsers.length === 0" class="empty-text">
            No users online yet.
          </p>

          <ul v-else>
            <li v-for="user in onlineUsers" :key="user">
              <span class="presence-dot"></span>
              {{ user }}
            </li>
          </ul>
        </section>
      </aside>

      <section class="chat-panel">
        <div class="chat-header">
          <div>
            <h2>Messages</h2>
            <p>Send a message to another connected user.</p>
          </div>

          <select v-model="receiverId">
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

        <form class="message-form" @submit.prevent="sendMessage">
          <input
            v-model="messageText"
            type="text"
            placeholder="Type your message"
          />

          <button type="submit">
            Send
          </button>
        </form>
      </section>
    </section>
  </main>
</template>

<style scoped>
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





