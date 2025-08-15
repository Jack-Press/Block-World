using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class biomeClass
{
    [Header("Biome Specs")]
    public string biomeName;
    public Color biomeColor;
    public BlockClass[] biomeBlocks;
    public List<string> blocksbyName;

    [Header ("Cave Gen")]
    public float caveFreq = 0.05f;
    public bool genCaves = true;
    public Texture2D caveNoise;

    [Header("Terrain Gen")]
    public float terrainFreq = 0.05f;
    public float heightMultiplier = 4f;
    public float surfaceValue = 0.25f;

    [Header("Structure Generation")]
    public int treeChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Add-ons")]
    public int addChance = 3;

    [Header("Ore Gereation")]
    public OreClass[] ores;

    public void Start()
    {
        foreach(BlockClass block in biomeBlocks)
            blocksbyName.Add(block.name);
    }
}
