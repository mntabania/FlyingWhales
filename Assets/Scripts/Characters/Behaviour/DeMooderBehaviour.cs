using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class DeMooderBehaviour : CharacterBehaviourComponent {

    public DeMooderBehaviour() {
        priority = 30;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n{character.name} is a De-Mooder";
        if (character.behaviourComponent.canDeMood) {
            log += $"\n-Can De-Mood";
            //if de-mood village target is not null
            if (character.behaviourComponent.deMoodVillageTarget != null) {
                log += $"\n-character is already at target village";
                //check if character is already at village target.
                if (character.hexTileLocation != null && character.behaviourComponent.deMoodVillageTarget.Contains(character.hexTileLocation)) {
                    //if yes, pick target inside target village, then do De-Mood action towards it.
                    List<Character> choices = GetTargetChoices(character.behaviourComponent.deMoodVillageTarget);
                    if (choices != null) {
                        //Create job to de mood target.
                        Character target = CollectionUtilities.GetRandomElement(choices);
                        log += $"\n-character will now De-Mood target {target.name}";
                        return character.jobComponent.TriggerDecreaseMood(target, out producedJob);
                    } else {
                        //if no target was found then clear village target then go back to territory.
                        character.behaviourComponent.SetDeMoodVillageTarget(null);
                        log += $"\n-character can target no one at target village, clearing data and returning to territory";
                        return character.jobComponent.TriggerReturnTerritory(out producedJob);
                    }    
                } else {
                    log += $"\n-character not yet at target village. Going there now...";
                    //if not, then go there.
                    HexTile targetHextile =
                        CollectionUtilities.GetRandomElement(character.behaviourComponent.deMoodVillageTarget);
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHextile.locationGridTiles);
                    return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                }
            } else {
                log += $"\n-character does not have a target village";
                //if de-mood village target is null
                //check if will trigger de-mood attack
                if (GameUtilities.RollChance(15, ref log)) {
                    log += $"\n-De-Mood chance met. Will find target village";
                    //if de-mood attack was triggered, pick village to go to, set that as de-mood village target, then trigger go there.
                    List<HexTile> deMoodVillageTargets = character.behaviourComponent.GetVillageTargetsByPriority();
                    if (deMoodVillageTargets != null && deMoodVillageTargets.Count > 0) {
                        log += $"\n-target village found and will go there now.";
                        //Go to target village/area
                        character.behaviourComponent.SetDeMoodVillageTarget(deMoodVillageTargets);
                        HexTile targetHextile =
                            CollectionUtilities.GetRandomElement(character.behaviourComponent.deMoodVillageTarget);
                        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHextile.locationGridTiles);
                        return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                    } else {
                        log += $"\n-no valid villages found, will roam around territory instead.";
                      //if no valid village targets were found, then trigger roam around territory  
                      return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
                    }
                } else {
                    log += $"\n-De-Mood chance not met.";
                    //if de-mood attack was not triggered, check if is at territory
                    if (character.hexTileLocation != null && character.territorries.Contains(character.hexTileLocation)) {
                        log += $"\n-character is at territory";
                        //if is at territory, check if there are any villagers in its territory,
                        List<Character> charactersAtTerritory =
                            character.hexTileLocation.GetAllCharactersInsideHexThatMeetCriteria(c =>
                                c.isNormalCharacter && c.isDead == false && c != character);
                        if (charactersAtTerritory != null) {
                            log += $"\n-There are villagers in territory, will do De-Mood towards them";
                            //if there are villagers in its territory, then do De-Mood action towards them.
                            //Trigger De-Mood Action towards target
                            Character target = CollectionUtilities.GetRandomElement(charactersAtTerritory);
                            return character.jobComponent.TriggerDecreaseMoodInTerritory(target, out producedJob);
                        } else {
                            log += $"\n-There are no villagers in territory, will do roam around territory";
                            //if there are NO villagers in its territory then trigger roam around territory.
                            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
                        }
                    } else {
                        log += $"\n-Character is not at territory, going back...";
                        //if not, trigger go back to territory
                        return character.jobComponent.TriggerReturnTerritory(out producedJob);
                    }
                }
            }
        } else {
            log += $"\n-Cannot De-Mood because ability is in cooldown";
            //if cannot de mood, check if at territory
            if (character.hexTileLocation != null && character.territorries.Contains(character.hexTileLocation)) {
                log += $"\n-character is at territory, roaming around territory";
                //if at territory, trigger roam around territory
                return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
            } else {
                log += $"\n-character is NOT at territory, returning to territory";
                //if not at territory, go back to territory
                return character.jobComponent.TriggerReturnTerritory(out producedJob);
            }
        }
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.traitContainer.AddTrait(character, "Stealthy");
        character.behaviourComponent.OnBecomeDeMooder();
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.traitContainer.RemoveTrait(character, "Stealthy");
        character.behaviourComponent.OnNoLongerDeMooder();
    }

    private List<Character> GetTargetChoices(List<HexTile> tiles) {
        List<Character> characters = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            List<Character> charactersAtHexTile = tile.GetAllCharactersInsideHex();
            if (charactersAtHexTile != null) {
                for (int j = 0; j < charactersAtHexTile.Count; j++) {
                    Character character = charactersAtHexTile[j];
                    if (character.isNormalCharacter && character.isDead == false 
                        && character.traitContainer.HasTrait("Dolorous") == false) {
                        if (characters == null) {
                            characters = new List<Character>();
                        }
                        characters.Add(character);
                    }
                }    
            }
        }
        return characters;
    }
}
