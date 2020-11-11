using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Locations.Settlements;
using Ruinarch;
using UnityEngine;
using UnityEngine.UI;

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
        Messenger.AddListener<HexTile>(HexTileSignals.HEXTILE_LEFT_CLICKED, OnTileLeftClicked);
        
        pickPortalMessage.anchoredPosition = new Vector2(0f, -110);
        pickPortalMessage.DOAnchorPosY(110f, 0.5f).SetEase(Ease.OutBack);
    }
    private void OnTileLeftClicked(HexTile hexTile) {
        if (hexTile.CanBuildDemonicStructureHere(STRUCTURE_TYPE.THE_PORTAL)) {
            UIManager.Instance.ShowYesNoConfirmation("Build Portal", "Are you sure you want to build your portal here?", () => PlacePortal(hexTile), showCover: true, layer: 50);
        }
    }
    private void PlacePortal(HexTile hexTile) {
        Debug.Log($"Placed portal at {hexTile}");
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        Messenger.RemoveListener<HexTile>(HexTileSignals.HEXTILE_LEFT_CLICKED, OnTileLeftClicked);
        
        hexTile.SetElevation(ELEVATION.PLAIN);
        LandmarkManager.Instance.CreateNewLandmarkOnTile(hexTile, LANDMARK_TYPE.THE_PORTAL);
        PlayerSettlement playerSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(hexTile);
        playerSettlement.SetName("Demonic Intrusion");
        WorldConfigManager.Instance.mapGenerationData.portal = hexTile;
        PlayerManager.Instance.InitializePlayer(hexTile);
        
        hexTile.StartCorruption();
        LandmarkManager.Instance.PlaceBuiltStructureForSettlement(hexTile.settlementOnTile, hexTile.region.innerMap, hexTile, STRUCTURE_TYPE.THE_PORTAL, RESOURCE.NONE);

        isPickingPortal = false;
        
        pickPortalMessage.DOAnchorPosY(-110f, 0.5f).SetEase(Ease.InBack);

        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
            loadOutMenu.OnClickContinue();
        } else {
            configureLoadoutBtnGO.gameObject.SetActive(true);    
        }
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
        MainMenuManager.Instance.StartGame();
    }
    #endregion

    #region Loadout Menu
    public void OnClickConfigureLoadOut() {
        loadOutMenu.Show();
    }
    #endregion
}
