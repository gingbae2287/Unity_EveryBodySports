using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ColorWarGameState : MonoBehaviourPunCallbacks, IPunObservable{
    

    IEnumerator cor;

    // game state var============
    enum GameState{ //room's custom property
        GameEnter,      //0 game Enter
        SetTeam,    //1 finished player's team Set
        GameReady,  //2 all player spawned and start count
        Gaming,     //3 gaming
        GameEnd, 
    }
    GameState gameState;
    Hashtable stateHash;
    public bool isGaming{get; private set;}
    public bool isAllPlayerSpawn{get; private set;}
     // Player info============
    int myTeam, playerNumber;

    // team variable===================
    public int winnerTeam{get; private set;}
    int teamSize;
    

    // gaming var========================
    public int timer{get; private set;}
    int playerCount;
    int sendTimer;
    WaitForSeconds delay, timerDelay;


    private void Awake() {
        stateHash=new Hashtable();
        delay=new WaitForSeconds(0.1f);
        timerDelay=new WaitForSeconds(1f);
    }
    void Start(){
    }
    public void GameInit(){
        if(isGaming) return;
        playerCount=ColorWar.Instance.playerCount;
        playerNumber=ColorWar.Instance.playerNumber;
        isGaming=true;

        if(!PhotonNetwork.IsMasterClient) return;
        gameState=GameState.GameEnter;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
        cor=GameProgress();
        StartCoroutine(cor);
    }
    void ChangeGameState(GameState state){
        gameState=state;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
    }
    IEnumerator GameProgress(){
        while(isGaming){
        switch(gameState){
            case GameState.GameEnter:
                while(!ColorWar.Instance.isTeamSet) yield return delay;
                ChangeGameState(GameState.SetTeam);
                break;

            case GameState.SetTeam:
                while(!CheckAllPlayerSpawn()) yield return delay;
                break;

            case GameState.GameReady:
                while(timer>0){
                    ColorWar.Instance.Notification(0, timer);
                    yield return timerDelay;
                    timer--;
                }
                GameStart();
                break;

            case GameState.Gaming:
                while(timer>0){
                    ColorWar.Instance.Notification(2, timer);
                    timer--;
                    yield return timerDelay;
                }
                GameEnd();
                break;

            case GameState.GameEnd:
                isGaming=false;
                StopCoroutine(cor);
                break;

        }
        yield return delay;
        }
    }
//for master func


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
        ChangeGameState(GameState.GameReady);
        timer=3;
        return isAllPlayerSpawn=true;
    }
    void GameStart(){
        if(!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("ColorWarStartRPC",RpcTarget.AllViaServer);
        ChangeGameState(GameState.Gaming);
        timer=ColorWar.Instance.playTime;
    }
    [PunRPC]
    void ColorWarStartRPC(){
        ColorWar.Instance.GameStart();
        isGaming=true;
    }

    void GameEnd(){
        if(!PhotonNetwork.IsMasterClient) return;
        isGaming=false;
        photonView.RPC("ColorWarGameEndRPC", RpcTarget.AllViaServer);
        ChangeGameState(GameState.GameEnd);
    }
    [PunRPC]
    void ColorWarGameEndRPC(){
        isGaming=false;
        ColorWar.Instance.GameEnd();
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //실시간 동기화
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
        //호스트 마이그레이션
        if(!PhotonNetwork.IsMasterClient) return;
        gameState=(GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if(isGaming){
            cor=GameProgress();
            StartCoroutine(cor);
        }
    }
}