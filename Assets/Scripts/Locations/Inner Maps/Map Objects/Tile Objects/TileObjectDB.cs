using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;

public static class TileObjectDB {

    public static TileObjectData Default = new TileObjectData() {
        maxHP = 200,
        neededCharacterClass = new string[] { "Craftsman" },
        occupiedSize = new Point(1, 1),
    };

    //tile objects
    public static Dictionary<TILE_OBJECT_TYPE, TileObjectData> tileObjectData = new Dictionary<TILE_OBJECT_TYPE, TileObjectData>() {
        { TILE_OBJECT_TYPE.WOOD_PILE, new TileObjectData() {
            maxHP = 600,
            neededCharacterClass =  new string[] { "Craftsman" },
        } },
        { TILE_OBJECT_TYPE.ANIMAL_MEAT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.RAT_MEAT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.ELF_MEAT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.HUMAN_MEAT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.VEGETABLES, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.FISH_PILE, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.STONE_PILE, new TileObjectData() {
            maxHP = 600,
        } },
        { TILE_OBJECT_TYPE.METAL_PILE, new TileObjectData() {
            maxHP = 600,
        } },
        { TILE_OBJECT_TYPE.WURM_HOLE, new TileObjectData() {
            maxHP = 1,
        } },
        { TILE_OBJECT_TYPE.BED_CLINIC, new TileObjectData() {
            maxHP = 400,
        } },
        { TILE_OBJECT_TYPE.BED, new TileObjectData() {
            maxHP = 400,
            neededCharacterClass =  new string[] { "Craftsman" },
            repairCost = 5,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)    
                ),
            }
        } },
        { TILE_OBJECT_TYPE.DESK, new TileObjectData() {
            maxHP = 250,
            neededCharacterClass =  new string[] { "Craftsman" },
            repairCost = 5,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)    
                ),
            }
        } },
        { TILE_OBJECT_TYPE.GUITAR, new TileObjectData() {
            maxHP = 120,
            neededCharacterClass =  new string[] { "Craftsman" },
            repairCost = 5,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)    
                ),
            }
        } },
        { TILE_OBJECT_TYPE.TABLE, new TileObjectData() {
            maxHP = 250,
            neededCharacterClass =  new string[] { "Craftsman" },
            repairCost = 5,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)    
                ),
            }
        } },
        { TILE_OBJECT_TYPE.TREE_OBJECT, new TileObjectData() {
            maxHP = 600,
            neededCharacterClass =  new string[] { "Craftsman" },
        } },
        { TILE_OBJECT_TYPE.BIG_TREE_OBJECT, new TileObjectData() {
            maxHP = 1200,
            neededCharacterClass =  new string[] { "Craftsman" },
            occupiedSize =  new Point(2, 2),
        } },
        { TILE_OBJECT_TYPE.HEALING_POTION, new TileObjectData() {
            maxHP = 150,
            neededCharacterClass = null,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    //new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WATER_FLASK, 1),
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.HERB_PLANT, 1)
                ),
            }
        } },
        { TILE_OBJECT_TYPE.POISON_FLASK, new TileObjectData() {
            maxHP = 150,
            neededCharacterClass = null,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WATER_FLASK, 1)
                    //new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.HERB_PLANT, 1)
                ),
            }
        } },
         { TILE_OBJECT_TYPE.ANTIDOTE, new TileObjectData() {
            maxHP = 150,
            neededCharacterClass = null,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.POISON_FLASK, 1)
                ),
            }
        } },
        { TILE_OBJECT_TYPE.LOCUST_SWARM, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.POISON_CLOUD, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.TORNADO, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.BALL_LIGHTNING, new TileObjectData() {
            maxHP = 500,
        } },
        { TILE_OBJECT_TYPE.FIRE_BALL, new TileObjectData() {
            maxHP = 500,
        } },
        { TILE_OBJECT_TYPE.FROSTY_FOG, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.VAPOR, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT, new TileObjectData() {
            maxHP = 5000,
            occupiedSize = new Point(4, 3)
        } },
        { TILE_OBJECT_TYPE.FIRE_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.WATER_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.POISON_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.ICE_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.BLOCK_WALL, new TileObjectData() {
            maxHP = 500,
        } },
        { TILE_OBJECT_TYPE.ROCK, new TileObjectData() {
            maxHP = 200,
        } },
        { TILE_OBJECT_TYPE.MAGIC_CIRCLE, new TileObjectData() {
            maxHP = 750,
        } },
        { TILE_OBJECT_TYPE.ORE, new TileObjectData() {
            maxHP = 350,
        } },
        { TILE_OBJECT_TYPE.BERRY_SHRUB, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.WATER_WELL, new TileObjectData() {
            maxHP = 1000,
            neededCharacterClass = new string[] { "Craftsman" },
            repairCost = 5,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 30)
                ),
            }
        } },
        { TILE_OBJECT_TYPE.TOMBSTONE, new TileObjectData() {
            maxHP = 400,
        } },
        { TILE_OBJECT_TYPE.MUSHROOM, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.CORN_CROP, new TileObjectData() {
            maxHP = 1000,
        } },
        { TILE_OBJECT_TYPE.SMITHING_FORGE, new TileObjectData() {
            maxHP = 350,
        } },
        { TILE_OBJECT_TYPE.TABLE_HERBALISM, new TileObjectData() {
            maxHP = 250,
        } },
        { TILE_OBJECT_TYPE.TABLE_ALCHEMY, new TileObjectData() {
            maxHP = 250,
        } },
        { TILE_OBJECT_TYPE.CAULDRON, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.HERB_PLANT, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT, new TileObjectData() {
            maxHP = 500,
        } },
        { TILE_OBJECT_TYPE.TOOL, new TileObjectData() {
            maxHP = 150,
        } },
        { TILE_OBJECT_TYPE.EMBER, new TileObjectData() {
            maxHP = 150,
        } },
        { TILE_OBJECT_TYPE.WATER_FLASK, new TileObjectData() {
            maxHP = 150,
        } },
        { TILE_OBJECT_TYPE.WINTER_ROSE, new TileObjectData() {
            maxHP = 700,
        } },
        { TILE_OBJECT_TYPE.DESERT_ROSE, new TileObjectData() {
            maxHP = 700,
        } },
        { TILE_OBJECT_TYPE.RACK_STAVES, new TileObjectData() {
            maxHP = 250,
        } },
        { TILE_OBJECT_TYPE.QUICKSAND, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.SNOW_MOUND, new TileObjectData() {
            maxHP = 200,
        } },
        { TILE_OBJECT_TYPE.ICE, new TileObjectData() {
            maxHP = 200,
        } },
        { TILE_OBJECT_TYPE.TREASURE_CHEST, new TileObjectData() {
            maxHP = 200,
        } },
        { TILE_OBJECT_TYPE.CULTIST_KIT, new TileObjectData() {
            maxHP = 150,
            neededCharacterClass = null,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10)
                    //new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)
                ),
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)
                ),
            }
        } },
        { TILE_OBJECT_TYPE.TORCH, new TileObjectData() {
            maxHP = 200,
            neededCharacterClass = new string[] { "Craftsman" },
            repairCost = 5,
            craftRecipes = new [] {
                new TileObjectRecipe(
                    new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 30)
                ),
            }
        } },
        { TILE_OBJECT_TYPE.PHYLACTERY, new TileObjectData() {
            maxHP = 200,
            neededCharacterClass = new string[] { "Shaman" },
            repairCost = 5,
            craftRecipes = new [] {
                new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
                new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10)),
                new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.METAL_PILE, 10)),
            }
        } },
        { TILE_OBJECT_TYPE.EYE_WARD, new TileObjectData() {
            maxHP = 48,
        } },
    };

    public static bool HasTileObjectData(TILE_OBJECT_TYPE objType) {
        return tileObjectData.ContainsKey(objType);
    }
    public static TileObjectData GetTileObjectData(TILE_OBJECT_TYPE objType) {
        if (tileObjectData.ContainsKey(objType)) {
            return tileObjectData[objType];
        }
        //Debug.LogWarning("No tile data for type " + objType.ToString() + " used default tileobject data");
        return Default;
    }
    public static bool TryGetTileObjectData(TILE_OBJECT_TYPE objType, out TileObjectData data) {
        if (tileObjectData.ContainsKey(objType)) {
            data = tileObjectData[objType];
            return true;
        }
        data = Default;
        return false;
    }
}

