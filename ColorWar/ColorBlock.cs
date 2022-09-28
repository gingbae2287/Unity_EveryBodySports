using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ColorBlock : MonoBehaviourPun {
    int teamNumber;
    public int blockIdx{get; private set;}
    //bool isPlayerOn;
    MeshRenderer render;
    Color[] teamColors={
        new Color32(226,211,48,255),
        new Color32(49,95,243,255),
        new Color32(52,224,86,255),
        new Color32(255,57,57,255)
    };
    Material[] colorMat=new Material[4];
    Material[] mat;
    //material 0:yellow 1:blue 2:green 3:red
    private void Awake() {
        render=GetComponent<MeshRenderer>();
        mat=render.materials;
        LoadMaterials();
    }
    private void Start() {
        //photonView.RPC("ChangeColor", RpcTarget.AllViaServer,teamNumber);
        //object[] data=photonView.InstantiationData;
        //teamNumber=(int)data[0];
        //blockIdx=(int)data[1];
        //mat[0]=colorMat[teamNumber];
        teamNumber=0;
        render.materials=mat;
        
    }

    void LoadMaterials(){
        colorMat[0]=Resources.Load<Material>("ColorWar/ColorMaterials/Yellow");
        colorMat[1]=Resources.Load<Material>("ColorWar/ColorMaterials/Blue");
        colorMat[2]=Resources.Load<Material>("ColorWar/ColorMaterials/Green");
        colorMat[3]=Resources.Load<Material>("ColorWar/ColorMaterials/Red");
        if(colorMat[0]==null) Debug.LogError("colorMat[0] is null");
        if(colorMat[1]==null) Debug.LogError("colorMat[1] is null");
        if(colorMat[2]==null) Debug.LogError("colorMat[2] is null");
        if(colorMat[3]==null) Debug.LogError("colorMat[3] is null");
    }


    private void OnTriggerEnter(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
        photonView.RPC("ChangeColor", RpcTarget.AllViaServer,ColorWar.Instance.myTeam);
        ColorWar.Instance.ColorChangeSound();
    }
    /*private void OnTriggerExit(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
    }*/
    [PunRPC]
    void ChangeColor(int TeamNumber){
        if(PhotonNetwork.IsMasterClient){
            if(teamNumber!=0)ColorWar.Instance.GetScore(teamNumber,-1);
            ColorWar.Instance.GetScore(TeamNumber);
        }
        teamNumber=TeamNumber;
        mat[0]=colorMat[teamNumber];
        render.materials=mat;
        
    }
}