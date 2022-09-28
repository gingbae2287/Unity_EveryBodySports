using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ColorWar : MonoBehaviourPun{
    private static ColorWar instance;
    public static ColorWar Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //Component============================
    ColorWarGameState gameState;
    ColorWarUI ui;
    [SerializeField] GameObject[] readyWall;
    [SerializeField] Transform spawnPoint;
    public enum TeamName{
        Yellow,
        Blue,
        Green,
        Red
    }

    //bgm====
    [SerializeField] AudioSource changeColorSound;


    //==========


    //====game var===
    public int playTime=30;
    //===for Master====
    
    int[] teamScore=new int[4];
    List<int>[] teamPlayer=new List<int>[4];    //팀별 플레이어 번호 리스트

    //==for players====
    public int playerNumber{get; private set;}
    public int myTeam{get; private set;}
    Hashtable playerHash;
    Vector3 myStartPoint;

    // gaming var========================
    public int playerCount{get; private set;}
    public int winnerTeam{get; private set;}
    public int timer;

    // Team Var=========================
    public bool isTeamSet{get; private set;}
    public int teamSize{get; private set;}
    //============================================

    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
        GameManager.Instance.SetThirdPersonView(true);
        gameState=GetComponent<ColorWarGameState>();
        ui=GetComponent<ColorWarUI>();
        playerHash=new Hashtable();
    }
    private void Start() {
        GameInit();
    }

    public void GameInit(){
        playerCount=PhotonNetwork.PlayerList.Length;
        

        SetTeam();

        gameState.GameInit();
    }

    void SetTeam(){
        if(!PhotonNetwork.IsMasterClient) return;
        for(int i=4;i>0;i--) 
            if(playerCount%i==0) 
            {
                teamSize=i;
                break;
            }
        if(teamSize==1) Debug.LogError("팀이 안맞음");
        
        int[] arr=new int[playerCount];
        for(int i=0;i<playerCount;i++){
            arr[i]=i;
        }
        //팀배치 순서 랜덤 섞기
        for(int i=0;i<playerCount;i++){
            int rand=Random.Range(0,playerCount);
            int tmp=arr[rand];
            arr[rand]=arr[i];
            arr[i]=tmp;
        }
        for(int i=0;i<playerCount;i++){
            int team=arr[i]%teamSize;
            playerHash["Team"]=team;
            playerHash["playerNumber"]=i;
            //teamPlayer[team].Add(i);
            PhotonNetwork.PlayerList[i].SetCustomProperties(playerHash);
        }
        isTeamSet=true;
        photonView.RPC("SpawnPlayer", RpcTarget.AllViaServer,teamSize);
    }
    [PunRPC]
    void SpawnPlayer(int TeamSize){
        teamSize=TeamSize;
        myTeam=(int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        playerNumber=(int)PhotonNetwork.LocalPlayer.CustomProperties["playerNumber"];
        

        if(Player.LocalPlayerInstance!=null) return;
        myStartPoint=new Vector3(-4+playerNumber%5*2,0,-4+playerNumber/5*2);
        myStartPoint+=spawnPoint.position;
        PhotonNetwork.Instantiate("Character/TT_male",myStartPoint,Quaternion.identity);
        Player.LocalPlayerInstance.SetPlayerPosition(myStartPoint);
        Player.LocalPlayerInstance.GameSettingForPlayer(myStartPoint);
        Player.LocalPlayerInstance.StopMove(false);
        Player.LocalPlayerInstance.SetPlayerSpeed(1.3f,350f);
        playerHash["isSpawn"]=true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        GameManager.Instance.LoadingProgressFinish();
    }

    public void GameStart(){
        ui.SetNotification(1);
        ui.ScoreBoardInit(teamSize);
        //Player.LocalPlayerInstance.StopMove(false);
        foreach(GameObject wall in readyWall) wall.SetActive(false);
    }
    public void GameEnd(){
        if(PhotonNetwork.IsMasterClient) {
            winnerTeam=CheckWinnerTeam();
            photonView.RPC("ColorWarWinnerTeamRPC", RpcTarget.AllViaServer, winnerTeam);
        }
        Player.LocalPlayerInstance.StopMove(true);
        
    }
    public void Notification(int index, int Timer=0){
        if(!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("ColorWarNotificationRPC", RpcTarget.AllViaServer, index,Timer);
    }
    [PunRPC]
    void ColorWarNotificationRPC(int index, int Timer){
        ui.SetNotification(index, Timer);
    }
    
    
    public int GetTimer(){
        return gameState.timer;
    }

    //=============Score Board=================
    public void GetScore(int team, int value=1){
        if(!PhotonNetwork.IsMasterClient) return;
        //스코어 보드 동기화
        teamScore[team]+=value;
        photonView.RPC("ColorWarScore", RpcTarget.AllViaServer, teamScore);
    }
    [PunRPC]
    void ColorWarScore(int[] Score){
        if(!PhotonNetwork.IsMasterClient){
            for(int i=0;i<Score.Length;i++){
                teamScore[i]=Score[i];
            }
        }
        ui.UpdateScore(teamScore);
    }

    public int CheckWinnerTeam(){
        int WinnerTeam=-1, MaxScore=0;
        for(int i=0;i<teamSize;i++){
            if(teamScore[i]>MaxScore) {
                MaxScore=teamScore[i];
                WinnerTeam=i;
            }
            else if(teamScore[i]==MaxScore){
                //동점자 처리
            }
        }
        return winnerTeam=WinnerTeam;
    }
    [PunRPC]
    void ColorWarWinnerTeamRPC(int WinnerTeam){
        winnerTeam=WinnerTeam;
        ui.SetNotification(3);
        if(winnerTeam==myTeam) LeaderBoardUpdate();
    }
    //리더보드에 우승횟수 추가
    void LeaderBoardUpdate(){
        GooglePlayManager.Instance.ColorWarWin();
    }

    public void ColorChangeSound(){
        changeColorSound.Play();
        Debug.Log("사운드");
    }

}