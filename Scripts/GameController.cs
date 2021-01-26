using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(GameLogic))]
///<summary> Class <c>GameController</c> Is used to send Events to the <c>GameLogic</c> and generate Objects based on Events from <c>GameLogic</c></summary>
public class GameController : MonoBehaviour
{
    //All Public Variables are public to allow setting them in the Inspector. 
    ///<summary> Color for half the Game Board</summary>
    public Color boardColorA;
    ///<summary> Color for the other half the Game Board</summary>
    public Color boardColorB;

    ///<summary> Prefab for the Cube Shaped Player Token </summary>    
    public GameObject CubePrefabPlayer;
    ///<summary> Prefab for the Cylinder Shaped Player Token </summary>
    public GameObject CylinderPrefabPlayer;
    ///<summary> Prefab for the Cone Shaped Player Token </summary>
    public GameObject ConePrefabPlayer;
    ///<summary> Prefab for the Prism Shaped Player Token </summary>
    public GameObject PrismPrefabPlayer;

    ///<summary> Prefab for the Cube Shaped Building Token </summary>
    public GameObject CubePrefabWorld;
    ///<summary> Prefab for the Cube Shaped Building Token </summary>
    public GameObject DomePrefabWorld;

    ///<summary> Reference to the winScreen UI Object </summary>
    public Transform winScreen;
    ///<summary> Reference to the cubeDisplay UI Object </summary>
    public Transform cubeDisplay;
    ///<summary> Reference to the domeDisplay UI Object </summary>
    public Transform domeDisplay;


    private GameLogic logic;
    private PlayerTokenObject clickedToken;
    private bool deploying;
    private bool interactable;
    private TokenType buildToken;

    private Dictionary<PlayerToken, PlayerTokenObject> playerTokens;
    private Dictionary<BoardCell, BoardCellObject> boardCells;

    ///<summary> Awake is called upon Object creation. I use it to get the required References and subscribe to events</summary>
    public void Awake()
    {
        logic = GetComponent<GameLogic>();

        logic.onBoardCellCreated += OnBoardCellCreated;
        logic.onTokenDeploy += OnTokenDeploy;
        logic.onMoveToken += OnMoveToken;
        logic.onBuildToken += OnBuildToken;
        logic.onDeployFinish += OnDeployFinish;
        logic.onNextPlayer += OnNextPlayer;
        logic.onGameFinish += OnGameFinish;

        UIEvents uiEvents = FindObjectOfType<UIEvents>();
        uiEvents.onChangeBuildToken += OnChangeBuildToken;
        uiEvents.onSetInteractable += OnSetInteractable;
        uiEvents.onDeityCardActivated += OnDeityCardActivated;
        uiEvents.onForfeit += OnForfeit;
        uiEvents.onPlayAgain += OnPlayAgain;
        uiEvents.onQuit += OnQuit;
    }

    ///<summary> Start is called before the first Frame Update. I use it to initialize everything</summary>
    void Start()
    {
        this.Reset();
        logic.Reset();
    }

    #region GameLogic Events

    #region Start

    ///<summary> Executed when the onBoardCellCreated event is Invoked in <c>GameLogic</c>. 
    ///Creates the <c>BoardCellObject</s> for the given <c>BoardCell</c> (<paramref name="cell"/>).</summary>
    ///<param name="cell">The cell that was created</param>
    public void OnBoardCellCreated(BoardCell cell)
    {
        Transform plane = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
        plane.parent = this.transform;
        plane.position = ToGlobalCoords(cell.position);
        plane.localScale = Vector3.one * 0.1f;

        BoardCellObject cellObj = plane.gameObject.AddComponent<BoardCellObject>();
        cellObj.cell = cell;
        cellObj.onClick += OnBoardCellClicked;

        boardCells.Add(cell, cellObj);

        MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
        planeRenderer.material.color = (cell.position.x + cell.position.y) % 2 == 0 ? boardColorA : boardColorB;
    }

    ///<summary> Executed when the onTokenDeploy event is Invoked in <c>GameLogic</c>. 
    ///Creates the <c>PlayerTokenObject</s> for the given <c>PlayerToken</c> (<paramref name="token"/>).</summary>
    ///<param name="token">The token that was deployed</param>
    public void OnTokenDeploy(PlayerToken token)
    {
        Transform playerToken = CreateToken(token.shape);
        playerToken.parent = this.transform;
        playerToken.name = token.availableForPlayer + ", " + token.color;

        playerToken.localScale = Vector3.one * 0.5f;
        playerToken.position = ToGlobalCoords(token.position);
        playerToken.position += Vector3.up * playerToken.localScale.z / 2f;

        PlayerTokenObject tokenObj = playerToken.gameObject.AddComponent<PlayerTokenObject>();
        tokenObj.token = token;
        tokenObj.onClick += OnTokenClicked;

        playerTokens.Add(token, tokenObj);

        MeshRenderer playerTokenRenderer = playerToken.GetComponent<MeshRenderer>();
        playerTokenRenderer.material.color = token.color;
    }

