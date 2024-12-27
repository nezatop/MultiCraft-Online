import { playerData } from '../utils/storage.js';
import {
    clients,
    chunkMap,
    floraChunkMap,
    waterChunkMap,
    createChunk,
    getChunkIndex,
    getRandomSurfacePosition,
    createWaterChunk, createFloraChunk
} from '../utils/chunk.js';
import { PlayerData } from "../models/player.js";
import {createInventory, getInventory, setInventory} from "../utils/inventory.js";
import e from "express";

export function handleClientMessage(data, socket) {
    switch (data.type) {
        case 'connect':
            handleConnect(data, socket);
            break;
        case 'get_chunk':
            handleGetChunk(data.position, socket);
            break;
        case 'move':
            handleMove(data.position, socket);
            break;
        case 'get_players':
            handlePlayers(socket);
            break;
        case 'place_block':
            handlePlaceBlock(data.position, data.block_type, socket);
            break;
        case 'destroy_block':
            handleDestroyBlock(data.position, socket);
            break;
        case 'get_inventory':
            handleGetInventory(data.position, socket);
            break;
        case 'set_inventory':
            handleSetInventory(data.position, data.inventory, socket);
            break;
        default:
            console.log('Неизвестный тип сообщения:', data.type);
    }
}

function handleGetInventory(position, socket) {
    const clientId = clients.get(socket);
    if (clientId) {
        const inventory = getInventory(position);
        socket.send(JSON.stringify({ type: 'inventory', inventory: inventory }));
    }
}

function handleSetInventory(position, inventory, socket) {
    setInventory(position, inventory);
}

function handleConnect(data, socket) {
    const { login, password } = data;

    if (playerData.has(login)) {
        const playerInfo = playerData.get(login);
        console.log(`Клиент с логином ${login} повторно подключен.`);
        socket.send(JSON.stringify({
            type: 'connected',
            position: playerInfo.position,
            inventory: playerInfo.inventory
        }));
    } else {
        console.log(`Новый клиент с логином ${login} подключен.`);
        const startPosition = getRandomSurfacePosition();
        playerData.set(login, new PlayerData(login, password, startPosition, { x: 0, y: 0, z: 0 }, []));

        socket.send(JSON.stringify({
            type: 'connected',
            position: startPosition,
            rotation: { x: 0, y: 0, z: 0 },
            inventory: null,
        }));
    }

    clients.set(socket, login);
    broadcast(JSON.stringify({ type: 'player_connected', player_id: login, position: playerData.get(login).position }));
}

function handlePlayers(socket) {
    console.log('Подключенные игроки:');
    const playersArray = Array.from(playerData.values()).map(data => ({
        player_id: data.login,
        position: data.position
    }));

    console.log(JSON.stringify(playersArray, null, 2));

    socket.send(JSON.stringify({
        type: 'players_list',
        players: playersArray
    }));
}

function handleGetChunk(position, socket) {
    const chunkKey = `${position.x},${position.y},${position.z}`;
    let chunk;
    let waterChunk;
    let floraChunk;
    if(chunkMap.has(chunkKey))
        chunk = chunkMap.get(chunkKey);
    else {
        chunk = createChunk(position.x * 16, position.y * 256, position.z * 16);
        chunkMap.set(chunkKey,chunk);
    }

    if(waterChunkMap.has(chunkKey))
        waterChunk = chunkMap.get(chunkKey);
    else {
        waterChunk = createWaterChunk(position.x * 16, position.y * 256, position.z * 16);
        waterChunkMap.set(chunkKey,waterChunk);
    }

    if(floraChunkMap.has(chunkKey))
        floraChunk = chunkMap.get(chunkKey);
    else {
        floraChunk = createFloraChunk(position.x * 16, position.y * 256, position.z * 16);
        floraChunkMap.set(chunkKey,floraChunk);
    }

    socket.send(JSON.stringify({
        type: 'chunk_data',
        position: position,
        blocks: chunk,
        waterChunk: waterChunk,
        floraChunk: floraChunk,
    }));
}

function handleMove(position, socket) {
    const clientId = clients.get(socket);
    if (clientId && playerData.has(clientId)) {
        playerData.get(clientId).position = position;
    }
    broadcast(JSON.stringify({ type: 'player_moved', player_id: clientId, position: position }));
}

function updateBlock(position, blockType, socket) {
    const chunkPosition = getChunkContainingBlock(position);
    const chunkKey = `${chunkPosition.x},${chunkPosition.y},${chunkPosition.z}`;
    if (chunkMap.has(chunkKey)) {
        const chunk = chunkMap.get(chunkKey);
        const chunkOrigin = {
            x: chunkPosition.x * 16,
            y: chunkPosition.y * 16,
            z: chunkPosition.z * 16
        };

        const indexV3 = {
            x: position.x - chunkOrigin.x,
            y: position.y - chunkOrigin.y,
            z: position.z - chunkOrigin.z,
        };

        const index = indexV3.x + indexV3.z * 16 + indexV3.y * 16 * 16;
        chunk[index] = blockType;

        if(blockType === 1){
            createInventory(position);
        }

        chunkMap.set(chunkKey, chunk);

        broadcast(JSON.stringify({ type: 'block_update', position: position, block_type: blockType }));
    }
}

function handlePlaceBlock(position, blockType, socket) {
    updateBlock(position, blockType, socket);
}

function handleDestroyBlock(position, socket) {
    updateBlock(position, 0, socket); // 0 для удаления блока
}

function getChunkContainingBlock(blockWorldPosition) {
    let chunkPosition = {
        x: Math.trunc(blockWorldPosition.x / 16),
        y: Math.trunc(blockWorldPosition.y / 256),
        z: Math.trunc(blockWorldPosition.z / 16)
    };

    if (blockWorldPosition.x < 0) {
        if (blockWorldPosition.x % 16 !== 0) {
            chunkPosition.x--;
        }
    }
    if (blockWorldPosition.z < 0) {
        if (blockWorldPosition.z % 16 !== 0) {
            chunkPosition.z--;
        }
    }

    return chunkPosition;
}

export function broadcast(data) {
    clients.forEach((_, client) => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(data);
        }
    });
}
