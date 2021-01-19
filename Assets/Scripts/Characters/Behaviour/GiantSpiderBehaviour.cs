using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class GiantSpiderBehaviour : BaseMonsterBehaviour {

    public GiantSpiderBehaviour() {
        priority = 9;
    }
    
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.currentStructure is Kennel) {
            return false;
        }
        //between 12am to 3am
        if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) >= 0 && 
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) <= 3) {
            List<Character> webbedCharacters = GetWebbedCharactersAtHome(character);
            if (webbedCharacters == null || webbedCharacters.Count <= 1) { //check if there are only 1 or less abducted "Food" at home structure
                if (character.behaviourComponent.currentAbductTarget != null 
                    && (character.behaviourComponent.currentAbductTarget.isDead 
                        || character.behaviourComponent.currentAbductTarget.traitContainer.HasTrait("Restrained"))) {
                    character.behaviourComponent.SetAbductionTarget(null);
                }
            
                //set abduction target if none, and chance met
                if (character.homeStructure != null && character.behaviourComponent.currentAbductTarget == null  && GameUtilities.RollChance(1)) {
                    List<Character> characterChoices = ObjectPoolManager.Instance.CreateNewCharactersList();
                    for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                        Character c = character.currentRegion.charactersAtLocation[i];
                        if (c != character && !c.isDead && (c is Animal || (c.isNormalCharacter && c.traitContainer.HasTrait("Resting"))) && c.currentStructure is Kennel == false) {
                            characterChoices.Add(c);
                        }
                    }
                    if (characterChoices.Count > 0) {
                        Character chosenCharacter = CollectionUtilities.GetRandomElement(characterChoices);
                        character.behaviourComponent.SetAbductionTarget(chosenCharacter);
                    }
                    ObjectPoolManager.Instance.ReturnCharactersListToPool(characterChoices);
                }

                Character targetCharacter = character.behaviourComponent.currentAbductTarget;
                if (targetCharacter != null) {
                    //create job to abduct target character.
                    return character.jobComponent.TriggerMonsterAbduct(targetCharacter, out producedJob);
                }
            }
        }

        //try to lay an egg
        if (GameUtilities.RollChance(1) && (character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory()) && !(character.currentStructure is RuinedZoo)) {
            if (TryTriggerLayEgg(character, 4, out producedJob)) {
                return true;
            }
        }

        if (GameUtilities.RollChance(30)) {
            //Try and eat a webbed character at this spiders home cave
            List<Character> webbedCharacters = GetWebbedCharactersAtHome(character);
            if (webbedCharacters != null && webbedCharacters.Count > 0) {
                Character webbedCharacter = CollectionUtilities.GetRandomElement(webbedCharacters);
                return character.jobComponent.TriggerEatAlive(webbedCharacter, out producedJob);
            }    
        }
        
        return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, true);
    }
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob)) {
            return true;
        } else {
            TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.GetCurrentTimeInWordsOfTick();
            p_log = $"{p_log}\n-Will check if can abduct, current time is {currentTimeInWordsOfTick.ToString()}";
            if ((currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT) && GameUtilities.RollChance(5, ref p_log)) {
                List<Character> abductChoices = ObjectPoolManager.Instance.CreateNewCharactersList();
                for (int i = 0; i < p_character.currentRegion.charactersAtLocation.Count; i++) {
                    Character c = p_character.currentRegion.charactersAtLocation[i];
                    if (c != p_character && !c.isDead && c is Animal && c.currentStructure is DemonicStructure == false) {
                        abductChoices.Add(c);
                    }
                }
                p_log = $"{p_log}\n-Checking if can abduct any animals: {abductChoices.Count.ToString()}";
                if (abductChoices.Count > 0) {
                    Character chosenCharacter = CollectionUtilities.GetRandomElement(abductChoices);
                    ObjectPoolManager.Instance.ReturnCharactersListToPool(abductChoices);
                    LocationGridTile targetDropLocation = null;
                    if (p_character.homeSettlement?.mainStorage != null) {
                        targetDropLocation = CollectionUtilities.GetRandomElement(p_character.homeSettlement.mainStorage.tiles);
                    }
                    if (p_character.jobComponent.TriggerMonsterAbduct(chosenCharacter, out p_producedJob, targetDropLocation)) {
                        p_log = $"{p_log}\n-Will abduct {chosenCharacter.nameWithID} and drop at {targetDropLocation}";
                        return true;
                    }
                } else {
                    p_log = $"{p_log}\n-No valid animals to abduct, will find valid villager to abduct.";
                    //no available animals to abduct
                    for (int i = 0; i < p_character.currentRegion.charactersAtLocation.Count; i++) {
                        Character c = p_character.currentRegion.charactersAtLocation[i];
                        if (c != p_character && !c.isDead && c.isNormalCharacter && c.currentStructure is DemonicStructure == false && c.traitContainer.HasTrait("Resting") && 
                            c.faction != null && c.faction != p_character.faction && !p_character.faction.IsFriendlyWith(c.faction)) {
                            abductChoices.Add(c);
                        }
                    }
                    p_log = $"{p_log}\n-Checking if can abduct any villagers: {abductChoices.Count.ToString()}";
                    if (abductChoices.Count > 0) {
                        Character chosenCharacter = CollectionUtilities.GetRandomElement(abductChoices);
                        ObjectPoolManager.Instance.ReturnCharactersListToPool(abductChoices);
                        LocationGridTile targetDropLocation = null;
                        if (p_character.homeSettlement?.mainStorage != null) {
                            targetDropLocation = CollectionUtilities.GetRandomElement(p_character.homeSettlement.mainStorage.tiles);
                        }
                        if (p_character.jobComponent.TriggerMonsterAbduct(chosenCharacter, out p_producedJob, targetDropLocation)) {
                            p_log = $"{p_log}\n-Will abduct {chosenCharacter.nameWithID} and drop at {targetDropLocation}";
                            return true;
                        }
                    }
                }
            }

            p_log = $"{p_log}\n-Will try to take personal patrol job.";
            if (TryTakePersonalPatrolJob(p_character, 15, ref p_log, out p_producedJob)) {
                return true;
            }

            p_log = $"{p_log}\n-Will try to lay egg";
            if (GameUtilities.RollChance(3, ref p_log)) {
                if (TryTriggerLayEgg(p_character, 5, out p_producedJob)) {
                    p_log = $"{p_log}\n-Will lay an egg";
                    return true;
                }
            }

            return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
        }
    }
    
    private bool TryTriggerLayEgg(Character character, int maxResidentCount, out JobQueueItem producedJob) {
        int residentCount = 0;
        int eggCount = 0;
        if (character.homeSettlement != null) {
            residentCount = character.homeSettlement.residents.Count(x => x.isDead == false && (x is GiantSpider || x is SmallSpider));
            eggCount = character.homeSettlement.GetTileObjectsOfTypeThatMeetCriteria<SpiderEgg>()?.Count ?? 0;
        }
        else if (character.homeStructure != null) {
            residentCount = character.homeStructure.residents.Count(x => x.isDead == false);
            eggCount = character.homeStructure.GetNumberOfTileObjectsThatMeetCriteria(TILE_OBJECT_TYPE.SPIDER_EGG, null);
        }
        else if (character.HasTerritory()) {
            residentCount = character.homeRegion.GetCountOfAliveCharacterWithSameTerritory(character);
            eggCount += character.territory.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.SPIDER_EGG).Count;
        }
        if (residentCount < maxResidentCount && eggCount < 2) {
            return character.jobComponent.TriggerLayEgg(out producedJob);
        }
        producedJob = null;
        return false;
    }
    private List<Character> GetWebbedCharactersAtHome(Character character) {
        if (character.homeStructure != null) {
            return character.homeStructure.GetCharactersThatMeetCriteria(c => c.traitContainer.HasTrait("Webbed"));
        } else if (character.HasTerritory()) {
            List<Character> characters = character.territory.GetAllCharactersInsideHexThatMeetCriteria<Character>(c => c.traitContainer.HasTrait("Webbed"));
            return characters;
        }
        return null;
    }
   
}
