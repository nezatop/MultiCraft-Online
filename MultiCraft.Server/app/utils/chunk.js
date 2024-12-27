import {WorldGenerator,worldGeneratorConfig} from './WorldGenerator.js';

export const clients = new Map();
export const chunkMap = new Map();
export const waterChunkMap = new Map();
export const floraChunkMap = new Map();

const worldGenerator = new WorldGenerator(worldGeneratorConfig, 1478964);

export function getChunkIndex(position) {
    const x = position.x % 16;
    const y = position.y % 256;
    const z = position.z % 16;
    return x + y * 16 * 16 + z * 16;
}

export function getRandomSurfacePosition() {
    const chunkWidth = 16;
    const chunkDepth = 16;

    // Генерируем случайные координаты чанка
    const randomX = Math.floor(Math.random() * chunkWidth);
    const randomZ = Math.floor(Math.random() * chunkDepth);

    // Определяем ключ чанка по координатам
    const chunkKey = `${Math.floor(randomX / chunkWidth)},${Math.floor(randomZ / chunkDepth)}`;

    let chunk;

    // Проверяем, существует ли чанк
    if (chunkMap.has(chunkKey)) {
        chunk = chunkMap.get(chunkKey);
    } else {
        // Если чанка нет, создаем новый чанк
        chunk = createChunk(Math.floor(randomX / chunkWidth),0,Math.floor(randomZ / chunkDepth)); // Ваша функция создания чанка
        chunkMap.set(chunkKey, chunk); // Сохраняем новый чанк в карте
    }

    // Получаем поверхность по случайным координатам в чанке
    const surfaceY = findSurfaceHeight(chunk, randomX % chunkWidth, randomZ % chunkDepth);

    return { x: randomX, y: surfaceY + 2, z: randomZ }; // Возвращаем поверхность
}

// Функция для создания чанка
export function createChunk(offsetX, offsetY, offsetZ) {
    return worldGenerator.generate(offsetX, offsetZ);
}
export function createWaterChunk(offsetX, offsetY, offsetZ) {
    return worldGenerator.generate(offsetX, offsetZ);
}
export function createFloraChunk(offsetX, offsetY, offsetZ) {
    return worldGenerator.generate(offsetX, offsetZ);
}

function findSurfaceHeight(chunk, x, z) {
    const height = 256; // Максимальная высота
    for (let y = height - 1; y >= 0; y--) { // Начинаем с самой верхней высоты и идем вниз
        const index = getChunkIndex({ x: x, y: y, z: z });
        if (chunk[index] !== 0) { // Предполагаем, что 0 - это воздух
            return y; // Возвращаем Y-координату найденной поверхности
        }
    }
    // Если не найдено, возвращаем 0 (уровень моря)
    return 0;
}
