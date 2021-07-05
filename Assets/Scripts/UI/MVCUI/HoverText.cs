using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HoverHandler))]
[RequireComponent(typeof(UIHoverPosition))]
public class HoverText : MonoBehaviour
{
    public string hoverDisplayText;

    [SerializeField] private HoverHandler m_hoverHandler;
	[SerializeField] private UIHoverPosition m_uiHoverPosition;

	private void Awake() {
		if (m_hoverHandler == null) {
			m_hoverHandler = GetComponent<HoverHandler>();
			if(m_hoverHandler == null) {
				m_hoverHandler = gameObject.AddComponent<HoverHandler>();
			}
		}

		if (m_uiHoverPosition == null) {
			m_uiHoverPosition = GetComponent<UIHoverPosition>();
			if (m_uiHoverPosition == null) {
				m_uiHoverPosition = gameObject.AddComponent<UIHoverPosition>();
			}	
		}
		
	}

	private void Start() {
		m_hoverHandler.SetToolTipPosition(m_uiHoverPosition);
		m_hoverHandler.AddOnHoverOutAction(OnHoverOut);
		m_hoverHandler.AddOnHoverOverAction(OnHoverOver);
	}

	private void OnDestroy() {
		m_hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
		m_hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
	}
	public void SetText(string p_newText) {
		hoverDisplayText = p_newText;
	}

	public void Enable() {
		enabled = true;
		m_hoverHandler.enabled = true;
	}
	public void Disable() {
		enabled = false;
		m_hoverHandler.enabled = false;
	}
	
	public void OnHoverOver() {
		Tooltip.Instance.ShowSmallInfo(hoverDisplayText, autoReplaceText: false);
	}

	public void OnHoverOut() {
		Tooltip.Instance.HideSmallInfo();
	}
}
