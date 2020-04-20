using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class UnleashSummonUI : PopupMenuBase {
    [Header("General")]
    public ScrollRect summonsScrollRect;
    public Button summonButton;
    public TextMeshProUGUI summonButtonText;
    public TextMeshProUGUI titleText;
    public GameObject characterNameplateItemPrefab;
    public GameObject spellNameplateItemPrefab;
    //public Image summonIcon;
    //public TextMeshProUGUI summonText;
    //private Summon summon;
    private bool isGamePausedOnShowUI;
    public List<CharacterNameplateItem> characterNameplateItems { get; private set; }
    public List<SpellNameplateItem> spellNameplateItems { get; private set; }
    private List<Character> chosenSummons = new List<Character>();
    private List<SpellData> chosenMinionMonsters = new List<SpellData>();
    private List<LocationGridTile> entrances = new List<LocationGridTile>();
    private int manaCost => chosenSummons.Count * 50;

    public string identifier { get; private set; }

    private Character _targetCharacter;

    public void ShowUnleashSummonUI(string identifier) {
        this.identifier = identifier;
        titleText.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(identifier);
        if (characterNameplateItems == null) {
            characterNameplateItems = new List<CharacterNameplateItem>();
        }
        if (spellNameplateItems == null) {
            spellNameplateItems = new List<SpellNameplateItem>();
        }
        chosenSummons.Clear();
        chosenMinionMonsters.Clear();
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowUnleashSummonUI(identifier));
            return;
        }
        isGamePausedOnShowUI = GameManager.Instance.isPaused;
        if (!isGamePausedOnShowUI) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        //SetSummon(summon);
        PopulateMinionsMonstersSummons();
        UpdateSummonButton();
        base.Open();
    }
    public void SetTargetCharacter(Character character) {
        _targetCharacter = character;
    }
    private void PopulateMinionsMonstersSummons() {
        UtilityScripts.Utilities.DestroyChildren(summonsScrollRect.content);
        characterNameplateItems.Clear();
        spellNameplateItems.Clear();
        for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.summons.Count; i++) {
            Summon summon = PlayerManager.Instance.player.playerSkillComponent.summons[i];
            CharacterNameplateItem item = CreateNewCharacterNameplateItem();
            item.SetAsToggle();
            item.SetObject(summon);
            item.AddOnToggleAction(OnToggleCharacter);
            item.SetPortraitInteractableState(false);
            item.gameObject.SetActive(true);
        }
        for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.minionsSkills.Count; i++) {
            MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(PlayerManager.Instance.player.playerSkillComponent.minionsSkills[i]);
            SpellNameplateItem item = CreateNewSpellNameplateItem();
            item.SetSpell(minionPlayerSkill);
            item.SetToggleAction(OnToggleMinionMonster);
            item.UpdateInteractability();
            item.gameObject.SetActive(true);
        }
        for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.summonsSkills.Count; i++) {
            SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(PlayerManager.Instance.player.playerSkillComponent.summonsSkills[i]);
            SpellNameplateItem item = CreateNewSpellNameplateItem();
            item.SetSpell(summonPlayerSkill);
            item.SetToggleAction(OnToggleMinionMonster);
            item.UpdateInteractability();
            item.gameObject.SetActive(true);
        }
    }
    private void OnToggleCharacter(Character character, bool isOn) {
        if (isOn) {
            chosenSummons.Add(character);
        } else {
            chosenSummons.Remove(character);
        }
        UpdateSummonButton();
    }
    private void OnToggleMinionMonster(SpellData spellData, bool isOn) {
        if (isOn) {
            chosenMinionMonsters.Add(spellData);
        } else {
            chosenMinionMonsters.Remove(spellData);
        }
        UpdateSummonButton();
    }
    private void UpdateSummonButton() {
        if(chosenSummons.Count > 0 || chosenMinionMonsters.Count > 0) {
            summonButton.interactable = true;
        } else {
            summonButton.interactable = false;
        }
        summonButtonText.text = "CONFIRM";
    }
    private CharacterNameplateItem CreateNewCharacterNameplateItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterNameplateItemPrefab.name, Vector3.zero, Quaternion.identity, summonsScrollRect.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        go.SetActive(false);
        characterNameplateItems.Add(item);
        return item;
    }
    private SpellNameplateItem CreateNewSpellNameplateItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellNameplateItemPrefab.name, Vector3.zero, Quaternion.identity, summonsScrollRect.content);
        SpellNameplateItem item = go.GetComponent<SpellNameplateItem>();
        go.SetActive(false);
        spellNameplateItems.Add(item);
        return item;
    }
    private void HarassDefendInvade() {
        entrances.Clear();
        HexTile targetHex = PlayerUI.Instance.harassDefendInvadeTargetHex;
        //NPCSettlement targetNpcSettlement = targetHex.settlementOnTile as NPCSettlement;
        Character spawnedCharacter = null;
        LocationGridTile mainEntrance = targetHex.GetCenterLocationGridTile();
        entrances.Add(mainEntrance);

        int totalEntrances = chosenSummons.Count + chosenMinionMonsters.Count;
        for (int i = 0; i < entrances.Count; i++) {
            if (entrances.Count == totalEntrances) {
                break;
            }
            for (int j = 0; j < entrances[i].neighbourList.Count; j++) {
                LocationGridTile newEntrance = entrances[i].neighbourList[j];
                //if (newEntrance.objHere == null && newEntrance.charactersHere.Count == 0 && newEntrance.structure != null) {
                if (!entrances.Contains(newEntrance)) {
                    entrances.Add(newEntrance);
                    if (entrances.Count == totalEntrances) {
                        break;
                    }
                }
            }
        }
        if (identifier == "harass") {
            for (int i = 0; i < chosenSummons.Count; i++) {
                Summon summon = chosenSummons[i] as Summon;
                TryPlaceSummon(summon, entrances[0]);
                summon.behaviourComponent.SetIsHarassing(true, targetHex);
                entrances.RemoveAt(0);
            }
            for (int i = 0; i < chosenMinionMonsters.Count; i++) {
                SpellData minionMonsterPlayerSkll = chosenMinionMonsters[i];
                minionMonsterPlayerSkll.ActivateAbility(entrances[0], ref spawnedCharacter);
                spawnedCharacter.behaviourComponent.SetIsHarassing(true, targetHex);
                entrances.RemoveAt(0);
            }
            PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.HARASS).OnExecuteSpellActionAffliction();
        } else if (identifier == "defend") {
            for (int i = 0; i < chosenSummons.Count; i++) {
                Summon summon = chosenSummons[i] as Summon;
                TryPlaceSummon(summon, entrances[0]);
                summon.behaviourComponent.SetIsDefending(true, targetHex);
                entrances.RemoveAt(0);
            }
            for (int i = 0; i < chosenMinionMonsters.Count; i++) {
                SpellData minionMonsterPlayerSkll = chosenMinionMonsters[i];
                minionMonsterPlayerSkll.ActivateAbility(entrances[0], ref spawnedCharacter);
                spawnedCharacter.behaviourComponent.SetIsDefending(true, targetHex);
                entrances.RemoveAt(0);
            }
            PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.DEFEND).OnExecuteSpellActionAffliction();
        } else if (identifier == "invade") {
            for (int i = 0; i < chosenSummons.Count; i++) {
                Summon summon = chosenSummons[i] as Summon;
                TryPlaceSummon(summon, entrances[0]);
                summon.behaviourComponent.SetIsInvading(true, targetHex);
                entrances.RemoveAt(0);
            }
            for (int i = 0; i < chosenMinionMonsters.Count; i++) {
                SpellData minionMonsterPlayerSkll = chosenMinionMonsters[i];
                minionMonsterPlayerSkll.ActivateAbility(entrances[0], ref spawnedCharacter);
                spawnedCharacter.behaviourComponent.SetIsInvading(true, targetHex);
                entrances.RemoveAt(0);
            }
            PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.INVADE).OnExecuteSpellActionAffliction();
        }
        PlayerManager.Instance.player.threatComponent.AdjustThreat(5 + (5 * chosenSummons.Count));

        if(spawnedCharacter != null) {
            spawnedCharacter.CenterOnCharacter();
        } else if (chosenSummons.Count > 0) {
            chosenSummons[0].CenterOnCharacter();
        }
        Close();
    }
    private void Knockout() {
        entrances.Clear();
        //HexTile targetHex = _targetCharacter.gridTileLocation.GetTilesInRadius(5, includeTilesInDifferentStructure: true);
        Character spawnedCharacter = null;
        //LocationGridTile mainEntrance = targetHex.GetCenterLocationGridTile();
        entrances.AddRange(_targetCharacter.gridTileLocation.GetTilesInRadius(3, includeTilesInDifferentStructure: true));

        //int totalEntrances = chosenSummons.Count + chosenMinionMonsters.Count;
        //for (int i = 0; i < entrances.Count; i++) {
        //    if (entrances.Count == totalEntrances) {
        //        break;
        //    }
        //    for (int j = 0; j < entrances[i].neighbourList.Count; j++) {
        //        LocationGridTile newEntrance = entrances[i].neighbourList[j];
        //        //if (newEntrance.objHere == null && newEntrance.charactersHere.Count == 0 && newEntrance.structure != null) {
        //        if (!entrances.Contains(newEntrance)) {
        //            entrances.Add(newEntrance);
        //            if (entrances.Count == totalEntrances) {
        //                break;
        //            }
        //        }
        //    }
        //}
        for (int i = 0; i < chosenSummons.Count; i++) {
            Summon summon = chosenSummons[i] as Summon;
            TryPlaceSummon(summon, entrances[UnityEngine.Random.Range(0, entrances.Count)]);
            GoapPlanJob job = summon.jobComponent.CreateKnockoutJob(_targetCharacter);
            summon.jobComponent.SetFinalJobAssignment(job);
            summon.SetDestroyMarkerOnDeath(true);
            //entrances.RemoveAt(0);
        }
        for (int i = 0; i < chosenMinionMonsters.Count; i++) {
            SpellData minionMonsterPlayerSkll = chosenMinionMonsters[i];
            minionMonsterPlayerSkll.ActivateAbility(entrances[UnityEngine.Random.Range(0, entrances.Count)], ref spawnedCharacter);
            GoapPlanJob job = spawnedCharacter.jobComponent.CreateKnockoutJob(_targetCharacter);
            spawnedCharacter.jobComponent.SetFinalJobAssignment(job);
            spawnedCharacter.SetDestroyMarkerOnDeath(true);
            //entrances.RemoveAt(0);
        }
        PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.KNOCKOUT).OnExecuteSpellActionAffliction();

        if (spawnedCharacter != null) {
            spawnedCharacter.CenterOnCharacter();
        } else if (chosenSummons.Count > 0) {
            chosenSummons[0].CenterOnCharacter();
        }
        Close();
    }
    private void Kill() {
        entrances.Clear();
        //HexTile targetHex = _targetCharacter.gridTileLocation.collectionOwner.GetNearestHexTile();
        Character spawnedCharacter = null;
        //LocationGridTile mainEntrance = targetHex.GetCenterLocationGridTile();
        //entrances.Add(mainEntrance);
        entrances.AddRange(_targetCharacter.gridTileLocation.GetTilesInRadius(3, includeTilesInDifferentStructure: true));

        //int totalEntrances = chosenSummons.Count + chosenMinionMonsters.Count;
        //for (int i = 0; i < entrances.Count; i++) {
        //    if (entrances.Count == totalEntrances) {
        //        break;
        //    }
        //    for (int j = 0; j < entrances[i].neighbourList.Count; j++) {
        //        LocationGridTile newEntrance = entrances[i].neighbourList[j];
        //        //if (newEntrance.objHere == null && newEntrance.charactersHere.Count == 0 && newEntrance.structure != null) {
        //        if (!entrances.Contains(newEntrance)) {
        //            entrances.Add(newEntrance);
        //            if (entrances.Count == totalEntrances) {
        //                break;
        //            }
        //        }
        //    }
        //}

        for (int i = 0; i < chosenSummons.Count; i++) {
            Summon summon = chosenSummons[i] as Summon;
            TryPlaceSummon(summon, entrances[UnityEngine.Random.Range(0, entrances.Count)]);
            GoapPlanJob job = summon.jobComponent.CreateKillJob(_targetCharacter);
            summon.jobComponent.SetFinalJobAssignment(job);
            summon.SetDestroyMarkerOnDeath(true);
            //entrances.RemoveAt(0);
        }
        for (int i = 0; i < chosenMinionMonsters.Count; i++) {
            SpellData minionMonsterPlayerSkll = chosenMinionMonsters[i];
            minionMonsterPlayerSkll.ActivateAbility(entrances[UnityEngine.Random.Range(0, entrances.Count)], ref spawnedCharacter);
            GoapPlanJob job = spawnedCharacter.jobComponent.CreateKillJob(_targetCharacter);
            spawnedCharacter.jobComponent.SetFinalJobAssignment(job);
            spawnedCharacter.SetDestroyMarkerOnDeath(true);
            //entrances.RemoveAt(0);
        }
        PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.KILL).OnExecuteSpellActionAffliction();

        if (spawnedCharacter != null) {
            spawnedCharacter.CenterOnCharacter();
        } else if (chosenSummons.Count > 0) {
            chosenSummons[0].CenterOnCharacter();
        }
        Close();
    }
    private void TryPlaceSummon(Summon summon, LocationGridTile locationTile) {
        summon.SetHomeRegion(locationTile.structure.location);
        CharacterManager.Instance.PlaceSummon(summon, locationTile);
        Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summon);
        PlayerManager.Instance.player.playerSkillComponent.RemoveSummon(summon, true);
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
        if(identifier == "knockout") {
            Knockout();
        } else if (identifier == "kill") {
            Kill();
        } else {
            HarassDefendInvade();
        }

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
