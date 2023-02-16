using UnityEngine;
using StarWriter.Core;
using StarWriter.Core.HangerBuilder;

public class MainMenuActions : MonoBehaviour
{
    public void GameManagerOnClickTutorialButton()
    {
        GameManager.Instance.OnClickTutorialButton();
    }
    public void GameSettingChangeAudioEnabledSetting()
    {
        GameSetting.Instance.ChangeAudioEnabledStatus();
    }
    public void GameSettingInvertYEnabledStatus()
    {
        GameSetting.Instance.ChangeInvertYEnabledStatus();
    }
}