using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Amoebius.Utility.Singleton;
using StarWriter.Core.Audio;
using System;
using UnityEngine.UI;

namespace StarWriter.Core
{
    [DefaultExecutionOrder(0)]
    [RequireComponent(typeof(GameSetting))]
    public class GameManager : SingletonPersistent<GameManager>
    {
               
        [SerializeField]
        private bool isTutorialEnabled = true;

        [SerializeField]
        private bool hasCompletedTutorial = false;

        [SerializeField]
        private bool isGyroEnabled = true;

        private GameSetting gameSettings;

        public bool HasCompletedTutorial { get => hasCompletedTutorial; set => hasCompletedTutorial = value; }

        // Start is called before the first frame update
        void Start()
        {
           if(PlayerPrefs.GetInt("tutorialEnabled") == 0) // 0 false and 1 true
            {
                isTutorialEnabled = false;
            }
                      
        }

        // Update is called once per frame
        void Update()
        {
            if (!isTutorialEnabled) { return; }
            
        }

        public void OnClickTutorialToggleButton()
        {
            // Set gameSettings Tutorial status
            gameSettings.TutorialEnabled = isTutorialEnabled = !isTutorialEnabled;

            // Set PlayerPrefs Tutorial status
            if (isTutorialEnabled == true)
            {
                PlayerPrefs.SetInt("tutorialEnabled", 1);  //tutorial enabled
            }
            if (isTutorialEnabled == false)
            {
                PlayerPrefs.SetInt("tutorialEnabled", 0);  //tutorial disabled
            }
        }

        public void OnClickGyroToggleButton()
        {
            // Set gameSettings Gyro status
            gameSettings.GyroEnabled = isGyroEnabled= !isGyroEnabled;

            // Set PlayerPrefs Gyro status
            if (isGyroEnabled == true)
            {
                PlayerPrefs.SetInt("gyroEnabled", 1); //gyro enabled
            }
            if (isTutorialEnabled == false)
            {
                PlayerPrefs.SetInt("gyroEnabled", 0);  //gyro disabled
            }
        }




        public void OnReturnToMainMenuButtonPressed()
        {
            SceneManager.LoadScene(0);
        }

        public void OnReplayButtonPressed()
        {
            SceneManager.LoadScene(1);
        }

        public void OnResumeButtonPressed()
        {
            TogglePauseGame();
        }

        public void TogglePauseGame()
        {
            PauseSystem.TogglePauseGame();
        }

        public void OnQuitButtonPressed()
        {
            Application.Quit();
        }
    }
}

