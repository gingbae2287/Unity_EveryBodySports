using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCoinUI : MonoBehaviour {
    [SerializeField] Text bestScore;
    [SerializeField] Text myScore;
    [SerializeField] Text timer;
    [SerializeField] Text notif;
    string str;
    
    private void Start() {
        if(bestScore==null) Debug.LogError("bestScore is null");
        if(myScore==null) Debug.LogError("myScore is null");
        if(timer==null) Debug.LogError("timer is null");
        if(notif==null) Debug.LogError("notif is null");
        myScore.text="My Score: 0";
        bestScore.text="Best Score: 0";
        notif.text="";
        timer.text="";
    }
    public void UpdateScore(int Score){
        myScore.text="My Score: "+Score;
       
    }
    public void UpdateBestScore(int BestScore){
         bestScore.text="Best Score: "+BestScore;
    }
    public void Timer(int Timer){
        timer.text="Time Left: "+Timer;
    }
    public void SetNotification(int index, int timer=0){
        if(index==0) str=string.Format("{0}초 후 게임이 시작합니다.", timer);
        else if(index==1) str="게임 시작!";
        else if(index==2) {
            Timer(timer);
            str="";
        }
        else if(index==3) {
            str="게임 종료!\n아쉽게 우승하지 못했습니다";
            Timer(0);
        }
        else if(index==4) {
            str="게임 종료!\n당신은 우승했습니다.";
            Timer(0);
        }
        notif.text=str;
    }
    public void ButtonLobby(){
        GameManager.Instance.LeftRoom();
    }
}