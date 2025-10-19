# ♟️ Xiangqi Online Platform – Backend

This is the **backend server** for a modern **3D / 2D Xiangqi (Chinese Chess)** web game.
It powers **multiplayer rooms**, **lobby management**, **game validation**, **user accounts**, and **real-time gameplay**.
The **React.js frontend** (built with Vite + TypeScript + Bootstrap) consumes this API and the **SignalR** hub for real-time updates.

---

## 🚀 Features

### 👤 User & Profile

* Register, login, or guest login.
* Edit username and display name.
* Change password.
* Upload profile photo (Base64; can be extended to blob storage).
* Dashboard with:

  * Total games, wins/losses/draws, win rate.
  * Active streak.
  * 180-day heatmap (GitHub-style activity).
  * Online / last active status.

---

### 🏠 Room Management

* Create **public**, **private (password-protected)**, or **unlisted** rooms.
* Human-friendly room codes (`room1`, `room2`, …).
* Join / leave by **ID** or **code**.
* Spectator support with limits.
* List or filter rooms by:

  * Visibility (public / private / unlisted)
  * Capacity (open / full)
  * Owner, participant, or keyword search.

---

### 🛋️ Lobby & Waiting Room

* Players can enter a **lobby list** and join any available room.
* Room owners can assign seats (Red / Black) or move players back to waiting.
* Rooms display:

  * `isFull`,
  * waiting & spectator counts,
  * `canStart` flag (ready to play).
* Integrated with the frontend **React Lobby UI**:

  * 🔓 Public / 🔒 Private icons
  * “Create Room” / “Join Room” actions
  * “Waiting Room” page for pre-game setup

---

### 🎮 Game Lifecycle

* Start game when both Red and Black players are seated.
* Moves validated by full **Xiangqi rules**:

  * Horse leg blocking
  * Elephant river restriction
  * Flying general
  * Cannon capture rules
  * Pawn forward / no backward moves
* Clock (base + increment) support.
* Resign, draw, undo requests (resign implemented).
* Optional AI opponent (random move; pluggable α–β search).

---

### 📡 Real-Time via SignalR Hub (`/hub`)

**SignalR** provides instant updates for all clients:

* `LobbyUpdated`, `SeatsUpdated`
* `GameStarted`, `MoveApplied`, `GameEnded`

Each client automatically joins a group named `room:{roomId}`.

---

## 🛠️ Tech Stack

| Layer         | Technology                                    |
| ------------- | --------------------------------------------- |
| Backend       | **.NET 9 Minimal API** (C# 12)                |
| Realtime      | **SignalR**                                   |
| Architecture  | Domain-Driven (Domain / Infrastructure / API) |
| Documentation | Swagger (`/swagger`)                          |
| Storage       | In-Memory (extensible to EF Core + SQL)       |
| Frontend      | **React + TypeScript + Vite + Bootstrap 5**   |

---

## 📂 Project Structure

```
ChessProj/
├── Backend/
│   ├── Xiangqi.Api/              # Minimal API endpoints + SignalR hub
│   ├── Xiangqi.Domain/           # Entities, game logic, rule validation
│   ├── Xiangqi.Infrastructure/   # Repositories, services, models
│   └── README.md
└── frontend/
    ├── src/
    │   ├── pages/                # LobbyPage, WaitingRoomPage
    │   ├── components/           # FriendsPanel, RoomRow, etc.
    │   ├── api/                  # API calls (replace mock with real)
    │   └── types.ts
    └── vite.config.ts
```

---

## ⚙️ Getting Started

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
* [Node.js 18+](https://nodejs.org/)
* [npm](https://www.npmjs.com/)

---

### 🧩 Backend – Build & Run

```bash
# Build all backend projects
dotnet build

# Run API
cd Backend/Xiangqi.Api
dotnet run
```

Server will run at:

* **API:** [http://localhost:5xxx](http://localhost:5xxx)
* **Swagger:** [http://localhost:5xxx/swagger](http://localhost:5xxx/swagger)
* **SignalR Hub:** [http://localhost:5xxx/hub](http://localhost:5xxx/hub)

---

### 🌐 Frontend – Run React App

```bash
cd frontend
npm install
npm run dev
```

Then open the Vite dev URL (default: [http://localhost:5173](http://localhost:5173)).

The frontend will connect to:

* Backend REST API → `http://localhost:5xxx`
* SignalR Hub → `http://localhost:5xxx/hub`

---

## 📡 Example Flow (Swagger Test)

1. Register or login (`/auth/register`, `/auth/login`)
2. Create a room (`/rooms/create`) – choose public/private/unlisted
3. Join room as Red / Black / spectator (`/rooms/join`)
4. Waiting mode: players enter `/wait`, owner assigns seats `/seat`
5. Start game → `/rooms/{roomId}/game/start`
6. Play moves → `/rooms/{roomId}/move`
7. Resign → `/rooms/{roomId}/resign`
8. View dashboard → `/users/{id}/dashboard`

---

## 🧱 Frontend–Backend Integration Summary

| Frontend Route          | Backend Endpoint               | Purpose                            |
| ----------------------- | ------------------------------ | ---------------------------------- |
| `/` (Lobby)             | `GET /rooms`                   | Display all public/private rooms   |
| `/waiting/:roomId`      | `GET /rooms/{roomId}`          | Show waiting players / start state |
| `POST /rooms`           | Create new room                |                                    |
| `POST /rooms/{id}/join` | Join existing room             |                                    |
| `SignalR: /hub`         | Real-time lobby + move updates |                                    |

---

## 🧭 Future Enhancements

* Persistent storage (EF Core + SQL)
* Authentication via JWT tokens
* Advanced AI opponent (search tree / ML)
* Replay viewer with move history
* Matchmaking queue
* Integration with mobile clients

---

### 🀄 Author

**Yaya Zhang** – *Creative Technology & AI*
Howest University of Applied Sciences – Bruges

---
