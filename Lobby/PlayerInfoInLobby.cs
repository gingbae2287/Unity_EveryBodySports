using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoInLobby : MonoBehaviour {
    string playerName;
    bool isMaster;
    [SerializeField]GameObject masterIcon;
    [SerializeField] Text info;
    void Start(){
        if(info==null) Debug.LogError("playerinfo text is null");
        if(masterIcon==null) Debug.LogError("null");
    }

    public void SetPlayerInfo(string Name, bool IsMaster){
        playerName=Name;
        isMaster=IsMaster;
        masterIcon.SetActive(isMaster);
        info.text=string.Format("{0}", playerName);
    }
    public void ClearInfo(){
        
    }
}