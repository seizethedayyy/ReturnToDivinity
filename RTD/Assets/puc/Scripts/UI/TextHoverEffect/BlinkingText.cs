using UnityEngine;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    public TMP_Text pressAnyKeyText;
    public float blinkSpeed = 1.0f;

    private void Update()
    {
        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        pressAnyKeyText.alpha = alpha;
    }
}