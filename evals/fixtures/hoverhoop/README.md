# Hover Hoop

Browser party racer: 2-6 hover-boards race through hoops, 3 laps or 3 minutes, whichever comes first.

- `server/`: Colyseus 0.16 server, deployed on Colyseus Cloud (PM2 + Redis presence/driver).
- `client/`: Phaser 3 + colyseus.js 0.16, built with Vite. Published at https://play.hoverhoop.gg and on itch.io (HTML5 embed).

Local: `cd server && npm i && npm run dev`, then `cd client && npm i && npm run dev` (http://localhost:5173).
