using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class ThreatComponent {
    public Player player { get; private set; }

    public const int MAX_THREAT = 100;
    public int threat { get; private set; }
    public int threatPerHour { get; private set; }

    /// <summary>
    /// The list of characters that are currently attacking your demonic structure.
    /// NOTE: This is only updated when threat has reached maximum
    /// </summary>
    public List<Character> attackingCharacters { get; private set; }

    public ThreatComponent(Player player) {
        this.player = player;
        Messenger.AddListener(Signals.HOUR_STARTED, PerHour);
    }
    private void PerHour() {
        AdjustThreat(threatPerHour);
    }

    public void AdjustThreat(int amount) {
        if(!Tutorial.TutorialManager.Instance.HasTutorialBeenCompleted(Tutorial.TutorialManager.Tutorial.Invade_A_Village) && !Settings.SettingsManager.Instance.settings.skipTutorials) {
            //Threat does not increase until Tutorial is over, and since the last tutorial is Invade a village, it should be the checker
            //https://trello.com/c/WOZJmvzQ/1238-threat-does-not-increase-until-tutorial-is-over
            return;
        }
        if (WorldSettings.Instance.worldSettingsData.noThreatMode) {
            return;
        }

        if (threat != MAX_THREAT) {
            int supposedThreat = threat + amount;
            bool hasReachedMax = supposedThreat >= MAX_THREAT;
            threat = supposedThreat;
            threat = Mathf.Clamp(threat, 0, 100);

            if (amount > 0) {
                Messenger.Broadcast(Signals.THREAT_INCREASED, amount);
            }

            if (hasReachedMax) {
                OnMaxThreat();
                Messenger.Broadcast(Signals.THREAT_MAXED_OUT);
            }
            Messenger.Broadcast(Signals.THREAT_UPDATED);
        }
    }
    public void AdjustThreatPerHour(int amount) {
        threatPerHour += amount;
    }
    public void SetThreatPerHour(int amount) {
        threatPerHour = amount;
    }
    private void OnMaxThreat() {
        AssaultDemonicStructure(out List<Character> _attackingCharacters);
        attackingCharacters = _attackingCharacters;
        ResetThreatAfterHours(2);
    }
    private void ResetThreatAfterHours(int hours) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(hours));
        SchedulingManager.Instance.AddEntry(dueDate, ResetThreat, player);
    }
    public void ResetThreat() {
        threat = 0;
        SetThreatPerHour(0);
        Messenger.Broadcast(Signals.THREAT_UPDATED);
        Messenger.Broadcast(Signals.THREAT_RESET);
    }

    private void AssaultDemonicStructure(out List<Character> attackingCharacters) {
        string debugLog = GameManager.Instance.TodayLogString() + "Assault Demonic Structure";
        LocationStructure targetDemonicStructure = null;
        if (InnerMapManager.Instance.HasExistingWorldKnownDemonicStructure()) {
            targetDemonicStructure = InnerMapManager.Instance.worldKnownDemonicStructures[UnityEngine.Random.Range(0, InnerMapManager.Instance.worldKnownDemonicStructures.Count)];
        } else {
            targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
        }
        if (targetDemonicStructure == null) {
            //it is assumed that this only happens if the player casts a spell that is seen by another character,
            //but results in the destruction of the portal
            attackingCharacters = null;
            return;
        }
        debugLog += "\n-TARGET: " + targetDemonicStructure.name;
        List<Character> characters = new List<Character>();
        int count = 0;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if (character.canPerform && character.canMove && character.canWitness && character.faction.isMajorNonPlayerFriendlyNeutral
                && (character.race == RACE.HUMANS || character.race == RACE.ELVES) 
                && !character.combatComponent.isInCombat
                && !(character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)
                && character.traitContainer.HasTrait("Combatant")
                && character.isAlliedWithPlayer == false) {
                count++;
                debugLog += "\n-RETALIATOR: " + character.name;
                characters.Add(character);
                //character.behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
                if (count >= 5) {
                    break;
                }
            }
        }
        if (characters.Count < 3) {
            //Create Angels
            CharacterManager.Instance.SetCurrentDemonicStructureTargetOfAngels(targetDemonicStructure as DemonicStructure);
            //NPCSettlement spawnSettlement = LandmarkManager.Instance.GetRandomVillageSettlement();
            //region = spawnSettlement.region;
            Region region = targetDemonicStructure.location;
            HexTile spawnHex = targetDemonicStructure.location.GetRandomUncorruptedPlainHex();
            //if (spawnSettlement != null) {
            //    spawnHex = spawnSettlement.GetRandomHexTile();
            //} else {
            //    spawnHex = targetDemonicStructure.location.GetRandomPlainHex();
            //}
            characters.Clear();
            int angelCount = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < angelCount; i++) {
                SUMMON_TYPE angelType = SUMMON_TYPE.Warrior_Angel;
                if(UnityEngine.Random.Range(0, 2) == 0) { angelType = SUMMON_TYPE.Magical_Angel; }
                LocationGridTile spawnTile = spawnHex.GetRandomTile();
                Summon angel = CharacterManager.Instance.CreateNewSummon(angelType, FactionManager.Instance.vagrantFaction, homeRegion: region);
                CharacterManager.Instance.PlaceSummon(angel, spawnTile);
                angel.behaviourComponent.SetIsAttackingDemonicStructure(true, CharacterManager.Instance.currentDemonicStructureTargetOfAngels);
                characters.Add(angel);
            }
            attackingCharacters = characters;
            Messenger.Broadcast(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, characters);
        } else {
            for (int i = 0; i < characters.Count; i++) {
                characters[i].behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
            }
            attackingCharacters = characters;
            Messenger.Broadcast(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, characters, targetDemonicStructure as DemonicStructure);    
        }

        Debug.Log(debugLog);
    }
}
