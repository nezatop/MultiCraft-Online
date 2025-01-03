import { WebSocketServer } from 'ws'; // Импортируем WebSocketServer

import express from 'express';
import http from 'http';
import cors from 'cors';

import { loadPlayerData, savePlayerData, playerData } from './utils/storage.js';
import { handleClientMessage, broadcast } from './routes/player.js';
import { PORT } from './config.js';
import { clients } from './utils/chunk.js';

// Создаем приложение Express
const app = express();
const server = http.createServer(app);

// Настройка CORS
app.use(cors());

// Создаем сервер WebSocket
const wss = new WebSocketServer({ server }); // Используем WebSocketServer вместо WebSocket.Server

//loadPlayerData();

wss.on('connection', (socket) => {

    socket.on('message', (message) => {
        try {
            const data = JSON.parse(message);
            console.log(`[SERVER]Received massage ${JSON.stringify(data)}`);
            handleClientMessage(data, socket);
        } catch (error) {
            console.error('Ошибка при обработке сообщения:', error);
        }
    });

    socket.on('close', () => {
        const clientId = clients.get(socket);

        if (clientId) {
            const playerInfo = playerData.get(clientId);
            if (playerInfo) {
                savePlayerData();
                playerData.delete(clientId);
            }
        }
        broadcast(JSON.stringify({ type: 'player_disconnected', player_id: clientId }));
        clients.delete(socket);
    });
});

server.listen(PORT, () => {
    console.log(`[SERVER] Start on port ${PORT}`);
});
