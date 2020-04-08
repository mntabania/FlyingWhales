using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Archetype {
    public class Normal : PlayerArchetype {
        public Normal() : base(PLAYER_ARCHETYPE.Normal) {
            minionClasses = CharacterManager.sevenDeadlySinsClassNames.ToList();
            afflictions = PlayerDB.afflictions;
            spells = PlayerDB.spells;
            // SPELL_TYPE.THE_SPIRE,
            demonicStructuresSkills = new List<SPELL_TYPE>() { SPELL_TYPE.THE_EYE, SPELL_TYPE.THE_KENNEL, SPELL_TYPE.THE_CRYPT, SPELL_TYPE.THE_GOADER, SPELL_TYPE.TORTURE_CHAMBER, SPELL_TYPE.DEMONIC_PRISON };
            monsters = new List<RaceClass>() {
                new RaceClass(RACE.WOLF, "Ravager"), new RaceClass(RACE.GOLEM, "Golem"),
                new RaceClass(RACE.SKELETON, "Archer"), new RaceClass(RACE.SKELETON, "Marauder"),
                new RaceClass(RACE.ELEMENTAL, "FireElemental"), new RaceClass(RACE.DEMON, "Incubus"), 
                new RaceClass(RACE.DEMON, "Succubus"), new RaceClass(RACE.KOBOLD, "Kobold"),
                new RaceClass(RACE.SPIDER, "GiantSpider")
            };
            minionPlayerSkills = new List<SPELL_TYPE>() { SPELL_TYPE.DEMON_WRATH, SPELL_TYPE.DEMON_PRIDE, SPELL_TYPE.DEMON_LUST, SPELL_TYPE.DEMON_GLUTTONY, SPELL_TYPE.DEMON_SLOTH, SPELL_TYPE.DEMON_ENVY, SPELL_TYPE.DEMON_GREED, };
            summonPlayerSkills = new List<SPELL_TYPE>() { SPELL_TYPE.SKELETON_MARAUDER, };

            //playerActions = new List<string>();
            SetCanTriggerFlaw(true);
            SetCanRemoveTraits(true);
        }

        #region Overrides
        public override bool CanAfflict(SPELL_TYPE type) {
            return true;
        }
        public override bool CanDoPlayerAction(SPELL_TYPE type) {
            return true;
        }
        public override bool CanSummonMinion(Minion minion) {
            return true;
        }
        public override bool CanBuildDemonicStructure(SPELL_TYPE type) {
            return true;
        }
        //public override bool CanCastSpell(string spellName) {
        //    return true;
        //}
        #endregion
    }
}