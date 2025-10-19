
# ♟️ Xiangqi Online Platform – Frontend (React)

This is the **React + TypeScript + Vite + Bootstrap 5** web frontend for the **Xiangqi Online Platform**.
It connects to the **.NET 9 backend** via REST API and **SignalR** for real-time lobby and game updates.

---

## 🏯 Overview

The frontend serves as the **player interface** for:

* Joining or creating rooms (public/private)
* Viewing lobby and online players
* Waiting for opponents
* Watching real-time game updates
* Eventually: playing full 3D/2D Xiangqi matches

---

## 🚀 Tech Stack

| Layer     | Technology                               |
| --------- | ---------------------------------------- |
| Framework | **React 18 + Vite**                      |
| Language  | **TypeScript**                           |
| Styling   | **Bootstrap 5 + Bootstrap Icons**        |
| Routing   | **React Router v6**                      |
| Real-time | **SignalR client** (planned integration) |
| API Calls | `fetch()` or `axios` to .NET backend     |
| Dev Tools | Vite + ESLint + Hot Reload               |

---

## 📂 Project Structure

```
frontend/
├── src/
│   ├── api/
│   │   └── mockApi.ts          # Temporary mock data (replace with real backend)
│   ├── components/
│   │   ├── FriendsPanel.tsx    # Left sidebar of online users
│   │   ├── RoomRow.tsx         # Single room card (public/private)
│   │   └── CreateButtons.tsx   # Buttons to create rooms
│   ├── pages/
│   │   ├── LobbyPage.tsx       # Main room list view
│   │   └── WaitingRoomPage.tsx # Waiting room view
│   ├── types.ts                # TypeScript interfaces (Room, etc.)
│   ├── App.tsx                 # Route structure
│   ├── main.tsx                # React entrypoint + Bootstrap import
│   └── index.css               # Theme (paper texture & color palette)
├── vite.config.ts              # Dev proxy & build config
├── tsconfig.json
├── package.json
└── README.md
```

---

## 🧱 UI Preview

| Page                 | Description                                                                                                                          |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| 🏠 **Lobby**         | Displays all rooms in one unified list. Each has a 🔓 (public) or 🔒 (private) icon, room name, host, player count, and join button. |
| 🧩 **Waiting Room**  | Shows players seated (Red / Black), waiting status, and room code for private invites.                                               |
| 👥 **Friends Panel** | Displays online users and chat icons (UI placeholder for now).                                                                       |

---

## 🖼️ Visual Style

🎨 Theme inspired by **classical Chinese calligraphy**:

* Parchment-like paper background
* Red & black color palette
* Rounded “wood” cards
* Brush-style typography (planned)

Example mockups:

> Lobby – Unified Room List
> ![Lobby Screenshot](../docs/images/lobby_mock.png)

> Waiting Room – Opponent Awaiting
> ![Waiting Screenshot](../docs/images/waiting_mock.png)

---

## ⚙️ Setup Instructions

### Prerequisites

* [Node.js 18+](https://nodejs.org/)
* Backend server running (.NET 9 API + SignalR)

### 1️⃣ Install Dependencies

```bash
cd frontend
npm install
```

### 2️⃣ Run in Development Mode

```bash
npm run dev
```

Then open the printed URL (usually **[http://localhost:5173](http://localhost:5173)**).

### 3️⃣ Build for Production

```bash
npm run build
```

The compiled files will appear in the `dist/` folder.

---

## 🔗 Backend Connection

The React frontend communicates with the **.NET backend** via REST and SignalR.

### Example Proxy Config (`vite.config.ts`)

```ts
server: {
  proxy: {
    '/pyapi': {
      target: 'http://localhost:8000',
      changeOrigin: true,
      rewrite: p => p.replace(/^\/pyapi/, ''),
    },
    '/dotnetapi': {
      target: 'https://localhost:5001',
      changeOrigin: true,
      secure: false,
      rewrite: p => p.replace(/^\/dotnetapi/, ''),
    },
  },
}
```

---

## 📡 Data Flow

| Action              | Frontend Component | Backend Endpoint        |
| ------------------- | ------------------ | ----------------------- |
| Load all rooms      | `LobbyPage`        | `GET /rooms`            |
| Create room         | `CreateButtons`    | `POST /rooms`           |
| Join room           | `RoomRow`          | `POST /rooms/{id}/join` |
| Show waiting status | `WaitingRoomPage`  | `GET /rooms/{id}`       |
| Realtime updates    | *(coming soon)*    | `SignalR /hub`          |

---

## 🧭 Future Additions

* ✅ Replace mock API with real `fetch()` calls to backend
* ✅ Connect SignalR client for live room & move updates
* 🧩 Add in-browser Xiangqi board for actual gameplay
* 🎨 Add responsive mobile layout
* 🔔 Add sound & animation effects
* 🪶 Add brush-style font (e.g., Ma Shan Zheng or Noto Serif SC)

---

## 💡 Development Tips

| Shortcut        | Action                        |
| --------------- | ----------------------------- |
| `npm run dev`   | Start live server             |
| `Ctrl + C`      | Stop Vite                     |
| `npm run lint`  | Check TypeScript & formatting |
| `npm run build` | Generate production build     |

---

## 🧑‍💻 Developer

**Frontend Developer:** *Yaya Zhang*
**University:** Howest University of Applied Sciences – Bruges
**Program:** Creative Technology & Artificial Intelligence (CTAI)
**Contact:** [GitHub](https://github.com/) · [LinkedIn](https://linkedin.com/)

---

### 📁 Project Overview

| Folder      | Description                              |
| ----------- | ---------------------------------------- |
| `Backend/`  | .NET 9 Minimal API + SignalR (C#)        |
| `frontend/` | React + TypeScript + Bootstrap           |
| `docs/`     | Screenshots, Figma concepts, UI diagrams |

---

### 🀄 Acknowledgements

* Bootstrap Icons
* Vite + React
* SignalR (.NET 9)
* Xiangqi community for open game logic references

---