    ///<summary> Creates A GameObject based on the given <c>Shape</c> (<paramref name="shape"/>)</summary>
    ///<param name="shape">The Shape of token to create</param>
    private Transform CreateToken(Shape shape)
    {
        Transform playerToken;
        switch (shape)
        {
            case Shape.CUBE:
                playerToken = Instantiate(CubePrefabPlayer).transform;
                break;
            case Shape.CYLINDER:
                playerToken = Instantiate(CylinderPrefabPlayer).transform;
                break;
            case Shape.CONE:
                playerToken = Instantiate(ConePrefabPlayer).transform;
                break;
            case Shape.PRISM:
                playerToken = Instantiate(PrismPrefabPlayer).transform;
                break;
            default:
                //In case the token wasn't assigned a shape something went wrong
                throw new MissingReferenceException("The playerToken had a invalid shape assigned");
        }

        return playerToken;
    }

    ///<summary>Executed when the onDeployFinish event is Invoked in <c>GameLogic</c></summary>
    public void OnDeployFinish()
    {
        deploying = false;
    }

    #endregion

    ///<summary>Executed when the onMoveToken event is Invoked in <c>GameLogic</c>. 
    ///Moves the <c>PlayerTokenObject</c> associated with the given <c>PlayerToken</c> (<paramref name="token"/>) 
    ///to the given Coordinates (<paramref name="position"/>)</summary>
    ///<param name="token"> The token to move </param>
    ///<param name="position"> The position to move to </param>
    public void OnMoveToken(PlayerToken token, Vector2Int position)
    {
        MoveToken(playerTokens[token], position);
    }

    ///<summary>Executed when the onBuildToken event is Invoked in <c>GameLogic</c>
    ///Creates a new <c>BoardCellObject</c> on top of the given <c>BoardCell</c> (<paramref name="cell"/>)
    ///The created Object will have the Shape specified in <paramref name=type/></summary>
    ///<param name="cell"> The Cell to build on </param>
    ///<param name="type"> The type of Token to build </param>
    public void OnBuildToken(BoardCell cell, TokenType type, int amountLeft)
    {
        if (type == TokenType.CUBE)
            cubeDisplay.GetComponent<TextMeshProUGUI>().text = amountLeft + "/" + logic.maxCubeCount;
        else
            domeDisplay.GetComponent<TextMeshProUGUI>().text = amountLeft + "/" + logic.maxDomeCount;
        BuildToken(boardCells[cell], type);
    }

    ///<summary>Executed when the onNextPlayer event is Invoked in <c>GameLogic</c>
    ///Resets all variables to the state at the start of a turn</summary>
    public void OnNextPlayer()
    {
        clickedToken = null;
    }

    ///<summary>Executed when the onGameFinish event is Invoked in <c>GameLogic</c>
    ///Displays the winScreen if it was assigned</summary>
    ///<param name="player"> The Player that won </param>
    public void OnGameFinish(int player)
    {
        interactable = false;
        if (winScreen != null)
        {
            winScreen.gameObject.SetActive(true);
            winScreen.GetComponentInChildren<TextMeshProUGUI>().text = "Player " + player + " wins!";
        }
    }

    #endregion

    #region Click Events

    ///<summary> Executed, when a <c>BoardCellObject</c> Invokes it's onClick Event
    ///Tells the <c>GameLogic</c> to take a action based on the game state</summary>
    ///<param name="cellObj">The Object that was clicked</param>
    public void OnBoardCellClicked(BoardCellObject cellObj)
    {
        if (!interactable)
            return;
        if (deploying)
        {
            logic.PlacePlayerTokenInitial(cellObj.cell.position.x, cellObj.cell.position.y);
        }
        else if (clickedToken != null)
        {
            if (logic.canMove)
            {
                logic.TryMove(clickedToken.token, cellObj.cell.position.x, cellObj.cell.position.y);
            }
            else if (logic.canBuild)
            {
                logic.TryBuild(clickedToken.token, buildToken, cellObj.cell.position.x, cellObj.cell.position.y);
            }
        }
    }

    ///<summary> Executed, when a <c>PlayerTokenObject</c> Invokes it's onClick Event
    ///Stores the clicked Player for further use</summary>
    ///<param name="token">The Object that was clicked</param>
    public void OnTokenClicked(PlayerTokenObject token)
    {
        if (!interactable)
            return;
        //If you can still move, allow changing which token to move
        if (logic.canMove)
        {
            if (token.token.availableForPlayer == logic.currentPlayer)
            {
                clickedToken = token;
            }
        }
    }

    #endregion

    #region UI Events

