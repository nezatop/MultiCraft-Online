const inventories = new Map();

export function getInventory(clientId) {
    return inventories.get(clientId);
}

export function setInventory(position, inventory) {
    inventories.set(position, inventory);
}

export function createInventory(position) {
    let inv = new Array(27).fill(null).map(() => ({
        type: null,
        count: 0,
        durability: 0
    }));
    inventories.set(position, inv);
}
