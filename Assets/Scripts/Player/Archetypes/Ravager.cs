using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archetype {
    public class Ravager : PlayerArchetype {
        public Ravager() : base(PLAYER_ARCHETYPE.Ravager) {
            selectorDescription = "Master of monsters and calamities. Invading the world is easy for one that has access to such mayhem!";
            actions = new List<string>() { PlayerDB.Afflict_Action, PlayerDB.Seize_Object_Action, PlayerDB.Destroy_Action, PlayerDB.Ignite_Action, PlayerDB.Summon_Minion_Action
                    , PlayerDB.Corrupt_Action, PlayerDB.Stop_Action, PlayerDB.Return_To_Portal_Action
                    , PlayerDB.Harass_Action, PlayerDB.Raid_Action, PlayerDB.Invade_Action
                    , PlayerDB.End_Harass_Action, PlayerDB.End_Raid_Action, PlayerDB.End_Invade_Action
                    , PlayerDB.Combat_Mode_Action, PlayerDB.Build_Demonic_Structure_Action, PlayerDB.Breed_Monster_Action
                    , PlayerDB.Learn_Spell_Action, PlayerDB.Activate_Artifact_Action
            };
            monsters = new List<RaceClass> { new RaceClass(RACE.WOLF, "Ravager"), new RaceClass(RACE.GOLEM, "Golem") };
            demonicStructures = new List<LANDMARK_TYPE>() { LANDMARK_TYPE.THE_PIT, LANDMARK_TYPE.THE_KENNEL, LANDMARK_TYPE.DEMONIC_PRISON, LANDMARK_TYPE.THE_SPIRE, LANDMARK_TYPE.THE_CRYPT };
            minionClasses = new List<string>() { "Pride", "Envy", "Greed", "Wrath" };
            afflictions = new List<SPELL_TYPE>() { }; //No intial afflictions
            spells = new List<SPELL_TYPE>() { SPELL_TYPE.TORNADO, SPELL_TYPE.POISON_CLOUD, SPELL_TYPE.METEOR, SPELL_TYPE.LIGHTNING
                , SPELL_TYPE.FEEBLE_SPIRIT, SPELL_TYPE.LOCUST_SWARM, SPELL_TYPE.SPAWN_BOULDER/*, SPELL_TYPE.LANDMINE*/
                /*, SPELL_TYPE.ACID_RAIN, SPELL_TYPE.RAIN, SPELL_TYPE.HEAT_WAVE*/, SPELL_TYPE.EARTHQUAKE/*, SPELL_TYPE.SPAWN_MONSTER_LAIR*/ };
            SetCanTriggerFlaw(false);
            SetCanRemoveTraits(false);
        }
    }
}