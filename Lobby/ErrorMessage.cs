using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour {
    [SerializeField] Text message;
    void Start(){
        if(message==null) Debug.LogError("error message text is null");
        message.text=NetworkManager.Instance.infoMessage;
    }

    public void SetErrorMessage(string text){
        message.text=text;
    }
    public void OKButton(){
        gameObject.SetActive(false);
    }
}