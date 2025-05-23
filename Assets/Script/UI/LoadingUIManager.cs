using System.Collections;
using UnityEngine;

public class LoadingUIManager : MonoBehaviour
{
    public static LoadingUIManager Instance { get; private set; }

    [Header("필수 오브젝트")]
    public GameObject loadingPage;
    public CanvasGroup backgroundGroup;
    public CanvasGroup contentGroup;
    public RectTransform spinner;

    [Header("설정")]
    public float fadeDuration = 0.5f;
    public float spinnerSpeed = 200f;
    public float contentFadeDelay = 0.2f;  // 텍스트, 스피너 먼저 사라지고 0.2초 후 배경

    [Header("지연 설정")]
    public float minDisplayTime = 1.0f;

    private Coroutine currentRoutine;
    private float loadingStartTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (loadingPage.activeSelf && spinner != null)
        {
            spinner.Rotate(Vector3.forward, -spinnerSpeed * Time.unscaledDeltaTime);
        }
    }

    public void ShowLoading()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        loadingStartTime = Time.unscaledTime;
        currentRoutine = StartCoroutine(FadeIn());
    }

    public void HideLoading()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        float elapsed = Time.unscaledTime - loadingStartTime;

        if (elapsed < minDisplayTime)
        {
            float delay = minDisplayTime - elapsed;
            currentRoutine = StartCoroutine(DelayedFadeOut(delay));
        }
        else
        {
            currentRoutine = StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        loadingPage.SetActive(true);
        contentGroup.alpha = 0f;
        backgroundGroup.alpha = 0f;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            contentGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            backgroundGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        contentGroup.alpha = 1f;
        backgroundGroup.alpha = 1f;
    }

    private IEnumerator DelayedFadeOut(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        // Step 1: 텍스트 + 스피너 먼저 사라짐
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            contentGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        contentGroup.alpha = 0f;

        // Step 2: 잠깐 기다렸다가 배경 사라짐
        yield return new WaitForSecondsRealtime(contentFadeDelay);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            backgroundGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        backgroundGroup.alpha = 0f;
        loadingPage.SetActive(false);
    }
}