using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GoalZone : MonoBehaviourPun
{
    private void Start() {
    
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag=="Player"){
            Player GoalPlayer=other.gameObject.GetComponent<Player>();
            if(!GoalPlayer.photonView.IsMine) return;
            //if(GoalPlayer.gameEnd) return;
            //GoalPlayer.GameEnd();
            photonView.RPC("GoalInCallToMaster", RpcTarget.MasterClient, GoalPlayer.playerName);
        }
    }

    [PunRPC]
    void GoalInCallToMaster(string PlayerName){
        GlassBridgeGame.Instance.GoalInPlayer(PlayerName);
    }

    
}