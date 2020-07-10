using System;
using System.Collections.Generic;
using UtilityScripts;

public class DisablerBehaviour : CharacterBehaviourComponent {

    public DisablerBehaviour() {
        priority = 30;
    }

    #region Overrides
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        //check if can follow an invader.
        if (character.behaviourComponent.invaderToFollow == null) {
            List<Character> followChoices = GetInvaderToFollowChoices(character);
            if (followChoices != null) {
                Character characterToFollow = CollectionUtilities.GetRandomElement(followChoices);
                character.behaviourComponent.SetInvaderToFollow(characterToFollow);
            }
        }
        
        //if has invader to follow, create job to follow the target
        if (character.behaviourComponent.invaderToFollow != null) {
            if (character.behaviourComponent.canDisable && 
                character.marker.inVisionCharacters.Contains(character.behaviourComponent.invaderToFollow)) {
                //can already see target to follow, check if can disable anyone inside vision of follow target
                List<Character> targets = GetDisableTargetInVisionOfInvader(character);
                if (targets != null) {
                    Character target = CollectionUtilities.GetRandomElement(targets);
                    return character.jobComponent.TriggerDisable(target, out producedJob);
                }
            } 
            return character.jobComponent.CreateGoToJob(character.behaviourComponent.invaderToFollow, out producedJob);
        } else {
            if (character.behaviourComponent.canDisable) {
                if (character.IsAtTerritory() || character.isAtHomeStructure) {
                    log += $"\n-character is at territory";
                    //if is at territory, check if there are any villagers in its territory,
                    List<Character> charactersAtTerritory =
                        character.hexTileLocation.GetAllCharactersInsideHexThatMeetCriteria(c =>
                            c.isNormalCharacter && c.isDead == false && c != character && c.canMove && c.isAlliedWithPlayer == false);
                    if (charactersAtTerritory != null) {
                        log += $"\n-There are villagers in territory, will do De-Mood towards them";
                        //if there are villagers in its territory, then do De-Mood action towards them.
                        //Trigger De-Mood Action towards target
                        Character target = CollectionUtilities.GetRandomElement(charactersAtTerritory);
                        return character.jobComponent.TriggerDisable(target, out producedJob);
                    } else {
                        log += $"\n-There are no villagers in territory, will do roam around territory";
                        //if there are NO villagers in its territory then trigger roam around territory.
                        return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
                    }
                } else {
                    return character.jobComponent.TriggerReturnTerritory(out producedJob);
                }
            } else {
                if (character.IsAtTerritory() || character.isAtHomeStructure) {
                    return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
                } else {
                    return character.jobComponent.TriggerReturnTerritory(out producedJob);
                }
            }    
        }
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.traitContainer.AddTrait(character, "Stealthy");
        character.behaviourComponent.OnBecomeDisabler();
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.traitContainer.RemoveTrait(character, "Stealthy");
        character.behaviourComponent.OnNoLongerDisabler();
    }
    #endregion

    
    private List<Character> GetInvaderToFollowChoices(Character disabler) {
        List<Character> choices = null;
        //prioritize finding invaders without followers yet
        for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
            Character character = PlayerManager.Instance.player.playerFaction.characters[i];
            if (character.currentRegion == disabler.currentRegion && 
                character.behaviourComponent.HasBehaviour(typeof(InvadeBehaviour)) && character.isDead == false && 
                character.behaviourComponent.followerCount <= 0) {
                if (choices == null) {
                    choices = new List<Character>();
                }
                choices.Add(character);
            }
        }

        if (choices == null) {
            //if no invaders were found, check invaders that already have followers
            for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
                Character character = PlayerManager.Instance.player.playerFaction.characters[i];
                if (character.currentRegion == disabler.currentRegion && 
                    character.behaviourComponent.HasBehaviour(typeof(InvadeBehaviour)) && character.isDead == false) {
                    if (choices == null) {
                        choices = new List<Character>();
                    }
                    choices.Add(character);
                }
            }
        }
        
        return choices;
    }
    private List<Character> GetDisableTargetInVisionOfInvader(Character disabler) {
        List<Character> choices = null;
        for (int i = 0; i < disabler.behaviourComponent.invaderToFollow.marker.inVisionCharacters.Count; i++) {
            Character inVisionCharacter = disabler.behaviourComponent.invaderToFollow.marker.inVisionCharacters[i];
            if (inVisionCharacter != disabler && inVisionCharacter.canMove) {
                if (choices == null) {
                    choices = new List<Character>();
                }
                choices.Add(inVisionCharacter);
            }
        }
        return choices;
    }
}
