using System.Collections.Generic;
using UnityEngine;

///<summary> Class <c>BoardCell</c> Models a Grid Cell of the Game Board of Santorini </summary>
public class BoardCell
{
    ///<summary> property hasDome allows anyone to check if this cell has a dome on top </summary>
    public bool hasDome { get; private set; }
    ///<summary> property height allows anyone to check the height of this cell </summary>
    public int height { get { return tokens == null ? 0 : tokens.Count; } }
    ///<summary> property position allows anyone to look at the position of this cell </summary>
    public Vector2Int position { get; private set; }

    private List<TokenType> tokens;

    ///<summary> Constructor Initializes the new BoardCell at the position (<paramref name="x"/>,<paramref name="y"/>)</summary>
    ///<param name="x">The BoardCell's x-coordinate</param>
    ///<param name="y">The BoardCell's y-coordinate</param>
    public BoardCell(int x, int y)
    {
        position = new Vector2Int(x, y);
    }

    ///<summary> Adds a given Token (<paramref name="token"/>) to the list of Tokens kept in this BoardCell</summary>
    public void Add(TokenType token)
    {
        if (tokens == null)
            tokens = new List<TokenType>();
        tokens.Add(token);
    }
}

