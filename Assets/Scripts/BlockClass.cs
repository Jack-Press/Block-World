using UnityEngine;

[CreateAssetMenu(fileName = "newblockclass", menuName = "block Class")]

public class BlockClass : ScriptableObject
{
    public string blockName;
    public Sprite[] blockSprites;
    public Sprite[] dropSprites;
}
