using UnityEngine;

public class DustEffect : MonoBehaviour
{
    
    public float disableDelay = 1f;

    
    public void Activate()
    {
        gameObject.SetActive(true);
        CancelInvoke();
        Invoke("DisableEffect", disableDelay);
    }

    private void DisableEffect()
    {
        gameObject.SetActive(false);
    }
}