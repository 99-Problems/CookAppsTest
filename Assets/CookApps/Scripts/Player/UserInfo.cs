using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

static public class UserInfo
{
    static long userUID;
    public static AccountInfo accountInfo;
    public static long account1 = 10000001;
    public static long account2 = 10000002;
    public static int maxUnitSlot = 3;
    public static int maxStatPoint = 7;
    public static bool isLoggedIn = false;
    public static List<UnitData> Units { get; set; }

    public static UnitData GetUnit(long _accountID, int _index) => Units.Find(_1 => _1.AccountID == _accountID && _1.Index == _index);

    public static List<UnitData> GetUnitList(long _accountID)
    {
        List<UnitData> list = Units.Where(_ => _.AccountID == _accountID)?.ToList();

        return list;
    }


    public static void SetLoginData(LoginAccountData _loginAccountData)
    {
        Units = _loginAccountData.units;
        accountInfo = _loginAccountData.accountInfo;
    }

    static public Int64 GetAccountID()
    {
        return accountInfo?.AccountID ?? 1;
    }
    public static AccountInfo GetAccountInfo()
    {
        return accountInfo;
    }

    static long syncTicks = 0;
    public static DateTime GetTime(Define.TimeType timeType = Define.TimeType.UTC)
    {
        switch (timeType)
        {
            case Define.TimeType.UTC:
                return DateTime.UtcNow;
            case Define.TimeType.Local:
                return DateTime.Now;
            case Define.TimeType.ServerUTC:
                return DateTime.UtcNow.AddTicks(syncTicks);
            default:
                return DateTime.UtcNow.AddTicks(syncTicks);
        }

    }

    public static void SetUnitData(UnitData _unit)
    {
        var unit = GetUnit(_unit.AccountID, _unit.Index);
        if(unit == null)
        {
            Units.Add(_unit);
            SaveUnitData(_unit);
        }
        else
        {
            unit = _unit;
            SaveUnitData(_unit);
        }
    }
    public static void ChangeUnit(UnitData _unit, int _unitID)
    {
        _unit.UnitID = _unitID;
        _unit.IsDummy = _unitID <= 0;
        SetUnitData(_unit);
    }

    public static void SaveUnitData(UnitData _unit)
    {
        var unitKey = $"{_unit.AccountID}_{_unit.Index}";
        
        PlayerPrefs.SetInt($"{unitKey}_unitID", _unit.UnitID);
        PlayerPrefs.SetInt($"{unitKey}_stat1", (int)_unit.Stat1Value);
        PlayerPrefs.SetInt($"{unitKey}_stat2", (int)_unit.Stat2Value);
        PlayerPrefs.SetInt($"{unitKey}_stat3", (int)_unit.Stat3Value);
    }

    public static UnitData LoadUnitData(long accountID, int index)
    {
        UnitData data = new UnitData();

        var unitKey = $"{accountID}_{index}";
        
        var unitID = PlayerPrefs.GetInt($"{unitKey}_unitID", 0);
        
        var stat1Val = PlayerPrefs.GetInt($"{unitKey}_stat1", 0);
        var stat2Val = PlayerPrefs.GetInt($"{unitKey}_stat2", 0);
        var stat3Val = PlayerPrefs.GetInt($"{unitKey}_stat3", 0);

        data.AccountID = accountID;
        data.Index = index;
        data.UnitID = unitID;
        data.IsDummy = unitID <= 0;
        data.Stat1Value = stat1Val;
        data.Stat2Value = stat2Val;
        data.Stat3Value = stat3Val;

        return data;
    }
    

    #region client subjects
    public static Subject<Unit> OnChangeResult = new Subject<Unit>();
    #endregion

}
