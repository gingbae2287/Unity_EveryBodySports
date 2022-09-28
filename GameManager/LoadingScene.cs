using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class LoadingScene : MonoBehaviourPun {
    [SerializeField] Image loadingBar;
    IEnumerator cor;
    bool isSceneLoading;

    private void OnEnable() {
        isSceneLoading=false;
        cor=LoadingProgress();
        StartCoroutine(cor);
    }
    private void Start() {
        if(loadingBar==null) Debug.LogError("null");
    }

    IEnumerator LoadingProgress(){
        while(!isSceneLoading){
            loadingBar.fillAmount=PhotonNetwork.LevelLoadingProgress;
            if(loadingBar.fillAmount==1f) {
                isSceneLoading=true;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        StopCoroutine(cor);
        //gameObject.SetActive(false);
    }
}