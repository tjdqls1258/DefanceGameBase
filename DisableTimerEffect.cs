using Cysharp.Threading.Tasks;
using UnityEngine;

public class DisableTimerEffect : MonoBehaviour
{
    [SerializeField] private float m_disableTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        WaitDisable().Forget();
    }

    async UniTask WaitDisable()
    {
        await UniTask.WaitForSeconds(m_disableTime);
        if(this != null && gameObject != null)
            gameObject.SetActive(false);
    }
}
