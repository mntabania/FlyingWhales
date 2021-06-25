using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TipsUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickClose();
    }
    #endregion

    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public TipsUIModel UIModel => _baseAssetModel as TipsUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, TipsUIModel p_assets, Action<TipsUIView> p_onCreate) {
        var go = new GameObject(typeof(TipsUIView).ToString());
        var gui = go.AddComponent<TipsUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        p_onCreate?.Invoke(gui);
    }
    #endregion

    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onCloseClicked += p_listener.OnClickClose;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onCloseClicked -= p_listener.OnClickClose;
    }
    #endregion

    public Transform GetContentParent() {
        return UIModel.scrollViewContent;
    }
}
