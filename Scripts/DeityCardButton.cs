using UnityEngine.UI;

//Conditional Compiling. UnityEditor is only imported when the application is compiled in the Editor
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

///<summary> Extention to <c>UnityEngine.UI.Button</c> to allow me to associate a DeityCard with the Button</summary>
public class DeityCardButton : Button
{
    ///<summary> the Deity Card associated with this Button </summary>
    public DeityCard card;
}

//Conditional Compiling.
#if UNITY_EDITOR

[CustomEditor(typeof(DeityCardButton))]
///<summary> Extention to <c>UnityEditor.UI.ButtonEditor</c> to allow me to select the associated Deity Card in the Inspector of each Button </summary>
public class DeityCardButtonInspector : ButtonEditor
{
    ///<summary> Method gets called when the Inspector of a <c>DeityCardButton</c> is visible.c Adds a EnumPopup for choosing the Card </summary>
    public override void OnInspectorGUI()
    {
        DeityCardButton button = (DeityCardButton)target;
        button.card = (DeityCard)EditorGUILayout.EnumPopup("Card: ", button.card);

        base.OnInspectorGUI();
    }
}
#endif