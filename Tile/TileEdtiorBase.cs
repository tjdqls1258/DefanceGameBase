using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileEdtiorBase : TileBase, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public MapEditorUI mapEditorUI;
    public Vector2Int currentPos;
    public UnityAction<Vector2Int> onclickEnter;
    private Color currentColor = Color.white;

    public void InitTileEdtiorBase(MapData.TileData tileData)
    {
        if (tileData.type == MapData.MapObject.Delete)
            currentColor = Color.black;
        else
            currentColor = Color.white;

        SetTypeColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.pointerEnter == gameObject)
        {
            if(mapEditorUI.pathMode)
            {
                onclickEnter?.Invoke(currentPos);
                return;
            }
            if (mapEditorUI.GetCurrentSprite() == null)
                return;

            tileImage.sprite = mapEditorUI.GetCurrentSprite();
            onclickEnter?.Invoke(currentPos);
            if(mapEditorUI.GetCurrentType() == MapData.MapObject.Delete)
                currentColor = Color.black;
            else
                currentColor = Color.white;

            SetTypeColor();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
            tileImage.color = Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
            SetTypeColor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerEnter != gameObject)
        {
            return;
        }
    }

    private void SetTypeColor()
    {
        tileImage.color = currentColor;
    }
}
