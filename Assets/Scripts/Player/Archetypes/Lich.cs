using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class Lich : PlayerArchetype {
        public Lich() : base(PLAYER_ARCHETYPE.Lich) {
            selectorDescription = "Master of monsters and calamities. Invading the world is easy for one that has access to such mayhem!";
            //playerActions = new List<string>() { PlayerDB.Afflict_Action, PlayerDB.Poison_Action, PlayerDB.Seize_Object_Action, PlayerDB.Animate_Action, PlayerDB.Summon_Minion_Action
            //        , PlayerDB.Corrupt_Action, PlayerDB.Stop_Action, PlayerDB.Return_To_Portal_Action
            //        , PlayerDB.Harass_Action, PlayerDB.Raid_Action, PlayerDB.Invade_Action
            //        , PlayerDB.End_Harass_Action, PlayerDB.End_Raid_Action, PlayerDB.End_Invade_Action
            //        , PlayerDB.Combat_Mode_Action, PlayerDB.Build_Demonic_Structure_Action, PlayerDB.Breed_Monster_Action
            //        , PlayerDB.Learn_Spell_Action, PlayerDB.Activate_Artifact_Action
            //};
            monsters = new List<RaceClass> { new RaceClass(RACE.SKELETON, "Archer"), new RaceClass(RACE.SKELETON, "Marauder") };
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_PROFANE, LANDMARK_TYPE.THE_KENNEL, LANDMARK_TYPE.TORTURE_CHAMBER, LANDMARK_TYPE.DEMONIC_PRISON, LANDMARK_TYPE.THE_SPIRE, LANDMARK_TYPE.THE_CRYPT };
            minionClasses = new List<string>() { "Pride", "Sloth", "Lust", "Gluttony" };
            afflictions = new List<SPELL_TYPE>() { SPELL_TYPE.PESTILENCE, SPELL_TYPE.LYCANTHROPY, SPELL_TYPE.VAMPIRISM, SPELL_TYPE.ZOMBIE_VIRUS, SPELL_TYPE.CURSED_OBJECT, /*Befoul, Imp Seed*/ };
            spells = new List<SPELL_TYPE>() { SPELL_TYPE.FORLORN_SPIRIT, SPELL_TYPE.BRIMSTONES, SPELL_TYPE.RAIN, SPELL_TYPE.BLIZZARD/*, SPELL_TYPE.SPAWN_HAUNTED_GROUNDS*/ };
            SetCanTriggerFlaw(false);
            SetCanRemoveTraits(false);
        }
    }
}