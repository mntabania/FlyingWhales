using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Inner_Maps;

public class UnleashSummonUI : PopupMenuBase {
    [Header("General")]
    public ScrollRect summonsScrollRect;
    public Button summonButton;
    public TextMeshProUGUI summonButtonText;
    public GameObject characterNameplateItemPrefab;
    //public Image summonIcon;
    //public TextMeshProUGUI summonText;
    //private Summon summon;
    private bool isGamePausedOnShowUI;
    public List<CharacterNameplateItem> characterNameplateItems { get; private set; }
    private List<Character> chosenSummons;
    private List<LocationGridTile> entrances = new List<LocationGridTile>();
    private int manaCost => chosenSummons.Count * 50;

    public void ShowUnleashSummonUI() {
        if (characterNameplateItems == null) {
            characterNameplateItems = new List<CharacterNameplateItem>();
        }
        if (chosenSummons == null) {
            chosenSummons = new List<Character>();
        }
        chosenSummons.Clear();
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowUnleashSummonUI());
            return;
        }
        isGamePausedOnShowUI = GameManager.Instance.isPaused;
        if (!isGamePausedOnShowUI) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        //SetSummon(summon);
        PopulateSummons();
        UpdateSummonButton();
        base.Open();
    }
    private void PopulateSummons() {
        UtilityScripts.Utilities.DestroyChildren(summonsScrollRect.content);
        characterNameplateItems.Clear();
        for (int i = 0; i < PlayerManager.Instance.player.summons.Count; i++) {
            Summon summon = PlayerManager.Instance.player.summons[i];
            CharacterNameplateItem item = CreateNewCharacterNameplateItem();
            item.SetAsToggle();
            item.SetObject(summon);
            item.AddOnToggleAction(OnToggleCharacter);
            item.SetPortraitInteractableState(false);
            item.gameObject.SetActive(true);
        }
        //TODO:
        // List<Region> playerOwnedRegions = PlayerManager.Instance.player.playerFaction.ownedSettlements;
        // for (int i = 0; i < playerOwnedRegions.Count; i++) {
        //     Region region = playerOwnedRegions[i];
        //     if(region.mainLandmark != null && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_KENNEL) {
        //         for (int j = 0; j < region.charactersAtLocation.Count; j++) {
        //             Character character = region.charactersAtLocation[j];
        //             if(character is Summon && character.faction == PlayerManager.Instance.player.playerFaction) {
        //                 CharacterNameplateItem item = CreateNewCharacterNameplateItem();
        //                 item.SetAsToggle();
        //                 item.SetObject(character);
        //                 item.AddOnToggleAction(OnToggleCharacter);
        //                 item.SetPortraitInteractableState(false);
        //                 item.gameObject.SetActive(true);
        //             }
        //         }
        //     }
        // }
    }
    private void OnToggleCharacter(Character character, bool isOn) {
        if (isOn) {
            chosenSummons.Add(character);
        } else {
            chosenSummons.Remove(character);
        }
        UpdateSummonButton();
    }
    private void UpdateSummonButton() {
        if(chosenSummons.Count > 0) {
            summonButton.interactable = true;
            summonButtonText.text = "CONFIRM";
        } else {
            summonButton.interactable = false;
            summonButtonText.text = "CONFIRM";
        }
    }
    private CharacterNameplateItem CreateNewCharacterNameplateItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterNameplateItemPrefab.name, Vector3.zero, Quaternion.identity, summonsScrollRect.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        go.SetActive(false);
        characterNameplateItems.Add(item);
        return item;
    }
    private void HarassRaidInvade() {
        Settlement targetSettlement = PlayerUI.Instance.harassRaidInvadeTargetSettlement;
        //PlayerUI.Instance.harassRaidInvadeLeaderMinion.character.behaviourComponent.SetHarassInvadeRaidTarget(targetSettlement);
        if (PlayerUI.Instance.harassRaidInvade == "harass") {
            PlayerUI.Instance.harassRaidInvadeLeaderMinion.character.behaviourComponent.SetIsHarassing(true, targetSettlement);
            for (int i = 0; i < chosenSummons.Count; i++) {
                //chosenSummons[i].behaviourComponent.SetHarassInvadeRaidTarget(targetSettlement);
                chosenSummons[i].behaviourComponent.SetIsHarassing(true, targetSettlement);
            }
        } else if (PlayerUI.Instance.harassRaidInvade == "raid") {
            PlayerUI.Instance.harassRaidInvadeLeaderMinion.character.behaviourComponent.SetIsRaiding(true, targetSettlement);
            for (int i = 0; i < chosenSummons.Count; i++) {
                //chosenSummons[i].behaviourComponent.SetHarassInvadeRaidTarget(targetSettlement);
                chosenSummons[i].behaviourComponent.SetIsRaiding(true, targetSettlement);
            }
        } else if (PlayerUI.Instance.harassRaidInvade == "invade") {
            PlayerUI.Instance.harassRaidInvadeLeaderMinion.character.behaviourComponent.SetIsInvading(true, targetSettlement);
            for (int i = 0; i < chosenSummons.Count; i++) {
                //chosenSummons[i].behaviourComponent.SetHarassInvadeRaidTarget(targetSettlement);
                chosenSummons[i].behaviourComponent.SetIsInvading(true, targetSettlement);
            }
        }
        base.Close();
        //entrances.Clear();
        //InnerTileMap innerMap = InnerMapManager.Instance.currentlyShowingMap;
        //LocationGridTile mainEntrance = innerMap.GetRandomUnoccupiedEdgeTile();
        //entrances.Add(mainEntrance);
        //for (int i = 0; i < entrances.Count; i++) {
        //    if (entrances.Count == chosenSummons.Count) {
        //        break;
        //    }
        //    for (int j = 0; j < entrances[i].neighbourList.Count; j++) {
        //        LocationGridTile newEntrance = entrances[i].neighbourList[j];
        //        //if (newEntrance.objHere == null && newEntrance.charactersHere.Count == 0 && newEntrance.structure != null) {
        //        if (newEntrance.IsAtEdgeOfWalkableMap() && !entrances.Contains(newEntrance)) {
        //            entrances.Add(newEntrance);
        //            if (entrances.Count == chosenSummons.Count) {
        //                break;
        //            }
        //        }
        //    }
        //}
        //for (int i = 0; i < entrances.Count; i++) {
        //    TryPlaceSummon(chosenSummons[i] as Summon, entrances[i]);
        //    //chosenSummons[i].marker.InitialPlaceMarkerAt(entrances[i]);
        //}
        //chosenSummons[0].CenterOnCharacter();
    }
    private void TryPlaceSummon(Summon summon, LocationGridTile locationTile) {
        CharacterManager.Instance.PlaceSummon(summon, locationTile);
        Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summon);
    }
    //private void SetSummon(Summon summon) {
    //    this.summon = summon;
    //    if(this.summon != null) {
    //        summonIcon.sprite = CharacterManager.Instance.GetSummonSettings(summon.summonType).summonPortrait;
    //        string text = summon.name + " (" + summon.summonType.SummonName() + ")";
    //        text += "\nLevel: " + summon.level.ToString();
    //        text += "\nDescription: " + PlayerManager.Instance.player.GetSummonDescription(summon.summonType);
    //        summonText.text = text;
    //    }
    //}
    
    public void OnClickConfirm() {
        HarassRaidInvade();

        //if (PlayerManager.Instance.player.mana >= manaCost) {
        //    Close();
        //    PlayerManager.Instance.player.AdjustMana(-manaCost);
        //    AttackRegion();
        //} else {
        //    PlayerUI.Instance.ShowGeneralConfirmation("Mana Cost", "NOT ENOUGH MANA!");
        //}

        //if(summon != null) {
        //    PlayerUI.Instance.TryPlaceSummon(summon);
        //}
    }
    public void OnClickClose() {
        Close();
    }

    public override void Close() {
        gameObject.SetActive(false);
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            if (!isGamePausedOnShowUI) {
                UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
            }
        }
    }
}
