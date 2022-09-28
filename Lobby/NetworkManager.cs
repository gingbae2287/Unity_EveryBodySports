using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager instance;
    public static NetworkManager Instance{
        get{
            if(instance==null){
                return null;
            }
            return instance;
        }
    }

    public string gameVersion="1.0";
    byte maxPlayers=10;

    //Current State
    public bool isConnectedToMasterServer{get; private set;}
    public bool isConnectedToLobby{get; private set;}
    public bool isConnectedToRoom{get; private set;}

    bool isConnecting;  
    bool isLogin;
    //connecting to any state (master server, lobby, room)
    //while the value is true, can't connect any state.

    //bool[] playerNumber;

    //====Coroutine====
    IEnumerator cor;

    ///----Lobby Setting=======
    //TypedLobby CustomLobby=new TypedLobby("Custom",LobbyType.Default);
    Hashtable roomCP;
    string roomID="RoomID";
    public string infoMessage="";
    RoomOptions roomOption;


    void Awake(){
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
        isLogin=false;

        //--------------------
        roomOption=new RoomOptions();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Start()
    {
        //====state Init====
    }
    public void Login(){
        StateInit();
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }


    void StateInit(){
        isConnecting=false;
        isConnectedToMasterServer=false;
        isConnectedToLobby=false;
        isConnectedToRoom=false;
    }
    public void RandomRoom(){
        if(isConnecting) return;
        if(isConnectedToRoom) return;
        isConnecting=true;
        roomOption.MaxPlayers=8;
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: this.roomOption);
    }

    public void BackToLobby(){
        if(isConnecting) return;
        if(!PhotonNetwork.InRoom) return;
        
        if(isConnectedToRoom) 
        {
            isConnecting=true;
            PhotonNetwork.LeaveRoom();
        }
        
    }

    public void CreateCustomRoom(){
        
        if(isConnecting) return;
        if(isConnectedToRoom) return;
        isConnecting=true;
        int RandomRoomID=Random.Range(1000,10000);
        roomOption.MaxPlayers=8;
        roomOption.CustomRoomProperties=new Hashtable(){{roomID,RandomRoomID}};
        PhotonNetwork.CreateRoom(RandomRoomID.ToString(), roomOption);
    }

    public void JoinCustomRoom(int RoomID){
        //roomCP=new Hashtable(){{roomID, RoomID}};
        PhotonNetwork.JoinRoom(RoomID.ToString());
    }
    public void DisconnectServer(){
        if(PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        isLogin=false;
        PhotonNetwork.Disconnect();
    }

//=======CallBack==========
    
    public override void OnConnectedToMaster()
    {
        StateInit();
        //임시 닉네임
        if(!isLogin){
            isLogin=true;
            PhotonNetwork.LocalPlayer.NickName=AuthManager.Instance.userName;
        }
        isConnecting=false;
        isConnectedToMasterServer=true;
        Debug.Log("Success Connecting to Master Server. nickname: "+PhotonNetwork.LocalPlayer.NickName);
        
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if(!AuthManager.Instance.isLogin) {
            SceneManager.LoadScene("Login");
            return;
        }

        isConnectedToMasterServer=false;
        isConnecting=true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

    }
    public override void OnJoinedRoom(){
        isConnecting=false;
        isConnectedToRoom=true;
    }
    public override void OnLeftRoom()
    {
        //마스터 서버로 다시 연결
        isConnectedToRoom=false;
        SceneManager.LoadScene("Lobby");
    }


    public int GetRoomID(){
        if(!PhotonNetwork.InRoom) return 0;
        roomCP=PhotonNetwork.CurrentRoom.CustomProperties;
        return (int)roomCP[roomID];

    }

}
