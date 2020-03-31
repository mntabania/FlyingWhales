using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicStructurePlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEMONIC_STRUCTURE; } }
    public STRUCTURE_TYPE structureType { get; protected set; }

    public DemonicStructurePlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.StartCorruption();
        //LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkObj as string);
        // BaseLandmark newLandmark =
        //     LandmarkManager.Instance.CreateNewLandmarkOnTile(this, landmarkData.landmarkType);
        LandmarkManager.Instance.PlaceBuiltStructureForSettlement(targetHex.settlementOnTile, targetHex.region.innerMap, targetHex, structureType);
        PlayerManager.Instance.player.AdjustMana(-EditableValuesManager.Instance.buildStructureManaCost);
        targetHex.landmarkOnTile?.OnFinishedBuilding();
        base.ActivateAbility(targetHex);
    }
}