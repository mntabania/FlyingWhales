﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

public class Player : ILeader {

    private const int MAX_IMPS = 5;
    private const int MAX_INTEL = 3;

    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int maxImps { get; private set; }

    private int _lifestones;
    private float _currentLifestoneChance;
    private BaseLandmark _demonicPortal;
    private List<Token> _tokens;
    private List<Minion> _minions;
    private Dictionary<CURRENCY, int> _currencies;

    public Dictionary<JOB, PlayerJobData> roleSlots { get; private set; }
    public CombatGrid attackGrid { get; private set; }
    public CombatGrid defenseGrid { get; private set; }
    public List<Intel> allIntel { get; private set; }

    #region getters/setters
    public int id {
        get { return -645; }
    }
    public string name {
        get { return "Player"; }
    }
    public int lifestones {
        get { return _lifestones; }
    }
    public float currentLifestoneChance {
        get { return _currentLifestoneChance; }
    }
    public RACE race {
        get { return RACE.HUMANS; }
    }
    public BaseLandmark demonicPortal {
        get { return _demonicPortal; }
    }
    public Area specificLocation {
        get { return playerArea; }
    }
    public Area homeArea {
        get { return playerArea; }
    }
    public List<Token> tokens {
        get { return _tokens; }
    }
    public Dictionary<CURRENCY, int> currencies {
        get { return _currencies; }
    }
    public List<Minion> minions {
        get { return _minions; }
    }
    public List<Character> allOwnedCharacters {
        get { return minions.Select(x => x.character).ToList(); }
    }
    #endregion

    public bool hasSeenActionButtonsOnce = false;

    public Player() {
        playerArea = null;
        _tokens = new List<Token>();
        attackGrid = new CombatGrid();
        defenseGrid = new CombatGrid();
        attackGrid.Initialize();
        defenseGrid.Initialize();
        maxImps = 5;
        allIntel = new List<Intel>();
        SetCurrentLifestoneChance(25f);
        ConstructCurrencies();
        ConstructRoleSlots();
        AddListeners();
    }

    private void EverydayAction() {
        //DepleteThreatLevel();
    }

