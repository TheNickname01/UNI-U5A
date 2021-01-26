using UnityEngine;

///<summary> Class <c>PlayerTokenObject</c> Models the Actual GameObject represented by it's PlayerToken</summary>
public class PlayerTokenObject : MonoBehaviour
{
    ///<summary>property token to keep track of the PlayerToken represented by this GameObject</summary>
    public PlayerToken token { get; set; }

    ///<summary>Delegate for the OnClick Event</summary>
    public delegate void OnClick(PlayerTokenObject token);
    ///<summary>onClick Event gets Invoked every time this gameObject is clicked on</summary>
    public event OnClick onClick;

    ///<summary>Method Called by Unity every frame the mouse is hovering over this GameObject</summary>
    public void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (onClick != null)
                onClick.Invoke(this);
        }
    }
}
