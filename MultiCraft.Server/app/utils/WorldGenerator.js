import FastNoiseLite from 'fastnoise-lite';

export const worldGeneratorConfig = {
    BaseHeight: 64,
    WaterLevel: 63,
    RiverChance: -0.5,
    TreeFrequency: 0.5,
    Biomes: [
        {
            biomeName: "Desert",
            offset: 0,
            scale: 0,
            terrainHeight: 0,
            terrainScale: 0,
            surfaceBlock: 9,
            subSurfaceBlock: 9,
            treeType: [
                23
            ],
            leavesType: [
                -1
            ],
            maxTreeHeight: [
                3
            ],
            minTreeHeight: [
                1
            ],
            treeChance: [
                60
            ],
            biomeChance: 0.160999998,
            fullFloraChance: 0,
            floraIndex: 0,
            floraZoneScale: 0,
            floraThreshold: 0.119999997,
            floraPlacementScale: 0,
            floraPlacementThreshold: 0.100000001,
            placeFlora: false,
            floraId: [],
            floraChances: [],
            HeightMax: 0,
            HeightMin: 0,
            lodes: [],
            chances: 0
        },
        {
            biomeName: "Forest",
            offset: 0,
            scale: 0,
            terrainHeight: 0,
            terrainScale: 0,
            surfaceBlock: 4,
            subSurfaceBlock: 3,
            treeType: [
                5,
                98
            ],
            leavesType: [
                7,
                7
            ],
            maxTreeHeight: [
                7,
                7
            ],
            minTreeHeight: [
                5,
                5
            ],
            treeChance: [
                100,
                100
            ],
            biomeChance: 1,
            fullFloraChance: 100,
            floraIndex: 0,
            floraZoneScale: 0,
            floraThreshold: 0.100000001,
            floraPlacementScale: 0,
            floraPlacementThreshold: 0.100000001,
            placeFlora: true,
            floraId: [
                27,
                28,
                29
            ],
            floraChances: [
                80,
                10,
                10
            ],
            HeightMax: 0,
            HeightMin: 0,
            lodes: [],
            chances: 100
        },
        {
            biomeName: "ForestSpruce",
            offset: 0,
            scale: 0,
            terrainHeight: 0,
            terrainScale: 0,
            surfaceBlock: 1204,
            subSurfaceBlock: 3,
            treeType: [
                99
            ],
            leavesType: [
                578
            ],
            maxTreeHeight: [
                9
            ],
            minTreeHeight: [
                7
            ],
            treeChance: [
                75
            ],
            biomeChance: 0.226999998,
            fullFloraChance: 1,
            floraIndex: 0,
            floraZoneScale: 0,
            floraThreshold: 0.100000001,
            floraPlacementScale: 0,
            floraPlacementThreshold: 0.100000001,
            placeFlora: true,
            floraId: [
                556
            ],
            floraChances: [
                100
            ],
            HeightMax: 0,
            HeightMin: 0,
            lodes: [],
            chances: 0
        }
    ],
    BiomeNoiseOctaves: {
        NoiseType: "Cellular",
        Frequency: 0.00700000022,
        Amplitude: 2,
        FractalType: "DomainWarpProgressive",
        FractalOctaves: 6,
        FractalGain: 2.75,
        CellularDistanceFunction: "Euclidean",
        CellularReturnType: "CellValue",
        CellularJitter: 1,
        DomainWarpType: "OpenSimplex2Reduced",
        DomainWarpAmplitude: 4
    },
    FloraNoiseOctaves: {
        NoiseType: "Perlin",
        Frequency: 4.55999994,
        Amplitude: 1,
        FractalType: 0,
        FractalOctaves: 0,
        FractalGain: 0,
        CellularDistanceFunction: "Euclidean",
        CellularReturnType: "CellValue",
        CellularJitter: 0,
        DomainWarpType: "OpenSimplex2",
        DomainWarpAmplitude: 0
    },
    TreeNoiseOctaves: [
        {
            NoiseType: "Perlin",
            Frequency: 1.79999995,
            Amplitude: 2,
            FractalType: 0,
            FractalOctaves: 0,
            FractalGain: 0,
            CellularDistanceFunction: "Euclidean",
            CellularReturnType: "CellValue",
            CellularJitter: 0,
            DomainWarpType: "OpenSimplex2",
            DomainWarpAmplitude: 0
        }
    ],
    WaterNoiseOctaves: {
        NoiseType: "Cellular",
        Frequency: 0.00300000003,
        Amplitude: 4,
        FractalType: "Ridged",
        FractalOctaves: 1,
        FractalGain: 0.5,
        CellularDistanceFunction: "EuclideanSq",
        CellularReturnType: "Distance2Div",
        CellularJitter: 1,
        DomainWarpType: "OpenSimplex2",
        DomainWarpAmplitude: 12
    },
    SurfaceNoiseOctaves: [
        {
            NoiseType: "OpenSimplex2S",
            Frequency: 0.0120000001,
            Amplitude: 4,
            FractalType: 0,
            FractalOctaves: 0,
            FractalGain: 0,
            CellularDistanceFunction: "Euclidean",
            CellularReturnType: "CellValue",
            CellularJitter: 0,
            DomainWarpType: "OpenSimplex2",
            DomainWarpAmplitude: 0
        },
        {
            NoiseType: "OpenSimplex2S",
            Frequency: 0.0199999996,
            Amplitude: 12,
            FractalType: 0,
            FractalOctaves: 0,
            FractalGain: 0,
            CellularDistanceFunction: "Euclidean",
            CellularReturnType: "CellValue",
            CellularJitter: 0,
            DomainWarpType: "OpenSimplex2",
            DomainWarpAmplitude: 0
        },
        {
            NoiseType: "Perlin",
            Frequency: 1,
            Amplitude: 2,
            FractalType: 0,
            FractalOctaves: 0,
            FractalGain: 0,
            CellularDistanceFunction: "Euclidean",
            CellularReturnType: "CellValue",
            CellularJitter: 0,
            DomainWarpType: "OpenSimplex2",
            DomainWarpAmplitude: 0
        }
    ]
}
export class WorldGenerator {
    constructor(config, seed) {
        this.config = config;
        this.biomes = this.initializeBiomes();
        this.surfaceNoise = this.initializeNoiseGenerators(config.SurfaceNoiseOctaves, seed);
        this.biomeNoise = this.initializeNoiseGenerator(config.BiomeNoiseOctaves, seed);
        this.waterNoise = this.initializeNoiseGenerator(config.WaterNoiseOctaves, seed);
        this.floraNoise = this.initializeNoiseGenerator(config.FloraNoiseOctaves, seed);
        this.treeNoise = this.initializeNoiseGenerators(config.TreeNoiseOctaves, seed);
    }

