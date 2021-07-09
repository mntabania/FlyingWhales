using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Betrayed : Status {
        //Not singleton for responsible characters
        //public override bool isSingleton => true;

        public Betrayed() {
            name = "Betrayed";
            description = "Someone backstabbed it.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(48);
            moodEffect = -10;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.25f;
            hindersSocials = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }
        public override bool OnDeath(Character character) {
            //spawn a ghost after 30 minutes
            SchedulingManager.Instance.AddEntry(
                GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(30)),
                () => SpawnGhostOf(character), this);
            return base.OnDeath(character);
        }
        private void SpawnGhostOf(Character character) {
            if (character.gridTileLocation == null) {
#if DEBUG_LOG
                Debug.Log($"{character.name} was not placed because {character.name} no longer has a gridTileLocation.");
#endif
                return;
            }
            Summon ghost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, FactionManager.Instance.GetDefaultFactionForMonster(SUMMON_TYPE.Ghost), null, character.homeRegion);
            ghost.SetFirstAndLastName(character.firstName, character.surName);
            (ghost as Ghost).SetBetrayedBy(responsibleCharacter);
            CharacterManager.Instance.PlaceSummonInitially(ghost, character.gridTileLocation);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", this.name, "spawn_ghost", null, LogUtilities.Social_Life_Changes_Tags);
            log.AddToFillers(ghost, ghost.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToDatabase(true);
        }
    }
}