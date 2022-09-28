using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HideAndSeek : MonoBehaviourPunCallbacks
{
    private static HideAndSeek instance;
    public static HideAndSeek Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //==Component=====
    HideAndSeekUI ui;
    HideAndSeekGameState gameState;
    string path="HideAndSeek/Objects/";
    List<string> stuffNames;
    //==== game var=====
    [SerializeField] GameObject hiderParent;
    [SerializeField] Transform hiderSpawnPoint, seekerSpawnPoint, seekerWaitPoint, cameraStartPoint;
    public int playerCount{get; private set;}
    public bool isTeamSet{get; private set;}
    public bool isGameStart{get; private set;}
    bool[] isPlayerSeeker;
    
    public int hiderCount{get; private set;}
    public int seekerCount{get; private set;}

    //====Player var=====
    Hashtable playerHash;
    public bool isSeeker{get; private set;}
    //== Hider var=====
    public bool isCatched{get; private set;}
    int currentStuff;
    Vector3 currentPos;
    //===Seeker Var======
    GameObject seekerPlayer;
    public int Life{get; private set;}

    List<GameObject> stuffs;
    private void Awake() {
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }

        ui=GetComponent<HideAndSeekUI>();
        gameState=GetComponent<HideAndSeekGameState>();
        playerHash=new Hashtable();
        stuffNames=new List<string>();
        stuffs=new List<GameObject>();
        foreach(GameObject obj in Resources.LoadAll<GameObject>(path+"Small")){
            stuffNames.Add(path+"Small/"+obj.name);
        }
        foreach(GameObject obj in Resources.LoadAll<GameObject>(path+"Medium")){
            stuffNames.Add(path+"Medium/"+obj.name);
        }
    }
    /*public override void OnDisable() {
        base.OnDisable();
        instance=null;
    }*/
    private void Start() {
        GameInit();
    }
    void GameInit(){
        playerCount=PhotonNetwork.PlayerList.Length;
        if(!PhotonNetwork.IsMasterClient) return;
        hiderCount=0;
        seekerCount=0;
        SetSeeker();
        
    }
    void SetSeeker(){
        if(!PhotonNetwork.IsMasterClient) return;
        bool[] seekerArray=new bool[playerCount];
        playerHash.Add("isSeeker", false);
        for(int i=0;i<=playerCount/4;i++) seekerArray[i]=true;
        for(int i=0;i<playerCount;i++){
            bool tmp=seekerArray[i];
            int tmpnum=Random.Range(0,playerCount);
            seekerArray[i]=seekerArray[tmpnum];
            seekerArray[tmpnum]=tmp;
        }
        //
        //
        for(int i=0;i<playerCount;i++){
            playerHash["isSeeker"]=seekerArray[i];
            if(seekerArray[i]) seekerCount++;
            else hiderCount++;
            PhotonNetwork.PlayerList[i].SetCustomProperties(playerHash);
        }
        isTeamSet=true;

        
        photonView.RPC("HaSSpawnPlayer",RpcTarget.AllViaServer,seekerCount,hiderCount);
    }
    [PunRPC]
    void HaSSpawnPlayer(int SeekerCount, int HiderCount){
        isTeamSet=true;
        seekerCount=SeekerCount;
        hiderCount=HiderCount;
        Debug.Log("시커: "+seekerCount+"  하이더: "+hiderCount);
        if(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isSeeker"))
        { 
            isSeeker=(bool)PhotonNetwork.LocalPlayer.CustomProperties["isSeeker"];
            GameManager.Instance.SetThirdPersonView(!isSeeker);
            if(!isSeeker) 
            {   
                CreateStuffList();
                HiderInitSetting();
            }
            else{
                if(Player.LocalPlayerInstance!=null) return;
                seekerPlayer=PhotonNetwork.Instantiate("Character/TT_male",seekerWaitPoint.position,Quaternion.identity);
                Player.LocalPlayerInstance.StopMove(true);
                Player.LocalPlayerInstance.SetFreeCamMode(true);
                Camera.main.transform.position=cameraStartPoint.position;
                Camera.main.transform.rotation=cameraStartPoint.rotation;
                Life=3;
            }
        } 
        playerHash["isSpawn"]=true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        ui.SetUIInit(isSeeker);
        
        GameManager.Instance.LoadingProgressFinish();
    }
    void CreateStuffList(){
       for(int i=0;i<stuffNames.Count;i++){
            stuffs.Add(PhotonNetwork.Instantiate(stuffNames[i],new Vector3(0,-20f,0),Quaternion.identity));
            stuffs[i].GetComponent<HideStuff>().SetHuman();
            stuffs[i].SetActive(false);
        }
        
    }
    void HiderInitSetting(){
        currentStuff=Random.Range(0,stuffs.Count);
        stuffs[currentStuff].SetActive(true);
        Player.LocalPlayerInstance.SetThirdPersonView(!isSeeker);
        Vector3 pos=hiderSpawnPoint.position+new Vector3(Random.Range(-5,5),0,Random.Range(-5,5));
        //stuffs[currentStuff].transform.position=hiderSpawnPoint.position;
        Player.LocalPlayerInstance.SetPlayerPosition(pos);
        Player.LocalPlayerInstance.StopMove(false);
    }


    public void GameStart(){
        if(!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("HasGameStart", RpcTarget.AllViaServer);
        
    }
    [PunRPC]
    void HasGameStart(){
        isGameStart=true;
        ui.GameStart();
        if(isSeeker) 
        {   
            Vector3 pos=seekerSpawnPoint.position+new Vector3(Random.Range(-5,5),0,Random.Range(-5,5));
            Player.LocalPlayerInstance.GameSettingForPlayer(pos);
            Player.LocalPlayerInstance.SetPlayerPosition(pos);
            Player.LocalPlayerInstance.StopMove(false);
            Player.LocalPlayerInstance.SetFreeCamMode(false);
        }
        else{
            for(int i=0;i<stuffs.Count;i++){
                if(i==currentStuff) continue;
                PhotonNetwork.Destroy(stuffs[i].GetPhotonView());
            }
            Player.LocalPlayerInstance.SetPlayerSpeed(0.6f);
            Player.LocalPlayerInstance.gameObject.GetComponent<HideStuff>().GameStart();
        }
    }
   

    public void Catched(string SeekerName){
        if(isSeeker) return;
        if(isCatched) return;
        Player.LocalPlayerInstance.SetFreeCamMode(true);
        Player.LocalPlayerInstance.transform.position=new Vector3(0,-20f,0);
        isCatched=true;
        photonView.RPC("HasHiderCatched",RpcTarget.AllViaServer,SeekerName,PhotonNetwork.NickName);
    }
    public void LifeMinus(){
        if(!isSeeker) return;
        ui.LifeMinus(--Life); 
        if(Life<=0) {
            //사망
            Player.LocalPlayerInstance.SetFreeCamMode(true);
            Player.LocalPlayerInstance.StopMove(true);
            Player.LocalPlayerInstance.transform.position=seekerWaitPoint.position;
            photonView.RPC("HasPlayerDie",RpcTarget.AllViaServer,isSeeker);
            return;
        }
    }
    [PunRPC]
    void HasHiderCatched(string SeekerName, string HiderName){
        ui.CatchHider(SeekerName,HiderName);
        hiderCount--;
        Debug.Log("남은시커: "+seekerCount+" 남은 하이더: "+hiderCount);
        if(!PhotonNetwork.IsMasterClient) return;
        if(hiderCount<=0){
            GameEnd(true);
        }
    }
    [PunRPC]
    void HasPlayerDie(bool IsSeeker){
        //플레이어 나갔을 때도 작동
        if(IsSeeker){
            seekerCount--;
            if(seekerCount<=0){
            //게임종료 hider승리
            GameEnd(false);
            }
        } 
        else {
            hiderCount--;
            if(hiderCount<=0){
            // 게임종료 seeker승리.
            GameEnd(true);
            }
        }


        Debug.Log("남은시커: "+seekerCount+" 남은 하이더: "+hiderCount);
        if(!PhotonNetwork.IsMasterClient) return;
    }
     public void GameEnd(bool SeekerWin){
        if(!PhotonNetwork.IsMasterClient) return;
        if(!isGameStart) return;
        gameState.GameEnd();
        photonView.RPC("HasGameEnd",RpcTarget.AllViaServer,SeekerWin);
    }
    [PunRPC]
    void HasGameEnd(bool SeekerWin){
        isGameStart=false;
        Player.LocalPlayerInstance.StopMove(true);
        if(SeekerWin){
            ui.SetNotification(3,0);
        }
        else{
            ui.SetNotification(4,0);
        }
        
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        bool leftplayer=(bool)otherPlayer.CustomProperties["isSeeker"];
        photonView.RPC("HasPlayerDie",RpcTarget.AllViaServer,leftplayer);
        Debug.Log("나간플레이어"+leftplayer);
    }
    
    public void NextStuff(){
        if(isGameStart) return;
        if(isSeeker) return;
        currentPos=stuffs[currentStuff].transform.position;
        currentPos.y+=1;
        stuffs[currentStuff].SetActive(false);
        stuffs[currentStuff].transform.position=new Vector3(0,-20f,0);
        if(++currentStuff>=stuffs.Count) currentStuff=0;
        stuffs[currentStuff].SetActive(true);
        //stuffs[currentStuff].transform.position=currentPos;
        Player.LocalPlayerInstance.SetPlayerPosition(currentPos);
        Player.LocalPlayerInstance.StopMove(false);
    }
    public void PrevStuff(){
        if(isGameStart) return;
        if(isSeeker) return;
        currentPos=stuffs[currentStuff].transform.position;
        currentPos.y+=1;
        stuffs[currentStuff].SetActive(false);
        stuffs[currentStuff].transform.position=new Vector3(0,-20f,0);
        if(--currentStuff<0) currentStuff=stuffs.Count-1;
        stuffs[currentStuff].SetActive(true);
        //stuffs[currentStuff].transform.position=currentPos;
        Player.LocalPlayerInstance.SetPlayerPosition(currentPos);
        Player.LocalPlayerInstance.StopMove(false);
    }
}