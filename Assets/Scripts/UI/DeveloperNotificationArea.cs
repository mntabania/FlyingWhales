using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeveloperNotificationArea : InfoUIBase {

    [SerializeField] private GameObject notificationItemPrefab;
    [SerializeField] private ScrollRect notificationsScrollView;

    [SerializeField] private Vector2 defaultPos;
    [SerializeField] private Vector2 otherMenuOpenedPos;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
    }

    public void ShowNotification(string text, int expirationTicks, UnityAction onClickAction = null) {
        GameObject notificationGO = UIManager.Instance.InstantiateUIObject(notificationItemPrefab.name, notificationsScrollView.content);
        DeveloperNotificationItem notificationItem = notificationGO.GetComponent<DeveloperNotificationItem>();
        notificationGO.SetActive(true);
        notificationItem.SetNotification(text, expirationTicks, onClickAction);
    }

    private void OnMenuOpened(InfoUIBase openedBase) {
        if (openedBase is CharacterInfoUI || openedBase is FactionInfoUI) {
            this.transform.localPosition = otherMenuOpenedPos;
        }
        
    }
    private void OnMenuClosed(InfoUIBase openedBase) {
        if (openedBase is CharacterInfoUI || openedBase is FactionInfoUI) {
            this.transform.localPosition = defaultPos;
        }
    }
}
