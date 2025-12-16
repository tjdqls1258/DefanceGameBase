using FancyScrollView;
using System;
using UnityEngine;

public class CharacterPanelContext : FancyGridViewContext
{
    public int SelectedIndex = -1;
    public Action<CharacterData> OnCellClicked;
}

public class CharacterPanelScroll : FancyGridView<CharacterData, CharacterPanelContext>
{
    class CellGroup : DefaultCellGroup { }
    [SerializeField] CharacterCell cellPrefab = default;

    protected override void SetupCellTemplate() => Setup<CellGroup>(cellPrefab);

    public void OnCellClicked(Action<CharacterData> callback)
    {
        Context.OnCellClicked = callback;
    }
}
