using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideAndSeekUI : MonoBehaviour {
    //==component====
    HideAndSeek main;
    [SerializeField] FindUI findUI;
    [SerializeField] GameObject crossHair;
    [SerializeField] GameObject[] selectStuffButton;
    [SerializeField] Text notif;
    [SerializeField] Text timer;
    [SerializeField] GameObject lifeParent;
    [SerializeField] GameObject lifePrefab;
    GameObject[] lifes;
    bool isSeeker;
    string str;
    private void Awake() {
        main=GetComponent<HideAndSeek>();
    }

    public void Timer(int Timer){
        timer.text="Time Left: "+Timer;
    }

    public void SetUIInit(bool IsSeeker){
        isSeeker=IsSeeker;
        crossHair.SetActive(false);
        lifeParent.SetActive(isSeeker);
        foreach(GameObject obj in selectStuffButton) obj.SetActive(!isSeeker);
    } 


    public void GameStart(){
        if(isSeeker){
            crossHair.SetActive(true);
            lifes=new GameObject[main.Life];
            for(int i=0;i<main.Life;i++){
                lifes[i]=Instantiate(lifePrefab,Vector3.zero,Quaternion.identity);
                lifes[i].transform.SetParent(lifeParent.transform);
            }
        }
        else{
            foreach(GameObject obj in selectStuffButton) obj.SetActive(false);
        }
    }

    
    public void SetNotification(int index, int timer=0){
        if(index==0) Timer(timer);
        else if(isSeeker){
            switch(index){
                case 1:
                    str=string.Format("제한시간 동안 맵을 익히세요! {0}초 후 게임을 시작합니다!",timer);
                    break;
                case 2:
                    str="플레이어로 의심되는 물건을 찾으세요!";
                    break;
                case 3:
                    str="모든 플레이어를 찾았습니다! Seeker 승리!";
                    break;

                case 4:
                    str="모든 Hider를 찾지 못했어요. Seeker 패배";
                    break;
                

            }
        }
        else if(!isSeeker){
            switch(index){
                case 1:
                    str=string.Format("제한시간 동안 숨으세요! {0}초 후 술래들이 찾아옵니다!",timer);
                    break;

                case 2:
                    str="술래들이 움직이기 시작했어요!";
                    break;
                case 3:
                    str="모든 Hider가 발각됐어요! Hider 패배";
                    break;
                case 4:
                    str="Seeker가 모든 Hider를 찾지 못했어요. Hider 승리!";
                    break;
            }
        }
        notif.text=str;
    }
    public void CatchHider(string SeekerName, string HiderName){
        findUI.FindUIOn(SeekerName,HiderName);
    }
    public void LifeMinus(int currentLife){
        for(int i=currentLife;i<lifes.Length;i++){
            if(lifes[i].activeSelf) lifes[i].SetActive(false);
        }
    }

}