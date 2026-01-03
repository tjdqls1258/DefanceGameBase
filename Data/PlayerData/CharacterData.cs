using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
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

    //Todo ½ºÅÝ
    public MpCharacterState characterState;
    public Sprite characterSprite;

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
}

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

public class CharacterDataTest : CSVData
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

    public override int GetID()
    {
        return id;
    }

    public async UniTask<Sprite> GetSpriteAsync()
    {
        if(modelSprite == null)
            modelSprite = await AddressableManager.Instance.LoadAssetAndCacheAsync<Sprite>(modelSpriteName);

        return modelSprite;
    }

    public async UniTask<GameObject> GetModleObject()
    {
        return await AddressableManager.Instance.LoadAssetAndCacheAsync<GameObject>(modelObjectName);
    }

    public Sprite GetSprite()
    {
        if(modelSprite != null)
            return modelSprite;

        return null;
    }
}

public class CharacterDataList : CSVDataList<CharacterDataTest>
{
    public string GetName(int id)
    {
        return m_dataList[id].characterName;
    }
}