﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class EnvoyAvatar : MonoBehaviour {
	public Envoy envoy;
	public PandaBehaviour pandaBehaviour;
	public Animator animator;
	public bool collidedWithHostile;
	public General otherGeneral;

	private bool hasArrived = false;
//	private bool isMoving = false;
//	private Vector3 targetPosition = Vector3.zero;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();
	internal DIRECTION direction;
//	public float speed;

//	void Update(){
//		if(this.isMoving){
//			if(this.targetPosition != null){
//				float step = speed * Time.deltaTime;
//				this.transform.position = Vector3.MoveTowards (this.transform.position, this.targetPosition, step);
//				if(Vector3.Distance(this.transform.position, this.targetPosition) < 0.1f){
//					StopMoving ();
//				}
//			}
//		}
//	}
	internal void Init(Envoy envoy){
		this.envoy = envoy;
		this.direction = DIRECTION.LEFT;
		ResetValues ();
		this.AddBehaviourTree ();
	}
	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "General"){
			this.collidedWithHostile = false;
			if(this.gameObject != null && other.gameObject != null){
				if(other.gameObject.GetComponent<GeneralAvatar>().general.citizen.city.kingdom.id != this.envoy.citizen.city.kingdom.id){
					if(!other.gameObject.GetComponent<GeneralAvatar> ().general.citizen.isDead){
						this.collidedWithHostile = true;
						this.otherGeneral = other.gameObject.GetComponent<GeneralAvatar> ().general;
					}
				}
			}
		}


	}
	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
		if(startTile.transform.position.x <= targetTile.transform.position.x){
			if(this.animator.gameObject.transform.localScale.x > 0){
				this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
			}
		}else{
			if(this.animator.gameObject.transform.localScale.x < 0){
				this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
			}
		}
		if(startTile.transform.position.y < targetTile.transform.position.y){
			this.direction = DIRECTION.UP;
			this.animator.Play("Walk_Up");
		}else if(startTile.transform.position.y > targetTile.transform.position.y){
			this.direction = DIRECTION.DOWN;
			this.animator.Play("Walk_Down");
		}else{
			if(startTile.transform.position.x < targetTile.transform.position.x){
				this.direction = DIRECTION.RIGHT;
				this.animator.Play("Walk_Right");
			}else{
				this.direction = DIRECTION.LEFT;
				this.animator.Play("Walk_Left");
			}
		}
//		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
//		if(startTile.transform.position.x <= targetTile.transform.position.x){
//			if(this.generalAnimator.gameObject.transform.localScale.x > 0){
//				this.generalAnimator.gameObject.transform.localScale = new Vector3(this.generalAnimator.gameObject.transform.localScale.x * -1, this.generalAnimator.gameObject.transform.localScale.y, this.generalAnimator.gameObject.transform.localScale.z);
//			}
//		}else{
//			if(this.generalAnimator.gameObject.transform.localScale.x < 0){
//				this.generalAnimator.gameObject.transform.localScale = new Vector3(this.generalAnimator.gameObject.transform.localScale.x * -1, this.generalAnimator.gameObject.transform.localScale.y, this.generalAnimator.gameObject.transform.localScale.z);
//			}
//		}
//		if(startTile.transform.position.y < targetTile.transform.position.y){
//			this.generalAnimator.Play("Walk_Up");
//		}else{
//			this.generalAnimator.Play("Walk");
//		}
		this.GetComponent<SmoothMovement>().direction = this.direction;
		this.GetComponent<SmoothMovement>().Move(targetTile.transform.position);
//		this.targetPosition = targetTile.transform.position;
//		this.UpdateUI ();
//		this.isMoving = true;
	}
	private void StopMoving(){
		this.animator.Play("Idle");
//		this.isMoving = false;
//		this.targetPosition = Vector3.zero;
	}