    initializeBiomes() {
        const totalChance = this.config.Biomes.reduce((sum, biome) => sum + biome.biomeChance, 0);
        return this.config.Biomes.map(biome => ({
            ...biome,
            biomeChance: biome.biomeChance / totalChance // Normalize chances
        }));
    }

    initializeNoiseGenerator(NoiseOctave, seed) {
        const noise = new FastNoiseLite();
        noise.SetSeed(seed);
        noise.SetNoiseType(NoiseOctave.NoiseType);
        noise.SetFrequency(NoiseOctave.Frequency);
        noise.SetFractalType(NoiseOctave.FractalType);
        noise.SetFractalOctaves(NoiseOctave.FractalOctaves);
        noise.SetFractalGain(NoiseOctave.FractalGain);
        noise.SetCellularDistanceFunction(NoiseOctave.CellularDistanceFunction);
        noise.SetCellularReturnType(NoiseOctave.CellularReturnType);
        noise.SetCellularJitter(NoiseOctave.CellularJitter);
        noise.SetDomainWarpType(NoiseOctave.DomainWarpType);
        noise.SetDomainWarpAmp(NoiseOctave.DomainWarpAmplitude);
        return noise;
    }

    initializeNoiseGenerators(NoiseOctaves, seed) {
        return NoiseOctaves.map(octave => this.initializeNoiseGenerator(octave, seed));
    }

    generate(xOffset, zOffset) {
        const dimensions = { width: 16, height: 256, depth: 16 };
        const blocks = new Array(dimensions.width * dimensions.height * dimensions.depth).fill(0);
        const surfaceHeights = this.generateSurface(blocks, xOffset, zOffset);
        this.generateTrees(blocks, surfaceHeights, xOffset, zOffset);
        return blocks;
    }

    generateSurface(blocks, xOffset, zOffset) {
        const surfaceHeights = new Array(16 * 16).fill(0);

        for (let x = 0; x < 16; x++) {
            for (let z = 0; z < 16; z++) {
                const biome = this.getBiome(x + xOffset, z + zOffset);
                const waterChance = this.waterNoise.GetNoise(x + xOffset, z + zOffset);
                const height = Math.floor(this.getHeight(x + xOffset, z + zOffset) -
                    (waterChance > this.config.RiverChance ? waterChance * this.config.WaterNoiseOctaves.Amplitude : 0));

                this.fillColumn(blocks, x, z, height, biome, waterChance);
                surfaceHeights[x + z * 16] = height;
            }
        }

        return surfaceHeights;
    }

