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
  const unreadByUser = ref({})

  let typingTimeout = null
  let connection = null

  const currentUserId = computed(() => userId.value.trim())

  const availableReceivers = computed(() =>
    onlineUsers.value.filter(user => user !== currentUserId.value)
  )

  const visibleMessages = computed(() => {
    if (!receiverId.value.trim()) {
      return messages.value
    }

    return messages.value.filter(message =>
      (message.senderId === currentUserId.value && message.receiverId === receiverId.value) ||
      (message.senderId === receiverId.value && message.receiverId === currentUserId.value)
    )
  })

  const conversationPreviews = computed(() => {
    const peers = new Set(availableReceivers.value)

    messages.value.forEach(message => {
      const peerId = message.senderId === currentUserId.value
        ? message.receiverId
        : message.senderId

      if (peerId && peerId !== currentUserId.value) {
        peers.add(peerId)
      }
    })

    return [...peers].sort().map(peerId => {
      const conversationMessages = messages.value.filter(message =>
        (message.senderId === currentUserId.value && message.receiverId === peerId) ||
        (message.senderId === peerId && message.receiverId === currentUserId.value)
      )

      const lastMessage = conversationMessages.at(-1)
      const isOnline = onlineUsers.value.includes(peerId)
      const unreadCount = unreadByUser.value[peerId] || 0

      return {
        userId: peerId,
        isOnline,
        unreadCount,
        lastMessage,
        preview: getPreviewText(lastMessage)
      }
    })
  })

  function getPreviewText(message) {
    if (!message) {
      return 'No messages yet'
    }

    const prefix = message.senderId === currentUserId.value ? 'You: ' : ''
    return `${prefix}${message.data}`
  }

  function selectReceiver(nextReceiverId) {
    receiverId.value = nextReceiverId
    typingIndicator.value = ''

    if (nextReceiverId) {
      unreadByUser.value = {
        ...unreadByUser.value,
        [nextReceiverId]: 0
      }
    }
  }

  function addMessage(message) {
    messages.value = [...messages.value, message]

    const isIncomingMessage = message.senderId !== currentUserId.value
    const isInactiveConversation = message.senderId !== receiverId.value

    if (isIncomingMessage && isInactiveConversation) {
      unreadByUser.value = {
        ...unreadByUser.value,
        [message.senderId]: (unreadByUser.value[message.senderId] || 0) + 1
      }
    }
  }

  async function connectUser() {
    errorMessage.value = ''

    if (!currentUserId.value) {
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
            selectReceiver('')
          }
        }
      })

      connection.on('ReceiveMessage', message => {
        addMessage(message)
      })

      connection.on('ReceiveTyping', message => {
        if (message.senderId !== receiverId.value) {
          return
        }

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
          senderId: currentUserId.value,
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
        senderId: currentUserId.value,
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
      selectReceiver('')
    }
  }

  async function sendMessage() {
    errorMessage.value = ''

    if (!connection || connectionStatus.value !== 'Connected') {
      errorMessage.value = 'Connect before sending a message.'
      return
    }

    if (!receiverId.value.trim()) {
      errorMessage.value = 'Select a receiver.'
      return
    }

    if (!messageText.value.trim()) {
      errorMessage.value = 'Enter a message.'
      return
    }

    try {
      await connection.invoke('SendMessage', {
        type: 'chat',
        senderId: currentUserId.value,
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
        senderId: currentUserId.value,
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
    visibleMessages,
    errorMessage,
    typingIndicator,
    availableReceivers,
    conversationPreviews,
    selectReceiver,
    connectUser,
    disconnectUser,
    sendMessage,
    sendTyping
  }
}
