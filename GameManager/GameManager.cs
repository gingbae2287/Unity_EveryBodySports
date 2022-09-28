using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;

public class GameManager : MonoBehaviourPun {
    private static GameManager instance;
    public static GameManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
    }
    public string[] gameList;
    public int currentGame{get; private set;}
    [SerializeField] GameObject loadingScene;
    [SerializeField] GameObject inGameUI;
    [SerializeField] GameObject settingUI;
    public bool isGaming{get; private set;}
    public bool pause{get; private set;}
    //====GameOption=========
    public bool isThirdPersonView{get; private set;}
    //public bool isCameraMode{get; private set;}
    public float soundValue{get; private set;}
    public float bgmValue{get; private set;}


    //PlayerInfo
    public string nickName{get; private set;}



    //==================
    //===GlassBridge======
    public int maxPlayerOfGlassBridge=12;
    public int minPlayerOfGlassBridge=3;

    //===LiarGame==========

    public int maxPlayerOfLiarGame=8;
    public int minPlayerOfLiarGame=4;




    void Awake(){
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

    }
    void Start() {
        if(loadingScene==null) Debug.LogError("null");
        if(inGameUI==null) Debug.LogError("null");
        if(settingUI==null) Debug.LogError("null");
        loadingScene.SetActive(false);
    }
    public void LeftRoom(){
        
        inGameUI.SetActive(false);
        currentGame=-1;
        isGaming=false;
        //PhotonNetwork.LeaveRoom();
        NetworkManager.Instance.BackToLobby();
    }
    public void SetNickName(string name){
        nickName=name;
    }
    public void GameStart(int idx){
        currentGame=idx;
        inGameUI.SetActive(true);
        loadingScene.SetActive(true);
        isGaming=true;
        if(!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.LoadLevel(gameList[currentGame]);
    }
    //===게임 시점==
    public void SetThirdPersonView(bool isThird){
        isThirdPersonView=isThird;
        if(Player.LocalPlayerInstance!=null) Player.LocalPlayerInstance.SetThirdPersonView(isThirdPersonView);
    }
    public void LogOut(){
        isGaming=false;
        nickName="";
        inGameUI.SetActive(false);
    }
    public void LoadingProgressFinish(){
        loadingScene.SetActive(false);
    }
    public void ChangeBgmValue(float value){
        bgmValue=value;
    }
    public void ChangeSoundValue(float value){
        soundValue=value;
    }
    public void SettingUIActive(){
        settingUI.SetActive(true);
        pause=true;
    }
    public void SettingUIOff(){
        pause=false;
    }
}

