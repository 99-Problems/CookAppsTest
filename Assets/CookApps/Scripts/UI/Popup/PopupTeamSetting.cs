using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using UnityEngine.UI;
using System;

public class PBTeamSetting : PopupArg
{
    public Action onClose;
}


public class PopupTeamSetting : PopupBase
{
    public Button exitBtn;
    public Button cancelBtn;
    public Button confirmBtn;

    public Button selectBtn1;
    public Button selectBtn2;
    public Button selectBtn3;

    public GameObject exitObj;
    public bool isExit = false;

    public TeamSettingScrollView scrollView1;
    public TeamSettingScrollView scrollView2;

    public GameObject selectPanel;

    protected PBTeamSetting arg;

    private TeamSettingScrollViewItem curItem;
    private bool bSelect;

    private void Start()
    {
        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            if (isExit)
                return;

            exitObj.SetActive(true);
            isExit = true;
        });

        cancelBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            exitObj.SetActive(false);
            isExit = false;
        });
        confirmBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            Managers.Popup.ClosePopupBox(this);
        });

        selectBtn1.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            if (bSelect)
                return;

            OnChangeUnit(1);
        });
        selectBtn2.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            if (bSelect)
                return;
            OnChangeUnit(2);
        });
        selectBtn3.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            if (bSelect)
                return;
            OnChangeUnit(3);
        });

        scrollView1.OnItemClick.Subscribe(item =>
        {
            ShowSelectPanel(true);
            curItem = item;
        }).AddTo(this);

        scrollView2.OnItemClick.Subscribe(item =>
        {
            ShowSelectPanel(true);
            curItem = item;
        }).AddTo(this);

        var player1Units = UserInfo.GetUnitList(UserInfo.account1);
        var player2Units = UserInfo.GetUnitList(UserInfo.account2);

        scrollView1.SetItemList(player1Units);
        scrollView2.SetItemList(player2Units);
    }
    public override void PressBackButton()
    {
        if (isExit == false)
        {
            exitObj.SetActive(true);
            isExit = true;
            return;
        }
        else if (isExit == true)
        {
            return;
        }

        Managers.Popup.ClosePopupBox(this);
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);
        arg = (PBTeamSetting)_popupData;
    }

    public override void OnClosePopup()
    {
        base.OnClosePopup();
        arg.onClose?.Invoke();
    }

    public void UpdateUI()
    {

    }

    public void ShowSelectPanel(bool isOn)
    {
        selectPanel.SetActive(isOn);
    }

    public void OnChangeUnit(int unitID)
    {
        bSelect = true;
        var data = UserInfo.GetUnit((int)curItem.GetInfo.AccountID, curItem.GetInfo.Index);
        UserInfo.ChangeUnit(data, unitID);
        curItem.Init(data, curItem.index);
        ShowSelectPanel(false);
        bSelect = false;
    }
}
