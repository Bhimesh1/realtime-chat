<script setup>
import { nextTick, ref, watch } from 'vue'

const props = defineProps({
  receiverId: {
    type: String,
    default: ''
  },
  availableReceivers: {
    type: Array,
    required: true
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
  },
  connectionStatus: {
    type: String,
    required: true
  }
})

const emit = defineEmits([
  'select-receiver',
  'update:messageText',
  'send-message',
  'send-typing'
])

const scroller = ref(null)

watch(
  () => [props.messages.length, props.typingIndicator],
  async () => {
    await nextTick()
    scroller.value?.scrollTo({
      top: scroller.value.scrollHeight,
      behavior: 'smooth'
    })
  }
)

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

function getInitial(userId) {
  return userId.trim().charAt(0).toUpperCase()
}

function isReceiverOnline() {
  return props.availableReceivers.includes(props.receiverId)
}
</script>

<template>
  <section class="chat">
    <template v-if="receiverId">
      <header>
        <button
          type="button"
          class="back-button"
          @click="emit('select-receiver', '')"
        >
          Back
        </button>

        <div class="peer-info">
          <span class="peer-avatar">
            {{ getInitial(receiverId) }}
          </span>

          <div class="peer-text">
            <div class="peer-name">
              {{ receiverId }}
            </div>

            <div
              class="peer-status"
              :class="isReceiverOnline() ? 'online' : 'offline'"
            >
              {{ isReceiverOnline() ? 'online' : 'offline' }}
            </div>
          </div>
        </div>
      </header>

      <div ref="scroller" class="messages">
        <p v-if="messages.length === 0" class="empty">
          No messages yet - say hi to {{ receiverId }} 👋
        </p>

        <div
          v-for="message in messages"
          :key="`${message.senderId}-${message.receiverId}-${message.sentAt}-${message.data}`"
          class="row"
          :class="message.senderId === userId.trim() ? 'mine' : 'theirs'"
        >
          <div class="bubble">
            <span class="text">
              {{ message.data }}
            </span>

            <span class="time">
              {{ formatTime(message.sentAt) }}
            </span>
          </div>
        </div>

        <div v-if="typingIndicator" class="row theirs">
          <div class="bubble typing-bubble">
            <span
              v-for="n in 3"
              :key="n"
              class="tdot"
              :style="{ animationDelay: `${(n - 1) * 0.15}s` }"
            ></span>
          </div>
        </div>
      </div>

      <footer>
        <form @submit.prevent="emit('send-message')">
          <input
            :value="messageText"
            :placeholder="connectionStatus === 'Connected' ? `Message ${receiverId}…` : 'Reconnecting…'"
            :disabled="connectionStatus !== 'Connected'"
            maxlength="2000"
            @input="updateMessageText"
          />

          <button
            type="submit"
            :disabled="connectionStatus !== 'Connected' || !messageText.trim()"
          >
            Send
          </button>
        </form>
      </footer>
    </template>

    <div v-else class="placeholder">
      <div class="ph-icon">
        💬
      </div>

      <p>
        Select a conversation to start chatting.
      </p>
    </div>
  </section>
</template>

<style scoped>
.chat {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 20px;
  border-bottom: 1px solid var(--border);
  background: var(--bg-panel);
}

.peer-name {
  font-size: 15px;
  font-weight: 700;
}

.peer-status {
  font-size: 12px;
}

.peer-status.online {
  color: var(--green);
}

.peer-status.offline {
  color: var(--text-dim);
}

.peer-info {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.peer-avatar {
  width: 36px;
  height: 36px;
  flex-shrink: 0;
  border-radius: 50%;
  display: grid;
  place-items: center;
  background: var(--bubble-mine);
  color: #fff;
  font-weight: 600;
}

.peer-text {
  min-width: 0;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.empty {
  margin-top: 40px;
  color: var(--text-dim);
  text-align: center;
  display: grid;
  gap: 4px;
}

.empty strong {
  color: var(--text);
  font-weight: 600;
}

.empty span {
  font-size: 13px;
}

.row {
  display: flex;
  animation: fadeUp 0.18s ease;
}

.row.mine {
  justify-content: flex-end;
}

.row.theirs {
  justify-content: flex-start;
}

.bubble {
  max-width: min(65%, 520px);
  display: flex;
  align-items: flex-end;
  gap: 8px;
  padding: 9px 13px;
  border-radius: var(--radius);
  line-height: 1.45;
  word-break: break-word;
}

.mine .bubble {
  border-bottom-right-radius: 4px;
  background: var(--bubble-mine);
  color: #fff;
}

.theirs .bubble {
  border-bottom-left-radius: 4px;
  background: var(--bubble-theirs);
}

.text {
  min-width: 0;
}

.time {
  flex-shrink: 0;
  opacity: 0.65;
  font-size: 10px;
  white-space: nowrap;
}

.typing-bubble {
  gap: 4px;
  padding: 13px 15px;
}

.tdot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--text-dim);
  animation: blink 1.2s infinite;
}

footer {
  padding: 14px 20px;
  border-top: 1px solid var(--border);
  background: var(--bg-panel);
}

form {
  display: flex;
  gap: 10px;
}

input {
  flex: 1;
  padding: 12px 14px;
  border: 1px solid var(--border);
  border-radius: 12px;
  outline: none;
  background: var(--bg-elevated);
  transition: border-color 0.15s;
}

input:focus {
  border-color: var(--accent);
}

input:disabled {
  opacity: 0.5;
}

button {
  padding: 0 22px;
  border: none;
  border-radius: 12px;
  background: var(--accent);
  color: #fff;
  font-weight: 600;
  cursor: pointer;
}

button:disabled {
  opacity: 0.4;
  cursor: default;
}

.placeholder {
  flex: 1;
  display: grid;
  place-content: center;
  gap: 10px;
  color: var(--text-dim);
  text-align: center;
}

.ph-icon {
  font-size: 42px;
}

.back-button {
  display: none;
}

@media (max-width: 720px) {
  footer {
    padding: 10px 12px;
  }

  form {
    display: grid;
    grid-template-columns: minmax(0, 1fr) 72px;
    gap: 8px;
    align-items: center;
  }

  input {
    width: 100%;
    min-width: 0;
    height: 42px;
    padding: 0 12px;
    font-size: 13px;
  }

  button {
    height: 42px;
    padding: 0;
    font-size: 13px;
  }
}

@media (max-width: 390px) {
  form {
    grid-template-columns: minmax(0, 1fr) 66px;
  }

  button {
    font-size: 12px;
  }
}

@media (max-width: 720px) {
  header {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  

  .back-button {
    display: inline-flex;
    align-items: center;
    padding: 7px 10px;
    border: 1px solid var(--border);
    border-radius: 8px;
    background: var(--bg-elevated);
    color: var(--text-dim);
    font-size: 13px;
    cursor: pointer;
  }

  .back-button:hover {
    color: var(--text);
  }
}


</style>
