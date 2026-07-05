<script setup>
import { ref } from 'vue'

defineProps({
  userId: {
    type: String,
    default: ''
  },
  connectionStatus: {
    type: String,
    required: true
  },
  errorMessage: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:userId', 'connect'])

const busy = ref(false)

function updateUserId(event) {
  emit('update:userId', event.target.value)
}

function submit() {
  if (busy.value) {
    return
  }

  busy.value = true
  emit('connect')

  setTimeout(() => {
    busy.value = false
  }, 2000)
}
</script>

<template>
  <main class="login">
    <section class="card">
      <div class="logo">💬</div>

      <h1>Real-Time Chat</h1>

      <p class="sub">
        Pick a username to start chatting
      </p>

      <form @submit.prevent="submit">
        <input
          :value="userId"
          maxlength="64"
          placeholder="e.g. alice"
          autocomplete="off"
          autofocus
          @input="updateUserId"
        />

        <button type="submit" :disabled="!userId.trim() || busy">
          {{ busy ? 'Connecting…' : 'Join chat' }}
        </button>
      </form>

      <p v-if="errorMessage" class="error">
        {{ errorMessage }}
      </p>

      <p class="hint">
        Open a second browser tab and join with a different name to chat.
      </p>
    </section>
  </main>
</template>

<style scoped>
.login {
  height: 100%;
  display: grid;
  place-items: center;
  background:
    radial-gradient(600px 400px at 70% 20%, rgba(109, 124, 255, 0.12), transparent),
    var(--bg);
}

.card {
  width: min(380px, 90vw);
  background: var(--bg-panel);
  border: 1px solid var(--border);
  border-radius: 20px;
  padding: 40px 32px;
  text-align: center;
  animation: fadeUp 0.3s ease;
}

.logo {
  margin-bottom: 12px;
  font-size: 40px;
}

h1 {
  margin-bottom: 6px;
  font-size: 22px;
  font-weight: 700;
}

.sub {
  margin-bottom: 24px;
  color: var(--text-dim);
}

input {
  width: 100%;
  margin-bottom: 12px;
  padding: 12px 14px;
  border: 1px solid var(--border);
  border-radius: 10px;
  outline: none;
  background: var(--bg-elevated);
  transition: border-color 0.15s;
}

input:focus {
  border-color: var(--accent);
}

button {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 10px;
  background: var(--accent);
  color: #fff;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
}

button:disabled {
  opacity: 0.5;
  cursor: default;
}

button:not(:disabled):hover {
  opacity: 0.9;
}

.error {
  margin-top: 14px;
  color: var(--red);
  font-size: 13px;
}

.hint {
  margin-top: 20px;
  color: var(--text-dim);
  font-size: 12px;
}
</style>
