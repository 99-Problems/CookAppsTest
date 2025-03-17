using Cysharp.Threading.Tasks;
using Data;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI : MonoBehaviour
{
    public Canvas uiView;
    GraphicRaycaster graphicRaycaster;
    public bool isVisible = true;

    public GTMPro timerText;
    public GTMPro redTimerText;
    
    private IGameData gameData;
    private IDisposable textRefreshFunc;

    

    private Subject<Unit> hpChangeEvent = new Subject<Unit>();

    private void Start()
    {
        isVisible = false;
        graphicRaycaster = gameObject.GetOrAddComponent<GraphicRaycaster>();

        if (uiView == null)
            uiView = GetComponent<Canvas>();


        gameData = Managers.Scene.CurrentScene as IGameData;
        gameData.OnLoadingComplete.Subscribe(_=>
        {
            gameObject.UpdateAsObservable().Subscribe(OnUpdateEvent);
            //var playInfo = gameData?.PlayInfo;
            //if (playInfo == null)
            //    return;

            //float halfHp = UserInfo.MaxLife / 2;
            //redHpBar.fillAmount = Mathf.Clamp01(playInfo.playData.life / halfHp);
            //greenHpBar.fillAmount = Mathf.Clamp01(Mathf.Max(0, playInfo.playData.life - halfHp) / halfHp);
            //redPreHpBar.fillAmount = Mathf.Clamp01(playInfo.playData.life / halfHp);
            //greenPreHpBar.fillAmount = Mathf.Clamp01(Mathf.Max(0, playInfo.playData.life - halfHp) / halfHp);
        }).AddTo(this);
        var gameState = Managers.Scene.CurrentScene as IGameState;
        gameState.OnStateObservable.Subscribe(state =>
        {
            if(state == Define.EGAME_STATE.PLAY)
            {
                isVisible = true;
                gameState.SetMenuVisible(true);
            }
        }).AddTo(this);

        gameState.MenuVisibleObservable().Subscribe(_1 =>
        {
           SetMenuVisible(_1);
        }).AddTo(this);


        Managers.Popup.OnClosePopupSubject.Subscribe(_ =>
        {
            if(isVisible)
            {
                //�޴��� ���������� Ǯ �˾��� ���ٸ� ĵ������ �Ҵ�
                if (Managers.Popup.CheckExistFullPopup() == false)
                {
                    SetMenuVisible(true);
                }
            }
          
        }).AddTo(this);

        Managers.Popup.OnshowPopupSubject.Subscribe(_ =>
        {
            if (isVisible)
            {
                //�޴��� ���������� Ǯ �˾��� ���ٸ� ĵ������ ����
                if (Managers.Popup.CheckExistFullPopup())
                {
                    SetMenuVisible(false);
                }
            }
        }).AddTo(this);
   
        //InGamePlayInfo.OnHpChange.Subscribe(_ =>
        //{
        //    UpdateHP(_);
        //}).AddTo(this);
    }

    public void SetMenuVisible(bool _b)
    {
        uiView.enabled = _b;
        graphicRaycaster.enabled = _b;
    }

    //IEnumerator ScoreAni()
    //{
    //    if (scoreRect)
    //    {
    //        scoreRect.DOKill();
    //        scoreRect.localScale = Vector3.one;
    //        scoreRect.DOScale(1.5f, 0.05f).SetEase(Ease.OutQuint);
    //        yield return new WaitForSeconds(0.07f);
    //        scoreRect.DOScale(1f, 0.08f).SetEase(Ease.Linear);
    //    }
    //}

    public void OnUpdateEvent(Unit _unit)
    {
        var playInfo = gameData?.PlayInfo;
        if (playInfo == null)
            return;

        //if(playInfo.playData.score > scoreText.targetScore)
        //{
        //    if (textRefreshFunc != null)
        //        textRefreshFunc.Dispose();
        //    textRefreshFunc = Observable.FromCoroutine(_ => ScoreAni()).Subscribe(_ => { }).AddTo(this);
        //}

        //scoreText.SetTargetScore(playInfo.playData.score);
        var offset = playInfo.playData.limitTime - playInfo.playData.currentTime;
        if (offset < 11)
        {
            timerText.gameObject.SetActive(false);
            redTimerText.gameObject.SetActive(true);
            redTimerText.SetText((int)offset / 60, (int)offset % 60);
        }
        else
        {
            redTimerText.gameObject.SetActive(false);
            timerText.gameObject.SetActive(true);
            timerText.SetText((int)offset / 60, (int)offset % 60);
        }
    }

    public void UpdateHP(float hpRate)
    {
        //var playInfo = gameData?.PlayInfo;
        //if (playInfo == null)
        //    return;

        //float halfHp = UserInfo.MaxLife / 2;
        //redHpBar.fillAmount = Mathf.Clamp01((playInfo.playData.life + hpRate) / halfHp);
        //greenHpBar.fillAmount = Mathf.Clamp01(Mathf.Max(0, (playInfo.playData.life + hpRate) - halfHp) / halfHp);

        //var hpDelay = Mathf.Abs(hpRate);
        //UniTask.Delay(TimeSpan.FromSeconds(hpDelay));

        //redPreHpBar.DOFillAmount(redHpBar.fillAmount, hpDelay);
        //greenPreHpBar.DOFillAmount(greenHpBar.fillAmount, hpDelay);
    }
}
