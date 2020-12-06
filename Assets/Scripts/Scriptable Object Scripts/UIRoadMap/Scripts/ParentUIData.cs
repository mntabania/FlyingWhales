using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "parent Right Click UI", menuName = "Scriptable Objects/UI/ParentUIData")]
public class ParentUIData : ScriptableObject
{
    public ClickableMenuData clickableMenuData;

    public List<ParentUIData> subMenu = new List<ParentUIData>();
}