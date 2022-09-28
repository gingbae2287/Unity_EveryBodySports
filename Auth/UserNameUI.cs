using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserNameUI : MonoBehaviour{
    [SerializeField] InputField userName;

    public void CheckUserName(){
        
        if(AuthManager.Instance.SetUserName(userName.text)){
        }
        else {
            //중복확인 및 설정실패
            Debug.Log("닉네임 설정 실패");
        }
    }
}