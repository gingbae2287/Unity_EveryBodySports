using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class LiarGame : MonoBehaviourPunCallbacks,IPunObservable
{
    //Vector3[] startPoint;

    private static LiarGame instance;
    public static LiarGame Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //==class==
    [SerializeField] LiarUI liarUI;

    //===game setting==
    int maxPlayer=8, minPlayer=4;
    float fallingPoint=-5f;
    string VoteZonePath="Liar/VoteZone";
    Vector3 VoteZonePosition=new Vector3(-9, 0, 9.5f);
    string category,title;
    IEnumerator cor;
    enum GameState{ //room's custom property
        GameEnter,  //0 complete title db loading and set random title
        SetLiar,    //1 mix players order and set liar player => gameready and start count
        GameReady,
        Gaming,     //2 each player explane title 
        VoteTime,   //3 vote time
        OneMoreTurn,    //4 End vote time, notice result
        AnswerTime,
        Waiting,    //5 wait game restart
    }
    
    GameState gameState;
    Hashtable stateHash;
    //===for masterClient====
    int[] playerOrder;
    int currentOrder, timer, sendTime, voteResult, currentPlayers;
    bool isDBLoading, DBLoad, isGameReady, isAnswerTime, isPlaying, isReadExplanation, isVoteEnd;
    public bool isAllPlayerSpawn{get; private set;}
    DatabaseReference DBTitle;
    List<KeyValuePair<string,string>> titleList;
    int[] votePlayer;   //idx: playernumber, value: voted playernumber
    int[] voteCount;    //how many count voted for other players
    GameObject[] voteZones;
    int turnTime=30, voteTime=15, wattingTime=5;
    
    //===player setting

    public int playerNumber{get; private set;}
    public bool isVote;
    public bool isLiar{get; private set;}
    int liarPlayer;
    Vector3 startPoint;
    //====game var======
    WaitForSeconds delay, timerDelay;

    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
        GameManager.Instance.SetThirdPersonView(true);
        maxPlayer=GameManager.Instance.maxPlayerOfLiarGame;
        minPlayer=GameManager.Instance.minPlayerOfLiarGame;
        voteZones=new GameObject[maxPlayer];
        votePlayer=new int[maxPlayer];
        voteCount=new int[maxPlayer];
        playerOrder=new int[maxPlayer];
        titleList= new List<KeyValuePair<string,string>>();
        stateHash=new Hashtable();
        isDBLoading=false; DBLoad=false; isGameReady=false;

        delay=new WaitForSeconds(0.1f);
        timerDelay=new WaitForSeconds(1f);

        for(int i=0;i<maxPlayer;i++) votePlayer[i]=-1;

        InitValue();
        if(PhotonNetwork.IsMasterClient) {
            PhotonNetwork.KeepAliveInBackground=3f;
            ChangeGameState(GameState.GameEnter);
            GetDB();
        }
       
        
    }
    //DB for title
    void GetDB(){
        if(!PhotonNetwork.IsMasterClient) return;
        if(isDBLoading||DBLoad) return;
        isDBLoading=true;
        DBTitle=FirebaseDatabase.DefaultInstance.GetReference("LiarGameTitle");
        DBTitle.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                isDBLoading=false; DBLoad=false;
                Debug.LogError("error GetValueAsync()");
                return;
            }
            else if (task.IsCompleted) {
                DataSnapshot snapshot=task.Result;
                foreach(DataSnapshot category in snapshot.Children){
                    foreach(DataSnapshot title in category.Children){
                        titleList.Add(new KeyValuePair<string, string>
                                (category.Key,(string)title.Key)
                        );
                    }
                }
                DBLoad=true;         
            }
        });
    }
    void InitValue(){
        isVote=false; isLiar=false; isAnswerTime=false; isPlaying=false; isVoteEnd=false;
        liarPlayer=0; currentOrder=0; voteResult=0;
        title=""; category="";
        for(int i=0;i<maxPlayer;i++){
            //votePlayer[i]=-1;
            voteCount[i]=0;
            if(i<PhotonNetwork.PlayerList.Length){
                playerOrder[i]=i;
            }
            else playerOrder[i]=-1;
        }
    }

    void Start(){
        if(PhotonNetwork.IsMasterClient){
            cor=GameStateCheck();
            StartCoroutine(cor);

            CreateVoteZone();
        }
    }
    void ChangeGameState(GameState state){
        gameState=state;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
    }
    void CreateVoteZone(){
        if(!PhotonNetwork.IsMasterClient) return;
        for(int i=0;i<maxPlayer;i++){
            object[] data= new object[1];
            data[0]=i;
            Vector3 zonePos=VoteZonePosition+(new Vector3(6*(i%4),0,(-19)*(i/4)));
            voteZones[i]=PhotonNetwork.InstantiateRoomObject(VoteZonePath,zonePos, Quaternion.identity,0,data);
            voteZones[i].SetActive(false);
        }
    }

    IEnumerator GameStateCheck(){
        //게임 진행
       if(!PhotonNetwork.IsMasterClient) StopCoroutine(cor);

        while(!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gameState")){
                yield return delay;
        }
        
        string str;
        while(true){
            switch(gameState){
            case GameState.GameEnter:
                if(!(isDBLoading||DBLoad)) GetDB();
                while(!DBLoad) yield return delay;
                ChangeGameState(GameState.SetLiar);
            break;
            case GameState.SetLiar:
                if(!DBLoad){
                    ChangeGameState(GameState.GameEnter);
                    break;
                }
                if(isPlaying){
                    timer=turnTime;
                    ChangeGameState(GameState.Gaming);
                    break;
                }
                RandomPlayerNumber();      //set players order and Liar
                while(!CheckAllPlayerReady())  yield return delay;
                GameStart();
                while(!isPlaying) yield return delay;
                
            break;
            case GameState.GameReady:
                //Start Timer
                while(timer>0){
                    str=timer+"초 후 게임이 시작됩니다.";
                    photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
                    yield return timerDelay;
                    timer--;
                }
                str="Game Start!";
                photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
                yield return timerDelay;
                timer=turnTime;
                ChangeGameState(GameState.Gaming);
                
                
            break;
            case GameState.Gaming:
            //각 플레이어가 제시어 설명을 할 때마다 currentorder증가
            //각 플레이어는 제출 버튼시 마스터에게 rpc보내기
                photonView.RPC("ClearHistoryBox",RpcTarget.AllViaServer);
                while(currentOrder<currentPlayers){
                    photonView.RPC("NextPlayerTurn",RpcTarget.AllViaServer,currentOrder);
                    isReadExplanation=false;
                    while(timer>=0 && !isReadExplanation){
                        photonView.RPC("TimerRPC",RpcTarget.AllViaServer,timer);
                        yield return timerDelay;
                        timer--;
                    }
                    timer=turnTime;
                    currentOrder++;
                }
                timer=voteTime;
                ChangeGameState(GameState.VoteTime);
                

            break;
            case GameState.VoteTime:
                if(!isVoteEnd){
                    while(timer>=0){
                        str="라이어로 생각되는 플레이어의 발판에 들어가 투표하세요.\n";
                        str+="남은시간 "+timer+"초";
                        photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
                        yield return timerDelay;
                        timer--;
                    }
                    voteResult=VoteCheck();
                }
                
                if(voteResult==1){  // 미투표자가 과반수 이상. 한바퀴 더 돈다.
                    timer=wattingTime;
                    ChangeGameState(GameState.OneMoreTurn);
                }
                else if (voteResult==2){
                    //라이어가 누군지 맞췄을때
                    timer=15;
                    ChangeGameState(GameState.AnswerTime);
                }
                else if( voteResult==3){
                    //라이어를 못찾았을때
                    timer=wattingTime;
                    ChangeGameState(GameState.Waiting);
                }
            break;
            case GameState.OneMoreTurn:
                photonView.RPC("VoteEnded",RpcTarget.AllBufferedViaServer,0, false);
                while(timer>=0){
                        yield return timerDelay;
                        timer--;
                }
                timer=turnTime;
                ChangeGameState(GameState.Gaming);
                
                
            break;
            case GameState.AnswerTime:
                while(isAnswerTime && timer>=0){
                    photonView.RPC("TimerRPC",RpcTarget.AllViaServer,timer);
                    timer--;
                    yield return timerDelay;
                }
                timer=wattingTime;
                ChangeGameState(GameState.Waiting);

            break;
            case GameState.Waiting:
                while(timer>=0){
                    yield return timerDelay;
                    timer--;
                }
                photonView.RPC("RestartGame",RpcTarget.AllViaServer);

                ChangeGameState(GameState.SetLiar);
            break;
            }
            yield return delay;
        }
        
    }
    // GameState.SetLiar
    void RandomPlayerNumber(){
        //플레이어 순서 섞기
        if(!PhotonNetwork.IsMasterClient) return;
        currentPlayers=PhotonNetwork.PlayerList.Length;
        //int currentPlayers=PhotonNetwork.PlayerList.Length;
        for(int i=0;i<currentPlayers;i++){
            int tmpPlayer=playerOrder[i];
            int randomNum=Random.Range(0,currentPlayers);
            playerOrder[i]=playerOrder[randomNum];
            playerOrder[randomNum]=tmpPlayer;
            
        }
        //라이어 정하기
        liarPlayer=Random.Range(0,currentPlayers);
        for(int i=0;i<currentPlayers;i++){
            Hashtable hash =new Hashtable();
            hash.Add("playerNumber",i);
            if(i==liarPlayer) 
            {
                hash.Add("isLiar", true);
            }
            else hash.Add("isLiar", false);
            PhotonNetwork.PlayerList[playerOrder[i]].SetCustomProperties(hash);

        }
        photonView.RPC("SpawnPlayer", RpcTarget.AllBufferedViaServer,playerOrder,liarPlayer, currentPlayers);
        
    }
    [PunRPC]
    void SpawnPlayer(int[] PlayerOrder, int liar, int CurrentPlayers){
        liarPlayer=liar;
        playerOrder=PlayerOrder;
        currentPlayers=CurrentPlayers;
        //플레이어 스폰 또는 플레이어번호, 라이어 변경
        playerNumber=(int)PhotonNetwork.LocalPlayer.CustomProperties["playerNumber"];
        liarUI.SetMyOrder(playerNumber+1);
        isLiar=(bool)PhotonNetwork.LocalPlayer.CustomProperties["isLiar"];

        if(Player.LocalPlayerInstance!=null) return;
        startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
        PhotonNetwork.Instantiate("Character/TT_male",startPoint,Quaternion.identity);

        Player.LocalPlayerInstance.GameSettingForPlayer(startPoint);
        Player.LocalPlayerInstance.StopMove(false);

        Hashtable hash=new Hashtable();
        hash.Add("isSpawn",true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
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
        if(isPlaying) return;
        int idx=Random.Range(0,titleList.Count);
        category=titleList[idx].Key;
        title=titleList[idx].Value;
        //active votezone
        for(int i=0;i<maxPlayer;i++) voteZones[i].SetActive(false);
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++) voteZones[i].SetActive(true);
        isPlaying=true;
        photonView.RPC("GameStartRPC",RpcTarget.AllViaServer,category,title);
    }
    [PunRPC]
    void GameStartRPC(string Category, string Title){
        category=Category; title=Title;
        liarUI.SetTitleText(Category,Title,isLiar);
        isPlaying=true;
    }

    public void VotePlayer(int PlayerNumber, int VoteNumber){
        //if(!PhotonNetwork.IsMasterClient) return;
        votePlayer[PlayerNumber]=VoteNumber;
    }
    [PunRPC]    //공지메세지 변경
    void AnnouncementRPC(string str){
        liarUI.SetAnnouncement(str);
    }
    // GameState: Gaming
    [PunRPC]
    void NextPlayerTurn(int CurrentOrder){
        liarUI.ClearAnnouncment();
        currentOrder=CurrentOrder;
        if(playerNumber==CurrentOrder){
            string str;
            if(isLiar) {
                str="제시어를 추측하세요.\n";
                str+="라이어임을 들키지 않게 제시어에 관한 말을 하세요.";
            }
            else str="제시어에 관해 하고싶은 말을 제출하세요.";
            liarUI.PlayerTurn(str);
        }
    }
    [PunRPC]
    void TimerRPC(int sec){
        string str;
        if(isAnswerTime){
            if(isLiar){
                if(sec<=0) {
                    SubmitExplanationButton();   //타임아웃시 자동 제출
                    str="시간초과";
                }   
                else str="남은시간: "+sec+"초";
            }
            else{
                str="라이어가 정답을 맞추고 있습니다.\n남은시간: "+sec+"초";
            }
        }
        else{
            if(playerNumber!=currentOrder)
                str=(currentOrder+1)+"번 플레이어 차례입니다.\n남은시간: "+sec+"초";
            else 
            {
                if(sec<=0) {
                    SubmitExplanationButton();   //타임아웃시 자동 제출
                    str="시간초과";
                }
                else str="남은시간: "+sec+"초";
            }
        }
        liarUI.SetAnnouncement(str);
    }
    public void SubmitExplanationButton(){      //답변이나 제시어 설명 제출하기
        string str=liarUI.GetExplanation();
        if(!isAnswerTime){
            photonView.RPC("ReadExplanation", RpcTarget.AllViaServer,playerNumber, str);
            liarUI.ClearAnnouncment();
        }
        else if(isAnswerTime){
            photonView.RPC("CheckAnswer", RpcTarget.AllViaServer,str);

        }
        
    }
    [PunRPC]    //현재 차례의 플레이어가 제출한 설명을 기록 및 마스터에서 확인
    void ReadExplanation(int PlayerNumber, string ex){
        liarUI.AddHistory(PlayerNumber, ex);
        if(PhotonNetwork.IsMasterClient) 
        {  
            isReadExplanation=true;
        }
    }

    int VoteCheck(){
        if(!PhotonNetwork.IsMasterClient) return 0;
        for(int i=0;i<maxPlayer;i++)voteCount[i]=0;
        int maxCount=-1, maxCountPlayer=-1, sameCountPlayer=-1,count=0;
        int currentVotes=PhotonNetwork.PlayerList.Length;
        
        for(int i=0;i<maxPlayer;i++){
            if(votePlayer[i]<0) continue;
            count++;
            voteCount[votePlayer[i]]++;
            if(maxCount<voteCount[votePlayer[i]]){
                maxCount=voteCount[votePlayer[i]];
                maxCountPlayer=votePlayer[i];
                sameCountPlayer=-1;
            }
            else if(maxCount==voteCount[votePlayer[i]])
            {
                sameCountPlayer=votePlayer[i];
            }
        }
        int result;
        string str;
        isVoteEnd=true;
        if(count<(currentVotes/2+currentVotes%2)){
            //게임 한바퀴 더
            currentOrder=0;
            str="투표자 보다 미투표자가 더 많아 잠시후 한바퀴 더 돕니다.";
            photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
            result=1;
            
        }
        
        else if(maxCountPlayer==liarPlayer){
            //라이어 맞춤
            isAnswerTime=true;
            str=string.Format("최다 득표자 {0}번은 라이어였습니다.\n 라이어가 정답을 맞추는 중입니다.",maxCountPlayer+1);
            photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
            photonView.RPC("LiarAnswerTime",RpcTarget.AllViaServer,true);
            result=2;

        }
        else{
            //라이어 틀림
            str=string.Format("최다 득표자 {0}번은 라이어가 아닙니다.\n라이어는 {1}번 입니다. 라이어 승리",maxCountPlayer+1, liarPlayer+1);
            photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
            result=3;
        }
        photonView.RPC("VoteEnded",RpcTarget.AllViaServer,result, true);
        return result;
    }
    [PunRPC]
    void VoteEnded(int VoteResult, bool voteEnd){
        voteResult=VoteResult;
        isVoteEnd=voteEnd;
    }
    [PunRPC]
    void LiarAnswerTime(bool answerTime){
        isAnswerTime=answerTime;
        if(!isAnswerTime) return;
        if(!isLiar) return;
        string str="제시어를 추측해 정답을 맞추세요.";
        liarUI.PlayerTurn(str);
    }
    [PunRPC]
    void CheckAnswer(string Answer){
        if(!isAnswerTime) return;
        isAnswerTime=false;
        isPlaying=false;
        if(!PhotonNetwork.IsMasterClient) return;
        string str;
        if(Answer==title){
            str="라이어가 정답을 맞췄습니다.\n라이어 승리!";
        }
        else{
            str="라이어가 정답을 맞추지 못했습니다.\n라이어 정답: "+Answer;
        }
        photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
    }

    [PunRPC]
    void RestartGame(){
        InitValue();
        liarUI.GetExplanation();
    }

    [PunRPC]
    void ClearHistoryBox(){
        liarUI.ClearHistoryBox();
        currentOrder=0;
        voteResult=0;
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if(PhotonNetwork.IsMasterClient){
            if(isPlaying){
                int num=(int)otherPlayer.CustomProperties["playerNumber"];
                if((bool)otherPlayer.CustomProperties["isLiar"]){
                    StopCoroutine(cor); //stop game state

                    cor=LiarOut();
                    StartCoroutine(cor);
                }
                else if(num==currentOrder){

                }
                
                    //int num=(int)otherPlayer.CustomProperties["playerNumber"];
                voteZones[num-1].SetActive(false);

            }
        }
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        
        gameState=(GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        //voteZones
        foreach(VoteZone script in FindObjectsOfType<VoteZone>(true)){
            int idx=script.playerNumberToVote;
            voteZones[idx]=script.gameObject;
        }
        cor=GameStateCheck();
        StartCoroutine(cor);
    }
    IEnumerator LiarOut(){
        string str="라이어가 게임에서 나가 재시작 합니다.";
        photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
         photonView.RPC("RestartGame",RpcTarget.AllViaServer);
        int i=4;
        while(i>0) {
            i--;
            yield return new WaitForSeconds(1f);
        }
        
       
       
        gameState=GameState.SetLiar;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
        cor=GameStateCheck();
        StartCoroutine(cor);
    }
    public override void OnLeftRoom()
    {
        if(PhotonNetwork.IsMasterClient){
           photonView.RPC("TimerSynchronization", RpcTarget.AllViaServer, timer);
        }
    }

    //IPunObservable
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            if(sendTime!=timer) {
                sendTime=timer;
                stream.SendNext(timer);
            }

        }

        else
        {
            timer = (int)stream.ReceiveNext();
        }
    }
}

