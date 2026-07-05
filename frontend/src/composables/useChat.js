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

  let incomingTypingTimeout = null
  let outgoingTypingStopTimeout = null
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

  function clearIncomingTypingIndicator() {
    typingIndicator.value = ''

    if (incomingTypingTimeout) {
      clearTimeout(incomingTypingTimeout)
      incomingTypingTimeout = null
    }
  }

  function clearOutgoingTypingTimer() {
    if (outgoingTypingStopTimeout) {
      clearTimeout(outgoingTypingStopTimeout)
      outgoingTypingStopTimeout = null
    }
  }

  function getMessageKey(message) {
    return [
      message.senderId,
      message.receiverId,
      message.sentAt,
      message.data
    ].join('|')
  }

  function mergeMessages(nextMessages) {
    const mergedMessages = [...messages.value]
    const existingKeys = new Set(mergedMessages.map(getMessageKey))

    nextMessages.forEach(message => {
      const messageKey = getMessageKey(message)

      if (!existingKeys.has(messageKey)) {
        mergedMessages.push(message)
        existingKeys.add(messageKey)
      }
    })

    messages.value = mergedMessages.sort((first, second) =>
      new Date(first.sentAt) - new Date(second.sentAt)
    )
  }

  async function loadHistory(nextReceiverId) {
    if (!connection || connectionStatus.value !== 'Connected') {
      return
    }

    if (!nextReceiverId.trim()) {
      return
    }

    try {
      const history = await connection.invoke('GetHistory', nextReceiverId.trim())
      mergeMessages(history)
    } catch (error) {
      console.error(error)
      errorMessage.value = 'Could not load message history.'
    }
  }

  async function sendTypingStatus(status) {
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
        data: status
      })
    } catch (error) {
      console.error(error)
    }
  }

  async function selectReceiver(nextReceiverId) {
    if (receiverId.value && receiverId.value !== nextReceiverId) {
      await sendTypingStatus('stop')
    }

    clearOutgoingTypingTimer()
    clearIncomingTypingIndicator()

    receiverId.value = nextReceiverId

    if (nextReceiverId) {
      unreadByUser.value = {
        ...unreadByUser.value,
        [nextReceiverId]: 0
      }

      await loadHistory(nextReceiverId)
    }
  }

  function addMessage(message) {
    mergeMessages([message])

    const isIncomingMessage = message.senderId !== currentUserId.value
    const isInactiveConversation = message.senderId !== receiverId.value

    if (isIncomingMessage && isInactiveConversation) {
      unreadByUser.value = {
        ...unreadByUser.value,
        [message.senderId]: (unreadByUser.value[message.senderId] || 0) + 1
      }
    }
  }

  function getConnectionErrorMessage(error) {
    const message = error instanceof Error ? error.message : ''

    if (message.includes('This username is already in use.')) {
      return 'This username is already in use.'
    }

    return 'Could not connect to chat server.'
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
            void selectReceiver('')
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

        if (message.data === 'stop') {
          clearIncomingTypingIndicator()
          return
        }

        typingIndicator.value = `${message.senderId} is typing...`

        if (incomingTypingTimeout) {
          clearTimeout(incomingTypingTimeout)
        }

        incomingTypingTimeout = setTimeout(() => {
          typingIndicator.value = ''
          incomingTypingTimeout = null
        }, 1500)
      })

      connection.on('ReceiveError', error => {
        errorMessage.value = error.data
      })

      connection.onreconnecting(() => {
        connectionStatus.value = 'Reconnecting'
        clearOutgoingTypingTimer()
        clearIncomingTypingIndicator()
      })

      connection.onreconnected(async () => {
        connectionStatus.value = 'Connected'

        await connection.invoke('RegisterUser', {
          type: 'connect',
          senderId: currentUserId.value,
          receiverId: '',
          data: ''
        })

        if (receiverId.value) {
          await loadHistory(receiverId.value)
        }
      })

      connection.onclose(() => {
        connectionStatus.value = 'Disconnected'
        onlineUsers.value = []
        clearOutgoingTypingTimer()
        clearIncomingTypingIndicator()
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
      if (connection) {
        try {
          await connection.stop()
        } catch (stopError) {
          console.error(stopError)
        }
      }

      connection = null
      connectionStatus.value = 'Disconnected'
      onlineUsers.value = []
      errorMessage.value = getConnectionErrorMessage(error)
      console.error(error)
    }
  }

  async function disconnectUser() {
    errorMessage.value = ''

    if (!connection) {
      return
    }

    try {
      await sendTypingStatus('stop')
      await connection.stop()
    } catch (error) {
      errorMessage.value = 'Could not disconnect cleanly.'
      console.error(error)
    } finally {
      connection = null
      connectionStatus.value = 'Disconnected'
      onlineUsers.value = []
      clearOutgoingTypingTimer()
      clearIncomingTypingIndicator()
      void selectReceiver('')
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
      clearOutgoingTypingTimer()
      await sendTypingStatus('stop')
    } catch (error) {
      errorMessage.value = 'Message could not be sent.'
      console.error(error)
    }
  }

  async function sendTyping() {
    if (!messageText.value.trim()) {
      clearOutgoingTypingTimer()
      await sendTypingStatus('stop')
      return
    }

    await sendTypingStatus('start')

    clearOutgoingTypingTimer()

    outgoingTypingStopTimeout = setTimeout(() => {
      void sendTypingStatus('stop')
      outgoingTypingStopTimeout = null
    }, 1200)
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
