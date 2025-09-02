# ♟️ Xiangqi Online Platform – Backend

This is the **backend server** for a 3D Xiangqi (Chinese Chess) game.  
It powers **multiplayer rooms**, **lobby system**, **game rules validation**, **user accounts**, and **real-time gameplay**.  
The frontend (Unity 3D) consumes this API + SignalR hub.

---

## 🚀 Features

### 👤 User & Profile

- Register, login, or guest login.
- Edit username & display name.
- Change password.
- Upload profile photo (Base64, extendable to blob storage).
- Dashboard with:
  - Total games, wins/losses/draws, win rate.
  - Active streak.
  - 180-day heatmap (like GitHub contributions).
  - Online/last active status.

### 🏠 Room Management

- Create **public**, **private (password-protected)**, or **unlisted** rooms.
- Human-friendly room codes (`room1`, `room2`, …).
- Join/leave by **ID** or **code**.
- Spectator support with spectator limits.
- List/filter rooms by visibility, capacity (full/open), owner, participant, or search.

### 🛋️ Lobby / Waiting Mode

- Players can join a **waiting list** until seated.
- Room owner assigns Red/Black seats.
- Unseat players back to waiting.
- Room shows **isFull**, waiting & spectator counts, `canStart` flag.

### 🎮 Game Lifecycle

- Start a game when both Red & Black filled.
- Moves validated against **Xiangqi rules** (horse leg block, elephant river, flying general, cannon captures, pawn rules, etc.).
- Clocks (base + increment).
- Resign, draw, undo requests (basic resign implemented).
- Robot opponent (random move AI; pluggable for alpha–beta).

### 📡 Real-time (SignalR Hub `/hub`)

- Broadcasts:
  - `LobbyUpdated`, `SeatsUpdated`
  - `GameStarted`, `MoveApplied`, `GameEnded`
- Clients join groups (`room:{roomId}`).

---

## 🛠️ Tech Stack

- **.NET 9 Minimal API** (C# 12)
- **SignalR** for real-time events
- **Layered architecture**:
  - `Domain`: Xiangqi rules, move validation, FEN
  - `Infrastructure`: in-memory repositories
  - `Api`: endpoints & SignalR hub
- **Swagger** (`/swagger`) for API testing
- In-memory persistence (swap for EF Core/SQL later)

---

## 📂 Project Structure

```
Backend/
├── Xiangqi.Api/ # Minimal API endpoints + SignalR Hub
├── Xiangqi.Domain/ # Entities, game logic, move validation
├── Xiangqi.Infrastructure/ # Repositories, services, models
└── README.md
ChineseChess/
```

## ⚙️ Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)

### Build & Run

```bash
# Restore & build all projects
dotnet build

# Run API
cd Xiangqi.Api
dotnet run
```

Server runs at:

API → [http://localhost:5xxx](http://localhost:5xxx)

Swagger UI → [http://localhost:5xxx/swagger](http://localhost:5xxx/swagger)

SignalR hub → [http://localhost:5xxx/hub](http://localhost:5xxx/hub)

## 📡 Example Flow (Swagger Test)

> 1. Register or login (**```/auth/register```**, **```/auth/login```**).
> 2. Create room (**```public/private/unlisted```**).
> 3. Join room (as Red, Black, or spectator; private needs password)
> 4. Waiting mode: guest joins **```/wait```**, owner assigns seat **/```seat```**.
> 5. Start game → **```/rooms/{roomId}/game/start```**.
> 6. Play moves → **```/rooms/{roomId}/move```**.
> 7. Resign → **```/rooms/{roomId}/resign```**.
> 8. Check dashboard → **```/users/{id}/dashboard```**

