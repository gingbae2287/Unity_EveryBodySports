using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorWarUI : MonoBehaviour {
    [SerializeField] GameObject[] ScoreObjs=new GameObject[4];
    [SerializeField] Text notification;

    Text[] teamScore=new Text[4];
    IEnumerator cor;
    int teamSize;
    string str;

    //=================
    void Awake(){
        for(int i=0;i<4;i++){
            teamScore[i]=ScoreObjs[i].GetComponentInChildren<Text>();
        }
    }
    private void Start() {
        if(ScoreObjs==null) Debug.LogError("ScoreObjs is null");
        if(notification==null) Debug.LogError("notification is null");
        notification.text="";
        foreach(GameObject obj in ScoreObjs) obj.SetActive(false);
    }
    public void ScoreBoardInit(int TeamSize){
        teamSize=TeamSize;
        for(int i=0;i<teamSize;i++) ScoreObjs[i].SetActive(true);
    }
    public void UpdateScore(int[] Score){
        for(int i=0;i<teamSize;i++){
            teamScore[i].text=": "+Score[i];
        }
    }
    public void ButtonLobby(){
        GameManager.Instance.LeftRoom();
    }
    public void SetNotification(int index, int timer=0){
        if(index==0) str=string.Format("{0}초 후 게임을 시작합니다.", timer);
        else if(index==1) str= "게임시작!";
        else if(index==2) str=string.Format("남은시간: {0}",timer);
        else if(index==3) str=string.Format("게임종료!\n{0}팀 승리!",(ColorWar.TeamName)(ColorWar.Instance.winnerTeam));
        notification.text=str;
    }
    
}