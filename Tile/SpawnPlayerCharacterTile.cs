using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnPlayerCharacterTile : TileBase, IPointerDownHandler, IPointerUpHandler
{
    private Action<Vector2Int> spawnAction;
    private bool m_spawnUnitTile = false;
    private PlayerAttackController m_character;

    public bool CheckSpawn() => m_spawnUnitTile;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerDownAction();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerUpAction();
    }

    public void SpawnUnit(PlayerAttackController character)
    {
        m_spawnUnitTile = true;
        m_character = character;
    }

    public void UnitDie()
    {
        m_spawnUnitTile = false;
    }
}
