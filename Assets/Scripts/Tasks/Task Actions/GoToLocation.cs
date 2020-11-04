using UnityEngine;
using System.Collections;

public class GoToLocation : TaskAction {

    private ILocation targetLocation;
    private PATHFINDING_MODE _pathfindingMode = PATHFINDING_MODE.USE_ROADS;

    #region getters/setters
    public HexTile targetTile {
        get {
			if (targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
				return (targetLocation as BaseLandmark).tileLocation;
            } else {
                return (targetLocation as HexTile);
            }
        }
    }
    #endregion

    public GoToLocation(CharacterTask task) : base(task) {}

    #region overrides
    public override void InitializeAction(ILocation target) {
        base.InitializeAction(target);
        targetLocation = target;
    }
    #endregion

    internal void SetPathfindingMode(PATHFINDING_MODE pathfindingMode) {
        _pathfindingMode = pathfindingMode;
    }

    /*
     This will check if the character is already at target location,
     if it is, action is done, otherwise, make the character move
     to the target location.
         */
    internal void Generic() {
        if (actionDoer.currLocation.id == targetTile.id) {
            //action doer is already at the target location
            ActionDone(TASK_ACTION_RESULT.SUCCESS);
        } else {
            //if (actionDoer.avatar == null) {
            //    //Instantiate a new character avatar
            //    actionDoer.CreateNewAvatar();
            //}
            //actionDoer.avatar.SetTarget(targetLocation);
            //actionDoer.avatar.StartPath(_pathfindingMode, () => ActionDone(TASK_ACTION_RESULT.SUCCESS));
        }
    }

	internal void Expand() {
		if (actionDoer.currLocation.id == targetTile.id) {
			//action doer is already at the target location
			ActionDone(TASK_ACTION_RESULT.SUCCESS);
		} else {
			//if (actionDoer.avatar == null) {
			//	//Instantiate a new character avatar
			//	actionDoer.CreateNewAvatar();
			//}
			//actionDoer.avatar.SetTarget (targetLocation);
			//actionDoer.avatar.StartPath(PATHFINDING_MODE.MAJOR_ROADS, () => ActionDone(TASK_ACTION_RESULT.SUCCESS));
            //actionDoer.currentAction.AddNewLog(actionDoer.name + " goes to " + targetTile.name);

        }
	}
	internal void Defend() {
		if (actionDoer.currLocation.id == targetTile.id) {
			//action doer is already at the target location
			ActionDone(TASK_ACTION_RESULT.SUCCESS);
		} else {
			//if (actionDoer.avatar == null) {
			//	//Instantiate a new character avatar
			//	actionDoer.CreateNewAvatar();
			//}
			//actionDoer.avatar.SetTarget (targetLocation);
			//actionDoer.avatar.StartPath(PATHFINDING_MODE.MAJOR_ROADS, () => ActionDone(TASK_ACTION_RESULT.SUCCESS));
		}
	}
	internal void Attack() {
		if (actionDoer.currLocation.id == targetTile.id) {
			//action doer is already at the target location
			ActionDone(TASK_ACTION_RESULT.SUCCESS);
		} else {
			//if (actionDoer.avatar == null) {
			//	//Instantiate a new character avatar
			//	actionDoer.CreateNewAvatar();
			//}
			//actionDoer.avatar.SetTarget (targetLocation);
			//actionDoer.avatar.StartPath(PATHFINDING_MODE.MAJOR_ROADS, () => ActionDone(TASK_ACTION_RESULT.SUCCESS));
		}
	}
}
