/*******************************************************************************
 * Author: Kendal Hasek
 * Date: 05/09/2024
 * Description: TerrainSpawner.cs procedurally generates the game world
*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainSpawner : MonoBehaviour
{
    public static TerrainSpawner Instance;

    // World data --------------------------------------------------------------
    private int seed;
    private int seaLevel = 63;
    private int snowLine = 168;
    private int maxSpawnHeight = 256;
    private int maxLoadRadius = 128; // 8 Minecraft chunks (16 x 16)
    private int worldRadius = 6400;

    // Used for noise calculations ---------------------------------------------
    private int heightOctaves = 6;
    private float heightFreq = 12f;
    private float heightOffset;

    private int tempOctaves = 4;
    private float tempFreq = 12f;
    private float tempOffset;

    private int humidOctaves = 4;
    private float humidFreq = 12f;
    private float humidOffset;

    private int vegOctaves = 4; // 1-4
    private float vegFreq = 300000; // 3000
    private float vegOffset;

    // Biome data --------------------------------------------------------------
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private GameObject sandPrefab;
    [SerializeField] private GameObject dirtPrefab;
    [SerializeField] private GameObject grassBlockPrefab;
    [SerializeField] private GameObject grassTuftPrefab;
    [SerializeField] private GameObject cactusPrefab;
    [SerializeField] private GameObject oakWoodPrefab;
    [SerializeField] private GameObject oakTreePrefab;
    [SerializeField] private GameObject birchWoodPrefab;
    [SerializeField] private GameObject birchTreePrefab;
    [SerializeField] private GameObject acaciaWoodPrefab;
    [SerializeField] private GameObject acaciaTreePrefab;
    [SerializeField] private GameObject pineWoodPrefab;
    [SerializeField] private GameObject pineTreePrefab;
    [SerializeField] private GameObject snowPineTreePrefab;

    [SerializeField] private Color desertGrass;
    [SerializeField] private Color savannahGrass;
    [SerializeField] private Color mildGrass;
    [SerializeField] private Color taigaGrass;
    [SerializeField] private Color snowGrass;

    private float minTempMild = 0.35f;
    private float maxTempMild = 0.65f;


    // Unity functions ---------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this.gameObject);
    }

    // -------------------------------------------------------------------------

    private void Start()
    {
        int seed = int.Parse(GameManager.worldSeed);
        heightOffset = seed / 10000;
        tempOffset = seed / 10000 + 0.1234f;
        humidOffset = seed / 10000 + 0.3456f;
        vegOffset = seed / 10000 + 0.5678f;

        LoadWorld();
        Player.Instance.GetComponent<Rigidbody>().useGravity = true;
    }


    // Custom functions --------------------------------------------------------

    /// <summary>
    /// Procedurally generates a static patch of terrain centered on the origin
    /// </summary>
    public void LoadWorld()
    {
        // Load quadrant 1
        for (int x = 0; x <= maxLoadRadius; x++)
        {
            for (int z = 0; z <= maxLoadRadius - x; z++)
                SpawnTerrain(x, z);
        }

        // Load quadrant 2
        for (int x = 0; x <= maxLoadRadius; x++)
        {
            for (int z = -1; z >= -maxLoadRadius + x; z--)
                SpawnTerrain(x, z);
        }

        // Load quadrant 3
        for (int x = -1; x >= -maxLoadRadius; x--)
        {
            for (int z = 0; z >= -maxLoadRadius - x; z--)
                SpawnTerrain(x, z);
        }

        // Load quadrant 4
        for (int x = -1; x >= -maxLoadRadius; x--)
        {
            for (int z = 1; z <= maxLoadRadius + x; z++)
                SpawnTerrain(x, z);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Calculates height, temperature, and humidity at a given (x, z) 
    /// coordinate, then uses those values to determine what biome to spawn
    /// </summary>
    private void SpawnTerrain(int x, int z)
    {
        Vector3 spawnPos = new Vector3(x, CalculateHeight(x, z), z);

        if (spawnPos.y < seaLevel)
        {
            SpawnUnderwater(spawnPos);
            return;
        }

        float temperature = CalculateTemperature(x, z);
        float humidity = CalculateHumidity(x, z);

        if (temperature < minTempMild && humidity > 0.5 && spawnPos.y > snowLine)
            SpawnSnowy(spawnPos);
        else if (temperature < minTempMild)
            SpawnTaiga(spawnPos);
        else if (temperature < maxTempMild && humidity > 0.5)
            SpawnForest(spawnPos);
        else if (temperature < maxTempMild)
            SpawnGrassland(spawnPos);
        else if (temperature > maxTempMild && humidity > 0.5)
            SpawnSavannah(spawnPos);
        else
            SpawnDesert(spawnPos);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a y coordinate to build at
    /// </summary>
    private int CalculateHeight(int x, int z)
    {
        float xCoord = PerlinCoordinate(x, heightFreq, heightOffset);
        float zCoord = PerlinCoordinate(z, heightFreq, heightOffset);

        float y = FractalPerlin(xCoord, zCoord, heightOctaves);
        return (Mathf.RoundToInt(y * maxSpawnHeight));
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a temperature noise value between 0-1
    /// </summary>
    private float CalculateTemperature(int x, int z)
    {
        float xCoord = PerlinCoordinate(x, tempFreq, -tempOffset);
        float zCoord = PerlinCoordinate(z, tempFreq, tempOffset);

        float temperature = FractalPerlin(xCoord, zCoord, tempOctaves);
        return (temperature);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a humidity noise value between 0-1
    /// </summary>
    private float CalculateHumidity(int x, int z)
    {
        float xCoord = PerlinCoordinate(x, humidFreq, humidOffset);
        float zCoord = PerlinCoordinate(z, humidFreq, -humidOffset);

        float humidity = FractalPerlin(xCoord, zCoord, humidOctaves);
        return (humidity);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a vegetation noise value between 0-1
    /// </summary>
    private float CalculateVegetation(float x, float z)
    {
        float xCoord = PerlinCoordinate(Mathf.RoundToInt(x), vegFreq, -vegOffset);
        float zCoord = PerlinCoordinate(Mathf.RoundToInt(z), vegFreq, -vegOffset);

        float vegetation = FractalPerlin(xCoord, zCoord, vegOctaves);
        return (vegetation);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Converts an integer coordinate into a viable value for Mathf.PerlinNoise()
    /// </summary>
    private float PerlinCoordinate(int value, float frequency, float offset)
    {
        float coord = (float)value / worldRadius * frequency + offset;
        return coord;
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Uses fractal Perlin Noise to calculate a noise value between 0-1.
    /// Param "numOctaves" is the number of fractal iterations to use.
    /// </summary>
    private float FractalPerlin(float x, float z, int numOctaves)
    {
        float octMod;
        float y = 0.0f;
        float yMax = 0.0f;

        for (int i = 0; i < numOctaves; i++)
        {
            octMod = Mathf.Pow(2, i);
            y += Mathf.PerlinNoise(x * octMod, z * octMod) / octMod;
            yMax += 1 / octMod;
        }

        y /= yMax;
        return (y);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns an underwater block
    /// </summary>
    private void SpawnUnderwater(Vector3 pos)
    {
        // The first two underwater blocks should be dirt
        if (seaLevel - pos.y < 3)
            Instantiate(dirtPrefab, pos, Quaternion.identity);

        // Anything deeper is stone USE GRAYSCALE TEST CUBE??
        else
            Instantiate(stonePrefab, pos, Quaternion.identity);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a Snowy Biome block and vegetation 
    /// </summary>
    private void SpawnSnowy(Vector3 pos)
    {
        // Place grass block
        GameObject snowGrassBlock = Instantiate(grassBlockPrefab, pos, Quaternion.identity);
        snowGrassBlock.GetComponentInChildren<MeshRenderer>().material.color = snowGrass;

        // Plug holes with dirt
        pos.y -= 1;
        Instantiate(dirtPrefab, pos, Quaternion.identity);
        pos.y += 1;

        // Determine which vegetation to place, if any
        float vegScore = CalculateVegetation(pos.x, pos.z);
        float treeChance = 0.3f;

        Vector3 vegPos = pos;
        vegPos.y += 1;

        // Place snowy pine tree
        if (vegScore > (1 - treeChance) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, pineWoodPrefab);
            Instantiate(snowPineTreePrefab, vegPos, Quaternion.identity);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a Taiga Biome block and vegetation
    /// </summary>
    private void SpawnTaiga(Vector3 pos)
    {
        // Place grass block
        GameObject taigaGrassBlock = Instantiate(grassBlockPrefab, pos, Quaternion.identity);
        taigaGrassBlock.GetComponentInChildren<MeshRenderer>().material.color = taigaGrass;

        // Plug holes with dirt
        pos.y -= 1;
        Instantiate(dirtPrefab, pos, Quaternion.identity);
        pos.y += 1;

        // Determine which vegetation to place, if any
        float vegScore = CalculateVegetation(pos.x, pos.z);
        float tuftChance = 0.2f;
        float treeChance = 0.3f;

        Vector3 vegPos = pos;
        vegPos.y += 1;

        // Place grass tuft
        if (vegScore < tuftChance && (Mathf.Abs(pos.x + pos.z) % 2 == 0))
            Instantiate(grassTuftPrefab, vegPos, Quaternion.identity);

        // Place pine tree
        else if (vegScore > (1 - treeChance) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, pineWoodPrefab);
            Instantiate(pineTreePrefab, vegPos, Quaternion.identity);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a Forest Biome block and vegetation
    /// </summary>
    private void SpawnForest(Vector3 pos)
    {
        // Place grass block
        GameObject mildGrassBlock = Instantiate(grassBlockPrefab, pos, Quaternion.identity);
        mildGrassBlock.GetComponentInChildren<MeshRenderer>().material.color = mildGrass;

        // Plug holes with dirt
        pos.y -= 1;
        Instantiate(dirtPrefab, pos, Quaternion.identity);
        pos.y += 1;

        // Determine which vegetation to place, if any
        float vegScore = CalculateVegetation(pos.x, pos.z);
        float tuftChance = 0.2f;
        float treeChance = 0.35f;

        Vector3 vegPos = pos;
        vegPos.y += 1;

        // Place grass tuft
        if (vegScore < tuftChance && (Mathf.Abs(pos.x + pos.z) % 2 == 0))
            Instantiate(grassTuftPrefab, vegPos, Quaternion.identity);

        // Place birch tree
        else if (vegScore > (1 - treeChance * 0.25) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, birchWoodPrefab);
            Instantiate(birchTreePrefab, vegPos, Quaternion.identity);
        }

        // Place oak tree
        else if (vegScore > (1 - treeChance) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, oakWoodPrefab);
            Instantiate(oakTreePrefab, vegPos, Quaternion.identity);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a Grassland Biome block and vegetation
    /// </summary>
    private void SpawnGrassland(Vector3 pos)
    {
        // Place grass block
        GameObject mildGrassBlock = Instantiate(grassBlockPrefab, pos, Quaternion.identity);
        mildGrassBlock.GetComponentInChildren<MeshRenderer>().material.color = mildGrass;

        // Plug holes with dirt
        pos.y -= 1;
        Instantiate(dirtPrefab, pos, Quaternion.identity);
        pos.y += 1;

        // Determine which vegetation to place, if any
        float vegScore = CalculateVegetation(pos.x, pos.z);
        float tuftChance = 0.35f;
        float treeChance = 0.25f;

        Vector3 vegPos = pos;
        vegPos.y += 1;

        // Place grass tuft
        if (vegScore < tuftChance && (Mathf.Abs(pos.x + pos.z) % 2 == 0))
            Instantiate(grassTuftPrefab, vegPos, Quaternion.identity);

        // Place birch tree
        else if (vegScore > (1 - treeChance * 0.25) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, birchWoodPrefab);
            Instantiate(birchTreePrefab, vegPos, Quaternion.identity);
        }

        // Place oak tree
        else if (vegScore > (1 - treeChance) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, oakWoodPrefab);
            Instantiate(oakTreePrefab, vegPos, Quaternion.identity);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a Savannah Biome block and vegetation
    /// </summary>
    private void SpawnSavannah(Vector3 pos)
    {
        // Place grass block
        GameObject savannahGrassBlock = Instantiate(grassBlockPrefab, pos, Quaternion.identity);
        savannahGrassBlock.GetComponentInChildren<MeshRenderer>().material.color = savannahGrass;

        // Plug holes with dirt
        pos.y -= 1;
        Instantiate(dirtPrefab, pos, Quaternion.identity);
        pos.y += 1;

        // Determine which vegetation to place, if any
        float vegScore = CalculateVegetation(pos.x, pos.z);
        float tuftChance = 0.35f;
        float treeChance = 0.25f;

        Vector3 vegPos = pos;
        vegPos.y += 1;

        // Place grass tuft
        if (vegScore < tuftChance && (Mathf.Abs(pos.x + pos.z) % 2 == 0))
            Instantiate(grassTuftPrefab, vegPos, Quaternion.identity);

        // Place acacia tree
        else if (vegScore > (1 - treeChance) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, acaciaWoodPrefab);
            Instantiate(acaciaTreePrefab, vegPos, Quaternion.identity);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a Desert Biome block and vegetation
    /// </summary>
    private void SpawnDesert(Vector3 pos)
    {
        // Place sand block
        Instantiate(sandPrefab, pos, Quaternion.identity);

        // Plug holes with more sand
        pos.y -= 1;
        Instantiate(sandPrefab, pos, Quaternion.identity);
        pos.y += 1;

        // Determine which vegetation to place, if any
        float vegScore = CalculateVegetation(pos.x, pos.z);
        float treeChance = 0.25f;

        Vector3 vegPos = pos;
        vegPos.y += 1;

        // Place cactus
        if (vegScore > (1 - treeChance) && (Mathf.Abs(pos.x + pos.z) % 2 == 1))
        {
            vegPos = AddTreeTrunk(vegPos, vegScore, cactusPrefab);
            Instantiate(cactusPrefab, vegPos, Quaternion.identity);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds an extra 0-2 wood blocks beneath a tree, in order to vary height.
    /// Returns the position at which the tree should be spawned.
    /// </summary>
    private Vector3 AddTreeTrunk(Vector3 pos, float vegScore, GameObject woodPrefab)
    {
        int i = 0;
        while (i < (vegScore * 100) % 3)
        {
            Instantiate(woodPrefab, pos, Quaternion.identity);
            pos.y++;
            i++;
        }

        return (pos);
    }
}