public class TileObjectData {
    public int maxHP;
    public string[] neededCharacterClass;
    /// <summary>
    /// When this object is placed, how many tiles does it occupy? (Default is 0,0) meaning this object only occupies 1 tile. 
    /// </summary>
    public Point occupiedSize;
    /// <summary>
    /// List of recipes for crafting this object.
    /// NOTE: The first recipe will be considered as the main recipe and is the
    /// recipe that will be used by default for crafting <see cref="CraftTileObject"/> 
    /// </summary>
    public TileObjectRecipe[] craftRecipes;
    public int repairCost;
    public TileObjectRecipe mainRecipe => craftRecipes.FirstOrDefault();

    public bool TryGetPossibleRecipe(Region region, out TileObjectRecipe possibleRecipe) {
        for (int i = 0; i < craftRecipes.Length; i++) { 
            TileObjectRecipe recipe = craftRecipes[i];
            bool hasAllIngredients = region.GetTileObjectInRegionCount(recipe.ingredient.ingredient) > 0;

            //bool hasAllIngredients = true;
            //for (int j = 0; j < recipe.ingredient.Length; j++) {
            //    TileObjectRecipeIngredient ingredient = recipe.ingredient[j];
            //    if (region.GetTileObjectInRegionCount(ingredient.ingredient) <= 0) {
            //        hasAllIngredients = false;
            //        break;
            //    }
            //}
            if (hasAllIngredients) {
                possibleRecipe = recipe;
                return true;
            }
        }
        //if no possible recipe was found just return main recipe
        possibleRecipe = mainRecipe;
        return false;
    }
    public TileObjectRecipe GetRecipeThatUses(TILE_OBJECT_TYPE tileObjectType) {
        for (int i = 0; i < craftRecipes.Length; i++) {
            TileObjectRecipe recipe = craftRecipes[i];
            if (recipe.UsesIngredient(tileObjectType)) {
                return recipe;
            }
        }
        return mainRecipe;
    }

}

