using Ruinarch.MVCFramework;
using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
public class PurchaseSkillUIModel : MVCUIModel {

	public Action onCloseClicked;
	public Action onRerollClicked;
	public Action onHoverOverReroll;
	public Action onHoverOutReroll;
	public Action onClickCancelReleaseAbility;

	public RuinarchButton btnClose;
	public RuinarchButton btnReroll;
	public HoverHandler hoverHandlerReroll;
	
	public RuinarchText txtMessageDisplay;
	public Transform skillsParent;
	
	public Image imgCooldown;
	public GameObject goCover;
	public RuinarchText lblChaoticEnergy;

	[Header("Timer")] 
	public GameObject goReleaseAbilityTimer;
	public TimerItemUI timerReleaseAbility;
	public RuinarchButton btnCancelReleaseAbility;
	public HoverHandler hoverHandlerBtnCancelReleaseAbility;
	public System.Action onHoverOverCancelReleaseAbility;
	public System.Action onHoverOutCancelReleaseAbility;

	[Header("Cover")] 
	public CanvasGroup canvasGroupCover;
	
	[Header("Window")]
	public CanvasGroup canvasGroupMainWindow;
	public RectTransform rectTransformMainWindow;
	
	[Header("Frame")]
	public CanvasGroup canvasGroupFrameGlow;
	public CanvasGroup canvasGroupFrame;
	public RectTransform rectTransformFrame;
	
	[Header("Items")]
	public List<PurchaseSkillItemUI> skillItems = new List<PurchaseSkillItemUI>();
	
	public Vector2 defaultFrameSize { get; private set; }

	void Awake() {
		defaultFrameSize = rectTransformFrame.sizeDelta;
	}
	private void OnEnable() {
		btnClose.onClick.AddListener(ClickClose);
		btnReroll.onClick.AddListener(ClickReroll);
		hoverHandlerReroll.AddOnHoverOverAction(OnHoverOverReroll);
		hoverHandlerReroll.AddOnHoverOutAction(OnHoverOutReroll);
		btnCancelReleaseAbility.onClick.AddListener(OnClickCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.AddOnHoverOverAction(OnHoverOverCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.AddOnHoverOutAction(OnHoverOutCancelReleaseAbility);
	}

	private void OnDisable() {
		btnClose.onClick.RemoveListener(ClickClose);
		btnReroll.onClick.RemoveListener(ClickReroll);
		hoverHandlerReroll.RemoveOnHoverOverAction(OnHoverOverReroll);
		hoverHandlerReroll.RemoveOnHoverOutAction(OnHoverOutReroll);
		btnCancelReleaseAbility.onClick.RemoveListener(OnClickCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.RemoveOnHoverOverAction(OnHoverOverCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.RemoveOnHoverOutAction(OnHoverOutCancelReleaseAbility);
	}

	#region Buttons OnClick trigger
	void ClickClose() {
		onCloseClicked?.Invoke();
	}
	void ClickReroll() {
		onRerollClicked?.Invoke();
	}
	void OnHoverOverReroll() {
		onHoverOverReroll?.Invoke();	
	}
	void OnHoverOutReroll() {
		onHoverOutReroll?.Invoke();	
	}
	void OnClickCancelReleaseAbility() {
		onClickCancelReleaseAbility?.Invoke();
	}
	#endregion

	#region Hover Actions
	private void OnHoverOverCancelReleaseAbility() {
		onHoverOverCancelReleaseAbility?.Invoke();
	}
	private void OnHoverOutCancelReleaseAbility() {
		onHoverOutCancelReleaseAbility?.Invoke();
	}
	#endregion
}