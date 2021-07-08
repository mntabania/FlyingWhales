using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class HarpyBehaviour : BaseMonsterBehaviour {

    public HarpyBehaviour() {
        priority = 9;
    }

    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.currentStructure is Kennel) {
            return false;
        }
        if (character.IsAtHome()) {
            //try to lay an egg
            if (GameUtilities.RollChance(1) && !(character.currentStructure is RuinedZoo)) {
                if (TryTriggerLayEgg(character, 4, TILE_OBJECT_TYPE.HARPY_EGG, out producedJob)) {
                    return true;
                }
            }
            Harpy harpy = character as Harpy;
            if (!harpy.hasCapturedForTheDay) {
                harpy.SetHasCapturedForTheDay(true);
                if (ChanceData.RollChance(CHANCE_TYPE.Harpy_Capture)) {
                    if (TryCaptureCharacter(character, out producedJob)) {
                        return true;
                    }
                }
            }
            return character.jobComponent.TriggerRoamAroundTile(out producedJob);
        } else if (character.HasHome()) {
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        }
        return false;
    }
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob)) {
            return true;
        }
#if DEBUG_LOG
        p_log = $"{p_log}\n-Will try to take personal patrol job.";
#endif
        if (TryTakePersonalPatrolJob(p_character, 15, ref p_log, out p_producedJob)) {
            return true;
        }
#if DEBUG_LOG
        p_log = $"{p_log}\n-Will try to lay egg";
#endif
        if (GameUtilities.RollChance(1, ref p_log)) {
            if (TryTriggerLayEgg(p_character, 5, TILE_OBJECT_TYPE.HARPY_EGG, out p_producedJob)) {
#if DEBUG_LOG
                p_log = $"{p_log}\n-Will lay an egg";
#endif
                return true;
            }
        }
#if DEBUG_LOG
        p_log = $"{p_log}\n-Will roam";
#endif
        return p_character.jobComponent.TriggerRoamAroundTile(out p_producedJob);
    }

    private bool TryCaptureCharacter(Character actor, out JobQueueItem producedJob) {
        producedJob = null;
        Region region = actor.currentRegion;
        if(region != null) {
            Character chosenTargetCharacter = GetTargetForCapture(actor, region);
            if(chosenTargetCharacter != null) {
                LocationStructure chosenTargetStructure = GetDestinationToDropCapturedCharacter(actor, region);
                if(chosenTargetStructure != null) {
                    return actor.jobComponent.TryTriggerCaptureCharacter(chosenTargetCharacter, chosenTargetStructure, out producedJob, true);
                }
            }
        }
        return false;
    }
    private Character GetTargetForCapture(Character actor, Region region) {
        List<Character> characters = ObjectPoolManager.Instance.CreateNewCharactersList();
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if(character != actor && character.race != actor.race && !character.isHidden && !character.isDead && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried()) {
                characters.Add(character);
            }
        }
        Character chosenCharacter = null;
        if(characters.Count > 0) {
            chosenCharacter = characters[GameUtilities.RandomBetweenTwoNumbers(0, characters.Count - 1)];
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(characters);
        return chosenCharacter;
    }
    private LocationStructure GetDestinationToDropCapturedCharacter(Character actor, Region region) {
        List<LocationStructure> structures = RuinarchListPool<LocationStructure>.Claim();
        for (int i = 0; i < region.allSpecialStructures.Count; i++) {
            LocationStructure structure = region.allSpecialStructures[i];
            if (structure != actor.homeStructure && structure.passableTiles.Count > 0) {
                structures.Add(structure);
            }
        }
        LocationStructure chosenStructure = null;
        if (structures.Count > 0) {
            chosenStructure = structures[GameUtilities.RandomBetweenTwoNumbers(0, structures.Count - 1)];
        }
        RuinarchListPool<LocationStructure>.Release(structures);
        return chosenStructure;
    }
}