using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TemptUIView : MVCUIView {
    private TEMPTATION[] _allTemptationTypes;
    
    #region interface for listener
    public interface IListener {
        void OnToggleDarkBlessing(bool p_isOn);
        void OnToggleEmpower(bool p_isOn);
        void OnToggleCleanseFlaws(bool p_isOn);
        void OnClickClose();
        void OnClickConfirm();
    }
    #endregion
    
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public TemptUIModel UIModel => _baseAssetModel as TemptUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, TemptUIModel p_assets, Action<TemptUIView> p_onCreate)
    {
        var go = new GameObject(typeof(TemptUIView).ToString());
        var gui = go.AddComponent<TemptUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        p_onCreate?.Invoke(gui);
    }
    #endregion

    private void Awake() {
        _allTemptationTypes = UtilityScripts.CollectionUtilities.GetEnumValues<TEMPTATION>();
    }

    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onToggleDarkBlessing += p_listener.OnToggleDarkBlessing;
        UIModel.onToggleEmpower += p_listener.OnToggleEmpower;
        UIModel.onToggleCleanseFlaws += p_listener.OnToggleCleanseFlaws;
        UIModel.onClickClose += p_listener.OnClickClose;
        UIModel.onClickConfirm += p_listener.OnClickConfirm;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onToggleDarkBlessing -= p_listener.OnToggleDarkBlessing;
        UIModel.onToggleEmpower -= p_listener.OnToggleEmpower;
        UIModel.onToggleCleanseFlaws -= p_listener.OnToggleCleanseFlaws;
        UIModel.onClickClose -= p_listener.OnClickClose;
        UIModel.onClickConfirm -= p_listener.OnClickConfirm;
    }
    #endregion

    public void UpdateShownItems(Character p_target) {
        for (int i = 0; i < _allTemptationTypes.Length; i++) {
            TEMPTATION temptation = _allTemptationTypes[i];
            RuinarchToggle toggle = GetTemptationToggle(temptation);
            bool canTemptCharacter = temptation.CanTemptCharacter(p_target);
            toggle.gameObject.SetActive(canTemptCharacter);
        }
    }

    private RuinarchToggle GetTemptationToggle(TEMPTATION p_temptationType) {
        switch (p_temptationType) {
            case TEMPTATION.Dark_Blessing:
                return UIModel.tglDarkBlessing;
            case TEMPTATION.Empower:
                return UIModel.tglEmpower;
            case TEMPTATION.Cleanse_Flaws:
                return UIModel.tglCleanseFlaws;
            default:
                throw new ArgumentOutOfRangeException(nameof(p_temptationType), p_temptationType, null);
        }
    }
}
