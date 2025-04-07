using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainManager : MonoBehaviour
{
    [SerializeField] private GameObject grassBlockPrefab;

    // Sea level: 63
    private int seaLevel = 63;
    // Max spawn height: 256
    private int maxSpawnHeight = 256;
    // Max flight altitude: 320 (320+ is inaccessible)
    // Min height (for this demo): 0

    private int maxLoadRadius = 256; // 16 Minecraft chunks (16 x 16)
    private int worldRadius = 6400;
    private float scale = 8f;
    private float offset;

    public GameObject cubePrefab;
    public int columnsSpawned = 0;

    public GameObject tuft;
    public GameObject tree;


    void Start()
    {
        // Render all initial chunks before the game starts

        // Random world seed by default
        //int worldSeed = Mathf.RoundToInt(System.DateTime.Now);
        System.DateTime t = System.DateTime.UtcNow;
        string seed = t.Hour.ToString() + t.Minute.ToString() +
                         t.Second.ToString() + t.Millisecond.ToString();

        offset = int.Parse(seed) / 10000; // Setting the offset equal to the seed is too large
        print("Seed: " + seed);

        StartCoroutine(LoadQuadrant(1));
        StartCoroutine(LoadQuadrant(2));
        StartCoroutine(LoadQuadrant(3));
        StartCoroutine(LoadQuadrant(4));

        /*
        for (float x = 0.0f; x <= maxLoadRadius; x++)
        {
            float xCoord = (float)x / worldRadius * scale + offset;

            for (float z = 0.0f; z <= maxLoadRadius - x; z++)
            {
                float zCoord = (float)z / worldRadius * scale + offset;
                float y = Mathf.RoundToInt(Mathf.PerlinNoise(xCoord, zCoord) * maxSpawnHeight);
                print(xCoord + " " + zCoord + ", " + Mathf.PerlinNoise(xCoord, zCoord) + ", " + Mathf.PerlinNoise(x, z));

                StartCoroutine(SpawnColumn(x, y, z));
            }
        }
        */

        /*
        for (float x = -1.0f; x >= -maxLoadRadius; x--)
        {
            float xCoord = seed + (x / maxLoadRadius * 2 * scale);

            for (float z = 0.0f; z >= -maxLoadRadius - x; z--)
            {
                float zCoord = seed + (z / maxLoadRadius * 2 * scale);
                float y = Mathf.RoundToInt(Mathf.PerlinNoise(xCoord, zCoord) * maxSpawnHeight);
                StartCoroutine(SpawnColumn(x, y, z));
            }
        }
        */

        // Drop the player on top of the origin point
        // Player.Instance.gameObject.GetComponent<Rigidbody>().useGravity = true;
    }

    void Update()
    {
        // Render/destroy chunks as they enter/leave player's range
        // print(columnsSpawned);
    }


    private IEnumerator LoadQuadrant(int quadrant)
    {
        switch (quadrant)
        {
            case 1:

                for (int x = 0; x <= maxLoadRadius; x++)
                {
                    float xCoord = CalculateCoordinate(x);

                    for (int z = 0; z <= maxLoadRadius - x; z++)
                    {
                        float zCoord = CalculateCoordinate(z);
                        float y = CalculateHeight(xCoord, zCoord);
                        // print(xCoord + " " + zCoord + ", " + Mathf.PerlinNoise(xCoord, zCoord) + ", " + Mathf.PerlinNoise(x, z));

                        SpawnCube(x, y, z);
                        //yield return null;
                    }
                }

                break;

            case 2:

                for (int x = 0; x <= maxLoadRadius; x++)
                {
                    float xCoord = CalculateCoordinate(x);

                    for (int z = -1; z >= -maxLoadRadius + x; z--)
                    {
                        float zCoord = CalculateCoordinate(z);
                        float y = CalculateHeight(xCoord, zCoord);
                        // print(xCoord + " " + zCoord + ", " + Mathf.PerlinNoise(xCoord, zCoord) + ", " + Mathf.PerlinNoise(x, z));

                        SpawnCube(x, y, z);
                        //yield return null;
                    }
                }

                break;

            case 3:

                for (int x = -1; x >= -maxLoadRadius; x--)
                {
                    float xCoord = CalculateCoordinate(x);

                    for (int z = 0; z >= -maxLoadRadius - x; z--)
                    {
                        float zCoord = CalculateCoordinate(z);
                        float y = CalculateHeight(xCoord, zCoord);
                        // print(xCoord + " " + zCoord + ", " + Mathf.PerlinNoise(xCoord, zCoord) + ", " + Mathf.PerlinNoise(x, z));

                        SpawnCube(x, y, z);
                        //yield return null;
                    }
                }

                break;

            case 4:

                for (int x = -1; x >= -maxLoadRadius; x--)
                {
                    float xCoord = CalculateCoordinate(x);

                    for (int z = 1; z <= maxLoadRadius + x; z++)
                    {
                        float zCoord = CalculateCoordinate(z);
                        float y = CalculateHeight(xCoord, zCoord);
                        // print(xCoord + " " + zCoord + ", " + Mathf.PerlinNoise(xCoord, zCoord) + ", " + Mathf.PerlinNoise(x, z));

                        SpawnCube(x, y, z);
                        //yield return null;
                    }
                }

                break;

            default:
                break;
        }

        yield return null;
    }

    private float CalculateCoordinate(int c)
    {
        float coord = (float)c / worldRadius * scale + offset;
        return coord;
    }


    private IEnumerator SpawnColumn(float x, float yMax, float z)
    {
        
        for (float y = 0.0f; y <= yMax; y++)
        {
            Vector3 pos = new Vector3(x, y, z);
            GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity) as GameObject;

            float colorVal = pos.y / maxSpawnHeight;

            Color color = new Color(colorVal, colorVal, colorVal);
            cube.GetComponent<Renderer>().material.color = color;

            yield return null;
        }
        
        /*
        Vector3 pos = new Vector3(x, yMax, z);
        GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity) as GameObject;
        float colorVal = pos.y / maxSpawnHeight;
        Color color = new Color(colorVal, colorVal, colorVal);
        cube.GetComponent<Renderer>().material.color = color;
        yield return null;
        */
        columnsSpawned++;
    }

    private void SpawnCube(float x, float y, float z)
    {
        // Spawn cube at position (x, y, z)
        Vector3 pos = new Vector3(x, y, z);

        //if (pos.y >= seaLevel)
        if (false)
        {
            Instantiate(grassBlockPrefab, pos, Quaternion.identity);

            Vector3 vegPos = pos;
            vegPos.y++;

            // float vegLotto = (Mathf.Abs(pos.x) + (3 * pos.y) + Mathf.Abs(pos.z)) % 101;

            float xCoord = (float)pos.x / worldRadius * scale * 300 + offset;
            float zCoord = (float)pos.z / worldRadius * scale * 300 + offset;
            float vegLotto1 = Mathf.PerlinNoise(xCoord, zCoord);
            float vegLotto2 = Mathf.PerlinNoise(zCoord, xCoord);

            /*
            if (vegLotto < 0.1)
                Instantiate(tree, vegPos, Quaternion.identity);
            else if (vegLotto > 0.1 && vegLotto < 0.5)
                Instantiate(tuft, vegPos, Quaternion.identity);
            */

            if (vegLotto1 < 0.35 && vegLotto2 < 0.35 && ((pos.x + pos.z) % 2 == 0))
                Instantiate(tuft, vegPos, Quaternion.identity);
            else if (vegLotto1 > 0.8 && vegLotto2 > 0.8 && ((pos.x + pos.z) % 2 == 0))
                Instantiate(tree, vegPos, Quaternion.identity);
        }

        // Set cube grayscale color
        // Cubes get lighter the higher their elevation
        // Cubes below sea level get colored blue

        else
        {
            pos.y += 0.5f;
            GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity) as GameObject;

            float colorVal = pos.y / maxSpawnHeight;
            Color color = new Color(colorVal, colorVal, colorVal);

            if (pos.y < seaLevel)
                color.b = 1;

            cube.GetComponent<Renderer>().material.color = color;
        }

        /*
        GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity) as GameObject;

        float colorVal = pos.y / maxSpawnHeight;
        Color color = new Color(colorVal, colorVal, colorVal);

        if (pos.y < seaLevel)
            color.b = 1;

        cube.GetComponent<Renderer>().material.color = color;
        */
    }

    private float CalculateHeight(float x, float z)
    {
        // float y = Mathf.RoundToInt(Mathf.PerlinNoise(x, z) * maxSpawnHeight);

        int numOctaves = 6;
        float multiplier;
        float y = 0.0f;
        float yMax = 0.0f;

        for (int i = 0; i < numOctaves; i++)
        {
            multiplier = Mathf.Pow(2, i);
            y += Mathf.PerlinNoise(x * multiplier, z * multiplier) / multiplier;
            yMax += 1 / multiplier;
        }

        y = Mathf.RoundToInt(y / yMax * maxSpawnHeight);
        return (y);
    }
}
