using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Linq;
using System.Collections.Generic;

public class MaraudUIController : MVCUIController, MaraudUIView.IListener {

	[SerializeField]
	private MaraudUIModel m_maraudUIModel;
	private MaraudUIView m_maraudUIView;

	[SerializeField]
	private AvailableMonsterItemUI m_availableMonsterItemUI; //item to instantiate
	private List<AvailableMonsterItemUI> m_lesserDemonList = new List<AvailableMonsterItemUI>();
	private List<AvailableMonsterItemUI> m_minionList = new List<AvailableMonsterItemUI>();

	[SerializeField]
	private DeployedMonsterItemUI m_deployedMonsterItemUI; //item to instantiate
	private List<DeployedMonsterItemUI> m_deployedMonsters = new List<DeployedMonsterItemUI>();

	public FakePlayer fakePlayer;

	public bool isTestScene;

	private int m_defaultUnlockedCount = 3;

	private void Start() {

	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onSpireClicked -= OnSpireClicked;
		}
	}

	private void OnSpireClicked() {
		if (GameManager.Instance.gameHasStarted) {
			Init();
		}
	}

	public void Init() {

	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_maraudUIView == null) {
			MaraudUIView.Create(_canvas, m_maraudUIModel, (p_ui) => {
				m_maraudUIView = p_ui;
				m_maraudUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				ShowUI();
			});
		}
	}

	void OnAvailableMonsterClicked() {

	}

	void OnDeployedMonsterClicked() {

	}

	void OnUnlockClicked() {

	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {

	}

	public void OnCloseClicked() {
		HideUI();
	}

	public void OnLesserDemonClicked(bool isOn) {
		if (isOn) {
			
		}
	}
	public void OnMinionClicked(bool isOn) {
		if (isOn) {
			
		}
	}
	#endregion
}