using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TeamSettingScrollView : BaseScrollView<TeamSettingScrollViewItem, UnitData>
{
    protected override void InitFirstItem(TeamSettingScrollViewItem _obj)
    {
        base.InitFirstItem(_obj);
        _obj.btnSelect.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            itemClickSubject.OnNext(_obj);
        }).AddTo(this);

        _obj.btnChange.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            itemClickSubject.OnNext(_obj);
        }).AddTo(this);
    }
}
