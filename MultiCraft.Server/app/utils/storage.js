import fs from 'fs';
import { JSON_FILE_PATH } from '../config.js';
import { PlayerData } from '../models/player.js';

export const playerData = new Map();

export function loadPlayerData() {
    if (fs.existsSync(JSON_FILE_PATH)) {
        const data = fs.readFileSync(JSON_FILE_PATH);
        const players = JSON.parse(data);
        players.forEach(player => {
            playerData.set(player.login, new PlayerData(
                player.login,
                player.password,
                player.position,
                player.rotation,
                player.inventory
            ));
        });
    } else {}
}

export function savePlayerData() {
    const playersArray = Array.from(playerData.values()).map(data => ({
        login: data.login,
        password: data.password,
        position: data.position,
        rotation: data.rotation,
        inventory: data.inventory
    }));
    fs.writeFileSync(JSON_FILE_PATH, JSON.stringify(playersArray, null, 2));
}
