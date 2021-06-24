using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PiercingAndResistancesInfo : MonoBehaviour {
    [Header("Piercing")]
    [SerializeField] private TextMeshProUGUI piercingTxt;

    [Space(10)]
    [Header("Resistances")]
    //[SerializeField] private TextMeshProUGUI normalResistanceTxt;
    [SerializeField] private TextMeshProUGUI fireResistanceTxt;
    [SerializeField] private TextMeshProUGUI poisonResistanceTxt;
    [SerializeField] private TextMeshProUGUI waterResistanceTxt;
    [SerializeField] private TextMeshProUGUI iceResistanceTxt;
    [SerializeField] private TextMeshProUGUI electricResistanceTxt;
    [SerializeField] private TextMeshProUGUI earthResistanceTxt;
    [SerializeField] private TextMeshProUGUI windResistanceTxt;
    [SerializeField] private TextMeshProUGUI mentalResistanceTxt;
    [SerializeField] private TextMeshProUGUI physicalResistanceTxt;

    private Character _character;

    #region getters
    public bool isShowing => gameObject.activeSelf;
    #endregion

    public void Initialize() {
        Messenger.AddListener<Character>(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, UpdatePiercingAndResistancesFromSignal);
    }

    #region Listeners
    private void UpdatePiercingAndResistancesFromSignal(Character p_character) {
        if (isShowing && _character == p_character) {
            UpdatePiercingAndResistancesInfo();
        }
    }
    #endregion

    public void UpdatePierceUI(Character p_character) {
        _character = p_character;
        UpdatePiercingAndResistancesInfo();
    }

    #region General
    public void ShowPiercingAndResistancesInfo(Character p_character) {
        _character = p_character;
        UpdatePiercingAndResistancesInfo();
        gameObject.SetActive(true);
    }
    public void HidePiercingAndResistancesInfo() {
        _character = null;
        gameObject.SetActive(false);
    }
    private void UpdatePiercingAndResistancesInfo() {
        //UpdatePiercingInfo();
        UpdateResistancesInfo();
    }
    private void UpdatePiercingInfo() {
        piercingTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.piercingPower);  
    }
    private void UpdateResistancesInfo() {
        //normalResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Normal));
        fireResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Fire));
        poisonResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Poison));
        waterResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Water));
        iceResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Ice));
        electricResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Electric));
        earthResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Earth));
        windResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Wind));
        mentalResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Mental));
        physicalResistanceTxt.text = TransformToPercentString(_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Physical));
    }
    private string TransformToPercentString(float p_value) {
        return p_value.ToString("N0") + "%";
    }
    #endregion
}
