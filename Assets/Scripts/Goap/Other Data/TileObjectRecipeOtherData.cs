using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class TileObjectRecipeOtherData : OtherData {
    public TileObjectRecipe recipe { get; }
    public override object obj => recipe;
    
    public TileObjectRecipeOtherData(TileObjectRecipe recipe) {
        this.recipe = recipe;
    }
    public TileObjectRecipeOtherData(SaveDataTileObjectRecipeOtherData data) {
        this.recipe = data.recipe;
    }

    public override SaveDataOtherData Save() {
        SaveDataTileObjectRecipeOtherData save = new SaveDataTileObjectRecipeOtherData();
        save.Save(this);
        return save;
    }
}

#region Save Data
public class SaveDataTileObjectRecipeOtherData : SaveDataOtherData {
    public TileObjectRecipe recipe;
    public override void Save(OtherData data) {
        base.Save(data);
        TileObjectRecipeOtherData otherData = data as TileObjectRecipeOtherData;
        Assert.IsNotNull(otherData);
        recipe = otherData.recipe;    
    }
    public override OtherData Load() {
        return new TileObjectRecipeOtherData(this);
    }
}
#endregion