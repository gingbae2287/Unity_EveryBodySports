using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour{


    public void ButtonGuestLogin(){
        AuthManager.Instance.GuestLogin();
    }
    public void ButtonGooglePlayLogin(){
        GooglePlayManager.Instance.GooglePlayLogin();
    }
}