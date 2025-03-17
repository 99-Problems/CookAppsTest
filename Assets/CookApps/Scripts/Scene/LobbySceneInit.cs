using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LobbySceneInit : BaseScene
{
    public Define.ECONTENT_TYPE ContentType { get; set; }

    public UIOpenAni loadObj;

    protected override void Init()
    {
        base.Init();

        Managers.Scene.CurrentSceneType = Define.Scene.Lobby;
        ContentType = Define.ECONTENT_TYPE.LOBBY;

        
    }

    public virtual async UniTaskVoid Loading()
    {
        if (UserInfo.isLoggedIn == false)
        {
            loadObj.SetActive(true);
            UserInfo.isLoggedIn = true;
            await Managers.String.LoadStringInfo(); //스트링 로딩
            await Managers.Data.LoadScript(); // 스크립트 로딩
            await LoadUserInfo();
            bool bOpen = false;
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPush, new PBPush
            {
                strDesc = "welcome",
                pushTime = 1f,
                isTextAni = true,
                onClose = ()=> bOpen = true,
            }, false);

            await UniTask.WaitUntil(() => bOpen);
            loadObj.SetActive(false);
        }


        if (startButton)
        {
            startButton.OnClickAsObservableThrottleFirst().Subscribe(_ =>
            {
                if(UserInfo.Units.Any(_=>_.UnitID <= 0))
                {
                    Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPush, new PBPush
                    {
                        strDesc = "select team units",
                        pushTime = 1f,
                        isTextAni = true,

                    });
                    return;
                }
                Managers.Scene.LoadScene(Define.Scene.GameScene);
                startButton.interactable = false;

            }).AddTo(this);
        }

        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }).AddTo(this);

        settingBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupTeamSetting, new PBTeamSetting { });
        }).AddTo(this);
    }


    public override void Clear()
    {
    }
    public Button startButton;
    public Button settingBtn;
    public Button exitBtn;

    private void Start()
    {
        Loading();
        
    }

    public async UniTask LoadUserInfo()
    {
        UserInfo.Units = new List<UnitData>();
        for (int i = 1; i <= UserInfo.maxUnitSlot; i++)
        {
            UserInfo.SetUnitData(UserInfo.LoadUnitData(UserInfo.account1, i));
        }
        for (int i = 1; i <= UserInfo.maxUnitSlot; i++)
        {
            UserInfo.SetUnitData(UserInfo.LoadUnitData(UserInfo.account2, i));
        }

        Debug.ColorLog("UserInfo loaded");
    }
}
