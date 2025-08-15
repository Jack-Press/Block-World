using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    [Header("Biomes")]
    public float biomeFreq;
    public biomeClass[] biomeClasses;
    public Gradient biomeGradiant;
    public Texture2D biomeMap;

    [Header("Terrain Generation")]
    public int chunkSize;
    public int seaLevel;
    public int dirtLayer = 5;
    public bool genCaves = true;
    public BlockClass[] blockClasses;
    private List<string> TCbyName;

    [Header("Seed Map")]
    public float seed;
    public int worldSize;

    private GameObject[] worldChunks;
    private List<Vector2> worldBlocks = new List<Vector2>();


    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        TCbyName = new List<string>();
        foreach (BlockClass blockClass in blockClasses)
            TCbyName.Add(blockClass.name);

        DrawTextures();
        GenerateChunks();
        GenerateTerrain();
    }

    public void DrawTextures()
    {
        biomeMap = new Texture2D(worldSize, worldSize);
        DrawBiomeTexture();

        biomeClasses[0].caveNoise = new Texture2D(worldSize, worldSize);
        GenerateNoiseTexture(biomeClasses[0].caveFreq, biomeClasses[0].surfaceValue, biomeClasses[0].caveNoise);

        
        for (int b = 0; b < biomeClasses.Length; b++)
        {
            biomeClasses[b].caveNoise = new Texture2D(worldSize, worldSize);

            

            for (int i = 0; i < biomeClasses[b].ores.Length; i++)
            {
                biomeClasses[b].ores[i].noise = new Texture2D(worldSize, worldSize);
            }

            GenerateNoiseTexture(biomeClasses[b].caveFreq, biomeClasses[b].surfaceValue, biomeClasses[b].caveNoise);

            for (int i = 0; i < biomeClasses[b].ores.Length; i++)
            {
                if (biomeClasses[b].ores[i].rarity != 0)
                    GenerateNoiseTexture(biomeClasses[b].ores[i].rarity, biomeClasses[b].ores[i].size, biomeClasses[b].ores[i].noise);
                else
                    biomeClasses[b].ores[i].noise = null;
            }
        }
        
    }

    public void GenerateChunks()
    {
        int numChunks = worldSize/chunkSize;
        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject(name = i.ToString());
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public void DrawBiomeTexture()
    {
        for (int x = 0; x < biomeMap.width; x++)
        {
            float v = Mathf.PerlinNoise((x + seed) * biomeFreq, seed * biomeFreq);
            Color col = biomeGradiant.Evaluate(v);
            for (int y = 0; y < biomeMap.height; y++)
            {
                biomeMap.SetPixel(x, y, col);
            }
        }
        biomeMap.Apply();
    }

    public void GenerateTerrain()
    {
        Sprite[] blockSprite;
        for (int x = 0; x < worldSize; x++)
        {
            biomeClass curBio = GetCurBiome(biomeMap.GetPixel(x, 1));

            float height = Mathf.PerlinNoise((x + seed) * curBio.terrainFreq, seed * curBio.terrainFreq) * curBio.heightMultiplier + seaLevel;


            for (int y = 0; y < height; y++)
            {
                if (y < height - dirtLayer)
                {
                    blockSprite = blockClasses[TCbyName.IndexOf("Stone")].blockSprites;

                    for(int i = 0; i < curBio.ores.Length; i++)
                    {
                        if (curBio.ores[i].noise != null && curBio.ores[i].noise.GetPixel(x, y).r > 0.5f && height - y > curBio.ores[i].maxHeight)
                            blockSprite = curBio.ores[i].oreBlock.blockSprites;
                    }
                }

                else if (y < height - 1)
                {
                    blockSprite = blockClasses[TCbyName.IndexOf("Dirt")].blockSprites;
                }

                else
                {
                    blockSprite = blockClasses[TCbyName.IndexOf("GrassBlock")].blockSprites;
                }

                if (genCaves)
                {
                    if (curBio.caveNoise.GetPixel(x, y).r < 0.5f)
                    {
                        PlaceBlock(blockSprite, x, y);
                    }
                }

                else
                {
                    PlaceBlock(blockSprite, x, y);
                }

                if (y >= height - 1)
                {
                    int t = Random.Range(0, curBio.treeChance);
                    if (t == 1)
                    {
                        if (worldBlocks.Contains(new Vector2(x, y)))
                        {
                            if (curBio.biomeName == "Dessert")
                            {
                                GenCactus(x, y + 1);
                            }

                            else
                                GenTree(x, y + 1);
                        }
                    }
                    else if (t % curBio.addChance == 2)
                    {
                        if (worldBlocks.Contains(new Vector2(x, y)))
                        {
                            PlaceBlock(blockClasses[TCbyName.IndexOf("GrassBlock")].blockSprites, x, y + 1);
                        }
                    }
                }
            }
        }
    }

    public biomeClass GetCurBiome(Color biomecol)
    {
        biomeClass curBio = biomeClasses[0];
        for (int i = 0; i < biomeClasses.Length; i++)
        {
            if (biomeClasses[i].biomeColor == biomecol)
            {
                curBio = biomeClasses[i];
                break;
            }
        }
        return curBio;
    }

    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x+seed) * frequency, (y + seed) * frequency);
                if (v < limit)
                {
                    noiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x, y, Color.black);
                }
            }
        }

        noiseTexture.Apply();
    }

    //Structure ggeneration
    //General
    void GenTree(int x, int y)
    {
        biomeClass curBio = GetCurBiome(biomeMap.GetPixel(x, 1));
        int treeHeight = Random.Range(curBio.minTreeHeight, curBio.maxTreeHeight);
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceBlock(curBio.biomeBlocks[curBio.blocksbyName.IndexOf("Log")].blockSprites, x, y + i);
        }

        var leafBlock = curBio.biomeBlocks[curBio.blocksbyName.IndexOf("Leaf")].blockSprites;

        for (int i = 0; i < 3; i++)
        {
            PlaceBlock(leafBlock, x - 1, y + treeHeight + i - 1);
            PlaceBlock(leafBlock, x, y + treeHeight + i);
            PlaceBlock(leafBlock, x + 1, y + treeHeight + i - 1);
        }

        for (int i = 0; i < 2; i++)
        {
            PlaceBlock(leafBlock, x - 2, y + treeHeight + i - 1);
            PlaceBlock(leafBlock, x + 2, y + treeHeight + i - 1);
        }
    }

    //Desert
    void GenCactus(int x, int y)
    {
        biomeClass curBio = biomeClasses[2];
        int treeHeight = Random.Range(curBio.minTreeHeight, curBio.maxTreeHeight);
        var logBlock = curBio.biomeBlocks[curBio.blocksbyName.IndexOf("Log")].blockSprites;

        for (int i = 0; i < treeHeight; i++)
        {
            PlaceBlock(logBlock, x, y + i);
        }

        PlaceBlock(logBlock, x - 1, y + treeHeight - 3);

        for (int i = 0; i < 2; i++)
        {
            PlaceBlock(logBlock, x - 2, y + treeHeight + i - 3);
        }

        PlaceBlock(logBlock, x + 1, y + treeHeight - 2);

        for (int i = 0; i < 3; i++)
        {
            PlaceBlock(logBlock, x + 2, y + treeHeight + i - 2);
        }
    }

    public void PlaceBlock(Sprite[] blockSprite, int x, int y)
    {
        if (worldBlocks.Contains(new Vector2(x, y)))
            return;
        GameObject newBlock = new GameObject();

        int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;
        newBlock.transform.parent = worldChunks[chunkCoord].transform;

        newBlock.AddComponent<SpriteRenderer>();
        int spriteIndex = Random.Range(0, blockSprite.Length);
        newBlock.GetComponent<SpriteRenderer>().sprite = blockSprite[spriteIndex];
        newBlock.AddComponent<BoxCollider2D>();

        newBlock.name = blockSprite[spriteIndex].name;
        newBlock.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldBlocks.Add(newBlock.transform.position - (Vector3.one * 0.5f));
    }
}
