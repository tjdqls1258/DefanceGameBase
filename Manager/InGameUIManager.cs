using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : UIBaseFormMaker
{
    [SerializeField] private TextMeshProUGUI m_costText;
    [SerializeField] private UnitButton[] m_spawnButton;

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

    public InGameManager m_inGameManager { get; private set; }

    private Action<int> m_updateCostAction = null;

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

        System.Collections.Generic.List<CharacterData> testdatas = new()
        {
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(1),
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(2)
        };

        SetCharacterDatas(testdatas.ToArray());
        m_inGameManager = FindAnyObjectByType<InGameManager>();
        m_inGameManager.SetChargeAction(ChargeText);
        m_inGameManager.StartGame();

        ChargeText(m_inGameManager.currentCost);

        void ChargeText(int currentCost)
        {
            m_costText.text = currentCost.ToString();
            if (m_updateCostAction != null)
                m_updateCostAction.Invoke(currentCost);
        }
    }

    public void SetCharacterDatas(CharacterData[] characterDatas)
    {
        for (int characterCount = 0; characterCount < characterDatas.Length;characterCount++)
        {
            if (m_spawnButton.Length <= characterCount)
                break;
            m_spawnButton[characterCount].SetCharater(characterDatas[characterCount], this);
            m_updateCostAction += m_spawnButton[characterCount].UpdateCostAction;
        }
    }

    public void OnClickCharacter(InGameCharacterData characterData, Action activeAction = null, Action disableAction = null)
    {
        Get<OnClickCharacterPaenl>(0).OnClickCharacter(characterData, activeAction, disableAction);
    }

    public void ExitGame()
    {
        m_costText.text = "0";
        m_inGameManager.ExitGame();
        ResetCharacterDatas();

        m_inGameManager = null;
    }

    private void ResetCharacterDatas()
    {
       foreach(var buttonItem in m_spawnButton)
        {
            buttonItem.DeleteData();  
        }
    }
}
