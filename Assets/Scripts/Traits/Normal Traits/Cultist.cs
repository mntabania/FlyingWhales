using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Traits {
    public class Cultist : Trait {
        public override bool isSingleton => true;
        public override bool affectsNameIcon => true;
        public Cultist() {
            name = "Cultist";
            description = "Worships us, but only secretly. Produces a Chaos Orb when praying. May produce 4 Chaos Orbs by performing a Dark Ritual.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = false;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CULTIST_TRANSFORM };
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                //character.AddPlayerAction(SPELL_TYPE.CULTIST_TRANSFORM);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_POISON);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.EVANGELIZE);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.SPREAD_RUMOR);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.FOUND_CULT);
                character.jobComponent.AddAbleJob(JOB_TYPE.STEAL_CORPSE);
                character.jobComponent.AddAbleJob(JOB_TYPE.SUMMON_BONE_GOLEM);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character character) {
                character.behaviourComponent.AddBehaviourComponent(typeof(CultistBehaviour));
                character.AddItemAsInteresting("Cultist Kit");
                character.isInfoUnlocked = true;
                //character.AddPlayerAction(SPELL_TYPE.CULTIST_TRANSFORM);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_POISON);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.EVANGELIZE);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.SPREAD_RUMOR);
                character.AddPlayerAction(PLAYER_SKILL_TYPE.FOUND_CULT);
                character.jobComponent.AddAbleJob(JOB_TYPE.STEAL_CORPSE);
                character.jobComponent.AddAbleJob(JOB_TYPE.SUMMON_BONE_GOLEM);
                character.traitContainer.AddTrait(character, "Nocturnal");

                //if necromancer is a cultist then make the undead faction friendly towards the player.
                if (character.traitContainer.HasTrait("Necromancer")) {
                    FactionManager.Instance.undeadFaction.SetRelationshipFor(PlayerManager.Instance.player.playerFaction, FACTION_RELATIONSHIP_STATUS.Friendly);
                }
                if (character.traitContainer.HasTrait("Blessed")) {
                    character.traitContainer.RemoveTrait(character, "Blessed");
                }
                Messenger.Broadcast(CharacterSignals.CHARACTER_BECOME_CULTIST, character);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            base.OnRemoveTrait(sourceCharacter, removedBy);
            if (sourceCharacter is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(CultistBehaviour));
                character.RemoveItemAsInteresting("Cultist Kit");
                //character.RemovePlayerAction(SPELL_TYPE.CULTIST_TRANSFORM);
                character.RemovePlayerAction(PLAYER_SKILL_TYPE.CULTIST_POISON);
                character.RemovePlayerAction(PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP);
                character.RemovePlayerAction(PLAYER_SKILL_TYPE.EVANGELIZE);
                character.RemovePlayerAction(PLAYER_SKILL_TYPE.SPREAD_RUMOR);
                character.RemovePlayerAction(PLAYER_SKILL_TYPE.FOUND_CULT);
                character.jobComponent.RemoveAbleJob(JOB_TYPE.STEAL_CORPSE);
                character.jobComponent.RemoveAbleJob(JOB_TYPE.SUMMON_BONE_GOLEM);
                
                Messenger.Broadcast(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, character);
            }
        }
        public override bool OnDeath(Character character) {
            character.traitContainer.RemoveTrait(character, this);
            return base.OnDeath(character);
        }
        //Disabled Trigger flaw in cultist because cultists already have the Cultist Transform, Poison and Booby Trap actions
        // public override string TriggerFlaw(Character character) {
        //     CultistBehaviour cultistBehaviour = CharacterManager.Instance.GetCharacterBehaviourComponent<CultistBehaviour>(typeof(CultistBehaviour));
        //     string log = string.Empty;
        //     if (cultistBehaviour.TryCreateCultistJob(character, ref log, out var producedJob)) {
        //         character.jobQueue.AddJobInQueue(producedJob);
        //         return base.TriggerFlaw(character);
        //     } else {
        //         return "no_job";
        //     }
        // }
        #endregion
    }
}

