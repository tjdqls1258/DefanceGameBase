#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UIMakerTool : MonoBehaviour
{
    [SerializeField] CanvasGroup m_commandCanvas;
    [SerializeField] CanvasGroup m_mainCanvas;
    [SerializeField] CanvasGroup m_inGameCanvas;

    [SerializeField] UIScriptableData m_uiData;

    [ContextMenu("UI 생성")]
    public void UpdateUI()
    {
        m_uiData.MakeUIList(m_commandCanvas.transform, m_mainCanvas.transform, m_inGameCanvas.transform).Forget();
    }

    public void MakeUI()
    {
        m_uiData.MakeJson();
    }

    public void LoadData()
    {
        m_uiData.LoadJson();
    }

    [ContextMenu("현재 상황 업데이트")]
    public void MakeJson()
    {
        List<UIBaseFormMaker> commandUIlist = new();
        List<UIBaseFormMaker> mainUIList = new();
        List<UIBaseFormMaker> inGameList = new();

        commandUIlist = m_commandCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();
        mainUIList = m_mainCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();
        inGameList = m_inGameCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();

        List<UIBaseData> uiBaseDataList = new();

        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.Command, commandUIlist));
        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.MainUI, mainUIList));
        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.InGameUI, inGameList));

        if(uiBaseDataList.Count <= 0)
        {
            Debug.LogError("UI가 설정되지 않았습니다.");
            return;
        }

        JsonSerializerSettings settingJson = new();
        settingJson.Formatting = Formatting.Indented;
        settingJson.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        Debug.Log(JsonConvert.SerializeObject(uiBaseDataList, settingJson));
        m_uiData.SettingData(uiBaseDataList);

        AssetDatabase.SaveAssetIfDirty(m_uiData);
        AssetDatabase.SaveAssets();

        List<UIBaseData> DataSetting(UIBaseData.UIType uiType, List<UIBaseFormMaker> uiBase)
        {
            List<UIBaseData> dataList = new();
            foreach (UIBaseFormMaker commandUI in uiBase)
            {
                UIBaseData data = new();

                data.dataName = commandUI.name.Split("_")[0];
                data.uiType = uiType;
                data.SettingAnchorPos(commandUI.MyRT.anchoredPosition);
                data.SettingSizeDetail(commandUI.MyRT.sizeDelta);
                data.SettingAnchorMinMax(commandUI.MyRT.anchorMin, commandUI.MyRT.anchorMax);
                data.SettingPivot(commandUI.MyRT.pivot);

                dataList.Add(data);
            }

            return dataList;
        }
        MakeUI();
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {

        DeleteAll(m_commandCanvas.transform);
        DeleteAll(m_mainCanvas.transform);
        DeleteAll(m_inGameCanvas.transform);

        void DeleteAll(Transform tr)
        {
            for (int i = tr.childCount-1; i >= 0; i--)
            {
                DestroyImmediate(tr.GetChild(i).gameObject);
            }
        }
    }

    public void ShowUI(UIBaseData.UIType type)
    {
        switch (type)
        {
            case UIBaseData.UIType.Command:
                m_commandCanvas.alpha = 1;
                m_mainCanvas.alpha = 0;
                m_inGameCanvas.alpha = 0;
                break;
            case UIBaseData.UIType.MainUI:
                m_commandCanvas.alpha = 0;
                m_mainCanvas.alpha = 1;
                m_inGameCanvas.alpha = 0;
                break;
            case UIBaseData.UIType.InGameUI:
                m_commandCanvas.alpha = 0;
                m_mainCanvas.alpha = 0;
                m_inGameCanvas.alpha = 1;
                break;
        }
    }
}

[CustomEditor(typeof(UIMakerTool))]
public class UIMakerToolEditor : Editor
{
    UIMakerTool targetObject;
    private void Awake()
    {
        targetObject = (UIMakerTool)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("업데이트 UI"))
        {
            targetObject.MakeJson();
        }
        if(GUILayout.Button("UI 생성"))
        {
            targetObject.LoadData();
            targetObject.UpdateUI();
        }
        if(GUILayout.Button("UI 제거"))
        {
            targetObject.ClearAll();
        }

        GUILayout.Space(5);

        if(GUILayout.Button("공용 UI 보기"))
            targetObject.ShowUI(UIBaseData.UIType.Command);
        if (GUILayout.Button("메인 UI 보기"))
            targetObject.ShowUI(UIBaseData.UIType.MainUI);
        if (GUILayout.Button("게임 UI 보기"))
            targetObject.ShowUI(UIBaseData.UIType.InGameUI);
    }
}
#endif