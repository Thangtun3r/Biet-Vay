using System;
using UnityEngine;

public class SimpleFogAndCameraColor : MonoBehaviour
{
    public Color fogColor = Color.gray;
    public Color cameraColor = Color.black;

    private void OnEnable()
    {
        GameManager.OnWeather += SetWeather;
    }

    private void OnDisable()
    {
        GameManager.OnWeather -= SetWeather;
    }

    public void SetWeather()
    {
        RenderSettings.fogColor = fogColor;
        Camera.main.backgroundColor = cameraColor;
    }
}