    ///<summary> Executed when the <c>UIEvents</c> Invoke the onChangeBuildToken event. 
    ///Sets the <c>TokenType</c> that should be built in the future to the given one (<paramref name="tokenType"/>) </summary>
    ///<param name="tokenType">The type of token that should be used for building</param>
    public void OnChangeBuildToken(TokenType tokenType)
    {
        this.buildToken = tokenType;
    }

    ///<summary> Executed when the <c>UIEvents</c> Invoke the onSetInteractable event. 
    ///Sets the games state to be interactable or not based on the given state (<paramref name="state"/>)</summary>
    ///<param name="state">Whether the game should be interactable or not</param>
    public void OnSetInteractable(bool state)
    {
        this.interactable = state;
    }

    ///<summary> Executed when the <c>UIEvents</c> Invoke the onDeityCardActivated event. 
    ///Tells the <c>GameLogic</c> to activate the given <c>DeityCard</c> (<paramref name="card"/>)</summary>
    ///<param name="card">The card that should be activated </param>
    ///<returns>true if the card was Activated, false if it wasn't</returns>
    public bool OnDeityCardActivated(DeityCard card)
    {
        return logic.ActivateDeityCard(card);
    }

    ///<summary> Executed when the <c>UIEvents</c> Invoke the onForfeit event. 
    ///Tells the <c>GameLogic</c> that the currnt player has given up</summary>
    public void OnForfeit()
    {
        logic.Forfeit();
    }

    ///<summary> Executed when the <c>UIEvents</c> Invoke the onPlayAgain event. 
    ///Tells the <c>GameLogic</c> and this class to reset</summary>
    public void OnPlayAgain()
    {
        this.Reset();
        logic.Reset();
    }

    ///<summary> Executed when the <c>UIEvents</c> Invoke the onQuit event. Stios the Application</summary>
    public void OnQuit()
    {
        //Conditional Compilation, stops running the editor if in the editor, otherwise stops the application
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    ///<summary> Resets the game to it's initial state</summary>
    private void Reset()
    {
        //Remove all Game Objects
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        //reset all variables and references
        boardCells = new Dictionary<BoardCell, BoardCellObject>();
        playerTokens = new Dictionary<PlayerToken, PlayerTokenObject>();
        deploying = true;
        interactable = true;
    }

    ///<summary> Moves a <c>PlayerTokenObject</c> to the given Position (<paramref name="position"/>)</summary>
    ///<param name="token">The token to move</param>
    ///<param name="position">The position to move to</param>
    private void MoveToken(PlayerTokenObject token, Vector2Int position)
    {
        token.transform.position = ToGlobalCoords(position);
        token.transform.position += Vector3.up * token.transform.localScale.z / 2f;
    }

    ///<summary> Builds a <c>BoardCellObject</c> of the given Type (<paramref name="type"/>) 
    ///on top of the given one (<paramref name="cellObj"/>)</summary>
    ///<param name="cellObj">The cell to build on top of</param>
    ///<param name="type">The Type of token to build on top </param>
    private void BuildToken(BoardCellObject cellObj, TokenType type)
    {
        Transform build;
        switch (type)
        {
            case TokenType.CUBE:
                build = Instantiate(CubePrefabWorld).transform;
                break;
            case TokenType.DOME:
                build = Instantiate(DomePrefabWorld).transform;
                break;
            default:
                //If buildToken wasn't assigned something went wrong
                throw new UnassignedReferenceException("buildToken had a invalid type assigned");
        }
        build.position = ToGlobalCoords(cellObj.cell.position);
        //Move Cubes down because they are half height, don't move domes down since they are full height
        build.position += (type == TokenType.CUBE ? Vector3.down * transform.localScale.z / 4f : Vector3.zero);
        build.parent = cellObj.transform;


        BoardCellObject buildCellObject = build.gameObject.AddComponent<BoardCellObject>();
        buildCellObject.cell = cellObj.cell;
        buildCellObject.onClick += OnBoardCellClicked;

        MeshRenderer buildCellRenderer = build.gameObject.GetComponent<MeshRenderer>();
        buildCellRenderer.material.color = (cellObj.cell.position.x + cellObj.cell.position.y) % 2 == 0 ? boardColorA : boardColorB;
    }

    ///<summary> Converts from the given grid Coordinates (<paramref name="gridIndex"/>) to global coordinates, 
    ///taking into account the current height at the givven position</summary>
    ///<param name="gridIndex">The position in the grid to convert to global coordinates</param>
    private Vector3 ToGlobalCoords(Vector2Int gridIndex)
    {
        return new Vector3(gridIndex.x - (logic.boardSize - 1) / 2f, logic.GetHeight(gridIndex.x, gridIndex.y) / 2f, gridIndex.y - (logic.boardSize - 1) / 2f);
    }
}
