using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Archetype {
    public class Normal : PlayerArchetype {
        public Normal() : base(PLAYER_ARCHETYPE.Normal) {
            minionClasses = CharacterManager.sevenDeadlySinsClassNames.ToList();
            // SPELL_TYPE.THE_SPIRE,
            demonicStructuresSkills = new List<PLAYER_SKILL_TYPE>() { PLAYER_SKILL_TYPE.WATCHER, PLAYER_SKILL_TYPE.KENNEL, PLAYER_SKILL_TYPE.CRYPT, PLAYER_SKILL_TYPE.MEDDLER, PLAYER_SKILL_TYPE.TORTURE_CHAMBERS, PLAYER_SKILL_TYPE.DEMONIC_PRISON, PLAYER_SKILL_TYPE.SPIRE, PLAYER_SKILL_TYPE.MARAUD, PLAYER_SKILL_TYPE.MANA_PIT, PLAYER_SKILL_TYPE.DEFENSE_POINT, };
            monsters = new List<RaceClass>() {
                new RaceClass(RACE.WOLF, "Ravager"), new RaceClass(RACE.GOLEM, "Golem"),
                new RaceClass(RACE.SKELETON, "Archer"), new RaceClass(RACE.SKELETON, "Marauder"),
                new RaceClass(RACE.ELEMENTAL, "Fire Elemental"), new RaceClass(RACE.DEMON, "Incubus"), 
                new RaceClass(RACE.DEMON, "Succubus"), new RaceClass(RACE.KOBOLD, "Kobold"),
                new RaceClass(RACE.SPIDER, "Giant Spider")
            };
            minionPlayerSkills = new List<PLAYER_SKILL_TYPE>() { PLAYER_SKILL_TYPE.DEMON_WRATH, PLAYER_SKILL_TYPE.DEMON_PRIDE, PLAYER_SKILL_TYPE.DEMON_LUST, PLAYER_SKILL_TYPE.DEMON_GLUTTONY, PLAYER_SKILL_TYPE.DEMON_SLOTH, PLAYER_SKILL_TYPE.DEMON_ENVY, PLAYER_SKILL_TYPE.DEMON_GREED, };
            summonPlayerSkills = new List<PLAYER_SKILL_TYPE>() { PLAYER_SKILL_TYPE.SKELETON, };

            //playerActions = new List<string>();
            SetCanTriggerFlaw(true);
            SetCanRemoveTraits(true);
        }

        #region Overrides
        public override bool CanAfflict(PLAYER_SKILL_TYPE type) {
            return true;
        }
        public override bool CanDoPlayerAction(PLAYER_SKILL_TYPE type) {
            return true;
        }
        public override bool CanSummonMinion(Minion minion) {
            return true;
        }
        public override bool CanBuildDemonicStructure(PLAYER_SKILL_TYPE type) {
            return true;
        }
        //public override bool CanCastSpell(string spellName) {
        //    return true;
        //}
        #endregion
    }
}