using UnityEngine;
using TMPro;
using StarWriter.Core;
using static StarWriter.Core.GameSetting;

public class ScoringManager : MonoBehaviour
{
    
    [SerializeField]
    private int extendedLifeScore;
    [SerializeField]
    private int extendedLifeHighScore;
    [SerializeField]
    bool bedazzled; // whether or not the watchAdButton is amped up

    private static int score = 0;
    private static bool firstLife = true;
    private static float extralifeModifier = 0.8f;

    public TextMeshProUGUI scoreText;

    public bool FirstLife { get => firstLife; set => firstLife = value; }

    private void OnEnable()
    {
        GameManager.onDeath += OnDeath;
        GameManager.onExtendGamePlay += ExtendGamePlay;
        MutonPopUp.AddToScore += AddMutonBonus;
        //AdvertisementMenu.onDeclineAd += OnDeclineAd;
    }

    private void OnDisable()
    {
        GameManager.onDeath -= OnDeath;
        MutonPopUp.AddToScore -= AddMutonBonus;
        GameManager.onExtendGamePlay -= ExtendGamePlay;
        //AdvertisementMenu.onDeclineAd -= OnDeclineAd;
    }

    public void UpdateScoreBoard(int value)
    {
        scoreText.text = value.ToString("D3"); // score text located on the fuel bar
    }

    private void AddMutonBonus(string uuid, int amount)
    {
        if (uuid == "admin") { score += amount; }

        UpdateScoreBoard(score);
    }

    private void OnDeath()
    {
        bool advertisements = firstLife;//(PlayerPrefs.GetInt(GameSetting.PlayerPrefKeys.adsEnabled.ToString()) == 1);
        if (advertisements)
            bedazzled = ((PlayerPrefs.GetInt(PlayerPrefKeys.firstLifeHighScore.ToString()) * extralifeModifier) <= score);  //Sets beddazed value
        else
            bedazzled = ((PlayerPrefs.GetInt("High Score")) <= score);   

        UpdatePlayerPrefScores();

        // reset first life if this is now the second life
        // hmm. this feels weird
        if (!firstLife)
            firstLife = true;
    }

    public static bool IsScoreBedazzleWorthy
    {
        get => firstLife ?
            (PlayerPrefs.GetInt(PlayerPrefKeys.firstLifeHighScore.ToString()) * extralifeModifier) <= score :
            (PlayerPrefs.GetInt(PlayerPrefKeys.highScore.ToString())) <= score;
    }

    public void UpdatePlayerPrefScores()
    {
        PlayerPrefs.SetInt("Score", score);

        //Compares Score to High Score and saves the highest value
        if (PlayerPrefs.GetInt(PlayerPrefKeys.highScore.ToString()) < score)
        {
            PlayerPrefs.SetInt(PlayerPrefKeys.highScore.ToString(), score);
        }
        if (firstLife)
        {
            if (PlayerPrefs.GetInt(PlayerPrefKeys.firstLifeHighScore.ToString()) < score)
            {
                PlayerPrefs.SetInt(PlayerPrefKeys.firstLifeHighScore.ToString(), score);
            }
        }
        PlayerPrefs.Save();
    }

    private void ExtendGamePlay()
    {
        Debug.Log("ScoringManager.ExtendGamePlay");
        firstLife = false;

        //PlayerPrefs.SetInt(GameSetting.PlayerPrefKeys.adsEnabled.ToString(), 0);  // set false 
        //PlayerPrefs.Save();
    }
}