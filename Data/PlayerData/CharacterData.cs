using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


public class InGameCharacterData
{
    public InGameCharacterData(CharacterData data)
    {
        characterData = data;
        upgradeCount = 0;
    }

    public CharacterData characterData;
    public int upgradeCount;
}

public class CharacterData : CSVData
{
    public int id;
    public int cost;
    public int rating;
    public string characterName;
    public float maxHp = 1;
    public int atkPower;
    public int defPower;
    public int atkSpeed;
    public float maxMp = 1;

    public string modelObjectName;
    public string modelSpriteName;

    private Sprite modelSprite;

    public MpCharacterState characterState;
    private Sprite characterSprite;

    private bool m_setCharacterSprite = false;

    public void SetCharacterState()
    {
        characterState = new()
        {
            maxHp = maxHp,
            atkPower = atkPower,
            atkSpeed = atkSpeed,
            maxMp = maxMp,
            defPower = defPower
        };
    }

    public override int GetID()
    {
        return id;
    }

    public async UniTask<Sprite> GetSpriteAsync()
    {
        if (modelSprite == null)
        {
            await SetSprite();
        }

        return modelSprite;
    }

    public async UniTask<GameObject> GetModleObject()
    {
        return await AddressableManager.Instance.LoadAssetAndCacheAsync<GameObject>(modelObjectName);
    }

    public Sprite GetCharacterSprite()
    {
        if(characterSprite != null)
            return characterSprite;

        return null;
    }

    public async UniTask SetSprite()
    {
        if (m_setCharacterSprite) return;

        m_setCharacterSprite = true;
        Texture2D texture = await AddressableManager.Instance.LoadAssetAndCacheAsync<Texture2D>(string.Format(Util.CHARACTER_SPRITE_PATH, modelSpriteName));
        characterSprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}

public class CharacterDataList : CSVDataList<CharacterData>
{
    public string GetName(int id)
    {
        return m_dataList[id].characterName;
    }

    public async UniTask<Sprite> GetSpriteAsync(int id)
    {
        if (m_dataList.ContainsKey(id))
            await m_dataList[id].GetSpriteAsync();

        return null;
    }

    public List<CharacterData> GetAllList()
    {
        return m_dataList.Values.ToList();
    }

    public List<CharacterData> GetDefaultList()
    {
        List<CharacterData> datas = new();

        foreach (var id in m_dataList.Keys)
            datas.Add(m_dataList[id]);

        return datas;
    }

    public async UniTask CharacterSpriteSetting()
    {
        List<UniTask> taskList = new();

        foreach (var id in m_dataList.Keys)
            taskList.Add(m_dataList[id].SetSprite());

        await UniTask.WhenAll(taskList);
    }
}