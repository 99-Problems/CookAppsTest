using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using UnityEngine.UI;
using System;

public class PBUnitStatus : PopupArg
{
    public long accountID;
    public int partyIndex;
}


public class PopupUnitStatus : PopupBase
{
    public GTMPro stat1ValText;
    public GTMPro stat2ValText;
    public GTMPro stat3ValText;
    public GTMPro remainPointText;
    public GTMPro TextDPS;

    public Button BtnStat1Plus;
    public Button BtnStat2Plus;
    public Button BtnStat3Plus;
    public Button BtnStat1Minus;
    public Button BtnStat2Minus;
    public Button BtnStat3Minus;


    public Button exitBtn;
    
    protected PBUnitStatus arg;

    private UnitData unitData;
    private int stat1Val;
    private int stat2Val;
    private int stat3Val;
    private int remainPoint;

    private void Start()
    {
        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            PressBackButton();
        });

        BtnStat1Plus.OnClickAsObservableThrottleFirst(0.2f).Subscribe(_ =>
        {
            if(CalcPoint(1,true))
                UpdateUI();

        });
        BtnStat2Plus.OnClickAsObservableThrottleFirst(0.2f).Subscribe(_ =>
        {
            if(CalcPoint(2, true))
                UpdateUI();
        });
        BtnStat3Plus.OnClickAsObservableThrottleFirst(0.2f).Subscribe(_ =>
        {
            if(CalcPoint(3, true))
                UpdateUI();
        });

        BtnStat1Minus.OnClickAsObservableThrottleFirst(0.2f).Subscribe(_ =>
        {
            if(CalcPoint(1, false))
                UpdateUI();
        });
        BtnStat2Minus.OnClickAsObservableThrottleFirst(0.2f).Subscribe(_ =>
        {
            if(CalcPoint(2, false))
                UpdateUI();
        });
        BtnStat3Minus.OnClickAsObservableThrottleFirst(0.2f).Subscribe(_ =>
        {
            if(CalcPoint(3, false))
                UpdateUI();
        });
    }
    public override void PressBackButton()
    {
        

        Managers.Popup.ClosePopupBox(this);
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);
        arg = (PBUnitStatus)_popupData;
        if (arg == null)
            return;

        unitData = UserInfo.GetUnit(arg.accountID, arg.partyIndex);
        if (unitData == null)
            return;

        stat1Val = (int)unitData.Stat1Value;
        stat1Val = stat1Val.GetFixedStat();

        stat2Val = (int)unitData.Stat2Value;
        stat2Val = stat2Val.GetFixedStat();

        stat3Val = (int)unitData.Stat3Value;
        stat3Val = stat3Val.GetFixedStat();

        remainPoint = UserInfo.maxStatPoint - (stat1Val + stat2Val + stat3Val - 3);

        UpdateUI();
    }

    public override void OnClosePopup()
    {
        base.OnClosePopup();
        SaveUnitStats();
    }

    public void SaveUnitStats()
    {
        var updateData = unitData;
            //unitData.CopyInstance();
        updateData.Stat1Value = stat1Val;
        updateData.Stat2Value = stat2Val;
        updateData.Stat3Value = stat3Val;
        UserInfo.SetUnitData(updateData);
    }

    public void UpdateUI()
    {
        stat1ValText.SetText(stat1Val);
        stat2ValText.SetText(stat2Val);
        stat3ValText.SetText(stat3Val);
        remainPointText.SetText(remainPoint);
        CalcDPS();
    }
    public void CalcDPS()
    {
        if (unitData == null)
        {
            TextDPS.SetText("NONE");
            return;
        }

        var unitInfo = Managers.Data.GetUnitInfo(unitData.UnitID);
        if(unitInfo == null)
        {
            TextDPS.SetText("NONE");
            return;
        }
        var tempStat = new UnitLogicStat();
        tempStat.Clear();
        tempStat.unitType = unitInfo.unitType;
        var str = new StatData { stat = Data.Define.EStatType.STR, val = stat1Val };
        var agi = new StatData { stat = Data.Define.EStatType.AGI, val = stat2Val };
        var _int = new StatData { stat = Data.Define.EStatType.INT, val = stat3Val };
        UnitLogicExtension.SetUnitStatData(str, tempStat);
        UnitLogicExtension.SetUnitStatData(agi, tempStat);
        UnitLogicExtension.SetUnitStatData(_int, tempStat);
        var criticalRate = tempStat.critical * 0.01f;
        var defaultCritDmg = 2f;
        var dps = tempStat.atk * tempStat.atk_speed;
        if(unitInfo.unitType != Data.Define.EUnitType.Support)
        {
            dps *= ((1 - criticalRate) + criticalRate * defaultCritDmg);
        }
        TextDPS.SetText(dps);
    }

    public bool CalcPoint(int statIndex, bool plus)
    {
        if (remainPoint <= 0 && plus)
            return false;

        switch (statIndex)
        {
            case 1:
                if(plus == false)
                {
                    if (stat1Val <= 1)
                        return false;

                    stat1Val--;
                    remainPoint++;
                }
                else
                {
                    if (stat1Val >= DataManager.maxStat)
                        return false;

                    stat1Val++;
                    remainPoint--;
                }
                break;
            case 2:
                if (plus == false)
                {
                    if (stat2Val <= 1)
                        return false;

                    stat2Val--;
                    remainPoint++;
                }
                else
                {
                    if (stat2Val >= DataManager.maxStat)
                        return false;

                    stat2Val++;
                    remainPoint--;
                }
                break;
            case 3:
                if (plus == false)
                {
                    if (stat3Val <= 1)
                        return false;

                    stat3Val--;
                    remainPoint++;
                }
                else
                {
                    if (stat3Val >= DataManager.maxStat)
                        return false;

                    stat3Val++;
                    remainPoint--;
                }
                break;
        }

        return true;
    }

}
