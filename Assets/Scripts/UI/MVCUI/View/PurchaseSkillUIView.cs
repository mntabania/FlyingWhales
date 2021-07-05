using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using DG.Tweening;

public class PurchaseSkillUIView : MVCUIView {
	#region interface for listener
	public interface IListener {
		void OnRerollClicked();
		void OnCloseClicked();
		void OnHoverOverReroll();
		void OnHoverOutReroll();
		void OnClickCancelReleaseAbility();
		void OnHoverOverCancelReleaseAbility();
		void OnHoverOutCancelReleaseAbility();
	}
	#endregion
	#region MVC Properties and functions to override
	/*
	 * this will be the reference to the model 
	 * */
	public PurchaseSkillUIModel UIModel {
		get {
			return _baseAssetModel as PurchaseSkillUIModel;
		}
	}

	/*
	 * Call this Create method to Initialize and instantiate the UI.
	 * There's a callback on the controller if you want custom initialization
	 * */
	public static void Create(Canvas p_canvas, PurchaseSkillUIModel p_assets, Action<PurchaseSkillUIView> p_onCreate) {
		var go = new GameObject(typeof(PurchaseSkillUIView).ToString());
		var gui = go.AddComponent<PurchaseSkillUIView>();
		var assetsInstance = Instantiate(p_assets);
		gui.Init(p_canvas, assetsInstance);
		if (p_onCreate != null) {
			p_onCreate.Invoke(gui);
		}
	}
	#endregion

	#region user defined functions
	public void DisableRerollButton() {
		UIModel.btnReroll.interactable = false;
	}
	public void EnableRerollButton() {
		UIModel.btnReroll.interactable = true;
	}
	public void ShowSkills() {
		UIModel.skillsParent.gameObject.SetActive(true);
		UIModel.txtMessageDisplay.gameObject.SetActive(false);
	}
	public void HideSkills() {
		UIModel.skillsParent.gameObject.SetActive(false);
		UIModel.txtMessageDisplay.gameObject.SetActive(true);
	}
	public Transform GetSkillsParent() {
		return UIModel.skillsParent;
	}
	public void SetMessage(string p_message) {
		UIModel.txtMessageDisplay.text = p_message;
	}
	public void SetRerollCooldownFill(float p_fill) {
		UIModel.imgCooldown.fillAmount = p_fill;
	}
	public void SetWindowCoverState(bool p_state) {
		// UIModel.goCover.SetActive(p_state);
	}
	public void SetTimerState(bool p_state) {
		UIModel.goReleaseAbilityTimer.SetActive(p_state);
		if (p_state) {
			UIModel.timerReleaseAbility.RefreshName();
		}
	}
	public void SetCurrentChaoticEnergyText(int p_amount) {
		UIModel.lblChaoticEnergy.text = $"Current Chaotic Energy: {UtilityScripts.Utilities.ChaoticEnergyIcon()}{p_amount.ToString()}";
	}
	#endregion

	#region Subscribe/Unsubscribe for IListener
	public void Subscribe(IListener p_listener) {
		UIModel.onCloseClicked += p_listener.OnCloseClicked;
		UIModel.onRerollClicked += p_listener.OnRerollClicked;
		UIModel.onHoverOverReroll += p_listener.OnHoverOverReroll;
		UIModel.onHoverOutReroll += p_listener.OnHoverOutReroll;
		UIModel.onClickCancelReleaseAbility += p_listener.OnClickCancelReleaseAbility;
		UIModel.onHoverOverCancelReleaseAbility += p_listener.OnHoverOverCancelReleaseAbility;
		UIModel.onHoverOutCancelReleaseAbility += p_listener.OnHoverOutCancelReleaseAbility;
	}

	public void Unsubscribe(IListener p_listener) {
		UIModel.onCloseClicked -= p_listener.OnCloseClicked;
		UIModel.onRerollClicked -= p_listener.OnRerollClicked;
		UIModel.onHoverOverReroll -= p_listener.OnHoverOverReroll;
		UIModel.onHoverOutReroll -= p_listener.OnHoverOutReroll;
		UIModel.onClickCancelReleaseAbility -= p_listener.OnClickCancelReleaseAbility;
		UIModel.onHoverOverCancelReleaseAbility -= p_listener.OnHoverOverCancelReleaseAbility;
		UIModel.onHoverOutCancelReleaseAbility -= p_listener.OnHoverOutCancelReleaseAbility;
	}
	#endregion
	
	#region Animations
	private Sequence _showSequence;
    public void PlayShowAnimation() {
	    UIModel.canvasGroupCover.alpha = 0f;
	    UIModel.canvasGroupMainWindow.alpha = 0f;
	    UIModel.canvasGroupFrame.alpha = 1f;
	    Vector2 targetPos = UIModel.rectTransformMainWindow.anchoredPosition;
        UIModel.rectTransformMainWindow.anchoredPosition = new Vector2(targetPos.x, targetPos.y - 100f);
        
        Vector2 defaultSize = UIModel.defaultFrameSize;
        UIModel.rectTransformFrame.sizeDelta = new Vector2(defaultSize.x + 500f, defaultSize.y);
        
        UIModel.canvasGroupFrameGlow.alpha = 0f;
        UIModel.canvasGroupFrameGlow.DOKill();
        UIModel.canvasGroupFrameGlow.DOFade(1f, 2f).SetEase(Ease.OutQuart).SetLoops(-1, LoopType.Yoyo);
        
        _showSequence = DOTween.Sequence();
        _showSequence.Append(UIModel.canvasGroupMainWindow.DOFade(1f, 0.5f));
        _showSequence.Join(UIModel.canvasGroupCover.DOFade(1f, 0.5f));
        _showSequence.Join(UIModel.rectTransformMainWindow.DOAnchorPos(targetPos, 0.5f));
        _showSequence.Join(UIModel.rectTransformFrame.DOSizeDelta(defaultSize, 0.8f));
        _showSequence.AppendInterval(0.02f);
        
        for (int i = 0; i < UIModel.skillItems.Count; i++) {
	        PurchaseSkillItemUI item = UIModel.skillItems[i];
	        _showSequence.Join(item.PrepareAnimation().SetDelay(i/5f));
        }

        _showSequence.Play();
    }
    private Sequence _itemsSequence;
    public void PlayItemsAnimation() {
	    _itemsSequence?.Kill();
	    _itemsSequence = DOTween.Sequence();
	    for (int i = 0; i < UIModel.skillItems.Count; i++) {
		    PurchaseSkillItemUI item = UIModel.skillItems[i];
		    _itemsSequence.Join(item.PrepareAnimation().SetDelay(i/5f));
	    }

	    _itemsSequence.Play();
    }
    public void PlayHideAnimation(System.Action onComplete) {
	    _showSequence?.Kill(true);
	    _showSequence = null;
        Sequence sequence = DOTween.Sequence();
        Vector2 targetFrameSize = UIModel.defaultFrameSize;
        targetFrameSize.x += 1000f;
        
        sequence.Append(UIModel.canvasGroupMainWindow.DOFade(0f, 0.5f));
        sequence.Join(UIModel.canvasGroupCover.DOFade(0f, 0.5f));
        sequence.Join(UIModel.rectTransformFrame.DOSizeDelta(targetFrameSize, 0.7f));
        sequence.Join(UIModel.canvasGroupFrame.DOFade(0f, 0.6f));
        sequence.OnComplete(() => onComplete());
        sequence.Play();
    }
    #endregion
}