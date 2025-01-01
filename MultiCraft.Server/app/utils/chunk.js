import {WorldGenerator,worldGeneratorConfig} from './WorldGenerator.js';

export const clients = new Map();
export const chunkMap = new Map();
export const waterChunkMap = new Map();
export const floraChunkMap = new Map();

export const entities = new Map();

const worldGenerator = new WorldGenerator(worldGeneratorConfig, 7898465);

export function getChunkIndex(position) {
    const x = position.x % 16;
    const y = position.y % 256;
    const z = position.z % 16;
    return x + y * 16 * 16 + z * 16;
}

export function getRandomSurfacePosition() {
    const chunkWidth = 16;
    const chunkDepth = 16;

    const randomX = Math.floor(Math.random() * chunkWidth);
    const randomZ = Math.floor(Math.random() * chunkDepth);

    const chunkKey = `${Math.floor(randomX / chunkWidth)},${Math.floor(randomZ / chunkDepth)}`;

    let chunk;

    if (chunkMap.has(chunkKey)) {
        chunk = chunkMap.get(chunkKey);
    } else {
        chunk = createChunk(Math.floor(randomX / chunkWidth),0,Math.floor(randomZ / chunkDepth)); // Ваша функция создания чанка
        chunkMap.set(chunkKey, chunk); // Сохраняем новый чанк в карте
    }

    const surfaceY = findSurfaceHeight(chunk, randomX % chunkWidth, randomZ % chunkDepth);

    return { x: randomX, y: surfaceY + 2, z: randomZ }; // Возвращаем поверхность
}

export function createChunk(offsetX, offsetY, offsetZ) {
    if(Math.random() < 0.1){
        if(Math.random() < 0.5){
        }
    }
    return worldGenerator.generate(offsetX, offsetZ);
}
export function createWaterChunk(offsetX, offsetY, offsetZ) {
    return worldGenerator.generateWater(offsetX, offsetZ);
}
export function createFloraChunk(offsetX, offsetY, offsetZ) {
    return worldGenerator.generateFlora(offsetX, offsetZ);
}

function findSurfaceHeight(chunk, x, z) {
    const height = 256; // Максимальная высота
    for (let y = height - 1; y >= 0; y--) { // Начинаем с самой верхней высоты и идем вниз
        const index = getChunkIndex({ x: x, y: y, z: z });
        if (chunk[index] !== 0) { // Предполагаем, что 0 - это воздух
            return y; // Возвращаем Y-координату найденной поверхности
        }
    }
    return 0;
}
