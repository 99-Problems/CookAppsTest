using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using Data;
using System;
using UniRx.Triggers;
using System.Linq;

public class PBInGameResult : PopupArg
{
    public Define.EBATTLE_RESULT result;
    public List<InGameUnitResultInfo> unitResultList;
}

public class PopupInGameResult : PopupBase
{
    public GTMPro remainTimeText;
    public GameObject player1WinObj;
    public GameObject player2WinObj;
    public GameObject player1LoseObj;
    public GameObject player2LoseObj;
    public GameObject player1DrawObj;
    public GameObject player2DrawObj;

    public InGameResultScrollView scrollView1;
    public InGameResultScrollView scrollView2;

    public double limitTime;
    private double curTime;
    private double time;

    private PBInGameResult arg;

    private bool bLoad;



    private void Start()
    {
        gameObject.FixedUpdateAsObservable().Subscribe(_ =>
        {
            curTime += Time.deltaTime;
            if (limitTime - curTime < 0)
                Managers.Popup.ClosePopupBox(this);

        }).AddTo(this);

        Observable.Interval(TimeSpan.FromSeconds(1f)).StartWith(0).Subscribe(_ =>
        {
            UpdateUI();
        }).AddTo(this);

        var player1Units = arg.unitResultList.Where(_ => _.AccountID == UserInfo.account1).OrderBy(_1 => _1.partyIndex);
        var player2Units = arg.unitResultList.Where(_ => _.AccountID == UserInfo.account2).OrderBy(_1 => _1.partyIndex);
        scrollView1.SetItemList(player1Units);
        scrollView2.SetItemList(player2Units);
    }


    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(this);
    }

    private async void ExitToLobby()
    {
        bLoad = true;
   
        await UniTask.WaitForEndOfFrame();

        Managers.Scene.LoadScene(Define.Scene.Lobby);
        var gameState = Managers.Scene.CurrentScene as IGameState;
        if(gameState != null)
            gameState.SetMenuVisible(false);
        await UniTask.WaitUntil(() => Managers.Scene.moveScene == false);
        // ·Îºñ¾À ·Îµù
        IngameLoadingImage.LoadingEvent.OnNext(10);
        await UniTask.DelayFrame(100);
        IngameLoadingImage.LoadingEvent.OnNext(30);
        await UniTask.DelayFrame(50);
        IngameLoadingImage.LoadingEvent.OnNext(50);
        await UniTask.DelayFrame(50);
        IngameLoadingImage.LoadingEvent.OnNext(70);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        IngameLoadingImage.LoadingEvent.OnNext(100);
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);
        arg = (PBInGameResult)_popupData;

        player1WinObj.SetActive(arg.result == Define.EBATTLE_RESULT.WIN);
        player2WinObj.SetActive(arg.result == Define.EBATTLE_RESULT.LOSE);
        player1LoseObj.SetActive(arg.result == Define.EBATTLE_RESULT.LOSE);
        player2LoseObj.SetActive(arg.result == Define.EBATTLE_RESULT.WIN);
        player1DrawObj.SetActive(arg.result == Define.EBATTLE_RESULT.DRAW);
        player2DrawObj.SetActive(arg.result == Define.EBATTLE_RESULT.DRAW);

        time = limitTime;

        UpdateUI();
    }

    public override void OnClosePopup()
    {
        base.OnClosePopup();
        ExitToLobby();
    }

    public void UpdateUI()
    {
        var remainTime = (limitTime - curTime).DecimalRound(Define.DECIMALROUND.RoundUp, 1);
        if (remainTime < time)
        {
            time = Mathf.Max((int)remainTime, 1);
        }

        remainTimeText.SetText(time);
    }
}