    private void AddListeners() {
        AddWinListener();
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, OnTileRemovedFromPlayerArea);
        Messenger.AddListener(Signals.TICK_STARTED, EverydayAction);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);

        //goap
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnCharacterDidAction);
        Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
    }

    #region ILeader
    public void LevelUp() {
        //Not applicable
    }
    #endregion

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        chosenCoreTile.SetCorruption(true);
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        _demonicPortal = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        Biomes.Instance.CorruptTileVisuals(chosenCoreTile);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
        //OnTileAddedToPlayerArea(playerArea, chosenCoreTile);
    }
    public void CreatePlayerArea(BaseLandmark portal) {
        _demonicPortal = portal;
        Area playerArea = LandmarkManager.Instance.CreateNewArea(portal.tileLocation, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        Biomes.Instance.CorruptTileVisuals(portal.tileLocation);
        portal.tileLocation.SetCorruption(true);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
    }
    public void LoadPlayerArea(Area area) {
        _demonicPortal = area.coreTile.landmarkOnTile;
        Biomes.Instance.CorruptTileVisuals(_demonicPortal.tileLocation);
        _demonicPortal.tileLocation.SetCorruption(true);
        SetPlayerArea(area);
        _demonicPortal.tileLocation.ScheduleCorruption();

    }
    private void SetPlayerArea(Area area) {
        playerArea = area;
        area.SetSuppliesInBank(_currencies[CURRENCY.SUPPLY]);
        //area.StopSupplyLine();
    }
    private void OnTileRemovedFromPlayerArea(Area affectedArea, HexTile removedTile) {
        if (playerArea != null && affectedArea.id == playerArea.id) {
            Biomes.Instance.UpdateTileVisuals(removedTile);
        }
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction playerFaction = FactionManager.Instance.CreateNewFaction(true);
        playerFaction.SetLeader(this);
        playerFaction.SetEmblem(FactionManager.Instance.GetFactionEmblem(6));
        SetPlayerFaction(playerFaction);
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    #endregion

    #region Token
    public void AddToken(Token token) {
        if (!_tokens.Contains(token)) {
            if (token is CharacterToken && (token as CharacterToken).character.minion != null) {

            } else {
                _tokens.Add(token);
                Debug.Log("Added token " + token.ToString());
                Messenger.Broadcast(Signals.TOKEN_ADDED, token);
            }
            token.SetObtainedState(true);
            if (token is CharacterToken) {
                Messenger.Broadcast(Signals.CHARACTER_TOKEN_ADDED, token as CharacterToken);
            } 
            //else if (token is SpecialToken) {
            //    (token as SpecialToken).AdjustQuantity(-1);
            //}
        }
    }
    public bool RemoveToken(Token token) {
        if (_tokens.Remove(token)) {
            token.SetObtainedState(false);
            Debug.Log("Removed token " + token.ToString());
            return true;
        }
        return false;
    }
    public Token GetToken(Token token) {
        for (int i = 0; i < _tokens.Count; i++) {
            if(_tokens[i] == token) {
                return _tokens[i];
            }
        }
        return null;
    }
    public bool HasSpecialToken(string tokenName) {
        for (int i = 0; i < _tokens.Count; i++) {
            if (_tokens[i].tokenName == tokenName) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Lifestone
    public void DecreaseLifestoneChance() {
        if(_currentLifestoneChance > 2f) {
            float decreaseRate = 5f;
            if(_currentLifestoneChance <= 15f) {
                decreaseRate = 1f;
            }
            _currentLifestoneChance -= decreaseRate;
        }
    }
    public void SetCurrentLifestoneChance(float amount) {
        _currentLifestoneChance = amount;
    }
    public void SetLifestone(int amount) {
        _lifestones = amount;
    }
    public void AdjustLifestone(int amount) {
        _lifestones += amount;
        Debug.Log("Adjusted player lifestones by: " + amount + ". New total is " + _lifestones);
    }
    #endregion

    #region Minions
    public void CreateInitialMinions() {
        _minions = new List<Minion>();
        for (int i = 0; i < 20; i++) {
            AddMinion(CreateNewMinion(RACE.DEMON));
        }
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));
        //AddMinion(CreateNewMinion(CharacterManager.Instance.GetRandomDeadlySinsClassName(), RACE.DEMON, false));

        //UpdateMinions();
    }
    public Minion CreateNewMinion(Character character) {
        return new Minion(character, true);
    }
    public Minion CreateNewMinion(RACE race) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, race, GENDER.MALE, playerFaction, playerArea, null), false);
        //minion.character.CreateMarker();
        return minion;
    }
    public Minion CreateNewMinion(string className, RACE race) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, race, GENDER.MALE, playerFaction, playerArea), false);
        return minion;
    }
    public void AddMinion(Minion minion) {
        minion.SetIndexDefaultSort(_minions.Count);

        if (PlayerUI.Instance.minionSortType == MINIONS_SORT_TYPE.LEVEL) {
            for (int i = 0; i < _minions.Count; i++) {
                if (minion.lvl <= _minions[i].lvl) {
                    _minions.Insert(i, minion);
                    break;
                }
            }
        } else if (PlayerUI.Instance.minionSortType == MINIONS_SORT_TYPE.TYPE) {
            string strMinionType = minion.character.characterClass.className;
            for (int i = 0; i < _minions.Count; i++) {
                int compareResult = string.Compare(strMinionType, minion.character.characterClass.className);
                if (compareResult == -1 || compareResult == 0) {
                    _minions.Insert(i, minion);
                    break;
                }
            }
        } else {
            _minions.Add(minion);
        }
    }
    public void RemoveMinion(Minion minion) {
        _minions.Remove(minion);
    }
    #endregion

    #region Currencies
    private void ConstructCurrencies() {
        _currencies = new Dictionary<CURRENCY, int>();
        _currencies.Add(CURRENCY.IMP, 0);
        _currencies.Add(CURRENCY.MANA, 0);
        _currencies.Add(CURRENCY.SUPPLY, 0);
        AdjustCurrency(CURRENCY.IMP, maxImps);
        AdjustCurrency(CURRENCY.SUPPLY, 5000);
        AdjustCurrency(CURRENCY.MANA, 5000);
    }
    public void AdjustCurrency(CURRENCY currency, int amount) {
        _currencies[currency] += amount;
        if(currency == CURRENCY.IMP) {
            _currencies[currency] = Mathf.Clamp(_currencies[currency], 0, maxImps);
        }else if (currency == CURRENCY.SUPPLY) {
            _currencies[currency] = Mathf.Max(_currencies[currency], 0);
            if (playerArea != null) {
                playerArea.SetSuppliesInBank(_currencies[currency]);
            }
        } else if (currency == CURRENCY.MANA) {
            _currencies[currency] = Mathf.Max(_currencies[currency], 0); //maybe 999?
        }
        Messenger.Broadcast(Signals.UPDATED_CURRENCIES);
    }
    public void SetMaxImps(int imps) {
        maxImps = imps;
        _currencies[CURRENCY.IMP] = Mathf.Clamp(_currencies[CURRENCY.IMP], 0, maxImps);
    }
    public void AdjustMaxImps(int adjustment) {
        maxImps += adjustment;
        AdjustCurrency(CURRENCY.IMP, adjustment);
    }
    #endregion

    #region Rewards
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.SUPPLY:
                AdjustCurrency(CURRENCY.SUPPLY, reward.amount);
                break;
            case REWARD.MANA:
                AdjustCurrency(CURRENCY.MANA, reward.amount);
                break;
            //case REWARD.EXP:
            //    state.assignedMinion.AdjustExp(reward.amount);
            //    break;
            default:
                break;
        }
    }
    #endregion

    #region Win/Lose Conditions
    private void AddWinListener() {
        Messenger.AddListener<Faction>(Signals.FACTION_LEADER_DIED, OnFactionLeaderDied);
    }
    private void OnFactionLeaderDied(Faction faction) {
        List<Faction> allUndestroyedFactions = FactionManager.Instance.allFactions.Where(
            x => x.name != "Neutral" 
            && x.name != "Player faction" 
            && x.isActive && !x.isDestroyed).ToList();
        if (allUndestroyedFactions.Count == 0) {
            Debug.LogError("All factions are destroyed! Player won!");
        }        
    }
    public void OnPlayerLandmarkRuined(BaseLandmark landmark) {
        switch (landmark.specificLandmarkType) {
            case LANDMARK_TYPE.DWELLINGS:
                //add 2 minion slots
                //AdjustMaxMinions(-2);
                break;
            case LANDMARK_TYPE.IMP_KENNEL:
                //adds 1 Imp capacity
                //AdjustMaxImps(-1);
                break;
            case LANDMARK_TYPE.DEMONIC_PORTAL:
                //player loses if the Portal is destroyed
                throw new System.Exception("Demonic Portal Was Destroyed! Game Over!");
            case LANDMARK_TYPE.RAMPART:
                //remove bonus 25% HP to all Defenders
                //for (int i = 0; i < playerArea.landmarks.Count; i++) {
                //    BaseLandmark currLandmark = playerArea.landmarks[i];
                //    currLandmark.RemoveDefenderBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                //    //if (currLandmark.defenders != null) {
                //    //    currLandmark.defenders.RemoveBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                //    //}
                //}
                break;
            default:
                break;
        }
    }
    #endregion

    #region Role Slots
    public void ConstructRoleSlots() {
        roleSlots = new Dictionary<JOB, PlayerJobData>();
        roleSlots.Add(JOB.SPY, new PlayerJobData(JOB.SPY));
        roleSlots.Add(JOB.SEDUCER, new PlayerJobData(JOB.SEDUCER));
        roleSlots.Add(JOB.DIPLOMAT, new PlayerJobData(JOB.DIPLOMAT));
        roleSlots.Add(JOB.INSTIGATOR, new PlayerJobData(JOB.INSTIGATOR));
        roleSlots.Add(JOB.DEBILITATOR, new PlayerJobData(JOB.DEBILITATOR));
    }
    public List<JOB> GetValidJobForCharacter(Character character) {
        List<JOB> validJobs = new List<JOB>();
        if (character.minion != null) {
            switch (character.characterClass.className) {
                case "Envy":
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.SEDUCER);
                    break;
                case "Lust":
                    validJobs.Add(JOB.DIPLOMAT);
                    validJobs.Add(JOB.SEDUCER);
                    break;
                case "Pride":
                    validJobs.Add(JOB.DIPLOMAT);
                    validJobs.Add(JOB.INSTIGATOR);
                    break;
                case "Greed":
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.INSTIGATOR);
                    break;
                case "Guttony":
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.SEDUCER);
                    break;
                case "Wrath":
                    validJobs.Add(JOB.INSTIGATOR);
                    validJobs.Add(JOB.DEBILITATOR);
                    break;
                case "Sloth":
                    validJobs.Add(JOB.DEBILITATOR);
                    validJobs.Add(JOB.DIPLOMAT);
                    break;
            }
        } else {
            switch (character.race) {
                case RACE.HUMANS:
                    validJobs.Add(JOB.DIPLOMAT);
                    validJobs.Add(JOB.SEDUCER);
                    break;
                case RACE.ELVES:
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.DIPLOMAT);
                    break;
                case RACE.GOBLIN:
                    validJobs.Add(JOB.INSTIGATOR);
                    validJobs.Add(JOB.SEDUCER);
                    break;
                case RACE.FAERY:
                    validJobs.Add(JOB.SPY);
                    validJobs.Add(JOB.DEBILITATOR);
                    break;
                case RACE.SKELETON:
                    validJobs.Add(JOB.DEBILITATOR);
                    validJobs.Add(JOB.INSTIGATOR);
                    break;
            }
        }
        return validJobs;
    }
    public bool CanAssignCharacterToJob(JOB job, Character character) {
        List<JOB> jobs = GetValidJobForCharacter(character);
        return jobs.Contains(job);
    }
    public bool CanAssignCharacterToAttack(Character character) {
        return GetCharactersCurrentJob(character) == JOB.NONE && !defenseGrid.IsCharacterInGrid(character);
    }
    public bool CanAssignCharacterToDefend(Character character) {
        return GetCharactersCurrentJob(character) == JOB.NONE && !attackGrid.IsCharacterInGrid(character);
    }
    public void AssignCharacterToJob(JOB job, Character character) {
        if (!roleSlots.ContainsKey(job)) {
            Debug.LogWarning("There is something trying to assign a character to " + job.ToString() + " but the player doesn't have a slot for it.");
            return;
        }
        if (roleSlots[job].assignedCharacter != null) {
            UnassignCharacterFromJob(job);
        }
        JOB charactersCurrentJob = GetCharactersCurrentJob(character);
        if (charactersCurrentJob != JOB.NONE) {
            UnassignCharacterFromJob(charactersCurrentJob);
        }

        roleSlots[job].AssignCharacter(character);
        Messenger.Broadcast(Signals.CHARACTER_ASSIGNED_TO_JOB, job, character);
    }
    public void UnassignCharacterFromJob(JOB job) {
        if (!roleSlots.ContainsKey(job)) {
            Debug.LogWarning("There is something trying to unassign a character from " + job.ToString() + " but the player doesn't have a slot for it.");
            return;
        }
        if (roleSlots[job] == null) {
            return; //ignore command
        }
        Character character = roleSlots[job].assignedCharacter;
        roleSlots[job].AssignCharacter(null);
        Messenger.Broadcast(Signals.CHARACTER_UNASSIGNED_FROM_JOB, job, character);
    }
    public void AssignAttackGrid(CombatGrid grid) {
        attackGrid = grid;
    }
    public void AssignDefenseGrid(CombatGrid grid) {
        defenseGrid = grid;
    }
    public JOB GetCharactersCurrentJob(Character character) {
        foreach (KeyValuePair<JOB, PlayerJobData> keyValuePair in roleSlots) {
            if (keyValuePair.Value.assignedCharacter != null && keyValuePair.Value.assignedCharacter.id == character.id) {
                return keyValuePair.Key;
            }
        }
        return JOB.NONE;
    }
    public bool HasCharacterAssignedToJob(JOB job) {
        return roleSlots[job].assignedCharacter != null;
    }
    private List<Character> GetValidCharactersForJob(JOB job) {
        List<Character> valid = new List<Character>();
        for (int i = 0; i < minions.Count; i++) {
            Character currMinion = minions[i].character;
            if (CanAssignCharacterToJob(job, currMinion) && GetCharactersCurrentJob(currMinion) == JOB.NONE) {
                valid.Add(currMinion);
            }
        }
        return valid;
    }
    public void PreAssignJobSlots() {
        foreach (KeyValuePair<JOB, PlayerJobData> kvp in roleSlots) {
            List<Character> validCharacters = GetValidCharactersForJob(kvp.Key);
            if (validCharacters.Count > 0) {
                Character chosenCharacter = validCharacters[UnityEngine.Random.Range(0, validCharacters.Count)];
                AssignCharacterToJob(kvp.Key, chosenCharacter);
            } else {
                Debug.LogWarning("Could not pre assign any character to job: " + kvp.Key.ToString());
            }
        }
    }
    #endregion

    #region Role Actions
    public List<PlayerJobAction> GetJobActionsThatCanTarget(JOB job, IPointOfInterest target) {
        List<PlayerJobAction> actions = new List<PlayerJobAction>();
        if (HasCharacterAssignedToJob(job)) {
            for (int i = 0; i < roleSlots[job].jobActions.Count; i++) {
                PlayerJobAction currAction = roleSlots[job].jobActions[i];
                if (currAction.CanTarget(target)) {
                    actions.Add(currAction);
                }
            }
        }
        return actions;
    }
    public PlayerJobAction currentActivePlayerJobAction { get; private set; }
    public void SetCurrentlyActivePlayerJobAction(PlayerJobAction action) {
        PlayerJobAction previousActiveAction = currentActivePlayerJobAction;
        currentActivePlayerJobAction = action;
        if (currentActivePlayerJobAction == null) {
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
            PlayerJobActionButton jobActionButton = PlayerUI.Instance.GetPlayerJobActionButton(previousActiveAction);
            jobActionButton?.UpdateInteractableState();
            jobActionButton?.SetSelectedIconState(false);
        } else {
            PlayerJobActionButton jobActionButton = PlayerUI.Instance.GetPlayerJobActionButton(currentActivePlayerJobAction);
            //change the cursor
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Cross);
            CursorManager.Instance.AddLeftClickAction(TryExecuteCurrentActiveAction);
            CursorManager.Instance.AddLeftClickAction(() => SetCurrentlyActivePlayerJobAction(null));
            jobActionButton?.SetSelectedIconState(true);
        }
        
    }
    private void TryExecuteCurrentActiveAction() {
        string summary = "Mouse was clicked. Will try to execute " + currentActivePlayerJobAction.name;
        if (InteriorMapManager.Instance.currentlyShowingMap != null && InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter != null) {
            summary += " targetting " + InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter.name;
            if (currentActivePlayerJobAction.CanPerformActionTowards(currentActivePlayerJobAction.parentData.assignedCharacter, InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter)) {
                summary += "\nActivated action!";
                currentActivePlayerJobAction.ActivateAction(currentActivePlayerJobAction.parentData.assignedCharacter, InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter);
            } else {
                summary += "\nDid not activate action! Did not meet requirements";
            }
            UIManager.Instance.SetTempDisableShowInfoUI(true);
        } else {
            LocationGridTile hoveredTile = InteriorMapManager.Instance.GetTileFromMousePosition();
            if (hoveredTile != null && hoveredTile.objHere != null) {
                summary += " targetting " + hoveredTile.objHere.name;
                if (currentActivePlayerJobAction.CanPerformActionTowards(currentActivePlayerJobAction.parentData.assignedCharacter, hoveredTile.objHere)) {
                    summary += "\nActivated action!";
                    currentActivePlayerJobAction.ActivateAction(currentActivePlayerJobAction.parentData.assignedCharacter, hoveredTile.objHere);
                } else {
                    summary += "\nDid not activate action! Did not meet requirements";
                }
                UIManager.Instance.SetTempDisableShowInfoUI(true);
            } else {
                summary += "\nNo Target!";
            }
        }
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region Utilities
    private void OnCharacterDied(Character character) {
        JOB job = GetCharactersCurrentJob(character);
        if (job != JOB.NONE) {
            UnassignCharacterFromJob(job);
        }
    }
    public void SeenActionButtonsOnce() {
        if (!hasSeenActionButtonsOnce) {
            hasSeenActionButtonsOnce = true;
            Messenger.Broadcast(Signals.HAS_SEEN_ACTION_BUTTONS);
        }
    }
    #endregion

    #region Intel
    public void AddIntel(Intel newIntel) {
        if (!allIntel.Contains(newIntel)) {
            for (int i = 0; i < allIntel.Count; i++) {
                Intel currIntel = allIntel[i];
                if (currIntel.intelLog == newIntel.intelLog) {
                    return;
                }
            }
            allIntel.Add(newIntel);
            if (allIntel.Count > MAX_INTEL) {
                RemoveIntel(allIntel[0]);
            }
            Messenger.Broadcast(Signals.PLAYER_OBTAINED_INTEL, newIntel);
        }
    }
    public void RemoveIntel(Intel intel) {
        if (allIntel.Remove(intel)) {
            Messenger.Broadcast(Signals.PLAYER_REMOVED_INTEL, intel);
        }
    }
    public bool AlreadyHasIntel(Intel intel) {
        return allIntel.Contains(intel);
    }
    /// <summary>
    /// Listener for when a character has finished doing an action.
    /// </summary>
    /// <param name="character">The character that finished the action.</param>
    /// <param name="action">The action that was finished.</param>
    private void OnCharacterDidAction(Character character, GoapAction action) {
        bool showPopup = false;
        if (action.showIntelNotification) {
            if (action.shouldIntelNotificationOnlyIfActorIsActive) {
                showPopup = ShouldShowNotificationFrom(character, true);
            } else {
                showPopup = ShouldShowNotificationFrom(character, action.currentState.descriptionLog);
            }
        }
        if (showPopup) {
            if(!action.isNotificationAnIntel) {
                Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, action.currentState.descriptionLog);
            } else {
                Messenger.Broadcast<Intel>(Signals.SHOW_INTEL_NOTIFICATION, InteractionManager.Instance.CreateNewIntel(action, character));
            }
        }
    }
    /// <summary>
    /// Listener for when a character starts an action.
    /// Character will go to target location. <see cref="GoapAction.DoAction"/>
    /// </summary>
    /// <param name="character">The character that will do the action.</param>
    /// <param name="action">The action that will be performed.</param>
    private void OnCharacterDoingAction(Character character, GoapAction action) {
        bool showPopup = false;
        Log log = action.GetCurrentLog();
        if (action.showIntelNotification && !action.IsActorAtTargetTile() && log != null) { //added checking if actor is already at target tile. So that travelling notification won't show if that is the case.
            if (action.shouldIntelNotificationOnlyIfActorIsActive) {
                showPopup = ShouldShowNotificationFrom(action.actor, true);
            } else {
                showPopup = ShouldShowNotificationFrom(action.actor, log);
            }
        }
        if (showPopup) {
            Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, log);
        }
    }
    /// <summary>
    /// Listener for when an action's state is set. Always means that the character has
    /// started performing the action.
    /// </summary>
    /// <param name="action">The action that is being performed.</param>
    /// <param name="state">The state that the action is in.</param>
    private void OnActionStateSet(GoapAction action, GoapActionState state) {
        bool showPopup = false;
        Log log = action.GetCurrentLog();
        if (action.showIntelNotification && state.duration > 0 && log != null) { //added checking for duration because this notification should only show for actions that have durations.
            if (action.shouldIntelNotificationOnlyIfActorIsActive) {
                showPopup = ShouldShowNotificationFrom(action.actor, true);
            } else {
                showPopup = ShouldShowNotificationFrom(action.actor, log);
            }
        }
        if (showPopup) {
            Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, log);
        }
    }
    #endregion

    #region Player Notifications
    public bool ShouldShowNotificationFrom(Character character, bool onlyClickedCharacter = false) {
#if TRAILER_BUILD
        if (character.name == "Fiona" || character.name == "Jamie" || character.name == "Audrey") {
            return true;
        }
        return false;
#endif

        if (!onlyClickedCharacter && !character.isDead && AreaMapCameraMove.Instance.gameObject.activeSelf) {
            if((UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) || AreaMapCameraMove.Instance.CanSee(character.marker.gameObject)) {
                return true;
            }
        } else if (onlyClickedCharacter && UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) {
            return true;
        } else if (roleSlots[JOB.SPY].activeAction is Track) {
            Track track = roleSlots[JOB.SPY].activeAction as Track;
            if (track.target == character) {
                return true;
            }
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Character character, Log log) {
#if TRAILER_BUILD
        if (character.name == "Fiona" || character.name == "Jamie" || character.name == "Audrey") {
            return true;
        }
        return false;
#endif
        if (ShouldShowNotificationFrom(character)) {
            return true;
        } else {
            return ShouldShowNotificationFrom(log.fillers.Where(x => x.obj is Character).Select(x => x.obj as Character).ToArray());
        }
    }
    private bool ShouldShowNotificationFrom(Character[] characters) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i])) {
                return true;
            }
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Character[] characters, Log log) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i], log)) {
                return true;
            }
        }
        return false;
    }
    public bool ShowNotificationFrom(Character character, Log log) {
        if (ShouldShowNotificationFrom(character, log)) {
            ShowNotification(log);
            return true;
        }
        return false;
    }
    public void ShowNotificationFrom(Log log, params Character[] characters) {
        if (ShouldShowNotificationFrom(characters, log)) {
            ShowNotification(log);
        }
    }
    public void ShowNotificationFrom(Log log, Character character, bool onlyClickedCharacter) {
        if (ShouldShowNotificationFrom(character, onlyClickedCharacter)) {
            ShowNotification(log);
        }
    }
    public void ShowNotification(Log log) {
        Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, log);
    }
    #endregion
}

