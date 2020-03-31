using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class PuppetMaster : PlayerArchetype {
        public PuppetMaster() : base(PLAYER_ARCHETYPE.Puppet_Master) {
            selectorDescription = "Master of monsters and calamities. Invading the world is easy for one that has access to such mayhem!";
            //playerActions = new List<string>() { PlayerDB.Afflict_Action, PlayerDB.Bless_Action, PlayerDB.Zap_Action, PlayerDB.Booby_Trap_Action
            //        , PlayerDB.Summon_Minion_Action, PlayerDB.Corrupt_Action, PlayerDB.Interfere_Action, PlayerDB.Stop_Action, PlayerDB.Return_To_Portal_Action
            //        , PlayerDB.Harass_Action, PlayerDB.Raid_Action, PlayerDB.Invade_Action
            //        , PlayerDB.End_Harass_Action, PlayerDB.End_Raid_Action, PlayerDB.End_Invade_Action
            //        , PlayerDB.Combat_Mode_Action, PlayerDB.Build_Demonic_Structure_Action, PlayerDB.Learn_Spell_Action
            //        , PlayerDB.Activate_Artifact_Action
            //};
            monsters = new List<RaceClass> { }; //No initial monsters
            //demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_EYE, LANDMARK_TYPE.THE_GOADER, LANDMARK_TYPE.DEMONIC_PRISON, LANDMARK_TYPE.THE_SPIRE, LANDMARK_TYPE.THE_CRYPT};
            minionClasses = new List<string>() { "Lust", "Envy", "Greed", "Gluttony" };
            afflictions = new List<SPELL_TYPE>() { SPELL_TYPE.PARALYSIS, SPELL_TYPE.UNFAITHFULNESS, SPELL_TYPE.KLEPTOMANIA, SPELL_TYPE.AGORAPHOBIA, SPELL_TYPE.PSYCHOPATHY, /*Pyromania, Cowardice, Bewitch*/ };
            spells = new List<SPELL_TYPE>() { SPELL_TYPE.RAVENOUS_SPIRIT, SPELL_TYPE.MANIFEST_FOOD/*, SPELL_TYPE.SPIDER_RAIN, SPELL_TYPE.WILD_GROWTH, SPELL_TYPE.FERTILITY, SPELL_TYPE.SPAWN_BANDIT_CAMP*/ };
            SetCanTriggerFlaw(true);
            SetCanRemoveTraits(true);
        }
    }
}