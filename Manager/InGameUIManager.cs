using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : UIBaseFormMaker
{
    [SerializeField] private UnitButton m_unitButtonBase;
    [SerializeField] private TextMeshProUGUI m_costText;
    private List<UnitButton> m_spawnButton = new();

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

        //최초 스폰 버튼 초기화
        m_spawnButton.Add(m_unitButtonBase);
        m_updateCostAction += m_unitButtonBase.UpdateCostAction;
    }

    public void SetInGameDataTest()
    {
        Logger.Log("Game Data Test Setting");

        System.Collections.Generic.List<CharacterData> testdatas = new()
        {
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(1),
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(2),
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(3),
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(4)
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
        for (int characterCount = 0; characterCount < GameData.MAX_SETTING_CHARACTERCOUNT; characterCount++)
        {
            if (characterCount >= characterDatas.Length)
            {
                break;
            }
            if (m_spawnButton.Count <= characterCount)
            {
                m_spawnButton.Add(Instantiate(m_unitButtonBase, m_unitButtonBase.transform.parent));
                m_updateCostAction += m_spawnButton[characterCount].UpdateCostAction;
            }

            m_spawnButton[characterCount].SetCharater(characterDatas[characterCount], this);
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
