using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverText : MonoBehaviour
{
    public string hoverDisplayText;

	public void OnHoverOver() {
		Tooltip.Instance.ShowSmallInfo(hoverDisplayText, autoReplaceText: false);
	}

	public void OnHoverOut() {
		Tooltip.Instance.HideSmallInfo();
	}
}