public struct TileObjectRecipe {
    public TileObjectRecipeIngredient ingredient;
    public bool hasValue;
    public TileObjectRecipe(TileObjectRecipeIngredient ingredient) {
        this.ingredient = ingredient;
        hasValue = true;
    }

    public int GetNeededAmountForIngredient(TILE_OBJECT_TYPE ingredient) {
        TileObjectRecipeIngredient recipeIngredient = this.ingredient;
        if (recipeIngredient.ingredient == ingredient) {
            return recipeIngredient.amount;
        }
        //for (int i = 0; i < this.ingredient.Length; i++) {
        //    TileObjectRecipeIngredient recipeIngredient = this.ingredient[i];
        //    if (recipeIngredient.ingredient == ingredient) {
        //        return recipeIngredient.amount;
        //    }
        //}
        return 0;
    }
    public bool UsesIngredient(TILE_OBJECT_TYPE tileObjectType) {
        TileObjectRecipeIngredient recipeIngredient = ingredient;
        if (recipeIngredient.ingredient == tileObjectType) {
            return true;
        }
        //for (int i = 0; i < ingredient.Length; i++) {
        //    TileObjectRecipeIngredient recipeIngredient = ingredient[i];
        //    if (recipeIngredient.ingredient == tileObjectType) {
        //        return true;
        //    }
        //}
        return false;
    }
    
}
public struct TileObjectRecipeIngredient {
    public TILE_OBJECT_TYPE ingredient;
    public int amount;
    public string ingredientName;
    
    public TileObjectRecipeIngredient(TILE_OBJECT_TYPE ingredient, int amount) {
        this.ingredient = ingredient;
        this.amount = amount;
        ingredientName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(ingredient.ToString());
    }
    
}