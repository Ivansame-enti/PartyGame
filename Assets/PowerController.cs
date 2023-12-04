using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;
using TMPro;
using Unity.VisualScripting;

public class PowerController : MonoBehaviour
{
    //Variables de poder
    [SerializeField]
    private float currentPowerLevel;

    [SerializeField]
    private float minPowerLevel = 10;

    [SerializeField]
    private float maxPowerLevel = 300; //Habra que hacer pruebas

    //Variables de escalado
    private float scaleMultiplayer;

    [SerializeField]
    private float maxScaleMultiplier = 2;

    [SerializeField]
    private float minScaleMultiplier = 1;

    private Vector3 originalScale;

    //[SerializeField]
    //private  healthBarC;

    [SerializeField]
    private GameObject powerLevel;

    private TMP_Text powerLevelText;

    private Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        powerLevelText = powerLevel.GetComponent<TMP_Text>();
        canvas = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<Canvas>();
        SetupPowerLevelCanvas();
        originalScale = transform.localScale;
        SetCurrentPowerLevel(minPowerLevel);
    }

    // Update is called once per frame
    void Update()
    {
        //Formula para obtener el escalado del personaje
        float totalRange = maxPowerLevel - minPowerLevel;
        float scaleMultiplayer = ((currentPowerLevel - minPowerLevel) / totalRange) * (maxScaleMultiplier - minScaleMultiplier) + minScaleMultiplier;
        
        scaleMultiplayer = Mathf.Clamp(scaleMultiplayer, minScaleMultiplier, maxScaleMultiplier);

        this.gameObject.transform.localScale = originalScale * scaleMultiplayer;
        //StartCoroutine(ChangeScale, this.gameObject.transform, originalScale);
    }

    private void SetupPowerLevelCanvas()
    {
        powerLevel.transform.SetParent(canvas.transform);
    }

    public float GetCurrentPowerLevel()
    {
        return currentPowerLevel;
    }

    public void SetCurrentPowerLevel(float value)
    {
        currentPowerLevel = Mathf.RoundToInt(currentPowerLevel += value);
        powerLevelText.SetText(currentPowerLevel.ToString());
    }

    public void OnDieSetCurrentPowerLevel()
    {
        currentPowerLevel = Mathf.RoundToInt(currentPowerLevel = currentPowerLevel/2);
        if(currentPowerLevel<0) currentPowerLevel = 0;
        powerLevelText.SetText(currentPowerLevel.ToString());
    }

    IEnumerator ChangeScale(Transform transform, Vector3 originalScale, Vector3 upScale, float duration)
    {
        //Vector3 initialScale = transform.localScale;

        for (float time = 0; time < duration * 2; time += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(originalScale, upScale, time);
            yield return null;
        }
    }
}
