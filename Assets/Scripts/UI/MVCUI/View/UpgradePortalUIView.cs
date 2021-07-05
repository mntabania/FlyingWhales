using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePortalUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickClose();
        void OnClickUpgrade();
        void OnClickCancelUpgrade();
        void OnHoverOverCancelUpgrade();
        void OnHoverOutCancelUpgrade();
        void OnHoverOverUpgradeChaoticEnergy();
        void OnHoverOutUpgradeChaoticEnergy();
    }
    #endregion
    
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public UpgradePortalUIModel UIModel => _baseAssetModel as UpgradePortalUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, UpgradePortalUIModel p_assets, Action<UpgradePortalUIView> p_onCreate) {
        var go = new GameObject(typeof(UpgradePortalUIView).ToString());
        var gui = go.AddComponent<UpgradePortalUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        if (p_onCreate != null)
        {
            p_onCreate.Invoke(gui);
        }
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onClickClose += p_listener.OnClickClose;
        UIModel.onClickUpgrade += p_listener.OnClickUpgrade;
        UIModel.onClickCancelUpgradePortal += p_listener.OnClickCancelUpgrade;
        UIModel.onHoverOverCancelUpgradePortal += p_listener.OnHoverOverCancelUpgrade;
        UIModel.onHoverOutCancelUpgradePortal += p_listener.OnHoverOutCancelUpgrade;
        UIModel.onHoverOverChaoticEnergyCapacity += p_listener.OnHoverOverUpgradeChaoticEnergy;
        UIModel.onHoverOutChaoticEnergyCapacity += p_listener.OnHoverOutUpgradeChaoticEnergy;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onClickClose -= p_listener.OnClickClose;
        UIModel.onClickUpgrade -= p_listener.OnClickUpgrade;
        UIModel.onClickCancelUpgradePortal -= p_listener.OnClickCancelUpgrade;
        UIModel.onHoverOverCancelUpgradePortal -= p_listener.OnHoverOverCancelUpgrade;
        UIModel.onHoverOutCancelUpgradePortal -= p_listener.OnHoverOutCancelUpgrade;
        UIModel.onHoverOverChaoticEnergyCapacity -= p_listener.OnHoverOverUpgradeChaoticEnergy;
        UIModel.onHoverOutChaoticEnergyCapacity -= p_listener.OnHoverOutUpgradeChaoticEnergy;
    }
    #endregion

    #region User defined functions
    public void UpdateItems(PortalUpgradeTier p_upgradeTier, ThePortal portal) {
        if (portal.IsCurrentLevelUpTheLastOne()) {
            //last level up, do not show items, since will show awaken ruinarch text
            UIModel.scrollRectContent.gameObject.SetActive(false);
        } else {
            UIModel.scrollRectContent.gameObject.SetActive(true);
            for (int i = 0; i < UIModel.items.Length; i++) {
                UpgradePortalItemUI itemUI = UIModel.items[i];
                PLAYER_SKILL_TYPE skillType = p_upgradeTier.skillTypesToUnlock.ElementAtOrDefault(i);
                if (skillType != PLAYER_SKILL_TYPE.NONE) {
                    itemUI.SetData(skillType);
                    itemUI.gameObject.SetActive(true);
                } else {
                    itemUI.gameObject.SetActive(false);
                }
            }    
        }
        
    }
    public void SetHeader(string p_value) {
        UIModel.lblTitle.text = p_value;
    }
    public void SetUpgradeText(string p_value) {
        UIModel.btnUpgrade.SetButtonLabelName(p_value);
    }
    public void SetUpgradeBtnInteractable(bool p_state) {
        UIModel.btnUpgrade.interactable = p_state;
        UIModel.goUpgradeBtnCover.SetActive(!p_state);
    }
    public void AddHoverOverActionToItems(System.Action<UpgradePortalItemUI> p_action) {
        for (int i = 0; i < UIModel.items.Length; i++) {
            UIModel.items[i].AddHoverOverAction(p_action);
        }
    }
    public void AddHoverOutActionToItems(System.Action<UpgradePortalItemUI> p_action) {
        for (int i = 0; i < UIModel.items.Length; i++) {
            UIModel.items[i].AddHoverOutAction(p_action);
        }
    }
    public void SetUpgradeBtnState(bool p_state) {
        UIModel.btnUpgrade.gameObject.SetActive(p_state);
    }
    public void SetUpgradeTimerState(bool p_state) {
        UIModel.goUpgradePortalTimer.SetActive(p_state);
    }
    public void SetChaoticEnergyUpgradeText(int p_amount) {
        UIModel.lblChaoticEnergyCapacity.text = $"{p_amount.ToString()}{UtilityScripts.Utilities.ChaoticEnergyIcon()} Capacity";
    }
    public void SetChaoticEnergyUpgradeGOState(bool p_state) {
        UIModel.goChaoticEnergyCapacity.SetActive(p_state);
    }
    public void SetCurrentSpiritEnergyText(int p_amount) {
        UIModel.lblSpiritEnergy.text = $"Current Spirit Energy: {UtilityScripts.Utilities.SpiritEnergyIcon()}{p_amount.ToString()}";
    }
    #endregion

    #region Animations
    private Sequence _showSequence;
    public void PlayShowAnimation() {
        UIModel.canvasGroupWindow.alpha = 0f;
        UIModel.canvasGroupCover.alpha = 0f;
        Vector2 targetPos = UIModel.rectWindow.anchoredPosition;
        UIModel.rectWindow.anchoredPosition = new Vector2(targetPos.x, targetPos.y - 100f);
        UIModel.canvasGroupUpgradeInteraction.alpha = 0f;
        UIModel.canvasGroupFrame.alpha = 1f;
        Vector2 defaultSize = UIModel.defaultFrameSize;
        UIModel.rectFrame.sizeDelta = new Vector2(defaultSize.x + 500f, defaultSize.y);
        UIModel.canvasGroupFrameGlow.alpha = 0f;
        UIModel.canvasGroupFrameGlow.DOKill();
        UIModel.canvasGroupFrameGlow.DOFade(1f, 2f).SetEase(Ease.OutQuart).SetLoops(-1, LoopType.Yoyo);
        
        _showSequence = DOTween.Sequence();
        _showSequence.Append(UIModel.canvasGroupWindow.DOFade(1f, 0.5f));
        _showSequence.Join(UIModel.canvasGroupCover.DOFade(1f, 0.5f));
        _showSequence.Join(UIModel.rectWindow.DOAnchorPos(targetPos, 0.5f));
        _showSequence.Join(UIModel.rectFrame.DOSizeDelta(defaultSize, 0.8f));
        _showSequence.AppendInterval(0.05f);

        if (UIModel.scrollRectContent.gameObject.activeSelf) {
            int lastIndex = 0;
            for (int i = 0; i < UIModel.items.Length; i++) {
                UpgradePortalItemUI upgradePortalItemUI = UIModel.items[i];
                if (upgradePortalItemUI.gameObject.activeSelf) {
                    Vector2 defaultPos = upgradePortalItemUI.contentParent.anchoredPosition; 
                    float targetPosY = defaultPos.y;
                    upgradePortalItemUI.canvasGroupContent.alpha = 0f;
                    upgradePortalItemUI.SetHoverInteractionAllowedState(false);
                
                    upgradePortalItemUI.contentParent.anchoredPosition = new Vector2(defaultPos.x, defaultPos.y + 50f);
                    _showSequence.Join(upgradePortalItemUI.contentParent.DOAnchorPosY(targetPosY, 0.3f).SetDelay((i + 1)/30f));
                    _showSequence.Join(upgradePortalItemUI.canvasGroupContent.DOFade(1f, 0.4f).OnComplete(() => upgradePortalItemUI.SetHoverInteractionAllowedState(true)));
                    lastIndex = i;
                }
            }
            if (UIModel.contentChaoticEnergyUpgrade.gameObject.activeInHierarchy) {
                Vector2 position = UIModel.contentChaoticEnergyUpgrade.anchoredPosition; 
                float targetPosition = position.y;
                UIModel.canvasGroupChaoticEnergyUpgrade.alpha = 0f;
                
                UIModel.contentChaoticEnergyUpgrade.anchoredPosition = new Vector2(position.x, position.y + 50f);
                _showSequence.Join(UIModel.contentChaoticEnergyUpgrade.DOAnchorPosY(targetPosition, 0.3f).SetDelay((lastIndex + 1)/30f));
                _showSequence.Join(UIModel.canvasGroupChaoticEnergyUpgrade.DOFade(1f, 0.4f));    
            }
        } else if (UIModel.lblAwakenRuinarch.gameObject.activeSelf) {
            Vector2 position = UIModel.lblAwakenRuinarch.rectTransform.anchoredPosition; 
            float targetPosition = position.y;
            UIModel.canvasGroupAwakenRuinarch.alpha = 0f;
            
            UIModel.lblAwakenRuinarch.rectTransform.anchoredPosition = new Vector2(position.x, position.y + 50f);
            _showSequence.Join(UIModel.lblAwakenRuinarch.rectTransform.DOAnchorPosY(targetPosition, 0.3f).SetDelay(1/30f));
            _showSequence.Join(UIModel.canvasGroupAwakenRuinarch.DOFade(1f, 0.4f));
        }
        _showSequence.Append(UIModel.canvasGroupUpgradeInteraction.DOFade(1f, 0.3f));
        _showSequence.Play();
    }
    public void PlayHideAnimation(System.Action onComplete) {
        _showSequence?.Kill(true);
        _showSequence = null;
        Sequence sequence = DOTween.Sequence();
        Vector2 targetFrameSize = UIModel.defaultFrameSize;
        targetFrameSize.x += 1000f;
        
        sequence.Append(UIModel.canvasGroupWindow.DOFade(0f, 0.5f));
        sequence.Join(UIModel.canvasGroupCover.DOFade(0f, 0.5f));
        sequence.Join(UIModel.rectFrame.DOSizeDelta(targetFrameSize, 0.7f));
        sequence.Join(UIModel.canvasGroupFrame.DOFade(0f, 0.6f));
        sequence.OnComplete(() => onComplete());
        sequence.Play();
    }
    #endregion

    public void UpdateAwakenRuianrchText(ThePortal portal) {
        if (portal.IsCurrentLevelUpTheLastOne()) {
            UIModel.lblAwakenRuinarch.gameObject.SetActive(true);
            UIModel.lblAwakenRuinarch.text = "Awaken Ruinarch";
        } else {
            UIModel.lblAwakenRuinarch.gameObject.SetActive(false);
        }
    }
    public void UpdateUpgradeChaoticEnergyItem(ThePortal portal) {
        UIModel.goChaoticEnergyCapacity.SetActive(false);
        // if (portal.IsCurrentLevelUpTheLastOne()) {
        //     UIModel.goChaoticEnergyCapacity.SetActive(false);
        // } else {
        //     UIModel.goChaoticEnergyCapacity.SetActive(true);
        // }
    }
}
