using UnityEngine;

[System.Serializable]
///<summary> Class <c>PlayerToken</c> models a token in the game Santorini </summary>
public class PlayerToken
{
    //All public variables are public to allow setting them in the inspector
    ///<summary>Color of the token</summary>
    public Color color;
    ///<summary>Shape of the token</summary>
    public Shape shape;
    [Range(0, 3)]
    ///<summary>ID of the player that uses this token</summary>
    public int availableForPlayer;

    ///<summary>property that allows to check if this token is in use </summary>
    public bool inUse { get; private set; }
    ///<summary>property that allows looking at the position of this token
    public Vector2Int position { get; private set; }

    ///<summary>Places this token at the given coordinates (<paramref name="x"/>,<paramref name="y"/>) and marks it as used</summary>
    ///<param name="x">The x-coordinate to place at</param>
    ///<param name="y">The y-coordinate to place at</param>
    public void Place(int x, int y)
    {
        position = new Vector2Int(x, y);
        inUse = true;
    }

    ///<summary>Moves the token to the given coordinates (<paramref name="x"/>,<paramref name="y"/>)</summary>
    ///<param name="x">The x-coordinate to move to</param>
    ///<param name="y">The y-coordinate to move to</param>
    public void MoveTo(int x, int y)
    {
        position = new Vector2Int(x, y);
    }

    ///<summary>Resets this tokens position and inuse property</summary>
    public void Reset()
    {
        position = Vector2Int.zero;
        inUse = false;
    }
}

