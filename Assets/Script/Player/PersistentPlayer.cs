using UnityEngine;

public class PersistentPlayer : MonoBehaviour
{
    private static PersistentPlayer instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject); // ✅ 다른 씬에 있다면 파괴
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject); // ✅ 유지됨
    }
}