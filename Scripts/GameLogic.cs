using System.Collections.Generic;
using UnityEngine;

///<summary> Class <c>GameLogic</c> handles all of the logic of the game Santorini.</summary>
public partial class GameLogic : MonoBehaviour
{
    //All public Variables are public to allow setting them in the inspector.
    [Range(2, 4)]
    ///<summary>Amount of players in the game</summary>
    public int playerCount = 2;
    [Range(1, 4)]
    ///<summary>Amount of Tokens in the game per player</summary>
    public int tokenCount = 2;
    [Range(4, 10)]
    ///<summary>Size of the board</summary>
    public int boardSize = 5;
    [Range(1, 5)]
    ///<summary>Maximum Height of the towers</summary>
    public int maxTowerHeight = 3;
    ///<summary>Maximum Amount of Cubes that can be built</summary>
    public int maxCubeCount = 54;
    ///<summary>Maximum Amount of Domes that can be built</summary>
    public int maxDomeCount = 18;
    ///<summary>Available Player tokens for the game</summary>
    public List<PlayerToken> predesignedTokens;

    ///<summary>property that keeps track if the current player may activate a card</summary>
    private bool canActivateCard;
    ///<summary>property that keeps track if the current player can still move</summary>
    public bool canMove { get; private set; }
    ///<summary>property that keeps track if the current player can still build</summary>
    public bool canBuild { get; private set; }

    ///<summary>property that keeps track of the current player</summary>
    public int currentPlayer { get; private set; }
    ///<summary>property that keeps track of the current token for token deployment</summary>
    private int currentToken;

    ///<summary>Delegate for the onDeployFinish event</summary>
    public delegate void OnDeployFinish();
    ///<summary>onDeployFinish event gets invoked when the last player places their last token</summary>
    public event OnDeployFinish onDeployFinish;

    ///<summary>Delegate for the onTokenDeploy event</summary>
    public delegate void OnTokenDeploy(PlayerToken token);
    ///<summary>onTokenDeploy event gets invokend when any player places a token</summary>
    public event OnTokenDeploy onTokenDeploy;
    ///<summary>Delegate for the onMoveToken event</summary>
    public delegate void OnMoveToken(PlayerToken token, Vector2Int position);
    ///<summary>onMoveToken event gets invoked when any token is moved to a new position</summary>
    public event OnMoveToken onMoveToken;

    ///<summary>Delegate for the onBoardCellCreated event</summary>
    public delegate void OnBoardCellCreated(BoardCell cell);
    ///<summary>onBoardCellCreated event gets invoked when a new Board Cell is created during initialisation</summary>
    public event OnBoardCellCreated onBoardCellCreated;
    ///<summary>Delegate for the onBuildToken event</summary>
    public delegate void OnBuildToken(BoardCell cell, TokenType type, int amountLeft);
    ///<summary>onBuildToken event gets invoked when a Token is built on top a board Cell</summary>
    public event OnBuildToken onBuildToken;

    ///<summary>Delegate for the onNextPlayer event</summary>
    public delegate void OnNextPlayer();
    ///<summary>onNextPlayer event gets invoked when the turn of the next player starts</summary>
    public event OnNextPlayer onNextPlayer;
    ///<summary>Delegate for the onGameFinish event</summary>
    public delegate void OnGameFinish(int winner);
    ///<summary>onGameFinish gets invoked when someone wins</summary>
    public event OnGameFinish onGameFinish;

    private DeityCard activeDeityCard;
    private List<DeityCard> usedDeityCards;
    private BoardCell[,] board;
    private List<PlayerToken> playerTokens;

    private int athenaMoveUp;
    private int cubesLeft;
    private int domesLeft;

