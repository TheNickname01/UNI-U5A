using UnityEngine;

///<summary>Class <c>UIEvents</c> is used to send events from the UI to other classes</summary>
public class UIEvents : MonoBehaviour
{
    ///<summary>Delegate for the onChangeBuildToken event</summary>
    public delegate void OnChangeBuildToken(TokenType type);
    ///<summary>event onChangeBuildToken is invoked when one of the BuildToken UI Buttons is clicked</summary>
    public event OnChangeBuildToken onChangeBuildToken;
    ///<summary>Delegate for the onSetInteractable event</summary>
    public delegate void OnSetInteractable(bool state);
    ///<summary>event onSetInteractable is invoked when the deity cards are being shown/hidden</summary>
    public event OnSetInteractable onSetInteractable;
    ///<summary>Delegate for the onDeityCardActivated event</summary>
    public delegate bool OnDeityCardActivated(DeityCard card);
    ///<summary>event onDeityCardActivated is invoked when a deity card is clicked</summary>
    public event OnDeityCardActivated onDeityCardActivated;

    ///<summary>Delegate for the onQuit and onPlayAgain event</summary>
    public delegate void NoParamsVoid();
    ///<summary>event onQuit is invoked when the quit button is clicked</summary>
    public event NoParamsVoid onQuit;
    ///<summary>event onPlayAgain is invoked when the play again button is clicked</summary>
    public event NoParamsVoid onPlayAgain;
    ///<summary>event onForfeit is invoked when the give up button is clicked</summary>
    public event NoParamsVoid onForfeit;

    ///<summary>Referenceto the pause Menu Object</summary>
    public Transform pauseMenu;

    ///<summary>Called once per frame by the game Engine
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (onSetInteractable != null)
                onSetInteractable.Invoke(pauseMenu.gameObject.activeSelf);
            pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
        }
    }

    ///<summary> Called by the Return to Game UI Button </summary>
    public void OnCloseMenu()
    {
        if (onSetInteractable != null)
            onSetInteractable.Invoke(true);
        pauseMenu.gameObject.SetActive(false);
    }

    ///<summary> Called by the UI toggle with the cube build token</summary>
    public void OnBuildCubeToggle(bool state)
    {
        if (onChangeBuildToken != null)
            onChangeBuildToken.Invoke(state ? TokenType.CUBE : TokenType.DOME);
    }

    ///<summary> Called by the UI toggle that shows/hides the deity cards</summary>
    public void OnShowDeityCardsToggle(bool state)
    {
        if (onSetInteractable != null)
            onSetInteractable.Invoke(!state);
    }

    ///<summary> Called by any deity card when clicked</summary>
    public void OnDeityCard(DeityCardButton button)
    {
        if (onDeityCardActivated != null)
        {
            bool cardActive = onDeityCardActivated.Invoke(button.card);
            //If card active deactivate Object so the card can't be clicked again 
            //only visual, the game logic keeps track of used cards so you couldn't use cards twice anyways
            button.gameObject.SetActive(!cardActive);
        }
    }

    ///<summary> Called by the Quit UI button</summary>
    public void OnQuitPressed()
    {
        if (onQuit != null)
            onQuit.Invoke();
    }

    ///<summary>Called by the Play Again UI Button</summary>
    public void OnPlayAgainPressed()
    {
        OnCloseMenu();
        if (onPlayAgain != null)
            onPlayAgain.Invoke();
    }

    ///<summary>Called by the Give Up UI Button</summary>
    public void OnGiveUpPressed()
    {
        OnCloseMenu();
        if (onForfeit != null)
            onForfeit.Invoke();
    }
}
