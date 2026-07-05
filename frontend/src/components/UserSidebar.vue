<script setup>
defineProps({
  conversations: {
    type: Array,
    required: true
  },
  selectedReceiverId: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['select-user'])
</script>

<template>
  <section class="online-users">
    <h2>Conversations</h2>

    <p v-if="conversations.length === 0" class="empty-text">
      No users online yet.
    </p>

    <ul v-else class="conversation-list">
      <li
        v-for="conversation in conversations"
        :key="conversation.userId"
        class="conversation-item"
        :class="{ active: conversation.userId === selectedReceiverId }"
        @click="emit('select-user', conversation.userId)"
      >
        <div class="conversation-main">
          <span
            class="presence-dot"
            :class="{ offline: !conversation.isOnline }"
          ></span>

          <div class="conversation-text">
            <strong>{{ conversation.userId }}</strong>
            <span>{{ conversation.preview }}</span>
          </div>
        </div>

        <span
          v-if="conversation.unreadCount > 0"
          class="unread-badge"
        >
          {{ conversation.unreadCount }}
        </span>
      </li>
    </ul>
  </section>
</template>
