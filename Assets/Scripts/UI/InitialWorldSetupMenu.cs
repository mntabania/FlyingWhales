using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Inner_Maps;
using Locations.Settlements;
using Ruinarch;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class InitialWorldSetupMenu : MonoBehaviour  {

    [SerializeField] public SkillTreeSelector loadOutMenu;
    [SerializeField] public Button regenerateWorldBtn;
    [SerializeField] public Button configureLoadoutBtn;
    [SerializeField] public GameObject configureLoadoutBtnGO;
    [SerializeField] public GameObject regenerateWorldBtnGO;
    [SerializeField] public RectTransform pickPortalMessage;

    public bool isPickingPortal;
    
    public void Initialize() {
        loadOutMenu.Initialize();
        isPickingPortal = false;
    }
    public void Show() {
        isPickingPortal = true;
        UIManager.Instance.SetSpeedTogglesState(false);
        regenerateWorldBtnGO.SetActive(WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom);
        gameObject.SetActive(true);
        //pick tile to place portal.
        configureLoadoutBtnGO.gameObject.SetActive(false);
        pickPortalMessage.gameObject.SetActive(true);
        // throw new NotImplementedException();
        // Messenger.AddListener<HexTile>(AreaSignals.AREA_LEFT_CLICKED, OnTileLeftClicked);
        
        pickPortalMessage.anchoredPosition = new Vector2(0f, -110);
        pickPortalMessage.DOAnchorPosY(110f, 0.5f).SetEase(Ease.OutBack);

        List<Area> choices = GridMap.Instance.allAreas.Where(a => !a.structureComponent.HasStructureInArea() && a.elevationType == ELEVATION.PLAIN).ToList();
        
        PlacePortal(CollectionUtilities.GetRandomElement(choices));
    }
    // private void OnTileLeftClicked(Area p_area) {
    //     if (p_area.CanBuildDemonicStructureHere(STRUCTURE_TYPE.THE_PORTAL)) {
    //         UIManager.Instance.ShowYesNoConfirmation("Build Portal", "Are you sure you want to build your portal here?", () => PlacePortal(p_area), showCover: true, layer: 50);
    //     }
    // }
    private void PlacePortal(Area p_area) {
        Debug.Log($"Placed portal at {p_area}");
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        // Messenger.RemoveListener<HexTile>(AreaSignals.AREA_LEFT_CLICKED, OnTileLeftClicked);
        
        p_area.SetElevation(ELEVATION.PLAIN);
        PlayerSettlement playerSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(p_area);
        playerSettlement.SetName("Demonic Intrusion");
        WorldConfigManager.Instance.mapGenerationData.portal = p_area;
        PlayerManager.Instance.InitializePlayer(p_area);
        
        p_area.gridTileComponent.StartCorruption(p_area);
        LandmarkManager.Instance.PlaceBuiltStructureForSettlement(p_area.settlementOnArea, p_area.region.innerMap, p_area, STRUCTURE_TYPE.THE_PORTAL, RESOURCE.NONE);

        isPickingPortal = false;
        
        pickPortalMessage.DOAnchorPosY(-110f, 0.5f).SetEase(Ease.InBack);

        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial || WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled) {
            loadOutMenu.OnClickContinue();
        } else {
            configureLoadoutBtnGO.gameObject.SetActive(true);    
        }
        InnerMapManager.Instance.ShowInnerMap(p_area.region);
    }
    public void Hide() {
        UIManager.Instance.SetSpeedTogglesState(true);
        gameObject.SetActive(false);
    }

    #region Reload World
    public void OnClickReloadWorld() {
        UIManager.Instance.ShowYesNoConfirmation("Regenerate World", 
            "Are you sure you want to regenerate a new World?", ReGenerateWorld);
    }
    private void ReGenerateWorld() {
        DOTween.Clear(true);
        WorldSettings.Instance.worldGenOptionsUIController.ApplyBiomeSettings(); //this is  so that random biomes will be randomized again
        MainMenuManager.Instance.StartGame();
    }
    #endregion

    #region Loadout Menu
    public void OnClickConfigureLoadOut() {
        loadOutMenu.Show();
    }
    #endregion
}
