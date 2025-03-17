using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUnitResultInfo
{
    public int partyIndex;
    public double damage;
    public double recieved;
    public double heal;
    public double hp;
    public bool isAlive;
    public long AccountID;
}

public class InGameResultScrollViewItem : BaseScrollViewItem<InGameUnitResultInfo>
{
    public GTMPro indexText;
    public GTMPro damageText;
    public GTMPro recievedText;
    public GTMPro healText;
    public GTMPro hpText;

    public GameObject aliveObj;
    public GameObject deadObj;

    public InGameUnitResultInfo Info { get; private set; }

    public override void Init(InGameUnitResultInfo _info, int _index)
    {
        if (_info == null)
            return;

        Info = _info;
        
        UpdateUI();
    }

    public void UpdateUI()
    {
        indexText.SetText(Info.partyIndex);
        damageText.SetText(Info.damage.DecimalRound(Data.Define.DECIMALROUND.Round, 1).ValueToString());
        recievedText.SetText(Info.recieved.DecimalRound(Data.Define.DECIMALROUND.Round, 1).ValueToString());
        healText.SetText(Info.heal.DecimalRound(Data.Define.DECIMALROUND.Round, 1).ValueToString());
        hpText.SetText(Info.hp.DecimalRound(Data.Define.DECIMALROUND.Round, 1).ValueToString());
        aliveObj.SetActive(Info.isAlive);
        deadObj.SetActive(!Info.isAlive);
    }
}
