<script setup>
defineProps({
  onlineUsers: {
    type: Array,
    required: true
  }
})
</script>

<template>
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
</template>

