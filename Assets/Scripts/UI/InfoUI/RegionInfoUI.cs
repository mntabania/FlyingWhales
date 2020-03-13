using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Pathfinding.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RegionInfoUI : InfoUIBase {

    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI regionNameLbl;
    [SerializeField] private TextMeshProUGUI regionTypeLbl;
    [SerializeField] private LocationPortrait locationPortrait;

    [Header("Main")]
    [SerializeField] private TextMeshProUGUI descriptionLbl;
    [SerializeField] private TextMeshProUGUI featuresLbl;

    [Header("Characters")]
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject characterItemPrefab;

    [Header("Events")]
    [SerializeField] private GameObject worldEventNameplatePrefab;
    [SerializeField] private ScrollRect worldEventsScrollView;

    [Header("Invasion")]
    [SerializeField] private Button invadeBtn;
    [SerializeField] private Image invadeProgress;
    
    [Header("Invasion Confirmation")]
    [FormerlySerializedAs("invConfrimationGO")] [SerializeField] private GameObject invConfirmationGO;
    [FormerlySerializedAs("invConfrimationTitleLbl")] [SerializeField] private TextMeshProUGUI invConfirmationTitleLbl;

    [Header("Demolition")]
    [SerializeField] private Button demolishBtn;

    [Header("Building")]
    [SerializeField] private Button buildBtn;
    [SerializeField] private Image buildProgress;

    [Header("Demonic Landmark")]
    [SerializeField] private PlayerBuildLandmarkUI playerBuildLandmarkUI;
    [SerializeField] private PlayerResearchUI playerResearchUI;
    [SerializeField] private TheProfaneUI theProfaneUI;
    [SerializeField] private PlayerSummonMinionUI playerSummonMinionUI;
    [SerializeField] private PlayerUpgradeUI playerUpgradeUI;
    [SerializeField] private TheEyeUI theEyeUI;
    [SerializeField] private TheNeedlesUI needlesUI;
    public TheFingersUI fingersUI;

    public Region activeRegion { get; private set; }
    private List<WorldEventNameplate> activeWorldEventNameplates = new List<WorldEventNameplate>();

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.AddListener<Region>(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, ShowAppropriateContentOnSignal);
    }

    public override void OpenMenu() {
        Region previousRegion = activeRegion;
        previousRegion?.HideBorders();
        activeRegion = _data as Region;
        base.OpenMenu();
        UpdateBasicInfo();
        UpdateRegionInfo();
        UpdateCharacters();
        UpdateEventInfo();
        //ShowAppropriateContentOnOpen();
    }
    public override void CloseMenu() {
        activeRegion.HideBorders();
        activeRegion = null;
        base.CloseMenu();
    }

    public void UpdateInfo() {
        UpdateBasicInfo();
        UpdateRegionInfo();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        // locationPortrait.SetLocation(activeRegion);
        regionNameLbl.text = activeRegion.name;
        regionTypeLbl.text = string.Empty;
        // regionTypeLbl.text = activeRegion.mainLandmark.specificLandmarkType.LandmarkToString();
    }
    #endregion

    #region Main
    private void UpdateRegionInfo() {
        descriptionLbl.text = activeRegion.description;
        featuresLbl.text = string.Empty;

        // if (activeRegion.features.Count == 0) {
        //     featuresLbl.text = $"{featuresLbl.text}None";
        // } else {
        //     for (int i = 0; i < activeRegion.features.Count; i++) {
        //         TileFeature feature = activeRegion.features[i];
        //         if (i != 0) {
        //             featuresLbl.text = $"{featuresLbl.text}, ";
        //         }
        //         featuresLbl.text = $"{featuresLbl.text}<link=\"{i}\">{feature.name}</link>";
        //     }
        // }
    }
    public void OnHoverFeature(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            // UIManager.Instance.ShowSmallInfo(activeRegion.features[index].description);
        }
    }
    public void OnHoverExitFeature() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Characters
    private void OnCharacterEnteredRegion(Character character, Region region) {
        if (region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void OnCharacterExitedRegion(Character character, Region region) {
        if (region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void UpdateCharacters() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        for (int i = 0; i < activeRegion.charactersAtLocation.Count; i++) {
            Character character = activeRegion.charactersAtLocation[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, charactersScrollView.content);
            CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
            item.SetObject(character);
            item.SetAsDefaultBehaviour();
        }
        OrderCharacterItems();
    }
    public void OrderCharacterItems() {
        List<CharacterNameplateItem> visitors = new List<CharacterNameplateItem>();

        List<CharacterNameplateItem> residents = new List<CharacterNameplateItem>();
        CharacterNameplateItem[] characterItems = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < characterItems.Length; i++) {
            CharacterNameplateItem currItem = characterItems[i];
            if (currItem.character.homeRegion != null && activeRegion.id == currItem.character.homeRegion.id) {
                residents.Add(currItem);
            } else {
                visitors.Add(currItem);
            }
        }

        List<CharacterNameplateItem> orderedVisitors = new List<CharacterNameplateItem>(visitors.OrderByDescending(x => x.character.level));
        List<CharacterNameplateItem> orderedResidents = new List<CharacterNameplateItem>(residents.OrderByDescending(x => x.character.level));

        List<CharacterNameplateItem> orderedItems = new List<CharacterNameplateItem>();
        orderedItems.AddRange(orderedVisitors);
        orderedItems.AddRange(orderedResidents);
        
        for (int i = 0; i < orderedItems.Count; i++) {
            CharacterNameplateItem currItem = orderedItems[i];
            currItem.transform.SetSiblingIndex(i);
        }
    }
    #endregion

    #region Invade
    private Minion chosenMinionToInvade;
//    private void UpdateMainBtnState() {
//        if (activeRegion.coreTile.isCorrupted) {
//            invadeBtn.gameObject.SetActive(false);
//            invadeProgress.gameObject.SetActive(false);
//            if (activeRegion.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
//                //building
//                buildBtn.gameObject.SetActive(true);
//                invadeBtn.gameObject.SetActive(false);
//                buildBtn.interactable = false;
//                buildProgress.gameObject.SetActive(true);
//                buildProgress.fillAmount = ((float)activeRegion.demonicBuildingData.currentDuration / (float)activeRegion.demonicBuildingData.buildDuration);
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
//                //if active region is corrupted and landmark type is none
//                //show build button
//                buildBtn.gameObject.SetActive(true);
//                buildProgress.gameObject.SetActive(false);
//                buildBtn.interactable = true;
//                demolishBtn.gameObject.SetActive(false);
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL) {
//                //if active region is corrupted and landmark is the portal, just show demolish button, but do not allow interaction
//                demolishBtn.gameObject.SetActive(true);
//                demolishBtn.interactable = false;
//                buildBtn.gameObject.SetActive(false);
//                buildProgress.gameObject.SetActive(false);
//            } else {
//                //if the active region is corrupted and is not the demonic portal, show the demolish button
//                demolishBtn.gameObject.SetActive(true);
//                demolishBtn.interactable = true;
//                buildBtn.gameObject.SetActive(false);
//                buildProgress.gameObject.SetActive(false);
//            }
//        } else {
//            invadeBtn.gameObject.SetActive(true);
//            demolishBtn.gameObject.SetActive(false);
//            buildBtn.gameObject.SetActive(false);
//            invadeBtn.interactable = activeRegion.CanBeInvaded();
//            if (activeRegion.demonicInvasionData.beingInvaded) {
//                //invading
//                invadeProgress.gameObject.SetActive(true);
//                invadeProgress.fillAmount = ((float)activeRegion.demonicInvasionData.currentDuration / (float)activeRegion.mainLandmark.invasionTicks);
//            } else {
//                buildProgress.gameObject.SetActive(false);
//                invadeProgress.gameObject.SetActive(false);
//            }
//        }
//    }
    public void OnClickInvade() {
        // if (activeRegion.npcSettlement != null) {
        //     //simulate as if clicking the invade button while inside the are map
        //     InnerMapManager.Instance.ShowInnerMap(activeRegion);
        //     StartSettlementInvasion(activeRegion.npcSettlement);
        //     LoadActions();
        // } else {
        //     chosenMinionToInvade = null;
        //     UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), onClickAction: ChooseMinionForInvasion, validityChecker: CanMinionInvade,
        //         title: "Invasion (" + ((int)activeRegion.mainLandmark.invasionTicks / (int)GameManager.ticksPerHour).ToString() + " hours)\nChoose a minion that will invade " + activeRegion.name + ". NOTE: That minion will be unavailable while the invasion is ongoing.",
        //         onHoverAction: OnHoverEnterMinionInvade, onHoverExitAction: OnHoverExitMinionInvade,
        //         showCover: true, layer: 25);
        // }
        
    }
    private bool CanMinionInvade(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INVADER);
    }
    private void OnHoverEnterMinionInvade(Character character) {
        if (!CanMinionInvade(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = $"{character.name} is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INVADER)) {
                message = $"{character.name} does have not the required trait: Invader";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinionInvade(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void ChooseMinionForInvasion(object c) {
        Character character = c as Character;
        chosenMinionToInvade = character.minion;
        StartInvasion();
        UIManager.Instance.HideObjectPicker();
        
    }
    public void StartInvasion() {
        activeRegion.StartInvasion(chosenMinionToInvade);
        HideStartInvasionConfirmation();
        // LoadActions();
    }
    public void HideStartInvasionConfirmation() {
        chosenMinionToInvade = null;
        invConfirmationGO.SetActive(false);
    }
    public void StopSettlementInvasion() {
        
    }
    #endregion

    #region Demolish
    public void OnClickDemolish() {
        Region region = activeRegion;
        LandmarkManager.Instance.CreateNewLandmarkOnTile(region.coreTile, LANDMARK_TYPE.NONE);
        UIManager.Instance.ShowRegionInfo(region);
    }
    #endregion

    #region Events
    private void UpdateEventInfo() {
        if(worldEventsScrollView.content.childCount > 0) {
            UtilityScripts.Utilities.DestroyChildren(worldEventsScrollView.content);
        }
    }
    private void GenerateWorldEventNameplate(Region region) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(worldEventNameplatePrefab.name, Vector3.zero, Quaternion.identity, worldEventsScrollView.content);
        WorldEventNameplate item = go.GetComponent<WorldEventNameplate>();
        item.SetObject(region);
        activeWorldEventNameplates.Add(item);
    }
    #endregion

    #region Demonic Landmarks
    private void ShowAppropriateContentOnSignal(Region region) {
        if (region == activeRegion) {
            // LoadActions();
        }
    }
    //private void ShowAppropriateContentOnOpen() {
    //    ////Always show Overview tab first upon opening of Region Info UI
    //    //if (!overviewTabToggle.isOn) {
    //    //    overviewTabToggle.isOn = true;
    //    //}
    //    //OnDemonicToggleStateChanged(overviewTabToggle.isOn);
    //    ////if (overviewTabToggle.isOn) {
    //    ////    //UpdateDemonicLandmarkToggleState();
    //    ////    OnDemonicToggleStateChanged(overviewTabToggle.isOn);
    //    ////}
    //}
//    private void UpdateAppropriateContentPerUpdateUI() {
//        if (playerBuildLandmarkUI.gameObject.activeSelf) {
//            UpdatePlayerBuildLandmarkUI();
//        } else if (playerResearchUI.gameObject.activeSelf) {
//            UpdatePlayerResearchUI();
//        } else if (theProfaneUI.gameObject.activeSelf) {
//            UpdateTheProfaneUI();
//        } else if (playerUpgradeUI.gameObject.activeSelf) {
//            UpdatePlayerUpgradeUI();
//        } else if (playerSummonMinionUI.gameObject.activeSelf) {
//            UpdatePlayerSummonMinionUI();
//        } else if (needlesUI.gameObject.activeSelf) {
//            UpdateTheNeedlesUI();
//        } else if (theEyeUI.gameObject.activeSelf) {
//            UpdateTheEyeUI();
//        } else if (fingersUI.gameObject.activeSelf) {
//            UpdateTheFingersUI();
//        }
//    }
//    public void OnDemonicToggleStateChanged(bool isOn) {
//        overviewGO.SetActive(isOn);
//        if (isOn) {
//            HidePlayerBuildLandmarkUI();
//            HidePlayerResearchUI();
//            HideTheProfaneUI();
//            HidePlayerUpgradeUI();
//            HidePlayerSummonMinionUI();
//            HideTheEyeUI();
//            HideTheNeedlesUI();
//            HideTheFingersUI();
//            //activate the neeeded UI for the tab
//            if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE && activeRegion.coreTile.isCorrupted) {
//                ShowPlayerBuildLandmarkUI();
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_SPIRE) {
//                ShowPlayerResearchUI();
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE) {
//                ShowTheProfaneUI();
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_ANVIL) {
//                ShowPlayerUpgradeUI();
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL) {
//                ShowPlayerSummonMinionUI();
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_EYE) {
//                ShowTheEyeUI();
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_NEEDLES) {
//                ShowTheNeedlesUI();
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_FINGERS) {
//                ShowTheFingersUI();
//            }
//        } else {
//            //deactivate the UI for the tab
//            HidePlayerBuildLandmarkUI();
//            HidePlayerResearchUI();
//            HideTheProfaneUI();
//            HidePlayerUpgradeUI();
//            HidePlayerSummonMinionUI();
//            HideTheEyeUI();
//            HideTheNeedlesUI();
//            HideTheFingersUI();
//        }
//    }

//    #endregion

//    #region Player Build Landmark Content
//    private void ShowPlayerBuildLandmarkUI() {
//        playerBuildLandmarkUI.ShowPlayerBuildLandmarkUI(activeRegion.coreTile);
//    }
//    private void HidePlayerBuildLandmarkUI() {
//        playerBuildLandmarkUI.HidePlayerBuildLandmarkUI();
//    }
//    private void UpdatePlayerBuildLandmarkUI() {
//        playerBuildLandmarkUI.UpdatePlayerBuildLandmarkUI();
//    }
//    #endregion

//    #region Player Research Content
//    private void ShowPlayerResearchUI() {
//        playerResearchUI.ShowPlayerResearchUI(activeRegion.mainLandmark as TheSpire);
//    }
//    private void HidePlayerResearchUI() {
//        playerResearchUI.HidePlayerResearchUI();
//    }
//    private void UpdatePlayerResearchUI() {
//        playerResearchUI.UpdatePlayerResearchUI();
//    }
//    #endregion

//    #region Player Delay Divine Intervention Content
//    private void ShowTheProfaneUI() {
//        theProfaneUI.ShowTheProfaneUI(activeRegion.mainLandmark as TheProfane);
//    }
//    private void HideTheProfaneUI() {
//        theProfaneUI.Hide();
//    }
//    private void UpdateTheProfaneUI() {
//        theProfaneUI.UpdateTheProfaneUI();
//    }
//    #endregion

//    #region Player Summon Minion Content
//    private void ShowPlayerSummonMinionUI() {
//        playerSummonMinionUI.ShowPlayerSummonMinionUI(activeRegion.mainLandmark as ThePortal);
//    }
//    private void HidePlayerSummonMinionUI() {
//        playerSummonMinionUI.HidePlayerSummonMinionUI();
//    }
//    private void UpdatePlayerSummonMinionUI() {
//        playerSummonMinionUI.UpdatePlayerSummonMinionUI();
//    }
//    #endregion

//    #region Player Upgrade Content
//    private void ShowPlayerUpgradeUI() {
//        playerUpgradeUI.ShowPlayerUpgradeUI(activeRegion.mainLandmark as TheAnvil);
//    }
//    private void HidePlayerUpgradeUI() {
//        playerUpgradeUI.HidePlayerResearchUI();
//    }
//    private void UpdatePlayerUpgradeUI() {
//        playerUpgradeUI.UpdatePlayerUpgradeUI();
//    }
//    public void OnPlayerUpgradeDone() {
//        //if (playerUpgradeUI.gameObject.activeSelf) {
//        //    playerUpgradeUI.OnUpgradeDone();
//        //}
//    }
//    #endregion

//    #region The Eye
//    private void ShowTheEyeUI() {
//        theEyeUI.ShowTheEyeUI(activeRegion.mainLandmark as TheEye);
//    }
//    private void HideTheEyeUI() {
//        theEyeUI.HideTheEyeUI();
//    }
//    private void UpdateTheEyeUI() {
//        theEyeUI.UpdateTheEyeUI();
//    }
//    #endregion

//    #region The Needles
//    [SerializeField] private TheNeedlesUI needlesUI;
//    private void ShowTheNeedlesUI() {
//        needlesUI.ShowTheNeedlesUI(activeRegion.mainLandmark as TheNeedles);
//    }
//    private void HideTheNeedlesUI() {
//        needlesUI.HideTheNeedlesUI();
//    }
//    private void UpdateTheNeedlesUI() {
//        needlesUI.UpdateUI();
//    }
//    #endregion

//    #region The Needles
//    [SerializeField] private TheFingersUI fingersUI;
//    private void ShowTheFingersUI() {
//        fingersUI.ShowTheFingersUI(activeRegion.mainLandmark as TheFingers);
//    }
//    private void HideTheFingersUI() {
//        fingersUI.HideTheFingersUI();
//    }
//    private void UpdateTheFingersUI() {
//        fingersUI.UpdateTheFingersUI();
//    }
    #endregion

    #region For Testing
    public void ShowLocationInfo() {
        UtilityScripts.TestingUtilities.ShowLocationInfo(activeRegion);
    }
    public void HideLocationInfo() {
        UtilityScripts.TestingUtilities.HideLocationInfo();
    }
    #endregion

    // #region Actions
    // //[Header("Actions")] 
    // //[SerializeField] private RectTransform actionsTransform;
    // //[SerializeField] private GameObject actionItemPrefab;
    // protected override void LoadActions() {
    //     Utilities.DestroyChildren(actionsTransform);
    //     // if (activeRegion.coreTile.isCorrupted) {
    //     //     //region is corrupted
    //     //     if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
    //     //         //if it doesn't have a landmark, show the build action
    //     //         ActionItem item = AddNewAction("Build", null, () => playerBuildLandmarkUI.OnClickBuild(activeRegion));
    //     //         if (activeRegion.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
    //     //             int remaining = activeRegion.demonicBuildingData.buildDuration -
    //     //                             activeRegion.demonicBuildingData.currentDuration;
    //     //             item.SetAsUninteractableUntil(remaining);
    //     //         }
    //     //     } else {
    //     //         //if it has a landmark then assume that the landmark is a demonic landmark and show that landmarks action.
    //     //         //if the landmark is not the portal, then show the demolish action
    //     //         if (activeRegion.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL) {
    //     //             AddNewAction("Demolish", null, OnClickDemolish);    
    //     //         }
    //     //         ActionItem item;
    //     //         int remaining = 0;
    //     //         switch (activeRegion.mainLandmark.specificLandmarkType) {
    //     //             case LANDMARK_TYPE.THE_SPIRE:
    //     //                 TheSpire spire = activeRegion.mainLandmark as TheSpire;
    //     //                 item = AddNewAction("Extract Spell", null, () => playerResearchUI.OnClickResearch(spire));
    //     //                 if (spire.isInCooldown) {
    //     //                     remaining = spire.cooldownDuration - spire.currentCooldownTick;
    //     //                     item.SetAsUninteractableUntil(remaining);
    //     //                 }
    //     //                 break;
    //     //             case LANDMARK_TYPE.THE_EYE:
    //     //                 TheEye eye = activeRegion.mainLandmark as TheEye;
    //     //                 item = AddNewAction("Interfere", null, () => theEyeUI.OnClickInterfere(eye));
    //     //                 if (eye.isInCooldown) {
    //     //                     remaining = eye.cooldownDuration - eye.currentCooldownTick;
    //     //                     item.SetAsUninteractableUntil(remaining);
    //     //                 }
    //     //                 break;
    //     //             case LANDMARK_TYPE.THE_ANVIL:
    //     //                 TheAnvil anvil = activeRegion.mainLandmark as TheAnvil;
    //     //                 item = AddNewAction("Upgrade", null, () => playerUpgradeUI.OnClickUpgrade(anvil));
    //     //                 if (string.IsNullOrEmpty(anvil.upgradeIdentifier) == false) {
    //     //                     item.SetAsUninteractableUntil(anvil.dueDate);
    //     //                 }
    //     //                 break;
    //     //             case LANDMARK_TYPE.THE_PORTAL:
    //     //                 ThePortal portal = activeRegion.mainLandmark as ThePortal;
    //     //                 item = AddNewAction("Summon", null, () => playerSummonMinionUI.OnClickSummon(portal));
    //     //                 if (portal.currentMinionToSummonIndex != -1) {
    //     //                     remaining = portal.currentSummonDuration - portal.currentSummonTick;
    //     //                     item.SetAsUninteractableUntil(remaining);
    //     //                 }
    //     //                 break;
    //     //             case LANDMARK_TYPE.GOADER:
    //     //                 Goader fingers = activeRegion.mainLandmark as Goader;
    //     //                 item = AddNewAction("Create Faction", null, () => fingersUI.OnClickCreate(fingers));
    //     //                 item = AddNewAction("Force Leave Faction", null, () => fingersUI.OnClickForceLeaveFaction());
    //     //                 item = AddNewAction("Force Join Faction", null, () => fingersUI.OnClickForceJoinFaction());
    //     //                 if (fingers.hasBeenActivated) {
    //     //                     remaining = fingers.duration - fingers.currentTick;
    //     //                     item.SetAsUninteractableUntil(remaining);
    //     //                 }
    //     //                 break;
    //     //             case LANDMARK_TYPE.THE_NEEDLES:
    //     //                 TheNeedles needles = activeRegion.mainLandmark as TheNeedles;
    //     //                 item = AddNewAction("Convert to Mana", null, () => needlesUI.OnClickConvert(needles));
    //     //                 if (needles.isInCooldown) {
    //     //                     remaining = needles.cooldownDuration - needles.currentCooldownTick;
    //     //                     item.SetAsUninteractableUntil(remaining);
    //     //                 }
    //     //                 break;
    //     //             case LANDMARK_TYPE.THE_PROFANE:
    //     //                 TheProfane profane = activeRegion.mainLandmark as TheProfane;
    //     //                 item = AddNewAction("Corrupt", null, () => theProfaneUI.OnClickCorrupt(profane));
    //     //                 if (profane.isInCooldown) {
    //     //                     remaining = profane.cooldownDuration - profane.currentCooldownTick;
    //     //                     item.SetAsUninteractableUntil(remaining);
    //     //                 }
    //     //                 break;
    //     //         }
    //     //     }
    //     // } 
    //     // else {
    //     //     //region is not corrupted
    //     //     //show invade action.
    //     //     if (activeRegion.CanBeInvaded()) {
    //     //         ActionItem item = AddNewAction("Invade", null, OnClickInvade);
    //     //         if (activeRegion.demonicInvasionData.beingInvaded) {
    //     //             int remaining = activeRegion.mainLandmark.invasionTicks -
    //     //                             activeRegion.demonicInvasionData.currentDuration;
    //     //             item.SetAsUninteractableUntil(remaining);
    //     //         } 
    //     //         //else {
    //     //         //    item.SetInteractable(PlayerManager.Instance.player.currentNpcSettlementBeingInvaded != null);
    //     //         //}
    //     //     }
    //     // }
    // }
    // //private ActionItem AddNewAction(string actionName, Sprite actionIcon, System.Action action) {
    // //    GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(actionItemPrefab.name, Vector3.zero,
    // //        Quaternion.identity, actionsTransform);
    // //    ActionItem item = obj.GetComponent<ActionItem>();
    // //    item.SetAction(action, actionIcon, actionName);
    // //    return item;
    // //}
    // #endregion
}
