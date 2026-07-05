import { computed, ref } from 'vue'
import { createChatConnection } from '../services/chatConnection'

export function useChat() {
  const userId = ref('')
  const receiverId = ref('')
  const messageText = ref('')
  const connectionStatus = ref('Disconnected')
  const onlineUsers = ref([])
  const messages = ref([])
  const errorMessage = ref('')
  const typingIndicator = ref('')

  let typingTimeout = null
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

      connection.on('ReceiveTyping', message => {
        typingIndicator.value = `${message.senderId} is typing...`

        if (typingTimeout) {
          clearTimeout(typingTimeout)
        }

        typingTimeout = setTimeout(() => {
          typingIndicator.value = ''
        }, 1500)
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

  async function disconnectUser() {
    errorMessage.value = ''

    if (!connection) {
      return
    }

    try {
      await connection.stop()
    } catch (error) {
      errorMessage.value = 'Could not disconnect cleanly.'
      console.error(error)
    } finally {
      connection = null
      connectionStatus.value = 'Disconnected'
      onlineUsers.value = []
      receiverId.value = ''
      typingIndicator.value = ''
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

  async function sendTyping() {
    if (!connection || connectionStatus.value !== 'Connected') {
      return
    }

    if (!receiverId.value.trim()) {
      return
    }

    try {
      await connection.invoke('SendTyping', {
        type: 'typing',
        senderId: userId.value.trim(),
        receiverId: receiverId.value.trim(),
        data: 'typing'
      })
    } catch (error) {
      console.error(error)
    }
  }

  return {
    userId,
    receiverId,
    messageText,
    connectionStatus,
    onlineUsers,
    messages,
    errorMessage,
    typingIndicator,
    availableReceivers,
    connectUser,
    disconnectUser,
    sendMessage,
    sendTyping
  }
}
