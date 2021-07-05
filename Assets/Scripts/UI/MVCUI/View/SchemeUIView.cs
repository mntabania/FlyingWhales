using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;

public class SchemeUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickClose();
        void OnClickConfirm();
        void OnClickBlackmail();
        void OnClickTemptation();
        void OnHoverOverBlackmailBtn(UIHoverPosition p_hoverPos);
        void OnHoverOverTemptBtn(UIHoverPosition p_hoverPos);
        void OnHoverOverSuccessRate();
        void OnHoverOutBlackmailBtn();
        void OnHoverOutTemptBtn();
        void OnHoverOutSuccessRate();
    }
    #endregion

    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public SchemeUIModel UIModel => _baseAssetModel as SchemeUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, SchemeUIModel p_assets, Action<SchemeUIView> p_onCreate)
    {
        var go = new GameObject(typeof(SchemeUIView).ToString());
        var gui = go.AddComponent<SchemeUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        p_onCreate?.Invoke(gui);
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onCloseClicked += p_listener.OnClickClose;
        UIModel.onClickConfirm += p_listener.OnClickConfirm;
        UIModel.onClickBlackmail += p_listener.OnClickBlackmail;
        UIModel.onClickTemptation += p_listener.OnClickTemptation;
        UIModel.onHoverOverBlackmailBtn += p_listener.OnHoverOverBlackmailBtn;
        UIModel.onHoverOutBlackmailBtn += p_listener.OnHoverOutBlackmailBtn;
        UIModel.onHoverOverTemptBtn += p_listener.OnHoverOverTemptBtn;
        UIModel.onHoverOutTemptBtn += p_listener.OnHoverOutTemptBtn;
        UIModel.onHoverOverSuccessRate += p_listener.OnHoverOverSuccessRate;
        UIModel.onHoverOutSuccessRate += p_listener.OnHoverOutSuccessRate;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onCloseClicked -= p_listener.OnClickClose;
        UIModel.onClickConfirm -= p_listener.OnClickConfirm;
        UIModel.onClickBlackmail -= p_listener.OnClickBlackmail;
        UIModel.onClickTemptation -= p_listener.OnClickTemptation;
        UIModel.onHoverOverBlackmailBtn -= p_listener.OnHoverOverBlackmailBtn;
        UIModel.onHoverOutBlackmailBtn -= p_listener.OnHoverOutBlackmailBtn;
        UIModel.onHoverOverTemptBtn -= p_listener.OnHoverOverTemptBtn;
        UIModel.onHoverOutTemptBtn -= p_listener.OnHoverOutTemptBtn;
        UIModel.onHoverOverSuccessRate -= p_listener.OnHoverOverSuccessRate;
        UIModel.onHoverOutSuccessRate -= p_listener.OnHoverOutSuccessRate;
    }
    #endregion

    #region user defined functions
    public void SetSuccessRate(string p_successRate) {
        UIModel.txtSuccessRate.text = p_successRate;
    }
    public void SetTitle(string p_title) {
        UIModel.txtTitle.text = p_title;
    }
    public void SetBlackmailBtnInteractableState(bool p_state) {
        UIModel.btnBlackmail.interactable = p_state;
    }
    public void SetTemptBtnInteractableState(bool p_state) {
        UIModel.btnTemptation.interactable = p_state;
    }
    #endregion
}
