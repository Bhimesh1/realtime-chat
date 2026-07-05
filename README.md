# Real-Time Chat App

A simple full-stack real-time chat application built with ASP.NET Core, SignalR, and Vue 3.

The app allows two connected users to exchange messages instantly, see online users, view typing indicators, and continue conversations using in-memory message history.

## Tech Stack

### Backend

* ASP.NET Core
* C#
* SignalR
* xUnit for backend tests

### Frontend

* Vue 3
* Vite
* JavaScript
* SignalR JavaScript client

## Features

* Connect as a user with a simple user id
* See online users in the sidebar
* Send and receive real-time messages
* Show typing indicators
* Track typing start and stop states
* Show conversation previews
* Show unread message counts
* Store recent messages in memory on the server
* Load message history when selecting a conversation
* Handle disconnects without crashing the server
* Validate incoming message format safely
* Reject invalid messages with server-side error responses

## Project Structure

```text
realtime-chat/
├── backend/
│   ├── src/
│   │   └── ChatApp.Server/
│   │       ├── Hubs/
│   │       │   └── ChatHub.cs
│   │       ├── Models/
│   │       │   └── ChatMessage.cs
│   │       ├── Services/
│   │       │   ├── MessageStore.cs
│   │       │   └── MessageValidator.cs
│   │       └── Program.cs
│   └── tests/
│       └── ChatApp.Tests/
│           └── MessageValidatorTests.cs
│
└── frontend/
    ├── src/
    │   ├── components/
    │   │   ├── ChatPanel.vue
    │   │   ├── LoginPanel.vue
    │   │   └── UserSidebar.vue
    │   ├── composables/
    │   │   └── useChat.js
    │   ├── services/
    │   │   └── chatConnection.js
    │   ├── App.vue
    │   └── main.js
    └── package.json
```

## Message Format

All chat-related messages follow this structure:

```json
{
  "type": "chat",
  "senderId": "user-1",
  "receiverId": "user-2",
  "data": "Hello",
  "sentAt": "2026-07-05T12:00:00Z"
}
```

## Message Types

| Type      | Purpose                                              |
| --------- | ---------------------------------------------------- |
| `connect` | Registers a user connection with the SignalR hub     |
| `chat`    | Sends a plain text message to another connected user |
| `typing`  | Sends typing status updates                          |
| `error`   | Sent by the server when a client message is invalid  |

Clients can send:

```text
connect
chat
typing
```

The server sends:

```text
error
```

when validation fails.

## Typing Message Format

Typing messages use the same base structure.

```json
{
  "type": "typing",
  "senderId": "user-1",
  "receiverId": "user-2",
  "data": "start"
}
```

Valid typing values are:

```text
start
stop
```

## Real-Time Flow

```text
User opens frontend
        ↓
User enters user id
        ↓
Vue creates SignalR connection
        ↓
Client calls RegisterUser
        ↓
Server stores connection id for the user
        ↓
Server broadcasts presence update
        ↓
Another user connects
        ↓
Users select each other and send chat messages
        ↓
Server validates each message
        ↓
Server stores the message in memory
        ↓
Server sends the message to sender and receiver
```

## Validation Rules

The backend validates incoming messages before processing them.

Current validation includes:

* Message cannot be empty
* Message type is required
* Only client-supported message types are accepted
* Sender id is required
* Sender id cannot be longer than 64 characters
* Receiver id is required for chat and typing messages
* Receiver id cannot be longer than 64 characters
* Sender and receiver cannot be the same user
* Chat message text is required
* Chat message text cannot be longer than 2000 characters
* Typing status must be `start` or `stop`
* Invalid messages return an error response instead of crashing the server

## Running the Backend

From the project root:

```bash
cd backend
dotnet restore
dotnet run --project src/ChatApp.Server
```

The backend runs on the URL shown in the terminal.

The frontend expects the SignalR hub at:

```text
http://localhost:5046/chatHub
```

If your backend uses a different port, update the frontend environment variable.

## Running the Frontend

From the project root:

```bash
cd frontend
npm install
npm run dev
```

The frontend usually runs at:

```text
http://localhost:5173
```

## Frontend Environment Variable

The frontend reads the SignalR hub URL from:

```text
VITE_CHAT_HUB_URL
```

Example:

```env
VITE_CHAT_HUB_URL=http://localhost:5046/chatHub
```

If the variable is not provided, the frontend uses the default local hub URL.

## Running Tests

From the project root:

```bash
cd backend
dotnet test
```

The tests cover message validation and ensure invalid message formats are rejected safely.

## Current Limitations

This project intentionally keeps the scope simple for a junior full-stack challenge.

Current limitations:

* No real authentication
* No database persistence
* Message history is stored in memory only
* Refreshing the server clears all message history
* Users are identified by simple user ids
* No group chats
* No file uploads
* No message delete or edit support

## What I Would Improve Next

With more time, I would add:

* Database persistence for users and messages
* Authentication and authorization
* Better duplicate username handling
* Message delivery/read receipts
* Group chat support
* End-to-end integration tests for SignalR hub behavior
* Docker setup for easier local development
* Deployment configuration for production hosting

## Design Notes

The UI keeps a simple two-panel chat layout:

```text
Sidebar       Chat panel
users         selected conversation
presence      messages
previews      input
unread count  typing indicator
```

The original starting design is preserved while improving structure, readability, and user experience.
