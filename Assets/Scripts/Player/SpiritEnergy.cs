using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class SpiritEnergy : PooledObject {

	private const int ExpiryInHours = 2;

	private string expiryKey;
	private Coroutine positionCoroutine;
	private Vector3 randomPos;
	private Vector3 velocity = Vector3.zero;
	[SerializeField] private Collider2D _collider;
	[SerializeField] private TrailRenderer _trail;

	public Region location { get; private set; }

	private int m_amount;

	public void Initialize(Region location, int p_amount) {
		m_amount = p_amount;
		this.location = location;
		GameDate expiry = GameManager.Instance.Today();
		expiry = expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(ExpiryInHours));
		expiryKey = SchedulingManager.Instance.AddEntry(expiry, Expire, this);

		randomPos = transform.position;
		randomPos.x += Random.Range(-1.5f, 1.5f);
		randomPos.y += Random.Range(-1.5f, 1.5f);
		positionCoroutine = StartCoroutine(GoTo(randomPos, 0.5f));
		_collider.enabled = true;
		_trail.enabled = false;
	}
	public void Initialize(Vector3 pos, Region location) {
		this.location = location;
		GameDate expiry = GameManager.Instance.Today();
		expiry = expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(ExpiryInHours));
		expiryKey = SchedulingManager.Instance.AddEntry(expiry, Expire, this);

		randomPos = pos;
		transform.position = pos;
		_collider.enabled = true;
		_trail.enabled = false;
	}
	private IEnumerator GoTo(Vector3 targetPos, float smoothTime, System.Action onReachAction = null) {
		while (Mathf.Approximately(transform.position.x, targetPos.x) == false &&
			   Mathf.Approximately(transform.position.y, targetPos.y) == false) {
			transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
			yield return null;
		}
		Vector3 finalPos = transform.position;
		transform.position = new Vector3(finalPos.x, finalPos.y, -1f);
		onReachAction?.Invoke();
		positionCoroutine = null;
	}

	private void Expire() {
		Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_EXPIRED, this);
		Destroy();
	}
	private void Destroy() {
		if (string.IsNullOrEmpty(expiryKey) == false) {
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		ObjectPoolManager.Instance.DestroyObject(this);
		//PlayerManager.Instance.RemoveChaosOrbFromAvailability(this);
	}
	public void OnPointerEnter(BaseEventData data) {
		if (positionCoroutine != null) {
			StopCoroutine(positionCoroutine);
		}
		_collider.enabled = false;
		_trail.enabled = true;
		Vector3 manaContainerPos = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.spiritEnergyLabel.transform.position);

		Vector3 controlPointA = transform.position;
		controlPointA.x += 5f;

		Vector3 controlPointB = manaContainerPos;
		controlPointB.y -= 5f;

		if (InnerMapCameraMove.Instance.target == transform) {
			InnerMapCameraMove.Instance.CenterCameraOn(null); //this is so that the camera will not follow this orb when it is animating towards the mana container.
		}

		transform.DOPath(new[] { manaContainerPos, controlPointA, controlPointB }, 0.7f, PathType.CubicBezier)
			.SetEase(Ease.InSine)
			.OnComplete(GainSpiritEnergy);
		Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_COLLECTED);
	}
	private void GainSpiritEnergy() {
		PlayerManager.Instance.player.AdjustSpiritEnergy(m_amount);
		Destroy();
	}
	public override void Reset() {
		base.Reset();
		PlayerManager.Instance.RemoveSpiritEnergyFromAvailability(this);
		location = null;
		_trail.Clear();
		_collider.enabled = true;
		positionCoroutine = null;
	}
}

[System.Serializable]
public class SaveSpiritEnergy {
	public Vector3 pos;
	public string regionID;

	public void Save(SpiritEnergy orb) {
		pos = orb.transform.position;
		regionID = orb.location.persistentID;
	}

	public void Load() {
		Region region = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(regionID);
		PlayerManager.Instance.CreateSpiritEnergyFromSave(pos, region);
	}
}