    ///<summary>Resets all variables to the starting state</summary>
    public void Reset()
    {
        currentPlayer = 0;
        currentToken = 0;

        cubesLeft = maxCubeCount;
        domesLeft = maxDomeCount;

        activeDeityCard = DeityCard.NONE;
        usedDeityCards = new List<DeityCard>();

        playerTokens = new List<PlayerToken>();

        canMove = false;
        canBuild = false;
        canActivateCard = false;

        board = new BoardCell[boardSize, boardSize];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                board[x, y] = new BoardCell(x, y);
                if (onBoardCellCreated != null)
                    onBoardCellCreated.Invoke(board[x, y]);
            }
        }
        foreach (PlayerToken token in predesignedTokens)
        {
            token.Reset();
        }
    }

    #region Deployment

    ///<summary>Used for initial token deployment. The next token will be placed at (<paramref name="x"/>,<paramref name="y"/>)</summary>
    ///<param name="x"> x Coordinate to place the token at
    ///<param name="y"/> y Coordinate to place the token at
    public bool PlacePlayerTokenInitial(int x, int y)
    {
        if (CheckPlayerPresent(x, y))
            return false;
        //Look for an available token for this player
        foreach (PlayerToken token in predesignedTokens)
        {
            if (token.availableForPlayer == currentPlayer)
            {
                if (!token.inUse)
                {
                    token.Place(x, y);
                    playerTokens.Add(token);
                    if (onTokenDeploy != null)
                        onTokenDeploy.Invoke(token);
                    currentPlayer++;
                    CheckDeployFinish();
                    return true;
                }
            }
        }
        //Should be unreachable since tokenCount is limited to 4 and predesignedTokens contains 4 tokens for each Player
        return false;
    }

    ///<summary>Used to check if Token deployment has finished (as in the last player placed their last token)</summary>
    private void CheckDeployFinish()
    {
        if (currentPlayer == playerCount)
        {
            currentToken++;
            currentPlayer = 0;
            if (currentToken == tokenCount)
            {
                currentToken = 0;
                canActivateCard = true;
                canMove = true;
                canBuild = false;
                if (onDeployFinish != null)
                    onDeployFinish.Invoke();
            }
        }
    }

    #endregion

    #region Movement

    ///<summary>Used to try to move a given token (<paramref name="token"/>) to a given position (<paramref name="targetX"/>,<paramref name="targetY"/>)</summary>
    ///<param name="token">The token to try move</param>
    ///<param name="targetX">The target x-Position</param>
    ///<param name="targetY">The target y-Position</param>
    public void TryMove(PlayerToken token, int targetX, int targetY)
    {
        if (!board[targetX, targetY].hasDome && GetValidMoves(token).Contains(new Vector2Int(targetX, targetY)))
        {
            //Checks if the move swaps the position with another player
            CheckApolloSwap(token, targetX, targetY);
            //Checks if the move prevents the next player from moving up
            CheckAthenaMoveUp(token, targetX, targetY);

            token.MoveTo(targetX, targetY);
            //Player moved so no card can be activated anymore this turn
            canActivateCard = false;

            if (onMoveToken != null)
                onMoveToken.Invoke(token, new Vector2Int(targetX, targetY));
            if (board[targetX, targetY].height == maxTowerHeight)
            {
                if (onGameFinish != null)
                    onGameFinish.Invoke(token.availableForPlayer);
                Debug.Log("Player " + token.availableForPlayer + " wins");
            }
            //If artemis is active allow for a second move action
            if (activeDeityCard == DeityCard.ARTEMIS)
            {
                activeDeityCard = DeityCard.NONE;
            }
            //Otherwise jump to building
            else
            {
                canMove = false;
                canBuild = true;
            }
        }
    }

    ///<summary>Used to check if the player moves up while having played the Athena card</summary>
    ///<param name="token">The token that moves</param>
    ///<param name="targetX">The target x-coordinate</param>
    ///<param name="targetY">The target y-coordinate</param>
    private void CheckAthenaMoveUp(PlayerToken token, int targetX, int targetY)
    {
        if (activeDeityCard == DeityCard.ATHENA && board[token.position.x, token.position.y].height < board[targetX, targetY].height)
        {
            //Set athena counter to 2 -> end of turn results in 1 -> still active for next turn -> end of turn results in 0 -> not active anymore
            athenaMoveUp = 2;
        }
    }

    ///<summary>Used to check if the player moves onto the space of another player while having played the Apollo card</summary>
    ///<param name="token">The token that moves</param>
    ///<param name="targetX">The target x-coordinate</param>
    ///<param name="targetY">The target y-coordinate</param>
    private void CheckApolloSwap(PlayerToken token, int targetX, int targetY)
    {
        if (activeDeityCard == DeityCard.APOLLO)
        {
            foreach (PlayerToken otherToken in playerTokens)
            {
                if (otherToken.position == new Vector2Int(targetX, targetY))
                {
                    otherToken.MoveTo(token.position.x, token.position.y);
                    if (onMoveToken != null)
                        onMoveToken.Invoke(otherToken, token.position);
                }
            }
        }
    }

    ///<summary>Used to Generate a List of all possible moves a Token can do</summary>
    ///<param name="token">The token token to check</param>
    ///<returns>A List of all possible grid positions the given token can move to</returns>
    private List<Vector2Int> GetValidMoves(PlayerToken token)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        bool hermes = activeDeityCard == DeityCard.HERMES;
        int currentHeight = board[token.position.x, token.position.y].height;
        //Loop offset from -1 to 1 by default and from -boardSize to boardSize if hermes is active to cover the whole board
        for (int xOffset = (hermes ? -boardSize : -1); xOffset <= (hermes ? boardSize : 1); xOffset++)
        {
            for (int yOffset = (hermes ? -boardSize : -1); yOffset <= (hermes ? boardSize : 1); yOffset++)
            {
                if ((xOffset == 0 && yOffset == 0) || InvalidPosition(token, xOffset, yOffset))
                    continue;
                Vector2Int position = new Vector2Int(token.position.x + xOffset, token.position.y + yOffset);

                //Default movement height condition -> can only move to <=current+1
                bool heightConditionDefault = !hermes && board[position.x, position.y].height <= currentHeight + 1;
                //Hermes movement height condition -> can only move on same layer
                bool heightConditionHermes = hermes && board[position.x, position.y].height == currentHeight;
                //Athena movement height condition -> check if previous player moved up and current is trying to move up
                bool heightConditionAthena = athenaMoveUp > 0 && board[position.x, position.y].height > currentHeight;
                //Full height condition -> movement not prevented by athena and allowed by default od hermes
                bool heightCondition = (heightConditionDefault || heightConditionHermes) && !heightConditionAthena;

                //Check if a player is present or apollo is active
                bool playerContition = !CheckPlayerPresent(position.x, position.y) || activeDeityCard == DeityCard.APOLLO;

                //check if move is valid based on all the conditions above
                if (!board[position.x, position.y].hasDome && heightCondition && playerContition)
                {
                    validMoves.Add(position);
                }
            }
        }
        return validMoves;
    }

    #endregion

    #region Building

    ///<summary>Used to try to build a given Token type (<paramref name="type"/>) on a given space (<paramref name="targetX"/>,<paramref name="targetY"/>) 
    ///as a given <c>PlayerToken</c> (<paramref name="token"/>)</summary>
    ///<param name="token">The token that is building</param>
    ///<param name="type">The type of token to build</param>
    ///<param name="targetX">The target x-coordinate</param>
    ///<param name="targetY">The target y-coordinate</param>
    public void TryBuild(PlayerToken token, TokenType type, int targetX, int targetY)
    {
        if (!CheckPlayerPresent(targetX, targetY) && GetVaildBuilds(token, type).Contains(new Vector2Int(targetX, targetY)))
        {
            board[targetX, targetY].Add(type);
            if (type == TokenType.CUBE)
                cubesLeft--;
            else
                domesLeft--;
            if (onBuildToken != null)
                onBuildToken.Invoke(board[targetX, targetY], type, type == TokenType.CUBE ? cubesLeft : domesLeft);
            //If Demeter is active allow for a second build action
            if (activeDeityCard == DeityCard.DEMETER)
            {
                activeDeityCard = DeityCard.NONE;
            }
            //Otherwise jump to the next player
            else
            {
                NextPlayer();
            }
        }
    }

    ///<summary>Used to generate a List of all Valid build positions of a given <c>TokenType</c> (<paramref name="buildToken"/>) 
    ///for a given <c>PlayerToken</c>(<paramref name="token"/></summary>
    ///<param name="token">The token that is building</param>
    ///<param name="buildToken">The type of token to build</param>
    ///<returns>A list of all valid Build lovations</returns>
    private List<Vector2Int> GetVaildBuilds(PlayerToken token, TokenType buildToken)
    {
        List<Vector2Int> validBuilds = new List<Vector2Int>();
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if ((xOffset == 0 && yOffset == 0) || InvalidPosition(token, xOffset, yOffset))
                    continue;
                Vector2Int position = new Vector2Int(token.position.x + xOffset, token.position.y + yOffset);
                if (!board[position.x, position.y].hasDome)
                {
                    bool cubeCondition = buildToken == TokenType.CUBE && board[position.x, position.y].height < maxTowerHeight;
                    bool domeCondition = buildToken == TokenType.DOME && (board[position.x, position.y].height == maxTowerHeight || activeDeityCard == DeityCard.ATLAS);

                    if (cubeCondition || domeCondition)
                    {
                        validBuilds.Add(position);
                    }
                }
            }
        }
        return validBuilds;
    }

    #endregion

    ///<summary>Used to make the current player give up. Doesn't work for more than 2 players </summary>
    public void Forfeit()
    {
        if (playerCount == 2)
        {
            if (onGameFinish != null)
                onGameFinish.Invoke((currentPlayer + 1) % 2);
        }
    }

    ///<summary>Used to start the turn for the next player</summary>
    private void NextPlayer()
    {
        canActivateCard = true;
        canMove = true;
        canBuild = false;
        //Reset deity card
        activeDeityCard = DeityCard.NONE;
        //decrease athena effect counter
        athenaMoveUp--;
        //goto next player
        currentPlayer = (currentPlayer + 1) % playerCount;
        if (onNextPlayer != null)
            onNextPlayer.Invoke();
    }

    ///<summary>Used to activate a given <c>DeityCard</c>(<paramref name="card"/>)</summary>
    ///<param name="card">The card to activate</param>
    ///<returns>True if the card was activated, false if it wasn't</returns>
    public bool ActivateDeityCard(DeityCard card)
    {
        if (!usedDeityCards.Contains(card) && canActivateCard)
        {
            activeDeityCard = card;
            usedDeityCards.Add(card);
            Debug.Log("Activating " + card);
            canActivateCard = false;
            return true;
        }
        return false;
    }

    ///<summary>Getter for the height at given coordinates(<paramref name="x"/>,<paramref name="y"/>)</summary>
    ///<param name="x">The target x-coordinate</param>
    ///<param name="y">The target y-coordinate</param>
    public int GetHeight(int x, int y)
    {
        return board[x, y].height;
    }

    ///<summary>Checks if a player is at given coordinates(<paramref name="x"/>,<paramref name="y"/>)</summary>
    ///<param name="x">The target x-coordinate</param>
    ///<param name="y">The target y-coordinate</param>
    private bool CheckPlayerPresent(int x, int y)
    {
        foreach (PlayerToken token in playerTokens)
        {
            if (token.position.x == x && token.position.y == y)
            {
                return true;
            }
        }
        return false;
    }

    ///<summary>Checks if the given <c>PlayerToken</c> (<paramref name="token"/>) would be outside the board 
    ///after applying an offset of(<paramref name="xOffset"/>,<paramref name="yOffset"/>)</summary>
    ///<param name="token">The token to check</param>
    ///<param name="xOffset">The offset in the x-coordinate</param>
    ///<param name="yOffset">The offset in the y-coordinate</param>
    private bool InvalidPosition(PlayerToken token, int xOffset, int yOffset)
    {
        return token.position.x + xOffset < 0 || token.position.x + xOffset >= boardSize || token.position.y + yOffset < 0 || token.position.y + yOffset >= boardSize;
    }
}
