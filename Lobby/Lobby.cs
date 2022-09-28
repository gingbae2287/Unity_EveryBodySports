using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Lobby : MonoBehaviourPunCallbacks {

    [SerializeField] GameObject CustomRoomObject; 
    [SerializeField] GameObject mainLobbyObject;
    [SerializeField] GameObject joinCustomRoomObject;
    [SerializeField] InputField inputFieldRoomID;
    [SerializeField] GameObject errorMessageObject;
    [SerializeField] Button createCutstomRoomButton;
    [SerializeField] Button joinCutstomRoomButton;
    [SerializeField] Button randomMatchingButton;
    [SerializeField] Text statusText;

    bool isJoinRandomRoom;
    IEnumerator cor;
    void Start(){
        if(CustomRoomObject==null) Debug.LogError("CustomRoomObject is null");
        if(mainLobbyObject==null) Debug.LogError("mainLobbyObj is null");
        if(inputFieldRoomID==null) Debug.LogError("inputFieldRoomID is null");
        if(errorMessageObject==null) Debug.LogError("errorMessageObject is null");
        if(createCutstomRoomButton==null) Debug.LogError("createCutstomRoomButton is null");
        if(joinCutstomRoomButton==null) Debug.LogError("joinCutstomRoomButton is null");
        if(randomMatchingButton==null) Debug.LogError("randomMatchingButton is null");
        if(statusText==null) Debug.LogError("statusText is null");
        statusText.text="Connecting to Server...";
        ButtonActive(false);
        
    }

    
    public void RandomMatchingButton(){
        NetworkManager.Instance.RandomRoom();
    }

    public void CreateCustomRoomButton(){
        NetworkManager.Instance.CreateCustomRoom();
    }
    public void JoinCustomRoomButton(){
        if(!NetworkManager.Instance.isConnectedToMasterServer) return;
        joinCustomRoomObject.SetActive(true);
    }
    //---override puncallback
    public override void OnConnectedToMaster()
    {
        ButtonActive(true);
        statusText.text="Connected to Server";
    }
    public override void OnCreatedRoom()
    {
        CustomRoomObject.SetActive(true);
        mainLobbyObject.SetActive(false);
    }
    public override void OnJoinedRoom(){
        CustomRoomObject.SetActive(true);
        mainLobbyObject.SetActive(false);
        joinCustomRoomObject.SetActive(false);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        NetworkManager.Instance.CreateCustomRoom();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
            NetworkManager.Instance.infoMessage="방이 존재하지 않습니다. ";
            errorMessageObject.SetActive(true);
    }


    public void CheckCustomRoomID(){
        if(NetworkManager.Instance.isConnectedToRoom) return;
        //int roomID=int.Parse(inputFieldRoomID.text);
        string roomID=inputFieldRoomID.text;
        //NetworkManager.Instance.JoinCustomRoom(roomID);
        PhotonNetwork.JoinRoom(roomID.ToString());
    }
    


    public void BackButton(){
        NetworkManager.Instance.BackToLobby();
        joinCustomRoomObject.SetActive(false);
    }

    void ButtonActive(bool act){
        createCutstomRoomButton.interactable=act;
        joinCutstomRoomButton.interactable=act;
        randomMatchingButton.interactable=act;
    }


}