    fillColumn(blocks, x, z, height, biome, waterChance) {
        for (let y = 0; y < Math.min(height, 256); y++) {
            const blockType = this.getBlockType(y, height, biome, waterChance);
            if (blockType !== null) blocks[this.index3DTo1D(x, y, z)] = blockType;
        }
    }

    getBlockType(y, height, biome, waterChance) {
        if (y < 1) return 1; // Bedrock
        if (y < height - 3) return 2; // Stone
        if (y >= height - 3 && y < height - 1) {
            return waterChance > this.config.RiverChance && height <= this.config.WaterLevel ? 3 : biome.subSurfaceBlock;
        }
        if (y >= height - 1 && y <= height) {
            return waterChance > this.config.RiverChance && height <= this.config.WaterLevel ? 3 : biome.surfaceBlock;
        }
        return null;
    }

    generateTrees(blocks, surfaceHeights, xOffset, zOffset) {
        for (let x = 0; x < 16; x++) {
            for (let z = 0; z < 16; z++) {
                const height = surfaceHeights[x + z * 16];
                const treeNoiseValue = this.GetTreeNoise(x + xOffset, z + zOffset);

                if (treeNoiseValue > this.config.TreeFrequency && height > this.config.BaseHeight - 1) {
                    const biome = this.getBiome(x + xOffset, z + zOffset);
                    this.placeTree(blocks, x, z, height, biome);
                }
            }
        }
    }

    placeTree(blocks, x, z, height, biome) {
        if (biome.treeType.length === 0) return;

        const treeIndex = Math.floor(Math.random() * biome.treeType.length);
        const treeType = biome.treeType[treeIndex];
        const leavesType = biome.leavesType[treeIndex];
        const treeHeight = this.getRandomInRange(
            biome.minTreeHeight[treeIndex],
            biome.maxTreeHeight[treeIndex]
        );

        if (Math.random() * 100 > biome.treeChance[treeIndex]) return;

        this.generateTreeBlocks(blocks, x, z, height, treeHeight, treeType, leavesType);
    }

    generateTreeBlocks(blocks, x, z, height, treeHeight, treeType, leavesType) {
        // Generate foliage
        for (let y = height + treeHeight - 1; y >= height + treeHeight / 2; y--) {
            const foliageRadius = Math.max(1, Math.min(3, treeHeight - (height + treeHeight - y)));
            this.generateFoliageLayer(blocks, x, z, y, foliageRadius, leavesType);
        }

        // Generate trunk
        for (let y = 0; y < treeHeight; y++) {
            blocks[this.index3DTo1D(x, height + y, z)] = treeType;
        }
    }

    generateFoliageLayer(blocks, x, z, y, radius, leavesType) {
        for (let dx = -radius; dx <= radius; dx++) {
            for (let dz = -radius; dz <= radius; dz++) {
                if (dx * dx + dz * dz <= radius * radius) {
                    blocks[this.index3DTo1D(x + dx, y, z + dz)] = leavesType;
                }
            }
        }
    }

    getBiome(x, z) {
        const biomeNoiseValue = (this.GetBiomeNoise(x, z) + 1) / 2;
        let accumulatedChance = 0;

        for (const biome of this.biomes) {
            accumulatedChance += biome.biomeChance;
            if (biomeNoiseValue <= accumulatedChance) return biome;
        }

        return this.biomes[this.biomes.length - 1];
    }

    getHeight(x, z) {
        return this.config.BaseHeight + this.surfaceNoise.reduce((height, noise, i) =>
            height + noise.GetNoise(x, z) * this.config.SurfaceNoiseOctaves[i].Amplitude / 2, 0);
    }

    GetTreeNoise(x, z) {
        return this.treeNoise.reduce((total, noise, i) =>
            total + noise.GetNoise(x, z) * this.config.TreeNoiseOctaves[i].Amplitude / 2, 0);
    }

    GetBiomeNoise(x, z) {
        return this.biomeNoise.GetNoise(x, z);
    }

    index3DTo1D(x, y, z) {
        return x + z * 16 + y * 256;
    }

    getRandomInRange(min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }
}
