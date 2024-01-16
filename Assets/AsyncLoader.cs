using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncLoader : MonoBehaviour
{

    [SerializeField] private Slider loadingSlider;
    private float loadingTimer = 0.0f;
    private float loadingBuffer = 1.0f;

    private void Update()
    {
        if ( loadingTimer != -1f)
        {
            loadingTimer += Time.deltaTime;
            if (loadingTimer >= loadingBuffer)
            {
                StartCoroutine(LoadLevelAsyc("GameScene"));
                loadingTimer = -1f;
            }
        }
    }

    IEnumerator LoadLevelAsyc(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        float progressValue;

        while (!loadOperation.isDone)
        {
            progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.value = progressValue;
            yield return null;
        }
    }

}
