using System;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : UIBaseFormMaker
{
    [SerializeField] private UnitButton[] m_spawnButton;
    [SerializeField] private CharacterData[] m_testData;
    private Camera m_camera;
    public Camera mainCamera
    {
        get 
        {
            if(m_camera == null)
                m_camera = Camera.main;

            return m_camera; 
        }
    }

    enum OnClickSettingPanel
    {
        OnClickSettingPanel,
    }

    protected override void Awake()
    {
        base.Awake();
        Bind<OnClickCharacterPaenl>(typeof(OnClickSettingPanel));
    }

    public void SetInGameDataTest()
    {
        Logger.Log("Game Data Test Setting");
        SetCharacterDatas(m_testData);
    }

    public void SetCharacterDatas(CharacterData[] characterDatas)
    {
        for (int characterCount = 0; characterCount < characterDatas.Length;characterCount++)
        {
            if (m_spawnButton.Length <= characterCount)
                break;
            m_spawnButton[characterCount].SetCharater(characterDatas[characterCount]);
        }
    }

    public void OnClickCharacter(InGameCharacterData characterData, Action activeAction = null, Action disableAction = null)
    {
        Get<OnClickCharacterPaenl>(0).OnClickCharacter(characterData, activeAction, disableAction);
    }

    public void ResetCharacterDatas()
    {
       foreach(var buttonItem in m_spawnButton)
        {
            buttonItem.DeleteData();  
        }
    }
}
