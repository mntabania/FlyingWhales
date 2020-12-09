using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BlackmailUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickClose();
        void OnClickConfirm();
    }
    #endregion
    
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public BlackmailUIModel UIModel => _baseAssetModel as BlackmailUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, BlackmailUIModel p_assets, Action<BlackmailUIView> p_onCreate)
    {
        var go = new GameObject(typeof(BlackmailUIView).ToString());
        var gui = go.AddComponent<BlackmailUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        p_onCreate?.Invoke(gui);
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onCloseClicked += p_listener.OnClickClose;
        UIModel.onClickConfirm += p_listener.OnClickConfirm;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onCloseClicked -= p_listener.OnClickClose;
        UIModel.onClickConfirm -= p_listener.OnClickConfirm;
    }
    #endregion

    public void DisplayBlackmailItems(List<IIntel> p_intel) {
        for (int i = 0; i < UIModel.blackmailUIItems.Length; i++) {
            BlackmailUIItem blackMailItem = UIModel.blackmailUIItems[i];
            IIntel intel = p_intel.ElementAtOrDefault(i);
            if (intel != null) {
                blackMailItem.SetItemDetails(intel);
                blackMailItem.gameObject.SetActive(true);
            } else {
                blackMailItem.gameObject.SetActive(false);    
            }
        }
    }
}
