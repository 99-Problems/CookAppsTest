using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class LoginAccountData
{
    public Int64 AccountID { get; set; }
    public AccountInfo accountInfo { get; set; }
    public List<UnitData> units { get; set; }

    public LoginAccountData CopyInstance()
    {
        return new LoginAccountData()
        {
            AccountID = this.AccountID,
            accountInfo = this.accountInfo,
            units = this.units,
        };
    }

    public bool CompareKey(Int64 AccountID)
    {
        return this.AccountID == AccountID;
    }

    public bool CompareKey(LoginAccountData rdata)
    {
        return AccountID == rdata.AccountID;
    }
}
public class AccountInfo
{
    public Int64 AccountID { get; set; }


    public AccountInfo CopyInstance()
    {
        return new AccountInfo()
        {
            AccountID = this.AccountID,
        };
    }

    public bool CompareKey(Int64 AccountID)
    {
        return this.AccountID == AccountID;
    }

    public bool CompareKey(AccountInfo rdata)
    {
        return AccountID == rdata.AccountID;
    }
}

public class UnitData
{
    public Int64 AccountID { get; set; }
    public int UnitID { get; set; }
    public int Index { get; set; }

    public bool IsDummy { get; set; }

    public Define.EStatType Stat1Type { get; set; }
    public long Stat1Value { get; set; }
    public Define.EStatType Stat2Type { get; set; }
    public long Stat2Value { get; set; }
    public Define.EStatType Stat3Type { get; set; }
    public long Stat3Value { get; set; }

    public UnitData CreateAddUnitData()
    {
        return new UnitData()
        {
            AccountID = this.AccountID,
            UnitID = this.UnitID,
            Index = this.Index,
            Stat1Type = this.Stat1Type,
            Stat1Value = this.Stat1Value,
            Stat2Type = this.Stat2Type,
            Stat2Value = this.Stat2Value,
            Stat3Type = this.Stat3Type,
            Stat3Value = this.Stat3Value,
        };
    }


    public UnitData CopyInstance()
    {
        return new UnitData()
        {
            AccountID = this.AccountID,
            UnitID = this.UnitID,
            Index = this.Index,
            Stat1Type = this.Stat1Type,
            Stat1Value = this.Stat1Value,
            Stat2Type = this.Stat2Type,
            Stat2Value = this.Stat2Value,
            Stat3Type = this.Stat3Type,
            Stat3Value = this.Stat3Value,
        };
    }

    public bool CompareKey(Int64 AccountID, int UnitID)
    {
        return this.AccountID == AccountID
             && this.UnitID == UnitID;
    }

    public bool CompareKey(UnitData rdata)
    {
        return AccountID == rdata.AccountID
             && UnitID == rdata.UnitID;
    }
}



