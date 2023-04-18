using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    public Slider slider;
    public Text percentLoaded;
    public GameObject loadingScreen;
    //public CanvasGroup canvasGroup;
    public void LoadLevel (string scene)
    {
        StartCoroutine(LoadAsynchronously(scene));
    }
    IEnumerator LoadAsynchronously (string scene)
    {
        loadingScreen.SetActive(true);
        //yield return StartCoroutine(FadeLoadingScreen(1,1));
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);

        while (!operation.isDone)
        {
            Debug.Log(operation.progress);
            slider.value = Mathf.Clamp01(operation.progress/ 0.9f);
            percentLoaded.text = Mathf.Round(slider.value * 100) + "%";
            
            yield return null;
        }
       // yield return StartCoroutine(FadeLoadingScreen(0, 1));
        loadingScreen.SetActive(false);
    }
    //IEnumerator FadeLoadingScreen(float targetValue, float duration)
    //{
        //float startValue = canvasGroup.alpha;
        //float time = 0;
        //while (time < duration)
        //{
            //canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time /duration);
            //time += Time.deltaTime;
            //yield return null;
        //}
        //canvasGroup.alpha = targetValue;
    //}

}
