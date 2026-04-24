using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroller : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 20f;
    [SerializeField] RectTransform rect;

    private void Update()
    {
        rect.position = new Vector3(rect.position.x, rect.position.y + scrollSpeed * Time.deltaTime, rect.position.z);

        if (rect.position.y > 23500f)
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
