using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Clickable Data", menuName = "Scriptable Objects/UI/ClickableData")]
public class ClickableMenuData : ScriptableObject
{
    public Sprite sprtIcon;
    public string strMenuName;
    public int column;
}