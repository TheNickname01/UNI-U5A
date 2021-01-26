using UnityEngine;

///<summary> Class <c>BoardCellObject</c> Models the Actual GameObject represented by it's BoardCell</summary>
public class BoardCellObject : MonoBehaviour
{
    ///<summary>property cell to keep track of the BoardCell represented by this GameObject</summary>
    public BoardCell cell { get; set; }

    ///<summary>Delegate for the OnClick Event</summary>
    public delegate void OnClick(BoardCellObject cell);
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
