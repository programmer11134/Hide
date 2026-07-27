using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Image progress;
    void Start()
    {
        progress.fillAmount = 0f;
        StartCoroutine(loadScene());

    }

    IEnumerator loadScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            progress.fillAmount = operation.progress;

            if (operation.progress >= 0.9f && !operation.allowSceneActivation)
            { 
                yield return new WaitForSeconds(1.5f); 
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }



}
