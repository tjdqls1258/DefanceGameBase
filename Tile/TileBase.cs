using UnityEngine;
using static MapData;

public class TileBase : CachObject
{
    protected SpriteRenderer tileImage;

    public TileData m_tileData
    {
        private set;
        get;
    }

    private void Awake()
    {
        tileImage = GetComponent<SpriteRenderer>();
    }

    public virtual void Init(TileData tileData)
    {
        m_tileData = tileData;

        transform.localPosition = new Vector3(tileData.x, tileData.y, 0);
    }

    public virtual void SetTileSprite(Sprite sprite)
    {
        tileImage.sprite = sprite;
    }

    public virtual bool CheckSpawnPoint(bool spawnPathCharacter = false)
    {
        if((m_tileData.type == MapObject.Spawn && spawnPathCharacter == false) || 
            (m_tileData.type == MapObject.Path && spawnPathCharacter))
        {
            return true;
        }

        return false;
    }
}
