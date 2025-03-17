using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Data;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
[System.Serializable]
public class UnitLogicStat
{
    //´É·ÂÄ¡
    public long strength;
    public long agility;
    public long intelligence;

    //½ºÅÈ
    public long atk;
    public long life;
    public double def;
    public double evasion;
    public float atk_speed;
    public float critical;
    //public long healAmount;

    public Define.EUnitType unitType;

    public void Clear()
    {
        strength = 0;
        agility = 0;
        intelligence = 0;

        atk = 0;
        life = 0;
        def = 0;
        evasion = 0;
        atk_speed = 1;
        critical = 5;

        unitType = Define.EUnitType.None;
    }
    public void Set(UnitInfoScript info, UnitData unitData)
    {
        Clear();
        if (info == null)
            return;

        unitType = info.unitType;

        var statList = Managers.Data.GetUnitStatData(info.unitID);
        foreach (var stat in statList)
        {
            switch (stat.stat)
            {
                case Define.EStatType.NONE:
                    break;
                case Define.EStatType.STR:
                    stat.val = Math.Max(1, unitData.Stat1Value);
                    break;
                case Define.EStatType.AGI:
                    stat.val = Math.Max(1, unitData.Stat2Value);
                    break;
                case Define.EStatType.INT:
                    stat.val = Math.Max(1, unitData.Stat3Value);
                    break;
                default:
                    break;
            }
            UnitLogicExtension.SetUnitStatData(stat, this);
        }
    }

}
