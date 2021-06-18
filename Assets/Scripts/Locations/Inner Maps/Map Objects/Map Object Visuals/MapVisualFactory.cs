using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MapVisualFactory {
    private static readonly string Tile_Object_Prefab_Name = "TileObjectGameObject";
    
    public GameObject CreateNewMeteorObject() {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("MeteorVisualObject", Vector3.zero, Quaternion.identity);
        return obj;
    }

    public GameObject CreateNewTileObjectMapVisual(TILE_OBJECT_TYPE objType) {
        GameObject obj;
        switch (objType) {
            case TILE_OBJECT_TYPE.TORNADO:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TornadoVisualObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.RAVENOUS_SPIRIT:
            case TILE_OBJECT_TYPE.FEEBLE_SPIRIT:
            case TILE_OBJECT_TYPE.FORLORN_SPIRIT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("SpiritGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.POISON_CLOUD:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("PoisonCloudMapObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.LOCUST_SWARM:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocustSwarmMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.TORCH:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TorchGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BED:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BedGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BALL_LIGHTNING:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BallLightningMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.CORN_CROP:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("CornCropMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.FROSTY_FOG:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("FrostyFogMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.VAPOR:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("VaporMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.FIRE_BALL:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("FireBallMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BERRY_SHRUB:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BerryShrubMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.MUSHROOM:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("MushroomMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.QUICKSAND:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("QuicksandMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BRAZIER:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BrazierGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.FIREPLACE:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("FireplaceGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BLOCK_WALL:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BlockWallGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.DOOR_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("DoorGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.EXCALIBUR:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("ExcaliburGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.CAMPFIRE:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("CampfireGameObject", Vector3.zero, Quaternion.identity);
                break; 
            case TILE_OBJECT_TYPE.TABLE:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TableGameObject", Vector3.zero, Quaternion.identity);
                break; 
            case TILE_OBJECT_TYPE.BED_CLINIC:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("ClinicBedGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.ROCK:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("RockGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.SMALL_TREE_OBJECT:
            case TILE_OBJECT_TYPE.BIG_TREE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TreeGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.ORE_VEIN:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("OreVeinGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.FISHING_SPOT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("FishingSpotGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("PortalGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.WATCHER_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BeholderGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.MANA_PIT_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("ManaPitGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BIOLAB_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BiolabGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.MARAUD_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("MaraudGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.SPIRE_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("SpireGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.IMP_HUT_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("ImpHutGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.DEFENSE_POINT_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("DefensePointGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.MEDDLER_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("MeddlerGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.CRYPT_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("CryptGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.DEMON_EYE:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("EyeWardGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.DEFILER_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("DefilerGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TortureChambersGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("KennelGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.HYPNO_HERB_CROP:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("HypnoHerbCropMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.ICEBERRY_CROP:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("IceberryCropMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.PINEAPPLE_CROP:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("PineappleCropMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.POTATO_CROP:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("PotatoCropMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BOAR_DEN:
            case TILE_OBJECT_TYPE.WOLF_DEN:
            case TILE_OBJECT_TYPE.BEAR_DEN:
            case TILE_OBJECT_TYPE.RABBIT_HOLE:
            case TILE_OBJECT_TYPE.MINK_HOLE:
            case TILE_OBJECT_TYPE.MOONCRAWLER_HOLE:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("AnimalBurrowGameObject", Vector3.zero, Quaternion.identity);
                break;
            default:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(Tile_Object_Prefab_Name, Vector3.zero, Quaternion.identity);
                break;
        }
        return obj;
    }

    public CharacterVisionTrigger CreateAndInittializeCharacterVisionTrigger(Character p_character) {
        GameObject collisionTriggerGO;
        switch (p_character.race) {
            case RACE.DRAGON:
                collisionTriggerGO = GameObject.Instantiate(InnerMapManager.Instance.dragonCollisionTriggerPrefab, p_character.marker.transform);
                break;
            default:
                collisionTriggerGO = GameObject.Instantiate(InnerMapManager.Instance.characterCollisionTriggerPrefab, p_character.marker.transform);
                break;
        }
        collisionTriggerGO.transform.localPosition = Vector3.zero;
        CharacterVisionTrigger characterVisionTrigger = collisionTriggerGO.GetComponent<CharacterVisionTrigger>();
        characterVisionTrigger.Initialize(p_character);
        return characterVisionTrigger;
    }
}
