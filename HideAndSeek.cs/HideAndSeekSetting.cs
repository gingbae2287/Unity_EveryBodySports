using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HideAndSeekSetting : MonoBehaviourPun
{
    private void Start() {
        InitSetting();
    }
    public void InitSetting(){
        GameManager.Instance.SetThirdPersonView(true);
    }
}