using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class DataManager 
{
    public static int maxStat = 10;
    public List<StatData> GetUnitStatData(int _unitID)
    {
        List<StatData> statDatas = new List<StatData>();

        var unitScript = GetUnitInfo(_unitID);
        if (unitScript == null)
        {
            return statDatas;
        }
        foreach (Define.EStatType stat in Enum.GetValues(typeof(Define.EStatType)))
        {
            var _statData = new StatData();
            // Èû,¹Î,Áö ±âº» ½ºÅÈ 1
            switch (stat)
            {
                case Define.EStatType.NONE:
                    continue;
                case Define.EStatType.STR:
                    _statData.stat = stat;
                    _statData.val = 1;
                    break;
                case Define.EStatType.AGI:
                    _statData.stat = stat;
                    _statData.val = 1;
                    break;
                case Define.EStatType.INT:
                    _statData.stat = stat;
                    _statData.val = 1;
                    break;
                default:
                    break;
            }

            statDatas.Add(_statData);
        }
        
        return statDatas;
    }
}
