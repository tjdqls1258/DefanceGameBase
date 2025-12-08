using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetExcute : Singleton<NetExcute>
{
    public CancellationTokenSource cancellation = new();

    public async UniTask Requset<T>(RequsetHeader header, Action<T> requsetAction)
    {
        string url = Path.Combine(Config.WebURL, header.GetRutor());
        Logger.Log($"[Tag RequsetData] Requset {url}");

        using(UnityWebRequest unityWeb = new UnityWebRequest(url, header.GetMethod()))
        {
            unityWeb.uploadHandler = new UploadHandlerRaw(header.GetData());
            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            unityWeb.SetRequestHeader("Content-Type", RequsetHeader.REQUSET_CONTENT_TYPE);

            await unityWeb.SendWebRequest().ToUniTask(cancellationToken: cancellation.Token);

            if(unityWeb.result == UnityWebRequest.Result.Success)
            {
                string downLoadValue = unityWeb.downloadHandler.text;

                if (downLoadValue != string.Empty)
                {
                    T res = JsonConvert.DeserializeObject<T>(downLoadValue);

                    if(requsetAction!= null)
                        requsetAction.Invoke(res);
                }
            }
            else
            {

            }
        }
    }
}

[Serializable]
public class Response
{

}

[Serializable]
public abstract class RequsetHeader
{
    public const string REQUSET_CONTENT_TYPE = "application/json";

    public abstract string GetRutor();

    public virtual string GetMethod()
    {
        return "post";
    }

    public virtual byte[] GetData()
    {
        return Encoding.UTF8.GetBytes(JsonUtility.ToJson(this));
    }

    public virtual string RequsetStaringData()
    {
        return JsonConvert.SerializeObject(this);
    }
}