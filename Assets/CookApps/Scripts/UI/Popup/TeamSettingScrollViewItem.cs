using Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;


public class TeamSettingScrollViewItem : BaseScrollViewItem<UnitData>
{
    public List<Sprite> unitIconList;
    public Dictionary<int, Sprite> Icons = new Dictionary<int, Sprite>();
    public Sprite dummyIcon;
    public Image unitIcon;
    public Button btnSelect;
    public Button btnChange;
    public Button btnStatus;

    public bool isEmptySlot { get; private set; }

    private UnitData Info;
    public UnitData GetInfo => Info;
    public int index { get; private set; }

    private void Start()
    {
        btnStatus.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            Debug.ColorLog($"{Info.AccountID} / {Info.Index} / {Info.Stat1Value} {Info.Stat2Value} {Info.Stat3Value}");
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupUnitStatus, new PBUnitStatus
            { 
                accountID = Info.AccountID,
                partyIndex = Info.Index,
            });
        }).AddTo(this);
    }

    public override void Init(UnitData _info, int _index)
    {
        if (_info == null)
            return;
        index = _index;

        Info =_info;
        isEmptySlot = Info.IsDummy;
        for (int i = 0; i < unitIconList.Count; i++)
        {
            if (Icons.TryGetValue(i + 1, out var sprite) == false)
            {
                Icons.Add(i + 1, unitIconList[i]);
            }
        }
       
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (Info.IsDummy)
            unitIcon.sprite = dummyIcon;
        else if(Icons.ContainsKey(Info.UnitID))
        {
           unitIcon.sprite = Icons[Info.UnitID];
        }
        else
        {
            unitIcon.sprite = null;
        }

        btnChange.gameObject.SetActive(!Info.IsDummy);
        btnSelect.interactable = Info.IsDummy;
        btnStatus.interactable = !Info.IsDummy;
    }

    public void Select(int unitID)
    {

    }
}
