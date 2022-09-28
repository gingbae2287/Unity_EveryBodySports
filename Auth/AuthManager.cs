using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour{
    //  singletone  -----////
    private static AuthManager instance;
    public static AuthManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
        
    }


    //------              ////
    public bool IsFirebaseReady{get; private set;}
    public bool IsSignInOnProgress {get;private set;}       //로그인 버튼 더블탭 방지

    [Header("Objects")]
    //public InputField emailField;
   // public InputField pswdField;
    //public Button BtSignIn;
    //public Button BtSignUp;
    [SerializeField] Button[] loginButtons;

    [Header("UI Object")]
    [SerializeField] GameObject UILogin;
    //[SerializeField] GameObject UI_SignIn;
    //[SerializeField] GameObject Ui_SignUp;
    [SerializeField] GameObject SetUserNameObj;

//========Firebase==============
    public FirebaseApp firebaseApp{get; private set;}
    public FirebaseAuth firebaseAuth{get; private set;}
    public DatabaseReference DBref{get; private set;}
    public FirebaseUser user{get; private set;}


    

    ///====var====
    public string userName{get; private set;}
    public string userId{get; private set;}
    public bool isGuest{get; private set;}
    public bool isLogin{get; private set;}
    public bool isConnecting;

    void Awake(){
        //singletone-------------///
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
        //------------------------
        //BtSignIn.interactable=false;

        
        FirebaseInitSetting();

    }
    void FirebaseInitSetting(){
        // firebase unity 연동 메뉴얼 https://firebase.google.com/docs/unity/setup?hl=ko
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>{
            var dependencyStatus = task.Result;
            if(dependencyStatus ==DependencyStatus.Available){
                
                firebaseApp = FirebaseApp.DefaultInstance;
                firebaseAuth=FirebaseAuth.DefaultInstance;
                DBref=FirebaseDatabase.DefaultInstance.RootReference;
                IsFirebaseReady=true;
                foreach(Button button in loginButtons) button.interactable=true;

            }
            else{
                IsFirebaseReady=false;
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
            
        })  ;
    }
    public void FirebaseConnect(string authCode){
        Firebase.Auth.Credential credential =
            Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
        firebaseAuth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            isConnecting=false;
            if (task.IsCanceled) {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }
            //로그인 성공
            user = task.Result;
            isLogin=true;
            CheckUserName();
        });
    }
    public void LogOut(){
        firebaseAuth.SignOut();
        isLogin=false;
        isGuest=false;
        userName="";
        userId="";
        isConnecting=false;
        //SceneManager.LoadScene("Login");
        GameManager.Instance.LogOut();
        NetworkManager.Instance.DisconnectServer();
        GooglePlayManager.Instance.GooglePlayLogout();
        
    }

    
    //Check UserName
    void CheckUserName(){
        DatabaseReference usersRef=FirebaseDatabase.DefaultInstance.GetReference("users");
        usersRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("error GetValueAsync()");
                return;
            }
            else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                userId=user.UserId;
                if(snapshot.HasChild(userId)){
                    userName=snapshot.Child(userId).Child("UserName").Value.ToString();
                    if(userName==""){
                        SetUserNameUI();
                    }
                    else{
                        //WriteByUserName(userName);
                        GoLobbyScene();
                    }
                }
                else{
                    SetUserNameUI();
                    WriteNewUser(userId,null);
                }
            }
        });

        
    }
    public void GuestLogin(){
        /*if(isConnecting) return;
        isConnecting=true;
        firebaseAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            isConnecting=false;
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            isGuest=true;
            isLogin=true;
            //user = task.Result;
            userName="guest"+Random.Range(0,10000);
            GoLobbyScene();
        });*/
        isGuest=true;
        isLogin=true;
        userName="guest"+Random.Range(0,10000);
        GoLobbyScene();
        isConnecting=false;
    }
    
    void SetUserNameUI(){
        UILogin.SetActive(false);
        SetUserNameObj.SetActive(true);
    }
    
    public bool SetUserName(string Name){
        bool isUserNameSet=false;
        DatabaseReference userNameRef=FirebaseDatabase.DefaultInstance.GetReference("userName");
        userNameRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("error GetValueAsync()");
                return;
            }
            else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if(snapshot.HasChild(Name)){
                   Debug.Log("이미 사용중인 이름입니다.");

                }
                else{
                    isUserNameSet=true;
                    userName=Name;
                    //WriteByUserName(Name);
                    WriteNewUser(user.UserId,Name);
                    GoLobbyScene();
                }
            }
        });
        return isUserNameSet;
    }

    public void GoLobbyScene(){
        NetworkManager.Instance.Login();
        SceneManager.LoadScene("Lobby");
    }

    void WriteNewUser(string userId, string name) {
        User user = new User(name);
        string json = JsonUtility.ToJson(user);
        DBref.Child("users").Child(userId).SetRawJsonValueAsync(json);
        DBref.Child("userName").Child(userName).Child("UID").SetValueAsync(userId);
    }
    //데이터 색인
    /*void WriteByUserName(string userName){
        User user = new User(name);
        string json = JsonUtility.ToJson(user);
        DBref.Child("userName").SetRawJsonValueAsync(json);
    }*/

    
}

public class User {
    public string UserName;
    //public string Email;

    public User() {
    }

    public User(string username) {
        this.UserName = username;
    }
}
