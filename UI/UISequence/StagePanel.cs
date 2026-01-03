using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StagePanel : UIBase
{
    enum RectTransforms
    {
        Content
    }

    protected override void Awake()
    {
        m_UISequence = UIManager.UISequence.StageSeletePanel;
        Init();
    }

    public void Init()
    {
        Bind<RectTransform>(typeof(RectTransforms));

        var rect = Get<RectTransform>(0);
        var baseprefab = GetComponentInChildren<StageMoveButton>(true);

        TaskHelp().Forget();
        async UniTask TaskHelp()
        {
            var textAsset = await AddressableManager.Instance.LoadAssetAndCacheAsync<TextAsset>("StageList");
            StageData stage = JsonConvert.DeserializeObject<StageData>(textAsset.text);

            for(int mainStage =0; mainStage < stage.MainStageCount; mainStage++)
            {
                for(int subStage = 0;subStage < stage.SubStages[mainStage].SubStageCount; subStage++)
                {
                    var obje = Instantiate(baseprefab, rect);
                    obje.Init(mainStage, subStage+1);
                    obje.gameObject.SetActive(true);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Make Json")]
    private void MakeJson()
    {
        StageData stagedata = new();

        stagedata.SubStages = new();

        stagedata.MainStageCount = 1;
        stagedata.SubStages.Add(new() { MainStage = 0, SubStageCount = 1});

        Logger.Log(JsonConvert.SerializeObject(stagedata));
    }
#endif
}

[Serializable]
public class StageData
{
    public int MainStageCount = 0;

    public List<SubStageData> SubStages = new();

    public void AddStageData(int mainStage)
    {
        if (SubStages.Any(x => x.MainStage == mainStage))
            SubStages.Find(x => x.MainStage == mainStage).SubStageCount++;
        else
        {
            MainStageCount++;
            SubStages.Add(new() { MainStage = mainStage, SubStageCount = 1 });
        }
    }
}
[Serializable]
public class SubStageData
{
    public int MainStage;
    public int SubStageCount;
}