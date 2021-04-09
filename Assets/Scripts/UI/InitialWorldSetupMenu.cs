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
    [SerializeField] public GameObject configureLoadoutBtnGO;
    [SerializeField] public GameObject regenerateWorldBtnGO;
    [SerializeField] public GameObject placePortalBtnGO;
    [SerializeField] public RectTransform pickPortalMessage;

	public void Initialize() {
        loadOutMenu.Initialize();
    }
    public void Show() {
        UIManager.Instance.SetSpeedTogglesState(false);
        regenerateWorldBtnGO.SetActive(WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom);
        //placePortalBtnGO.SetActive(true); already disabled this on scene
        configureLoadoutBtnGO.gameObject.SetActive(false);
        
        InnerMapManager.Instance.ShowInnerMap(GridMap.Instance.mainRegion);
        gameObject.SetActive(true);
        UIManager.Instance.DisableContextMenuInteractions(); 
        PlayerUI.Instance.DisableTopMenuButtons();
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        OnClickPlacePortal();
    }
    public void OnClickPlacePortal() {
        //pick tile to place portal.
        placePortalBtnGO.SetActive(false);
        pickPortalMessage.gameObject.SetActive(true);
        // Messenger.AddListener<HexTile>(AreaSignals.AREA_LEFT_CLICKED, OnTileLeftClicked);
        pickPortalMessage.anchoredPosition = new Vector2(0f, -110);
        pickPortalMessage.DOAnchorPosY(110f, 0.5f).SetEase(Ease.OutBack);
        PlayerManager.Instance.AddPlayerInputModule(PlayerManager.pickPortalInputModule);
        InnerMapCameraMove.Instance.SetZoom(InnerMapCameraMove.Instance.maxFOV);
        PlayerManager.pickPortalInputModule.AddOnPortalPlacedAction(OnPortalPlaced);
        PlayerManager.Instance.ShowStructurePlacementVisual(STRUCTURE_TYPE.THE_PORTAL);
    }
    
    private void OnPortalPlaced() {
        PlayerManager.Instance.HideStructurePlacementVisual();
        PlayerManager.pickPortalInputModule.RemoveOnPortalPlacedAction(OnPortalPlaced);
        PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.pickPortalInputModule);
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        regenerateWorldBtnGO.SetActive(false);
        pickPortalMessage.DOAnchorPosY(-110f, 0.5f).SetEase(Ease.InBack);

        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial || WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled) {
            loadOutMenu.OnClickContinue();
        } else {
            configureLoadoutBtnGO.gameObject.SetActive(true);    
        }
    }
    public void Hide() {
        UIManager.Instance.EnableContextMenuInteractions();
        PlayerUI.Instance.EnableTopMenuButtons();
        UIManager.Instance.SetSpeedTogglesState(true);
        InputManager.Instance.SetAllHotkeysEnabledState(true);
        gameObject.SetActive(false);
    }

    #region Reload World
    public void OnClickReloadWorld() {
        UIManager.Instance.ShowYesNoConfirmation("Regenerate World", 
            "Are you sure you want to regenerate a new World?", ReGenerateWorld, showCover: true, layer: 50);
    }
    private void ReGenerateWorld() {
        DOTween.Clear(true);
        // WorldSettings.Instance.worldGenOptionsUIController.ApplyBiomeSettings(); //this is  so that random biomes will be randomized again
        MainMenuManager.Instance.StartGame();
    }
    #endregion

    #region Loadout Menu
    public void OnClickConfigureLoadOut() {
        loadOutMenu.Show();
    }
    #endregion
}
