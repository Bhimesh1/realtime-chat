<script setup>
import { ref } from 'vue'
import { createChatConnection } from './services/chatConnection'

const userId = ref('')
const connectionStatus = ref('Disconnected')
const onlineUsers = ref([])
const errorMessage = ref('')

let connection = null

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
        onlineUsers.value = [...onlineUsers.value, changedUserId]
      }

      if (!isOnline) {
        onlineUsers.value = onlineUsers.value.filter(user => user !== changedUserId)
      }
    })

    connection.on('ReceiveError', error => {
      errorMessage.value = error.data
    })

    connection.onreconnecting(() => {
      connectionStatus.value = 'Reconnecting'
    })

    connection.onreconnected(() => {
      connectionStatus.value = 'Connected'
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
</script>

<template>
  <main class="page">
    <section class="chat-card">
      <h1>Real-Time Chat</h1>
      <p class="subtitle">Vue client connected to ASP.NET Core SignalR.</p>

      <div class="form-row">
        <input
          v-model="userId"
          type="text"
          placeholder="Enter user id, for example user-1"
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

      <p class="status">
        Status: <strong>{{ connectionStatus }}</strong>
      </p>

      <p v-if="errorMessage" class="error">
        {{ errorMessage }}
      </p>

      <section class="online-users">
        <h2>Online users</h2>

        <p v-if="onlineUsers.length === 0">
          No users online yet.
        </p>

        <ul v-else>
          <li v-for="user in onlineUsers" :key="user">
            {{ user }}
          </li>
        </ul>
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

.chat-card {
  width: min(520px, 92vw);
  padding: 24px;
  border-radius: 16px;
  background: white;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.12);
}

h1 {
  margin: 0;
  font-size: 28px;
}

.subtitle {
  margin-top: 8px;
  color: #64748b;
}

.form-row {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

input {
  flex: 1;
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
  margin-top: 20px;
}

.error {
  padding: 12px;
  border-radius: 10px;
  background: #fee2e2;
  color: #991b1b;
}

.online-users {
  margin-top: 24px;
}

.online-users h2 {
  font-size: 18px;
}

.online-users ul {
  padding-left: 20px;
}
</style>
