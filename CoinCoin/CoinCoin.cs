using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CoinCoin : MonoBehaviourPunCallbacks, IPunObservable{
    
    private static CoinCoin instance;
    public static CoinCoin Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //===component
    CoinCoinUI ui;
    public CoinManager coinManager{get;private set;}
    public enum GameState{ //room's custom property
        GameEnter,      //0 game room enter
        PlayerSetting,  //1 all player spawned and start count
        GameReady,     //2 game start countdown
        Gaming, 
        GameEnd,
    }
    GameState gameState;
    Hashtable stateHash;
    //==for master===
    
    public bool isGaming{get;private set;}
    public bool isGamePlaying{get;private set;}
    public bool isAllPlayerSpawn{get;private set;}
    public int playerCount{get;private set;}
    public int  bestPlayer{get;private set;}
    IEnumerator cor;
    int[] playerScores=new int[8];  //max player need
    int timer,sendTimer, gameTime=30;
    Hashtable playerHash;
    WaitForSeconds delay, timerDelay;
    //==for Players==
    public int myScore{get; private set;}
    int playerNum, mapSize=30;
    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
        GameManager.Instance.SetThirdPersonView(true);
        playerHash=new Hashtable();
        stateHash=new Hashtable();
        
        ui=GetComponent<CoinCoinUI>();
        coinManager=GetComponent<CoinManager>();

        delay=new WaitForSeconds(0.1f);
        timerDelay=new WaitForSeconds(1f);
    }
    void Start(){
        GameInit();
    }
    void GameInit(){
        playerCount=PhotonNetwork.PlayerList.Length;
        isGaming=true;
        if(PhotonNetwork.IsMasterClient)  {
            playerHash.Add("isSpawn",true);
            playerHash.Add("playerNumber",0);
            gameState=GameState.GameEnter;
            stateHash.Add("gameState",gameState);
            PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
            cor=GameProgress();
            StartCoroutine(cor);
        }
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
                    coinManager.CreateCoins();
                    while(!coinManager.coinCreated) yield return delay;
                    ChangeGameState(GameState.PlayerSetting);

                    break;
                case GameState.PlayerSetting:
                    PlayerSetting();
                    while(!CheckAllPlayerReady()) yield return delay;
                    break;
                case GameState.GameReady:
                    while(timer>0){
                        Notification(0,timer);
                        yield return timerDelay;
                        timer--;
                    }
                    GameStart();
                    yield return timerDelay;
                    break;
                case GameState.Gaming:
                    while(timer>0){
                        Notification(2,timer);
                        yield return timerDelay;
                        timer--;
                    }
                    GameEnd();
                    break;
            }
            yield return delay;
        }
        
    }
    public void Notification(int index, int Timer=0){
        if(!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("CoinCoinNotificationRPC", RpcTarget.AllViaServer, index,Timer);
    }
    [PunRPC]
    void CoinCoinNotificationRPC(int index, int Timer){
        ui.SetNotification(index, Timer);
    }
    void PlayerSetting(){
        if(!PhotonNetwork.IsMasterClient) return;
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++){
            playerHash["playerNumber"]=i;
            PhotonNetwork.PlayerList[i].SetCustomProperties(playerHash);
        }
        photonView.RPC("CoinCoinSpawnPlayer",RpcTarget.AllViaServer);
    }
    [PunRPC]
    void CoinCoinSpawnPlayer(){
        SpawnPlayer();
    }
    void SpawnPlayer(){
        playerNum=(int)PhotonNetwork.LocalPlayer.CustomProperties["playerNumber"];
        
        if(Player.LocalPlayerInstance!=null) return;
        int x=Random.Range(0,mapSize);
        int z=Random.Range(0,mapSize);
        Vector3 startPoint=new Vector3(x, 1,z);
        PhotonNetwork.Instantiate("Character/TT_male",startPoint,Quaternion.identity);
        Player.LocalPlayerInstance.GameSettingForPlayer(startPoint);
        Player.LocalPlayerInstance.StopMove(false);
        //Hashtable hash=new Hashtable();
        playerHash["isSpawn"]=true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        GameManager.Instance.LoadingProgressFinish();
    }
    bool CheckAllPlayerReady(){
        bool checkPlayerSpawn=true;
        int currentPlayers=PhotonNetwork.PlayerList.Length;
        for(int i=0;i<currentPlayers;i++){
            if(!PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("isSpawn")) return false;
            else if(!(bool)PhotonNetwork.PlayerList[i].CustomProperties["isSpawn"])return false;

        }
        timer=3;
        ChangeGameState(GameState.GameReady);
        return isAllPlayerSpawn=checkPlayerSpawn;
    }
    void GameStart(){
        if(!PhotonNetwork.IsMasterClient) return;
        isGamePlaying=true;
        coinManager.CoinStart();
        timer=gameTime;
        ChangeGameState(GameState.Gaming);
        photonView.RPC("CoinCoinStart",RpcTarget.AllViaServer);
    }
    [PunRPC]
    void CoinCoinStart(){
        isGamePlaying=true;
        Player.LocalPlayerInstance.StopMove(false);
        Notification(1);
    }
    void GameEnd(){
        if(!PhotonNetwork.IsMasterClient) return;
        ChangeGameState(GameState.GameEnd);
        photonView.RPC("CoinCoinEnd",RpcTarget.AllViaServer);
    }
    [PunRPC]
    void CoinCoinEnd(){
        isGamePlaying=false;
        isGaming=false;
        Player.LocalPlayerInstance.StopMove(true);
        
        if(bestPlayer==playerNum) {
            ui.SetNotification(4);
            GooglePlayManager.Instance.CoinCoinWin();
            GooglePlayManager.Instance.CoinCoinScore(myScore);
        }
        else ui.SetNotification(3);
        //점수 비교 1등 산출
    }


    ///===Score========
    public void GetScore(int Score){
        if(!isGamePlaying) return;
        myScore+=Score;
        ui.UpdateScore(myScore);
        photonView.RPC("UpdateScore",RpcTarget.AllViaServer,playerNum, myScore);
    }
    [PunRPC]
    void UpdateScore(int PlayerNumber, int newScore){
        playerScores[PlayerNumber]=newScore;
        if(playerScores[bestPlayer]<= playerScores[PlayerNumber]) {
            bestPlayer=PlayerNumber;

            ui.UpdateBestScore(playerScores[PlayerNumber]);
        }
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
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        gameState=(GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        if(isGaming){
            cor=GameProgress();
            StartCoroutine(cor);
        }
    }
}