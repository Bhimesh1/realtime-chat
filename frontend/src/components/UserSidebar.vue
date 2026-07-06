<script setup>

defineProps({
  userId: {
    type: String,
    default: ''
  },
  conversations: {
    type: Array,
    required: true
  },
  selectedReceiverId: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['select-user', 'disconnect'])

function getInitial(userId) {
  return userId.trim().charAt(0).toUpperCase()
}

</script>

<template>
  <aside class="sidebar">
    <div class="me">
      <span class="avatar">
        {{ getInitial(userId) }}
      </span>

      <div class="me-text">
        <div class="name">
          {{ userId }}
        </div>

        <div class="status online">
          online
        </div>
      </div>

      <button type="button" class="logout" @click="emit('disconnect')">
        Leave
      </button>
    </div>

    <div class="section-label">
      Conversations
    </div>

    <div class="list">
      <button
        v-for="conversation in conversations"
        :key="conversation.userId"
        type="button"
        class="user"
        :class="{ active: conversation.userId === selectedReceiverId }"
        @click="emit('select-user', conversation.userId === selectedReceiverId ? '' : conversation.userId)"
      >
        <span
          class="avatar"
          :class="{ offline: !conversation.isOnline }"
        >
          {{ getInitial(conversation.userId) }}
        </span>

        <div class="meta">
          <div class="name">
            {{ conversation.userId }}
          </div>

          <div
            class="status"
            :class="conversation.isOnline ? 'online' : 'offline'"
          >
            {{ conversation.preview }}
          </div>
        </div>

        <span
          v-if="conversation.unreadCount > 0"
          class="unread"
        >
          {{ conversation.unreadCount }}
        </span>

        <span
          v-else
          class="dot"
          :class="{ on: conversation.isOnline }"
        ></span>
      </button>

      <p v-if="conversations.length === 0" class="empty">
        No one else is here yet.
      </p>
    </div>

  </aside>
</template>

<style scoped>
.sidebar {
  width: 260px;
  background: var(--bg-panel);
  border-right: 1px solid var(--border);
  display: flex;
  flex-direction: column;
}

.me {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 16px;
  border-bottom: 1px solid var(--border);
}

.me-text {
  flex: 1;
  min-width: 0;
}

.section-label {
  padding: 14px 16px 6px;
  color: var(--text-dim);
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.list {
  flex: 1;
  overflow-y: auto;
  padding: 0 8px;
}

.user {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 8px;
  border: none;
  border-radius: 10px;
  background: none;
  cursor: pointer;
  text-align: left;
  outline: none;
  -webkit-tap-highlight-color: transparent;
  transition:
    background 0.12s,
    box-shadow 0.12s,
    transform 0.08s;
}

.user:hover {
  background: var(--bg-hover);
}

.user.active {
  background: var(--accent-soft);
}

.user:focus {
  outline: none;
}

.user:focus-visible {
  border-radius: 10px;
  box-shadow: 0 0 0 2px var(--accent);
}

.user:active {
  border-radius: 10px;
  transform: scale(0.99);
}

.avatar {
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

.avatar.offline {
  filter: grayscale(1) brightness(0.7);
}

.meta {
  flex: 1;
  min-width: 0;
}

.name {
  overflow: hidden;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.status {
  overflow: hidden;
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.status.online {
  color: var(--green);
}

.status.offline {
  color: var(--text-dim);
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--text-dim);
  opacity: 0.4;
}

.dot.on {
  background: var(--green);
  opacity: 1;
}

.unread {
  min-width: 20px;
  padding: 2px 6px;
  border-radius: 999px;
  background: var(--accent);
  color: #fff;
  font-size: 11px;
  font-weight: 700;
  text-align: center;
}

.empty {
  padding: 10px;
  color: var(--text-dim);
  font-size: 13px;
}


.logout {
  padding: 6px 9px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--bg-elevated);
  color: var(--text-dim);
  font-size: 12px;
  cursor: pointer;
}

.logout:hover {
  color: var(--text);
}

@media (max-width: 720px) {
  .sidebar {
    width: 100%;
    height: 100%;
    max-height: none;
    border-right: none;
    border-bottom: none;
  }

  .user:focus-visible {
    border-radius: 12px;
    box-shadow: 0 0 0 2px var(--accent);
  }

  .user:active {
    border-radius: 12px;
  }

  .me {
    flex-shrink: 0;
    padding: 14px 16px;
  }

  .section-label {
    flex-shrink: 0;
    padding: 14px 16px 8px;
  }

  .list {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 7px;
    overflow-y: auto;
    padding: 6px 12px 16px;
  }

  .user {
    width: 100%;
    min-width: 0;
    max-width: none;
    flex: 0 0 auto;
    padding: 10px 12px;
    border: 1px solid rgba(255, 255, 255, 0.06);
    border-radius: 12px;
    background: var(--bg-elevated);
  }

  .user:hover,
  .user.active {
    background: var(--bg-hover);
  }

  .user.active {
    border-color: var(--accent);
    background: var(--accent-soft);
  }

  .meta {
    min-width: 0;
  }

  .dot,
  .unread {
    margin-left: auto;
  }
}


</style>
