using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInGame : MonoBehaviour {

    [SerializeField]GameObject GoalTextObj;
    //[SerializeField] Text[] nameOfPlayerInRanking;

    void Awake(){
        //nameOfPlayerInRanking= new Text[3];
        
    }
    void Start(){
        if(GoalTextObj==null){
            Debug.LogError("miss GoaltextObj");
        }
    
        

        /*
        if(nameOfPlayerInRanking[0]==null) Debug.LogError("nameOfPlayerInRanking");
        if(nameOfPlayerInRanking[1]==null) Debug.LogError("nameOfPlayerInRanking");
        if(nameOfPlayerInRanking[2]==null) Debug.LogError("nameOfPlayerInRanking");
        InitRankingUI();
        */
    }
    public void LobbyButton(){
        GameManager.Instance.LeftRoom();
    }

}
