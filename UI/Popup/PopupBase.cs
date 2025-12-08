using System;
using UnityEngine;

public class PopupBase : UIBase
{
    public Action showAction;
    public Action closeAction;
    
    public virtual void Init(params object[] parm)
    {
        if(showAction != null)
            showAction.Invoke();
    }

    public virtual void Close()
    {
        if(closeAction != null) 
            closeAction.Invoke();

        PopupManager.Instance.CloseCurrentPopup();
        Destroy(MyObj);
    }
}