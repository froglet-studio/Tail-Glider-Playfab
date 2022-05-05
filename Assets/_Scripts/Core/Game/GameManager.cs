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
        private GameObject tutorialPanel;
        
        [SerializeField]
        private bool isTutorialEnabled = true;

        [SerializeField]
        private bool hasCompletedTutorial = false;

        private GameSetting gameSettings;

        public bool HasCompletedTutorial { get => hasCompletedTutorial; set => hasCompletedTutorial = value; }

        // Start is called before the first frame update
        void Start()
        {
           if(PlayerPrefs.GetInt("Skip Tutorial") == 0) // 0 false and 1 true
            {
                isTutorialEnabled = false;
            }
            else
            { 
                tutorialPanel.SetActive(isTutorialEnabled); 
            }
           
        }

        // Update is called once per frame
        void Update()
        {
            if (!isTutorialEnabled) { return; }
            
        }

        // Toggles the Tutorial Panel on/off 
        public void OnClickTutorialToggleButton()
        {
            gameSettings.TutorialEnabled = isTutorialEnabled = !isTutorialEnabled;
            if(isTutorialEnabled == true)
            {
                PlayerPrefs.SetInt("Skip Tutorial", 1);
            }
            if (isTutorialEnabled == false)
            {
                PlayerPrefs.SetInt("Skip Tutorial", 0);
            }

        }

        public void OnReplayButtonPressed()
        {
            SceneManager.LoadScene(1);
        }
        public void OnQuitButtonPressed()
        {
            Application.Quit();
        }
    }
}

