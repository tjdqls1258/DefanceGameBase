using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class AwakeScene : MonoBehaviour
{
    void Start()
    {
        GameMaster.Instance.Init();
    }
}
