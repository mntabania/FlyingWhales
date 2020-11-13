using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using Inner_Maps;
using Traits;
using UnityEngine;

public static class Extensions {

    #region Crimes
    public static bool IsLessThan(this CRIME_SEVERITY sub, CRIME_SEVERITY other) {
        return sub < other;
    }
    public static bool IsGreaterThanOrEqual(this CRIME_SEVERITY sub, CRIME_SEVERITY other) {
        return sub >= other;
    }
    public static bool IsReligiousCrime(this CRIME_TYPE crimeType) {
        switch (crimeType) {
            case CRIME_TYPE.Demon_Worship:
            case CRIME_TYPE.Nature_Worship:
            case CRIME_TYPE.Divine_Worship:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Structures
    /// <summary>
    /// Is this stucture contained within walls?
    /// </summary>
    /// <param name="sub"></param>
    /// <returns>True or false</returns>
    public static bool IsOpenSpace(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.WILDERNESS:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.POND:
            case STRUCTURE_TYPE.CITY_CENTER:
            case STRUCTURE_TYPE.THE_PORTAL:
            case STRUCTURE_TYPE.EYE:
            case STRUCTURE_TYPE.OCEAN:
            case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
                return true;
            default:
                return false;
        }
    }
    public static bool IsSettlementStructure(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.CITY_CENTER:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.PRISON:
            case STRUCTURE_TYPE.DWELLING:
            case STRUCTURE_TYPE.SMITHY:
            case STRUCTURE_TYPE.BARRACKS:
            case STRUCTURE_TYPE.APOTHECARY:
            case STRUCTURE_TYPE.GRANARY:
            case STRUCTURE_TYPE.MINER_CAMP:
            case STRUCTURE_TYPE.RAIDER_CAMP:
            case STRUCTURE_TYPE.ASSASSIN_GUILD:
            case STRUCTURE_TYPE.HUNTER_LODGE:
            case STRUCTURE_TYPE.MAGE_QUARTERS:
            case STRUCTURE_TYPE.FARM:
            case STRUCTURE_TYPE.LUMBERYARD:
            case STRUCTURE_TYPE.MINE_SHACK:
            case STRUCTURE_TYPE.TAVERN:
            case STRUCTURE_TYPE.CULT_TEMPLE:
                return true;
            default:
                return false;
        }
    }
    public static bool IsFacilityStructure(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.TAVERN:
            case STRUCTURE_TYPE.CITY_CENTER:
            case STRUCTURE_TYPE.WAREHOUSE:
            case STRUCTURE_TYPE.FARM:
            case STRUCTURE_TYPE.MINE_SHACK:
            case STRUCTURE_TYPE.LUMBERYARD:
            case STRUCTURE_TYPE.APOTHECARY:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.BARRACKS:
            case STRUCTURE_TYPE.MAGE_QUARTERS:
            case STRUCTURE_TYPE.CULT_TEMPLE:
                return true;
            default:
                return false;
        }
    }
    public static int StructurePriority(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.WILDERNESS:
            case STRUCTURE_TYPE.POND:
                return -1;
            case STRUCTURE_TYPE.DWELLING:
                return 0;
            case STRUCTURE_TYPE.CITY_CENTER:
                return 1;
            case STRUCTURE_TYPE.TAVERN:
                return 2;
            case STRUCTURE_TYPE.WAREHOUSE:
                return 3;
            case STRUCTURE_TYPE.PRISON:
                return 5;
            default:
                return 99;
        }
    }
    public static bool IsInterior(this STRUCTURE_TYPE structureType) {
        switch (structureType) {
            case STRUCTURE_TYPE.DWELLING:
            case STRUCTURE_TYPE.TAVERN:
            case STRUCTURE_TYPE.PRISON:
            case STRUCTURE_TYPE.SMITHY:
            case STRUCTURE_TYPE.GRANARY:
            case STRUCTURE_TYPE.BARRACKS:
            case STRUCTURE_TYPE.MINER_CAMP:
            case STRUCTURE_TYPE.WAREHOUSE:
            case STRUCTURE_TYPE.APOTHECARY:
            case STRUCTURE_TYPE.RAIDER_CAMP:
            case STRUCTURE_TYPE.HUNTER_LODGE:
            case STRUCTURE_TYPE.ASSASSIN_GUILD:
            case STRUCTURE_TYPE.DEMONIC_PRISON:
            case STRUCTURE_TYPE.TORTURE_CHAMBERS:
            case STRUCTURE_TYPE.MAGE_TOWER:
            case STRUCTURE_TYPE.ABANDONED_MINE:
            case STRUCTURE_TYPE.LUMBERYARD:
            case STRUCTURE_TYPE.MINE_SHACK:
            case STRUCTURE_TYPE.MAGE_QUARTERS:
            case STRUCTURE_TYPE.EYE:
            case STRUCTURE_TYPE.CRYPT:
            case STRUCTURE_TYPE.OSTRACIZER:
            case STRUCTURE_TYPE.MEDDLER:
            case STRUCTURE_TYPE.KENNEL:
            case STRUCTURE_TYPE.CAVE:
            case STRUCTURE_TYPE.DEFILER:
            case STRUCTURE_TYPE.RUINED_ZOO:
            case STRUCTURE_TYPE.BIOLAB:
                return true;
            default:
                return false;
        }
    }
    public static bool IsSpecialStructure(this STRUCTURE_TYPE structureType) {
        switch (structureType) {
            case STRUCTURE_TYPE.MAGE_TOWER:
            case STRUCTURE_TYPE.ABANDONED_MINE:
            case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
            case STRUCTURE_TYPE.ANCIENT_RUIN:
            case STRUCTURE_TYPE.MONSTER_LAIR:
            case STRUCTURE_TYPE.CAVE:
            case STRUCTURE_TYPE.TEMPLE:
            case STRUCTURE_TYPE.RUINED_ZOO:
                return true;
            default:
                return false;
        }
    }
    public static LANDMARK_TYPE GetLandmarkType(this STRUCTURE_TYPE structureType) {
        if (System.Enum.TryParse(structureType.ToString(), out LANDMARK_TYPE parsed)) {
            return parsed;
        } else {
            return LANDMARK_TYPE.HOUSES;
        }
    }
    #endregion

    #region Misc
    public static Cardinal_Direction OppositeDirection(this Cardinal_Direction dir) {
        switch (dir) {
            case Cardinal_Direction.North:
                return Cardinal_Direction.South;
            case Cardinal_Direction.South:
                return Cardinal_Direction.North;
            case Cardinal_Direction.East:
                return Cardinal_Direction.West;
            case Cardinal_Direction.West:
                return Cardinal_Direction.East;
        }
        throw new System.Exception($"No opposite direction for {dir}");
    }
    public static bool IsCardinalDirection(this GridNeighbourDirection dir) {
        switch (dir) {
            case GridNeighbourDirection.North:
            case GridNeighbourDirection.South:
            case GridNeighbourDirection.West:
            case GridNeighbourDirection.East:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// Is this action type considered to be a hostile action.
    /// </summary>
    /// <returns>True or false.</returns>
    public static bool IsHostileAction(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.ASSAULT:
            case INTERACTION_TYPE.STEAL:
            case INTERACTION_TYPE.RESTRAIN_CHARACTER:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// Will the given action type directly make it's actor enter combat state.
    /// </summary>
    /// <param name="type">The type of action.</param>
    /// <returns>True or false</returns>
    public static bool IsDirectCombatAction(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.ASSAULT:
                return true;
            default:
                return false;
        }
    }
    public static bool CanBeReplaced(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.SIT:
                return true;
            default:
                return false;
        }
    }
    public static bool WillAvoidCharactersWhileMoving(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.RITUAL_KILLING:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region State
    public static bool IsCombatState(this CHARACTER_STATE type) {
        switch (type) {
            case CHARACTER_STATE.BERSERKED:
            case CHARACTER_STATE.COMBAT:
            case CHARACTER_STATE.HUNT:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// This is used to determine what class should be created when saving a CharacterState. <see cref="SaveUtilities.CreateCharacterStateSaveDataInstance(CharacterState)"/>
    /// </summary>
    /// <param name="type">The type of state</param>
    /// <returns>If the state type has a unique save data class or not.</returns>
    public static bool HasUniqueSaveData(this CHARACTER_STATE type) {
        switch (type) {
            case CHARACTER_STATE.DOUSE_FIRE:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Furniture
    public static TILE_OBJECT_TYPE ConvertFurnitureToTileObject(this FURNITURE_TYPE type) {
        TILE_OBJECT_TYPE to;
        if (System.Enum.TryParse<TILE_OBJECT_TYPE>(type.ToString(), out to)) {
            return to;
        }
        return TILE_OBJECT_TYPE.NONE;
    }
    public static bool CanBeCraftedBy(this TILE_OBJECT_TYPE type, Character character) {
        if (type == TILE_OBJECT_TYPE.NONE) {
            return false;
        }
        TileObjectData data = TileObjectDB.GetTileObjectData(type);
        if (data.neededCharacterClass == null || data.neededCharacterClass.Length <= 0) {
            return true;
        }
        return data.neededCharacterClass.Contains(character.characterClass.className);
    }
    #endregion

    #region Tile Objects
    public static FURNITURE_TYPE ConvertTileObjectToFurniture(this TILE_OBJECT_TYPE type) {
        FURNITURE_TYPE to;
        if (System.Enum.TryParse<FURNITURE_TYPE>(type.ToString(), out to)) {
            return to;
        }
        return FURNITURE_TYPE.NONE;
    }
    public static bool IsPreBuilt(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.TABLE:
            case TILE_OBJECT_TYPE.BED:
            case TILE_OBJECT_TYPE.BED_CLINIC:
            case TILE_OBJECT_TYPE.DESK:
            case TILE_OBJECT_TYPE.GUITAR:
            case TILE_OBJECT_TYPE.TABLE_ARMOR:
            case TILE_OBJECT_TYPE.TABLE_ALCHEMY:
            case TILE_OBJECT_TYPE.TABLE_SCROLLS:
            case TILE_OBJECT_TYPE.TABLE_WEAPONS:
            case TILE_OBJECT_TYPE.TABLE_MEDICINE:
            case TILE_OBJECT_TYPE.TABLE_CONJURING:
            case TILE_OBJECT_TYPE.TABLE_HERBALISM:
            case TILE_OBJECT_TYPE.TABLE_METALWORKING_TOOLS:
            case TILE_OBJECT_TYPE.PLINTH_BOOK:
                return false;
            default:
                return true;
        }
    }
    public static bool IsTileObjectAnItem(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.HEALING_POTION:
            case TILE_OBJECT_TYPE.TOOL:
            case TILE_OBJECT_TYPE.ARTIFACT:
            case TILE_OBJECT_TYPE.CULTIST_KIT:
            case TILE_OBJECT_TYPE.ANTIDOTE:
            case TILE_OBJECT_TYPE.WATER_FLASK:
            case TILE_OBJECT_TYPE.EMBER:
            case TILE_OBJECT_TYPE.HERB_PLANT:
            case TILE_OBJECT_TYPE.POISON_FLASK:
            case TILE_OBJECT_TYPE.POISON_CRYSTAL:
            case TILE_OBJECT_TYPE.FIRE_CRYSTAL:
            case TILE_OBJECT_TYPE.WATER_CRYSTAL:
            case TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL:
            case TILE_OBJECT_TYPE.ICE_CRYSTAL:
            case TILE_OBJECT_TYPE.ICE:
            case TILE_OBJECT_TYPE.EXCALIBUR:
            case TILE_OBJECT_TYPE.WEREWOLF_PELT:
            case TILE_OBJECT_TYPE.PHYLACTERY:
                return true;
            default:
                return false;
        }
    }
    public static bool IsTileObjectVisibleByDefault(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.TOMBSTONE:
            case TILE_OBJECT_TYPE.TREASURE_CHEST:
            case TILE_OBJECT_TYPE.EXCALIBUR:
            case TILE_OBJECT_TYPE.HEIRLOOM:
            case TILE_OBJECT_TYPE.GUITAR:
            case TILE_OBJECT_TYPE.FISH_PILE:
            case TILE_OBJECT_TYPE.METAL_PILE:
            case TILE_OBJECT_TYPE.STONE_PILE:
            case TILE_OBJECT_TYPE.WOOD_PILE:
            case TILE_OBJECT_TYPE.TABLE:
                return true;
            default:
                return tileObjectType.IsTileObjectAnItem();
        }
    }
    public static bool CanBeRepaired(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.BED:
            case TILE_OBJECT_TYPE.TABLE:
            case TILE_OBJECT_TYPE.DESK:
            case TILE_OBJECT_TYPE.GUITAR:
            case TILE_OBJECT_TYPE.TORCH:
            case TILE_OBJECT_TYPE.WATER_WELL:
                return true;
            default:
                return false;
        }
    }
    public static bool IsTileObjectImportant(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.BED:
            case TILE_OBJECT_TYPE.TABLE:
            case TILE_OBJECT_TYPE.DESK:
            case TILE_OBJECT_TYPE.GUITAR:
            case TILE_OBJECT_TYPE.WATER_WELL:
            case TILE_OBJECT_TYPE.TREASURE_CHEST:
            case TILE_OBJECT_TYPE.HEALING_POTION:
            case TILE_OBJECT_TYPE.TOOL:
            case TILE_OBJECT_TYPE.ARTIFACT:
            case TILE_OBJECT_TYPE.CULTIST_KIT:
            case TILE_OBJECT_TYPE.ANTIDOTE:
            case TILE_OBJECT_TYPE.WATER_FLASK:
            case TILE_OBJECT_TYPE.EMBER:
            case TILE_OBJECT_TYPE.POISON_FLASK:
            case TILE_OBJECT_TYPE.ICE:
            case TILE_OBJECT_TYPE.EXCALIBUR:
            case TILE_OBJECT_TYPE.MAGIC_CIRCLE:
            case TILE_OBJECT_TYPE.BLOCK_WALL:
            case TILE_OBJECT_TYPE.DESERT_ROSE:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Jobs
    public static bool IsNeedsTypeJob(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.ENERGY_RECOVERY_URGENT:
            case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
            case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
            case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
            case JOB_TYPE.HAPPINESS_RECOVERY:
            case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
                return true;
            default:
                return false;
        }
    }
    public static bool IsFullnessRecoveryTypeJob(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
            case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
            case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
                return true;
            default:
                return false;
        }
    }
    public static bool IsTirednessRecoveryTypeJob(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.ENERGY_RECOVERY_URGENT:
            case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
                return true;
            default:
                return false;
        }
    }
    public static bool IsHappinessRecoveryTypeJob(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.HAPPINESS_RECOVERY:
                return true;
            default:
                return false;
        }
    }
    public static int GetJobTypePriority(this JOB_TYPE jobType) {
        int priority = 0;
        switch (jobType) {
            case JOB_TYPE.DIG_THROUGH:
                priority = 1300;
                break;
            case JOB_TYPE.FLEE_TO_HOME:
                priority = 1200;
                break;
            case JOB_TYPE.NEUTRALIZE_DANGER:
                priority = 1100;
                break;
            case JOB_TYPE.COMBAT:
                priority = 1090;
                break;
            case JOB_TYPE.FLEE_CRIME:
            case JOB_TYPE.BERSERK_ATTACK:
            case JOB_TYPE.BERSERK_STROLL:
                priority = 1089;
                break;
            case JOB_TYPE.NO_PATH_IDLE:
                priority = 1088;
                break;
            case JOB_TYPE.BUILD_CAMP:
            case JOB_TYPE.LYCAN_HUNT_PREY:
                priority = 1087;
                break;
            case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
                priority = 1086;
                break;
            //case JOB_TYPE.FLEE_CRIME:
            //case JOB_TYPE.BERSERK_ATTACK:
            case JOB_TYPE.DESTROY:
                //case JOB_TYPE.BERSERK_STROLL:
                ////case JOB_TYPE.GO_TO:
                priority = 1085;
                break;
            case JOB_TYPE.REPORT_CORRUPTED_STRUCTURE:
            case JOB_TYPE.COUNTERATTACK:
                priority = 1080;
                break;
            case JOB_TYPE.TRIGGER_FLAW:
            case JOB_TYPE.FIND_NEW_VILLAGE:
            case JOB_TYPE.STEAL_CORPSE:
            case JOB_TYPE.SUMMON_BONE_GOLEM:
                priority = 1050;
                break;
            case JOB_TYPE.RETURN_HOME_URGENT:
                priority = 1055;
                break;
            case JOB_TYPE.HIDE_AT_HOME:
            case JOB_TYPE.SEEK_SHELTER:
                priority = 1040;
                break;
            case JOB_TYPE.APPREHEND:
                priority = 1030;
                break;
            case JOB_TYPE.SCREAM:
                priority = 1020;
                break;
            case JOB_TYPE.RELEASE_CHARACTER:
                priority = 1015;
                break;
            case JOB_TYPE.BURY_SERIAL_KILLER_VICTIM:
                priority = 1010;
                break;
            case JOB_TYPE.OFFER_BLOOD:
                priority = 1009;
                break;
            case JOB_TYPE.REMOVE_STATUS:
            case JOB_TYPE.CURE_MAGICAL_AFFLICTION:
                priority = 1008;
                break;
            case JOB_TYPE.RECOVER_HP:
                priority = 1005;
                break;
            case JOB_TYPE.FEED:
            case JOB_TYPE.RITUAL_KILLING:
                priority = 1003;
                break;
            case JOB_TYPE.ENERGY_RECOVERY_URGENT:
            case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
            case JOB_TYPE.HUNT_PREY:
                priority = 1000;
                break;
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.BRAWL:
            case JOB_TYPE.ABSORB_LIFE:
            case JOB_TYPE.ABSORB_POWER:
            case JOB_TYPE.SPAWN_SKELETON:
            case JOB_TYPE.RAISE_CORPSE:
            case JOB_TYPE.HOARD:
                priority = 970;
                break;
            case JOB_TYPE.DOUSE_FIRE:
            case JOB_TYPE.SUICIDE_FOLLOW:
                priority = 950;
                break;
            case JOB_TYPE.DEMON_KILL:
                priority = 930;
                break;
            case JOB_TYPE.MOVE_CHARACTER:
            case JOB_TYPE.CAPTURE_CHARACTER:
                priority = 926;
                break;
            case JOB_TYPE.GO_TO:
                priority = 925;
                break;
            case JOB_TYPE.ZOMBIE_STROLL:
                priority = 915;
                break;
            //case JOB_TYPE.RECOVER_HP:
            //    priority = 920;
            //    break;
            case JOB_TYPE.UNDERMINE:
            case JOB_TYPE.POISON_FOOD:
            case JOB_TYPE.PLACE_TRAP:
            case JOB_TYPE.OPEN_CHEST:
            case JOB_TYPE.ROAM_AROUND_STRUCTURE:
                priority = 910;
                break;
            //case JOB_TYPE.FEED:
            case JOB_TYPE.MONSTER_INVADE:
                priority = 900;
                break;
            case JOB_TYPE.RESTRAIN:
                priority = 970;
                break;
            case JOB_TYPE.REPORT_CRIME:
                //case JOB_TYPE.BURY:
                priority = 870;
                break;
            case JOB_TYPE.BUILD_BLUEPRINT:
            case JOB_TYPE.PLACE_BLUEPRINT:
            case JOB_TYPE.SPAWN_LAIR:
            case JOB_TYPE.BUILD_VAMPIRE_CASTLE:
                priority = 850;
                break;
            case JOB_TYPE.SABOTAGE_NEIGHBOUR:
            case JOB_TYPE.DECREASE_MOOD:
            case JOB_TYPE.DISABLE:
            case JOB_TYPE.ARSON:
            case JOB_TYPE.DARK_RITUAL:
            case JOB_TYPE.CULTIST_TRANSFORM:
            case JOB_TYPE.CULTIST_POISON:
            case JOB_TYPE.CULTIST_BOOBY_TRAP:
            case JOB_TYPE.EVANGELIZE:
            case JOB_TYPE.SNATCH:
            case JOB_TYPE.VAMPIRIC_EMBRACE:
            case JOB_TYPE.IMPRISON_BLOOD_SOURCE:
            case JOB_TYPE.SPREAD_RUMOR:
                priority = 830;
                break;
            case JOB_TYPE.BURY:
                priority = 820;
                break;
            case JOB_TYPE.PRODUCE_FOOD:
            case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
            case JOB_TYPE.PRODUCE_METAL:
            case JOB_TYPE.PRODUCE_STONE:
            case JOB_TYPE.PRODUCE_WOOD:
            case JOB_TYPE.MONSTER_BUTCHER:
                priority = 800;
                break;
            case JOB_TYPE.CRAFT_OBJECT:
                priority = 750;
                break;
            case JOB_TYPE.HAUL:
                priority = 700;
                break;
            case JOB_TYPE.REPAIR:
                priority = 650;
                break;
            case JOB_TYPE.CLEANSE_TILES:
                priority = 630;
                break;
            case JOB_TYPE.CLEANSE_CORRUPTION:
            case JOB_TYPE.RECRUIT:
                priority = 600;
                break;
            case JOB_TYPE.JUDGE_PRISONER:
                priority = 570;
                break;
            //case JOB_TYPE.APPREHEND:
            //    priority = 550;
            //    break;
            case JOB_TYPE.KIDNAP:
            case JOB_TYPE.KIDNAP_RAID:
            case JOB_TYPE.STEAL_RAID:
                priority = 530;
                break;
            //case JOB_TYPE.MOVE_CHARACTER:
            //    priority = 520;
            //    break;
            case JOB_TYPE.TAKE_ITEM:
            case JOB_TYPE.INSPECT:
                priority = 510;
                break;
            case JOB_TYPE.CONFIRM_RUMOR:
            case JOB_TYPE.SHARE_NEGATIVE_INFO:
                priority = 505;
                break;
            case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
            case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
            case JOB_TYPE.HAPPINESS_RECOVERY:
                priority = 500;
                break;
            case JOB_TYPE.DROP_ITEM_PARTY:
                priority = 495;
                break;
            case JOB_TYPE.PARTY_GO_TO:
            case JOB_TYPE.GO_TO_WAITING:
            case JOB_TYPE.PARTYING:
                priority = 490;
                break;
            case JOB_TYPE.PATROL:
            case JOB_TYPE.EXPLORE:
            case JOB_TYPE.EXTERMINATE:
            case JOB_TYPE.COUNTERATTACK_PARTY:
            case JOB_TYPE.RESCUE:
            case JOB_TYPE.JOIN_GATHERING:
            case JOB_TYPE.RAID:
            case JOB_TYPE.HOST_SOCIAL_PARTY:
            case JOB_TYPE.HUNT_HEIRLOOM:
                priority = 450;
                break;
            case JOB_TYPE.MINE:
            case JOB_TYPE.TEND_FARM:
                priority = 440;
                break;
            case JOB_TYPE.DRY_TILES:
                priority = 430;
                break;
            case JOB_TYPE.CHECK_PARALYZED_FRIEND:
                priority = 400;
                break;
            case JOB_TYPE.OBTAIN_PERSONAL_FOOD:
                priority = 300;
                break;
            case JOB_TYPE.VISIT_FRIEND:
            case JOB_TYPE.VISIT_DIFFERENT_REGION:
                priority = 280;
                break;
            case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
            case JOB_TYPE.ABDUCT:
            case JOB_TYPE.LEARN_MONSTER:
            case JOB_TYPE.TAKE_ARTIFACT:
                priority = 260;
                break;
            case JOB_TYPE.IDLE_RETURN_HOME:
            case JOB_TYPE.IDLE_NAP:
            case JOB_TYPE.IDLE_SIT:
            case JOB_TYPE.IDLE_STAND:
            case JOB_TYPE.IDLE_GO_TO_INN:
            case JOB_TYPE.IDLE:
            case JOB_TYPE.ROAM_AROUND_TERRITORY:
            case JOB_TYPE.ROAM_AROUND_CORRUPTION:
            case JOB_TYPE.ROAM_AROUND_PORTAL:
            case JOB_TYPE.ROAM_AROUND_TILE:
            case JOB_TYPE.RETURN_TERRITORY:
            case JOB_TYPE.RETURN_PORTAL:
            case JOB_TYPE.STAND:
            case JOB_TYPE.STAND_STILL:
            case JOB_TYPE.DROP_ITEM:
            case JOB_TYPE.CRAFT_MISSING_FURNITURE:
            case JOB_TYPE.WARM_UP:
                priority = 250;
                break;
            case JOB_TYPE.COMBINE_STOCKPILE:
                priority = 200;
                break;
            case JOB_TYPE.COMMIT_SUICIDE:
            case JOB_TYPE.BURY_IN_ACTIVE_PARTY:
                priority = 150;
                break;
            case JOB_TYPE.STROLL:
                priority = 100;
                break;
            case JOB_TYPE.MONSTER_ABDUCT:
            case JOB_TYPE.MONSTER_EAT:
                priority = 90;
                break;
                // case JOB_TYPE.SNUFF_TORNADO:
                // case JOB_TYPE.INTERRUPTION:
                //     priority = 2;
                //     break;
                // case JOB_TYPE.COMBAT:
                //     priority = 3;
                //     break;
                // case JOB_TYPE.TRIGGER_FLAW:
                //     priority = 4;
                //     break;
                // case JOB_TYPE.MISC:
                // case JOB_TYPE.CORRUPT_CULTIST:
                // case JOB_TYPE.DESTROY_FOOD:
                // case JOB_TYPE.DESTROY_SUPPLY:
                // case JOB_TYPE.SABOTAGE_FACTION:
                // case JOB_TYPE.SCREAM:
                // case JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM:
                //     //case JOB_TYPE.INTERRUPTION:
                //     priority = 5;
                //     break;
                // case JOB_TYPE.TANTRUM:
                // case JOB_TYPE.CLAIM_REGION:
                // case JOB_TYPE.CLEANSE_REGION:
                // case JOB_TYPE.ATTACK_DEMONIC_REGION:
                // case JOB_TYPE.ATTACK_NON_DEMONIC_REGION:
                // case JOB_TYPE.INVADE_REGION:
                //     priority = 6;
                //     break;
                // // case JOB_TYPE.IDLE:
                // //     priority = 7;
                // //     break;
                // case JOB_TYPE.DEATH:
                // case JOB_TYPE.BERSERK:
                // case JOB_TYPE.STEAL:
                // case JOB_TYPE.RESOLVE_CONFLICT:
                // case JOB_TYPE.DESTROY:
                //     priority = 10;
                //     break;
                // case JOB_TYPE.KNOCKOUT:
                // case JOB_TYPE.SEDUCE:
                // case JOB_TYPE.UNDERMINE_ENEMY:
                //     priority = 20;
                //     break;
                // case JOB_TYPE.HUNGER_RECOVERY_STARVING:
                // case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
                //     priority = 30;
                //     break;
                // case JOB_TYPE.APPREHEND:
                // case JOB_TYPE.DOUSE_FIRE:
                //     priority = 40;
                //     break;
                // case JOB_TYPE.REMOVE_TRAIT:
                //     priority = 50;
                //     break;
                // case JOB_TYPE.RESTRAIN:
                //     priority = 60;
                //     break;
                // case JOB_TYPE.HAPPINESS_RECOVERY_FORLORN:
                //     priority = 100;
                //     break;
                // case JOB_TYPE.FEED:
                //     priority = 110;
                //     break;
                // case JOB_TYPE.BURY:
                // case JOB_TYPE.REPAIR:
                // case JOB_TYPE.WATCH:
                // case JOB_TYPE.DESTROY_PROFANE_LANDMARK:
                // case JOB_TYPE.PERFORM_HOLY_INCANTATION:
                // case JOB_TYPE.PRAY_GODDESS_STATUE:
                // case JOB_TYPE.REACT_TO_SCREAM:
                //     priority = 120;
                //     break;
                // case JOB_TYPE.BREAK_UP:
                //     priority = 130;
                //     break;
                // case JOB_TYPE.PATROL:
                //     priority = 170;
                //     break;
                // case JOB_TYPE.JUDGEMENT:
                //     priority = 220;
                //     break;
                // case JOB_TYPE.SUICIDE:
                // case JOB_TYPE.HAUL:
                //     priority = 230;
                //     break;
                // case JOB_TYPE.CRAFT_OBJECT:
                // case JOB_TYPE.PRODUCE_FOOD:
                // case JOB_TYPE.PRODUCE_WOOD:
                // case JOB_TYPE.PRODUCE_STONE:
                // case JOB_TYPE.PRODUCE_METAL:
                // case JOB_TYPE.TAKE_PERSONAL_FOOD:
                // case JOB_TYPE.DROP:
                // case JOB_TYPE.INSPECT:
                // case JOB_TYPE.PLACE_BLUEPRINT:
                // case JOB_TYPE.BUILD_BLUEPRINT:
                // case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
                //     priority = 240;
                //     break;
                // case JOB_TYPE.HUNGER_RECOVERY:
                // case JOB_TYPE.TIREDNESS_RECOVERY:
                // case JOB_TYPE.HAPPINESS_RECOVERY:
                //     priority = 270;
                //     break;
                // case JOB_TYPE.STROLL:
                // case JOB_TYPE.IDLE:
                //     priority = 290;
                //     break;
                // case JOB_TYPE.IMPROVE:
                // case JOB_TYPE.EXPLORE:
                //     priority = 300;
                //     break;
        }
        return priority;
    }
    public static bool IsJobLethal(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.APPREHEND:
            case JOB_TYPE.RITUAL_KILLING:
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.ABDUCT:
            case JOB_TYPE.LEARN_MONSTER:
            case JOB_TYPE.BRAWL:
            case JOB_TYPE.KIDNAP:
            case JOB_TYPE.MOVE_CHARACTER:
            case JOB_TYPE.RESTRAIN:
            case JOB_TYPE.KIDNAP_RAID:
            case JOB_TYPE.CAPTURE_CHARACTER:
            case JOB_TYPE.BERSERK_ATTACK:
            case JOB_TYPE.SNATCH:
                return false;
            default:
                return true;
        }
    }
    #endregion

    #region Summons
    public static string SummonName(this SUMMON_TYPE type) {
        switch (type) {
            default:
                return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
        }
    }
    #endregion

    #region Artifacts
    public static bool CanBeSummoned(this ARTIFACT_TYPE type) {
        switch (type) {
            case ARTIFACT_TYPE.None:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Intervention Abilities
    public static List<ABILITY_TAG> GetAbilityTags(this SPELL_TYPE type) {
        List<ABILITY_TAG> tags = new List<ABILITY_TAG>();
        switch (type) {
            case SPELL_TYPE.LYCANTHROPY:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case SPELL_TYPE.KLEPTOMANIA:
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case SPELL_TYPE.VAMPIRISM:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case SPELL_TYPE.UNFAITHFULNESS:
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case SPELL_TYPE.CANNIBALISM:
                tags.Add(ABILITY_TAG.MAGIC);
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case SPELL_TYPE.ZAP:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            //case SPELL_TYPE.JOLT:
            //    tags.Add(ABILITY_TAG.MAGIC);
            //    break;
            //case SPELL_TYPE.ENRAGE:
            //    tags.Add(ABILITY_TAG.MAGIC);
            //    break;
            //case SPELL_TYPE.PROVOKE:
            //    tags.Add(ABILITY_TAG.MAGIC);
            //    break;
            case SPELL_TYPE.RAISE_DEAD:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
                //case SPELL_TYPE.CLOAK_OF_INVISIBILITY:
                //    tags.Add(ABILITY_TAG.MAGIC);
                //    break;
        }
        return tags;
    }
    #endregion

    #region Landmarks
    public static bool IsPlayerLandmark(this LANDMARK_TYPE type) {
        switch (type) {
            case LANDMARK_TYPE.THE_PORTAL:
            case LANDMARK_TYPE.OSTRACIZER:
            case LANDMARK_TYPE.CRYPT:
            case LANDMARK_TYPE.KENNEL:
            case LANDMARK_TYPE.THE_ANVIL:
            case LANDMARK_TYPE.MEDDLER:
            case LANDMARK_TYPE.EYE:
            case LANDMARK_TYPE.DEFILER:
            case LANDMARK_TYPE.THE_NEEDLES:
            case LANDMARK_TYPE.TORTURE_CHAMBERS:
                return true;
            default:
                return false;
        }
    }
    public static STRUCTURE_TYPE GetStructureType(this LANDMARK_TYPE landmarkType) {
        switch (landmarkType) {
            case LANDMARK_TYPE.HOUSES:
                return STRUCTURE_TYPE.DWELLING;
            case LANDMARK_TYPE.VILLAGE:
                return STRUCTURE_TYPE.CITY_CENTER;
            default:
                STRUCTURE_TYPE parsed;
                if (System.Enum.TryParse(landmarkType.ToString(), out parsed)) {
                    return parsed;
                } else {
                    throw new System.Exception($"There is no corresponding structure type for {landmarkType.ToString()}");
                }
        }

    }
    #endregion

    #region Deadly Sins
    public static string Description(this DEADLY_SIN_ACTION sin) {
        switch (sin) {
            case DEADLY_SIN_ACTION.SPELL_SOURCE:
                return "Knows three Spells that can be extracted by the Ruinarch";
            case DEADLY_SIN_ACTION.INSTIGATOR:
                return "Can be assigned to spawn Chaos Events in The Fingers";
            case DEADLY_SIN_ACTION.BUILDER:
                return "Can construct demonic structures";
            case DEADLY_SIN_ACTION.SABOTEUR:
                return "Can interfere in Events spawned by non-combatant characters";
            case DEADLY_SIN_ACTION.INVADER:
                return "Can invade adjacent regions";
            case DEADLY_SIN_ACTION.FIGHTER:
                return "Can interfere in Events spawned by combat-ready characters";
            case DEADLY_SIN_ACTION.RESEARCHER:
                return "Can be assigned to research upgrades in The Anvil";
            default:
                return string.Empty;
        }
    }
    #endregion

    #region Combat Abilities
    public static string Description(this COMBAT_ABILITY ability) {
        switch (ability) {
            case COMBAT_ABILITY.SINGLE_HEAL:
                return "Heals a friendly unit by a percentage of its max HP.";
            case COMBAT_ABILITY.FLAMESTRIKE:
                return "Deal AOE damage in the surrounding npcSettlement.";
            case COMBAT_ABILITY.FEAR_SPELL:
                return "Makes a character fear any other character.";
            case COMBAT_ABILITY.SACRIFICE:
                return "Sacrifice a friendly unit to deal AOE damage in the surrounding npcSettlement.";
            case COMBAT_ABILITY.TAUNT:
                return "Taunts enemies into attacking this character.";
            default:
                return string.Empty;
        }

    }
    #endregion

    #region Races
    public static bool UsesGenderNeutralMarkerAssets(this RACE race) {
        switch (race) {
            // case RACE.HUMANS:
            // case RACE.ELVES:
            case RACE.LESSER_DEMON:
                return false;
            default:
                return true;
        }
    }
    public static bool UsesGenderNeutralPortrait(this RACE race) {
        switch (race) {
            case RACE.HUMANS:
            case RACE.ELVES:
            case RACE.LESSER_DEMON:
                return false;
            default:
                return true;
        }
    }
    public static bool IsSapient(this RACE race) {
        switch (race) {
            case RACE.HUMANS:
            case RACE.ELVES:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Faction
    public static string FactionRelationshipColor(this FACTION_RELATIONSHIP_STATUS relationshipStatus) {
        switch (relationshipStatus) {
            case FACTION_RELATIONSHIP_STATUS.Friendly:
                return "#19ff00";
            case FACTION_RELATIONSHIP_STATUS.Hostile:
                return "#ff0000";
            default:
                return "#F8E1A9";
        }
    }
    #endregion

    #region Tiles
    public static bool IsStructureType(this LocationGridTile.Ground_Type groundType) {
        switch (groundType) {
            case LocationGridTile.Ground_Type.Cobble:
            case LocationGridTile.Ground_Type.Wood:
            case LocationGridTile.Ground_Type.Structure_Stone:
            case LocationGridTile.Ground_Type.Ruined_Stone:
            case LocationGridTile.Ground_Type.Demon_Stone:
            case LocationGridTile.Ground_Type.Flesh:
            case LocationGridTile.Ground_Type.Cave:
            case LocationGridTile.Ground_Type.Corrupted:
            case LocationGridTile.Ground_Type.Bone:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Strings
    public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase) {
        return text.IndexOf(value, stringComparison) >= 0;
    }
    #endregion

    #region RectTransform
    public static bool RectOverlaps(this RectTransform rectTrans1, RectTransform rectTrans2) {
        //Vector3 localPos1 = new Vector3(rectTrans1.rect.x, rectTrans1.rect.y, 0f);
        //Vector3 localPos2 = new Vector3(rectTrans2.rect.x, rectTrans2.rect.y, 0f);

        //Vector3 worldPos1 = rectTrans1.TransformVector(localPos1);
        //Vector3 worldPos2 = rectTrans2.TransformVector(localPos2);

        //Rect rect1 = new Rect(worldPos1.x, worldPos1.y, rectTrans1.rect.width, rectTrans1.rect.height);
        //Rect rect2 = new Rect(worldPos2.x, worldPos2.y, rectTrans2.rect.width, rectTrans2.rect.height);

        //Rect rect1 = new Rect(rectTrans1.rect.width + rectTrans1.position.x, rectTrans1.rect.height - rectTrans1.position.y, rectTrans1.rect.width, rectTrans1.rect.height);
        //Rect rect2 = new Rect(rectTrans2.rect.width + rectTrans2.position.x, rectTrans2.rect.height - rectTrans2.position.y, rectTrans2.rect.width, rectTrans2.rect.height);

        //Vector2 size1 = rectTrans1.TransformVector(rectTrans1.sizeDelta);
        //Vector2 size2 = rectTrans2.TransformVector(rectTrans2.sizeDelta);

        //Rect rect1 = new Rect(rectTrans1.position.x - size1.x, rectTrans1.position.y - size1.y, size1.x, size1.y);
        //Rect rect2 = new Rect(rectTrans2.position.x - size2.x, rectTrans2.position.y - size2.y, size2.x, size2.y);

        //Rect rect1 = new Rect(rectTrans1.position.x, rectTrans1.position.y, rectTrans1.rect.width, rectTrans1.rect.height);
        //Rect rect2 = new Rect(rectTrans2.position.x, rectTrans2.position.y, rectTrans2.rect.width, rectTrans2.rect.height);

        //if (rect1.x + rect1.width * 0.5f < rect2.x - rect2.width * 0.5f) {
        //    return false;
        //}
        //if (rect2.x + rect2.width * 0.5f < rect1.x - rect1.width * 0.5f) {
        //    return false;
        //}
        //if (rect1.y + rect1.height * 0.5f < rect2.y - rect2.height * 0.5f) {
        //    return false;
        //}
        //if (rect2.y + rect2.height * 0.5f < rect1.y - rect1.height * 0.5f) {
        //    return false;
        //}
        //return true;

        //return rect1.Contains(rect2);
        //return rect1.Overlaps(rect2);
        return rectTrans1.WorldRect().Overlaps(rectTrans2.WorldRect());
    }
    public static Rect WorldRect(this RectTransform rectTransform) {
        Rect r = rectTransform.rect;
        //r.position = rectTransform.TransformPoint(rectTransform.localPosition);
        r.center = rectTransform.TransformPoint(r.center);
        r.size = rectTransform.TransformVector(r.size);
        return r;
    }
    #endregion
}