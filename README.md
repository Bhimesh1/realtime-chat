# Real-Time Chat App

A simple full-stack chat application where two users can exchange messages in real time.

The project was built with ASP.NET Core, SignalR, and Vue 3. It supports direct messages, online presence, typing indicators, server-side message validation, and basic in-memory conversation history.

## Features

* Join the chat with a simple username
* See online users
* Send and receive messages instantly
* Show typing indicators
* Handle disconnects without crashing the server
* Validate incoming messages on the server
* Return safe error messages for invalid input
* Keep recent messages in memory during the same server run

## Tech Stack

**Backend**

* ASP.NET Core
* C#
* SignalR
* xUnit

**Frontend**

* Vue 3
* Vite
* JavaScript
* SignalR JavaScript client

**Containerized setup**

* Docker
* Docker Compose
* Nginx for serving the built frontend in Docker

## Run Locally

### Backend

From the project root:

```
cd backend
dotnet restore
dotnet run --project src/ChatApp.Server
```

The backend runs at:

```
http://localhost:5046
```

The SignalR hub is available at:

```
http://localhost:5046/chatHub
```

### Frontend

Open a second terminal from the project root:

```
cd frontend
npm install
npm run dev
```

The frontend runs at:

```
http://localhost:5173
```

Open the app in two browser tabs and join with two different usernames to test the chat flow.

## Run with Docker

From the project root:

```
docker compose up --build -d
```

Then open:

```
http://localhost:5173
```

Docker exposes:

```
frontend: http://localhost:5173
backend:  http://localhost:5046
hub:      http://localhost:5046/chatHub
```

To stop the containers:

```
docker compose down
```

## Frontend Configuration

The frontend reads the SignalR hub URL from:

```
VITE_CHAT_HUB_URL
```

Example:

```
VITE_CHAT_HUB_URL=http://localhost:5046/chatHub
```

If the variable is not provided, the frontend falls back to the local backend URL.

## Message Format

The app uses a simple message envelope for communication between the client and server.

```
{
  "type": "chat",
  "senderId": "user-1",
  "receiverId": "user-2",
  "data": "Hello"
}
```

The `type` field describes what kind of event the message represents:

| Type      | Direction       | Purpose                                            |
| --------- | --------------- | -------------------------------------------------- |
| `connect` | Client → Server | Register a user with the SignalR hub               |
| `chat`    | Client → Server | Send a plain text message to another user          |
| `typing`  | Client → Server | Notify another user that typing started or stopped |
| `error`   | Server → Client | Return a validation or connection error safely     |



When the server accepts a chat message, it adds `sentAt` before sending the message to the sender and receiver.


## Real-Time Flow

```
User joins with a username
        ↓
Vue creates a SignalR connection
        ↓
Client registers the username
        ↓
Server stores the connection and broadcasts presence
        ↓
Users select another online user
        ↓
Messages and typing updates are sent through SignalR
        ↓
Server validates each message before forwarding it
        ↓
Invalid messages return an error response to the caller
```

## Run Tests

From the project root:

```
cd backend
dotnet test
```

## What I Would Improve Next

* Database persistence for users and messages
* Authentication and authorization
* Read receipts and delivery status
* Group chat support
* More complete SignalR integration tests
* Production deployment configuration
