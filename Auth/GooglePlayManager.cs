using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class GooglePlayManager : MonoBehaviour{
    public Text test;
    private static GooglePlayManager instance;
    public static GooglePlayManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
        
    }
    ILeaderboard lb;

    string authCode;
    bool isGoogleLogin;
    private void Awake() {
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        GooglePlayInitSetting();
        lb=PlayGamesPlatform.Instance.CreateLeaderboard();
    }

    void GooglePlayInitSetting(){
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .RequestServerAuthCode(false )
        .Build();
        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();
    }

    public void GooglePlayLogin(){
        if(AuthManager.Instance.isConnecting) return;
        if(isGoogleLogin) return;
        AuthManager.Instance.isConnecting=true;
        Social.localUser.Authenticate((bool success) => {
            if (success) {
                isGoogleLogin=true;
                authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                //testText.text=Social.localUser.userName+"  "+Social.localUser.id;
                AuthManager.Instance.FirebaseConnect(authCode);
            }
            else{
                AuthManager.Instance.isConnecting=false;
            }
        });
    }
    public void GooglePlayLogout(){
        PlayGamesPlatform.Instance.SignOut();
        isGoogleLogin=false;
    }
    public void ShowLeaderBoard(){
        Social.ShowLeaderboardUI();

    }
    public void CoinCoinWin(){
        if(!isGoogleLogin) {
            return;
        }
        int myScore=0;
        Social.LoadScores(GPGSIds.leaderboard_coincoin_win, scores => {
            if (scores.Length > 0)
            {
                foreach (IScore score in scores)
                    //myScore=int.Parse(score.formattedValue);
                    myScore=int.Parse(score.formattedValue);
                    //myScore=(int)score.value;
                    
            }
            else{

            }
            myScore+=1;
            Social.ReportScore(myScore, GPGSIds.leaderboard_coincoin_win, (bool success) => {
            });
        });
        myScore+=1;
        Social.ReportScore(myScore, GPGSIds.leaderboard_coincoin_win, (bool success) => {});
    }
    public void CoinCoinScore(int score){
        if(!isGoogleLogin) {
            return;
        }
        Social.ReportScore(score, GPGSIds.leaderboard_coincoin_score, (bool success) => {
        });
    }
    public void ColorWarWin(){
        //Social.ShowLeaderboardUI();
        if(!isGoogleLogin) {
            return;
        }
        int myScore=0;
        Social.LoadScores(GPGSIds.leaderboard_colorwar_win, scores => {
            if (scores.Length > 0)
            {
                foreach (IScore score in scores)
                    //myScore=int.Parse(score.formattedValue);
                    myScore=int.Parse(score.formattedValue);
                    myScore+=1;
                    Social.ReportScore(myScore, GPGSIds.leaderboard_colorwar_win, (bool success) => {});
                    //myScore=(int)score.value;
                    
            }
            else{
                myScore+=1;
                Social.ReportScore(myScore, GPGSIds.leaderboard_colorwar_win, (bool success) => {});
            }
            
        });
        
    }
}