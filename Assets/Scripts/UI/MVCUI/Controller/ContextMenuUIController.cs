using System.Collections.Generic;
using Ruinarch;
using UnityEngine;
using Ruinarch.MVCFramework;
using UtilityScripts;

public class ContextMenuUIController : MVCUIController, ContextMenuUIView.IListener {
	[SerializeField] private ContextMenuUIModel m_contextMenuUIModel;
	[SerializeField] private Camera m_camera;
	
	private ContextMenuUIView m_contextMenuUIView;
	private Transform m_transformToFollow;
	private Vector3 m_vectorPositionToFollow;
	private bool m_hasVectorPositionToFollow;
	private bool m_isScreenVectorPosition;
	private System.Action<IContextMenuItem, UIHoverPosition> _onHoverOverAction;
	private System.Action<IContextMenuItem> _onHoverOutAction;

    public IContextMenuItem currentlyOpenedParentContextItem { get; private set; }

	private void OnEnable() {
		ContextMenuUIObject.onMenuPress += OnMenuClicked;
		ContextMenuUIObject.onHoverOverItem += OnMenuHoveredOver;
		ContextMenuUIObject.onHoverOutItem += OnMenuHoveredOut;
	}

	private void OnDisable() {
		ContextMenuUIObject.onMenuPress -= OnMenuClicked;
		ContextMenuUIObject.onHoverOverItem -= OnMenuHoveredOver;
		ContextMenuUIObject.onHoverOutItem -= OnMenuHoveredOut;
	}
	private void Awake() {
		InstantiateUI();
		HideUI();
	}

	private void Start() {
		Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
	}
	public void SetOnHoverOverAction(System.Action<IContextMenuItem, UIHoverPosition> p_onHoverOverAction) {
		_onHoverOverAction = p_onHoverOverAction;
	}
	public void SetOnHoverOutAction(System.Action<IContextMenuItem> p_onHoverOutAction) {
		_onHoverOutAction = p_onHoverOutAction;
	}
	// private void LateUpdate() {
	// 	if (m_hasVectorPositionToFollow) {
	// 		if (m_isScreenVectorPosition) {
	// 			m_contextMenuUIView.SetPosition(m_vectorPositionToFollow, _canvas);
	// 		} else {
	// 			Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(m_camera, m_vectorPositionToFollow);
	// 			m_contextMenuUIView.SetPosition(screenPos, _canvas);	
	// 		}
	// 	} else if (!ReferenceEquals(m_transformToFollow, null)) {
 //            //follow current target
 //            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(m_camera, m_transformToFollow.position); 
 //            m_contextMenuUIView.SetPosition(screenPos, _canvas);
	// 	}
	// }
	public override void HideUI() {
		base.HideUI();
		m_contextMenuUIView.HideColumn(1);
	}
	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		ContextMenuUIView.Create(_canvas, m_contextMenuUIModel, (p_ui) => {
			m_contextMenuUIView = p_ui;
			m_contextMenuUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}
	
	private void OnMenuClicked(IContextMenuItem p_UIMenu, bool p_isAction, int p_currentColumn) {
        if (!p_isAction) {
            if (p_UIMenu.CanBePickedRegardlessOfCooldown()) {
                currentlyOpenedParentContextItem = p_UIMenu;
				bool dontShowName = false;
				if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character targetCharacter) {
					if (p_UIMenu.contextMenuName == "Trigger Flaw" && !targetCharacter.isInfoUnlocked) {
						dontShowName = true;
					}
				}
				m_contextMenuUIView.DisplaySubMenu(p_UIMenu.subMenus, p_currentColumn + 1, _canvas , dontShowName);
			}
		} else {
			p_UIMenu.OnPickAction();
		}
	}
	private void OnMenuHoveredOver(IContextMenuItem p_UIMenu, bool p_isAction, int p_currentColumn) {
		// if (!p_isAction) {
		// 	if (p_UIMenu.CanBePickedRegardlessOfCooldown()) {
		// 		m_contextMenuUIView.DisplaySubMenu(p_UIMenu.subMenus, p_currentColumn + 1, _canvas);
		// 	}
		// } else {
		// 	m_contextMenuUIView.HideColumn(p_currentColumn + 1);
		// }
		_onHoverOverAction?.Invoke(p_UIMenu, m_contextMenuUIView.GetTooltipHoverPositionToUse());
	}
	private void OnMenuHoveredOut(IContextMenuItem p_UIMenu, bool p_isAction, int p_currentColumn) {
		_onHoverOutAction?.Invoke(p_UIMenu);
	}
	public void ShowContextMenu(List<IContextMenuItem> p_initialItems, Vector3 p_screenPos, string p_title, InputManager.Cursor_Type p_cursorType) {
		m_contextMenuUIView.HideColumn(1);
		ShowUI();
		m_contextMenuUIView.InitializeUI(p_initialItems, _canvas);
		m_contextMenuUIView.SetPosition(p_screenPos, _canvas);
		RectTransform parentDisplayTransform = m_contextMenuUIView.UIModel.parentDisplay.transform as RectTransform;
		GameUtilities.PositionTooltip(p_screenPos,  m_contextMenuUIView.UIModel.parentDisplay.gameObject, parentDisplayTransform, parentDisplayTransform, p_cursorType, _canvas.transform as RectTransform);
		m_contextMenuUIView.SetTitleName(p_title);	
	}
	public void ShowContextMenu(List<IContextMenuItem> p_initialItems, string p_title) {
		m_contextMenuUIView.HideColumn(1);
		ShowUI();
		m_contextMenuUIView.InitializeUI(p_initialItems, _canvas);
		m_contextMenuUIView.SetTitleName(p_title);	
	}
	public void UpdateContextMenuItems(List<IContextMenuItem> p_initialItems) {
		m_contextMenuUIView.InitializeUI(p_initialItems, _canvas);
	}
	public void SetFollowPosition(Vector3 p_pos, bool p_isScreenPosition) {
		m_vectorPositionToFollow = p_pos;
		m_isScreenVectorPosition = p_isScreenPosition;
		m_hasVectorPositionToFollow = true;
		m_transformToFollow = null;
	}
	public void SetFollowPosition(Transform p_transformToFollow) {
		m_transformToFollow = p_transformToFollow;
		m_vectorPositionToFollow = Vector3.zero;
		m_hasVectorPositionToFollow = false;
	}
	public bool IsShowing() {
		return m_contextMenuUIView.UIModel.parentDisplay.gameObject.activeSelf;
	}
	public void OnHoverOverParentDisplay() { }
	public void OnHoverOutParentDisplay() {
		// m_contextMenuUIView.HideColumn(1);
	}

	private void OnReceiveKeyCodeSignal(KeyCode p_key) {
		if (p_key == KeyCode.Mouse1) {
			HideUI();
		}
	}
}