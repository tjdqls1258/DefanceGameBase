using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawShopPanel : UIBase
{
    enum DrawButton
    {
        Draw1,
        Draw10,
    }

    enum characterCards
    {
        Character,
        Character_1,
        Character_2,
        Character_3,
        Character_4,
        Character_5,
        Character_6,
        Character_7,
        Character_8,
        Character_9
    }

    protected override void Awake()
    {
        m_UISequence = UIManager.UISequence.ShopPanel;
        Bind<Button>(typeof(DrawButton));
        Bind<Image>(typeof(characterCards));
        Init();


        Get<Button>((int)DrawButton.Draw1).onClick.AddListener(() =>
        {
            DrawCharacter(1);
        });
        Get<Button>((int)DrawButton.Draw10).onClick.AddListener(() =>
        {
            DrawCharacter(10);
        });
    }

    private void DrawCharacter(int drawCount)
    {
        List<int> ids = new();
        for (int i = 0; i < drawCount; i++)
        {
            ids.Add(Random.Range(1, 30));
        }

        int count = 0;
        foreach (int id in ids)
        {
            Get<Image>(count).gameObject.SetActive(true);
            Get<Image>(count).sprite = GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(id).GetCharacterSprite();
            count += 1;
        }
    }
}
