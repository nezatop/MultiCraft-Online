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

// Обработка подключения клиентов
wss.on('connection', (socket) => {
    console.log('Клиент подключен');

    socket.on('message', (message) => {
        try {
            const data = JSON.parse(message);
            if (data.type !== 'move') console.log(data);
            handleClientMessage(data, socket);
        } catch (error) {
            console.error('Ошибка при обработке сообщения:', error);
        }
    });

    socket.on('close', () => {
        const clientId = clients.get(socket);
        console.log(`Клиент с ID ${clientId} отключен`);

        if (clientId) {
            const playerInfo = playerData.get(clientId);
            if (playerInfo) {
                console.log(`Данные игрока ${clientId}:`, playerInfo);
                playerData.delete(clientId);
                savePlayerData(); // Сохраняем данные отключенных игроков
            }
        }
        broadcast(JSON.stringify({ type: 'player_disconnected', player_id: clientId }));
        clients.delete(socket);
    });
});

// Запуск сервера
server.listen(PORT, () => {
    console.log(`Сервер запущен на порту ${PORT}`);
});
