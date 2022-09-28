using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HideAndSeekGameState : MonoBehaviourPunCallbacks, IPunObservable{
    

    [SerializeField] StuffManager stuffManager;
    HideAndSeek main;
    HideAndSeekUI ui;
    // game state var============
    enum GameState{ //room's custom property
        GameEnter,      //0 start game room
        SetSeeker,    //1 finished player's team
        HideTime,  //2 all player spawned and start count
        Gaming,     //3 gaming
        GameEnd, 
    }
    GameState gameState;
    Hashtable stateHash;
    IEnumerator cor;
    public bool isGaming{get; private set;}
    public bool isAllPlayerSpawn{get; private set;}
    // gaming var========================
    public int timer{get; private set;}
    int hideTime=20, gamingTime=120;
    int sendTimer;
    WaitForSeconds delay, timerDelay;
    private void Awake() {
        ui=GetComponent<HideAndSeekUI>();
        main=GetComponent<HideAndSeek>();
        delay=new WaitForSeconds(0.1f);
        timerDelay=new WaitForSeconds(1f);
        stateHash=new Hashtable();
    }
    private void Start() {
        GameInit();
    }

    void ChangeGameState(GameState state){
        gameState=state;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
    }
    public void GameInit(){
        if(isGaming) return;
        //playerCount=ColorWar.Instance.playerCount;
        //playerNumber=ColorWar.Instance.playerNumber;
        isGaming=true;

        if(!PhotonNetwork.IsMasterClient) return;
        ChangeGameState(GameState.GameEnter);
        cor=GameProgress();
        StartCoroutine(cor);
    }
    IEnumerator GameProgress(){
        while(isGaming){
        switch(gameState){
            case GameState.GameEnter:
                while(!stuffManager.CheckAllStuffSpawn()) yield return delay;
                ChangeGameState(GameState.SetSeeker);
                break;

            case GameState.SetSeeker:
                while(!CheckAllPlayerSpawn()) yield return delay;
                timer=hideTime;
                ChangeGameState(GameState.HideTime);
                SetNotification(1, timer);
                break;

            case GameState.HideTime:
                while(timer>0){
                    SetNotification(1, timer);
                    yield return timerDelay;
                    timer--;
                }
                timer=gamingTime;
                main.GameStart();
                ChangeGameState(GameState.Gaming);
                SetNotification(2, timer);
                break;

            case GameState.Gaming:
                while(timer>0){
                    SetNotification(0, timer);
                    timer--;
                    yield return timerDelay;
                }
                main.GameEnd(false);
                ChangeGameState(GameState.GameEnd);
                break;

            case GameState.GameEnd:
                isGaming=false;
                StopCoroutine(cor);
                break;

        }
        yield return delay;
        }
    }
    bool CheckAllPlayerSpawn(){
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++){
            if(!PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("isSpawn")){
                return false;
            }
            else if(!(bool)PhotonNetwork.PlayerList[i].CustomProperties["isSpawn"]){
                return false;
            }
        }
        //모든 플레이어가 스폰 돼면
        ChangeGameState(GameState.HideTime);
        timer=3;
        return isAllPlayerSpawn=true;
    }
    public void GameEnd(){
        if(gameState!=GameState.GameEnd) ChangeGameState(GameState.GameEnd);
        timer=0;
        isGaming=false;
    }
    public void SetNotification(int index, int Timer=0){
        if(!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("HaSNotifRPC", RpcTarget.AllViaServer, index, Timer);
    }
    [PunRPC]
    void HaSNotifRPC(int index, int Timer=0){
        ui.SetNotification(index, Timer);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(!isGaming) return;
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            if(sendTimer!=timer){
                sendTimer=timer;
                stream.SendNext(sendTimer);
            }
        }

        else
        {
           timer = (int)stream.ReceiveNext();
        }
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient){
        if(!PhotonNetwork.IsMasterClient) return;
        gameState=(GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if(isGaming){
            cor=GameProgress();
            StartCoroutine(cor);
        }
    }
}