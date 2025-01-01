const inventories = new Map();

export function getInventory(position) {
    return inventories.get(JSON.stringify(position));
}

export function setInventory(position, inventory) {
    inventories.set(JSON.stringify(position), JSON.parse(inventory));
}

export function addInventory(position) {
    inventories.set(JSON.stringify(position), createInventory());
   }

export function createInventory() {
    return new Array(9 * 4).fill(null).map(() => ({
        type: "null",
        count: 0,
        durability: 0
    }));
}
