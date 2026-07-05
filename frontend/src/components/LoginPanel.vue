<script setup>
defineProps({
  userId: {
    type: String,
    default: ''
  },
  connectionStatus: {
    type: String,
    required: true
  }
})

const emit = defineEmits(['update:userId', 'connect', 'disconnect'])

function updateUserId(event) {
  emit('update:userId', event.target.value)
}
</script>

<template>
  <h1>Real-Time Chat</h1>
  <p class="subtitle">ASP.NET Core SignalR with Vue.</p>

  <div class="connect-box">
    <label for="userId">Your user id</label>

    <div class="form-row">
      <input
        id="userId"
        :value="userId"
        type="text"
        placeholder="user-1"
        :disabled="connectionStatus === 'Connected'"
        @input="updateUserId"
      />

      <button
        v-if="connectionStatus !== 'Connected'"
        type="button"
        @click="emit('connect')"
      >
        Connect
      </button>

      <button
        v-else
        type="button"
        class="secondary-button"
        @click="emit('disconnect')"
      >
        Disconnect
      </button>
    </div>
  </div>

  <p class="status">
    Status: <strong>{{ connectionStatus }}</strong>
  </p>
</template>