//	internal void UpdateUI(){
//		if(this.general != null){
//			this.textMesh.text = this.general.army.hp.ToString ();
//		}
//	}
	[Task]
	public void IsThereCitizen(){
		if(this.envoy.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void IsThereEvent(){
		if(this.envoy.gameEvent != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedAtTargetHextile(){
		if(this.envoy.location == this.envoy.targetLocation){
			if(!this.hasArrived){
				this.hasArrived = true;
				this.envoy.Attack ();
			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}

	}
		
	[Task]
	public void HasCollidedWithHostileGeneral(){
		if(this.collidedWithHostile){
			this.collidedWithHostile = false;
			if(!this.otherGeneral.citizen.isDead){
				//Death by general
				this.envoy.gameEvent.DeathByGeneral (this.otherGeneral);
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}

		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasDiedOfOtherReasons(){
		if (this.envoy.citizen.isDead) {
			//Citizen has died
			this.envoy.gameEvent.DeathByOtherReasons ();
			Task.current.Succeed();
		}else {
			Task.current.Fail ();
		}
	}
	[Task]
	public void MoveToNextTile(){
		Move ();
		Task.current.Succeed ();
	}

	private void ResetValues(){
		this.collidedWithHostile = false;
		this.otherGeneral = null;
	}

	private void Move(){
		if(this.envoy.gameEvent.eventType == EVENT_TYPES.STATE_VISIT){
			if(this.envoy.targetLocation != null){
				if(this.envoy.path != null){
					if(this.envoy.path.Count > 0){
						if(this.envoy.daysBeforeMoving <= 0){
							this.MakeCitizenMove (this.envoy.location, this.envoy.path [0]);
							this.envoy.daysBeforeMoving = this.envoy.path [0].movementDays;
							this.envoy.location = this.envoy.path[0];
							this.envoy.citizen.currentLocation = this.envoy.path [0];
							this.envoy.path.RemoveAt (0);
						}
						this.envoy.daysBeforeMoving -= 1;
					}
				}
			}
		}else{
			if(this.envoy.targetLocation != null){
				if(this.envoy.path != null){
					if(this.envoy.path.Count > 0){
						this.MakeCitizenMove (this.envoy.location, this.envoy.path [0]);
//						this.envoy.daysBeforeMoving = this.envoy.path [0].movementDays;
						this.envoy.location = this.envoy.path[0];
						this.envoy.citizen.currentLocation = this.envoy.path [0];
						this.envoy.path.RemoveAt (0);
					}
				}
			}
		}

	}

	internal void AddBehaviourTree(){
		BehaviourTreeManager.Instance.allTrees.Add (this.pandaBehaviour);
	}

	internal void RemoveBehaviourTree(){
		BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
	}


	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			UIManager.Instance.ShowSmallInfo (this.envoy.gameEvent.eventType.ToString ());
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
		this.UnHighlightPath ();
	}

	void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		for (int i = 0; i < this.envoy.path.Count; i++) {
			this.envoy.path [i].highlightGO.SetActive (true);
			this.pathToUnhighlight.Add (this.envoy.path [i]);
		}
	}

	void UnHighlightPath(){
		for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
			this.pathToUnhighlight[i].highlightGO.SetActive(false);
		}
	}

	void OnDestroy(){
		BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
		UnHighlightPath ();
	}
	public void OnEndAttack(){
		this.envoy.gameEvent.DoneCitizenAction(this.envoy.citizen);
		this.envoy.DestroyGO ();
	}

	internal void HasAttacked(){
		this.GetComponent<SmoothMovement> ().hasAttacked = true;
	}
//	private string CampaignInfo(Campaign campaign){
//		string info = string.Empty;
//		info += "id: " + campaign.id;
//		info += "\n";
//
//		info += "campaign type: " + campaign.campaignType.ToString ();
//		info += "\n";
//
//		info += "general: " + this.general.citizen.name;
//		info += "\n";
//
//		info += "target city: " + campaign.targetCity.name;
//		info += "\n";
//		if (campaign.rallyPoint == null) {
//			info += "rally point: N/A";
//		} else {
//			info += "rally point: " + campaign.rallyPoint.name; 
//		}
//		info += "\n";
//
//		info += "leader: " + campaign.leader.name;
//		info += "\n";
//
//		info += "war type: " + campaign.warType.ToString ();
//		info += "\n";
//
//		info += "needed army: " + campaign.neededArmyStrength.ToString ();
//		info += "\n";
//
//		info += "army: " + campaign.GetArmyStrength ().ToString ();
//		info += "\n";
//
//		if (campaign.campaignType == CAMPAIGN.DEFENSE) {
//			if (campaign.expiration == -1) {
//				info += "expiration: none";
//			} else {
//				info += "will expire in " + campaign.expiration + " days";
//			}
//		} else {
//			info += "expiration: none";
//
//		}
//
//		return info;
//	}


}
