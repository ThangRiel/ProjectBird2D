using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCooldown : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject skillUIHolder;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private bool isCooldown = false;
    public bool IsCooldown => isCooldown;

    private void Start()
    {
        if (skillUIHolder != null)
            skillUIHolder.SetActive(false);

        if (cooldownOverlay != null)
            cooldownOverlay.enabled = false;

        if (cooldownText != null)
            cooldownText.text = "";
    }

    public void SetupUIOnUnlock()
    {
        if (skillUIHolder != null)
            skillUIHolder.SetActive(true);
    }

    public void StartCooldown(float cooldownTime)
    {
        if (!isCooldown)
            StartCoroutine(CooldownRoutine(cooldownTime));
    }

    private IEnumerator CooldownRoutine(float cooldownTime)
    {
        isCooldown = true;

        if (cooldownOverlay != null)
            cooldownOverlay.enabled = true;

        float t = cooldownTime;

        while (t > 0)
        {
            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(t).ToString();

            t -= Time.deltaTime;
            yield return null;
        }

        if (cooldownText != null)
            cooldownText.text = "";

        if (cooldownOverlay != null)
            cooldownOverlay.enabled = false;

        isCooldown = false;
    }
}