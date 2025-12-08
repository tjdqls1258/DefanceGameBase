using UnityEngine;

public class PopupMsg : PopupBase
{
    enum Button
    {
        CloseButton,
    }

    public override void Init(params object[] parm)
    {
        base.Init(parm);
        Bind<UnityEngine.UI.Button>(typeof(Button));

        Get<UnityEngine.UI.Button>((int)Button.CloseButton).onClick.AddListener(()=> 
        {
            Close();    
        });
    }
}
