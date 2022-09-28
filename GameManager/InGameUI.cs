using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InGameUI : MonoBehaviour {
    [SerializeField] GameObject settingUI;
    [SerializeField] GameObject padObj;
    [SerializeField] GameObject pad;
    
    void Start(){
        if(settingUI==null) Debug.LogError("null");
    }
    public void ButtonSettingUI(){
        GameManager.Instance.SettingUIActive();
    }
    

    
}