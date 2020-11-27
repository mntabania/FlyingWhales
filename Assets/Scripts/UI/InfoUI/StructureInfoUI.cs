using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class StructureInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private LocationPortrait locationPortrait;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI hpLbl;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;

    public LocationStructure activeStructure { get; private set; }

    #region Overrides
    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, LocationStructure>(StructureSignals.ADDED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
        Messenger.AddListener<Character, LocationStructure>(StructureSignals.REMOVED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
    }
    public override void CloseMenu() {
        base.CloseMenu();
        if(activeStructure != null) {
            Selector.Instance.Deselect();
            GameObject structureObject = null;
            if (activeStructure is ManMadeStructure manMadeStructure) {
                structureObject = manMadeStructure.structureObj.gameObject;
            } else if (activeStructure is DemonicStructure demonicStructure) {
                structureObject = demonicStructure.structureObj.gameObject;
            }
            if (structureObject != null && InnerMapCameraMove.Instance.target == structureObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeStructure = null;
    }
    public override void OpenMenu() {
        activeStructure = _data as LocationStructure;
        activeStructure.CenterOnStructure();
        base.OpenMenu();
        activeStructure.ShowSelectorOnStructure();
        UpdateStructureInfoUI();
        UpdateResidents();
    }
    #endregion

    public void UpdateStructureInfoUI() {
        if(activeStructure == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateInfo();
        //UpdateCharacters();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = $"{activeStructure.nameplateName}";
        // if (activeStructure.occupiedHexTile.hexTileOwner != null) {
        //     locationPortrait.SetPortrait(activeStructure.occupiedHexTile.hexTileOwner.landmarkOnTile.landmarkPortrait);    
        // } else {
            locationPortrait.SetPortrait(activeStructure.structureType.GetLandmarkType());
        // }
    }
    private void UpdateInfo() {
        hpLbl.text = $"{activeStructure.currentHP}/{activeStructure.maxHP}";
    }
    private void UpdateResidents() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        List<Character> residents = activeStructure.residents;
        if (residents != null && residents.Count > 0) {
            for (int i = 0; i < residents.Count; i++) {
                Character character = residents[i];
                if (character != null) {
                    GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
                    CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                    item.SetObject(character);
                    item.SetAsDefaultBehaviour();
                }
            }
        }
    }
    #region Listeners
    private void UpdateResidentsFromSignal(Character resident, LocationStructure structure) {
        if (isShowing && activeStructure == structure) {
            UpdateResidents();
        }
    }
    #endregion
    #region For Testing
    public void ShowStructureTestingInfo() {
#if UNITY_EDITOR
        string summary = $"{activeStructure.name} Info:";
        summary += "\nDamage Contributing Objects:";
        for (int i = 0; i < activeStructure.objectsThatContributeToDamage.Count; i++) {
            IDamageable damageable = activeStructure.objectsThatContributeToDamage.ElementAt(i);
            summary += $"\n\t- {damageable}";
        }
        UIManager.Instance.ShowSmallInfo(summary);
#endif
    }
    public void HideStructureTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
