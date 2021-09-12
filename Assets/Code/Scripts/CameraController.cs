using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed;

    private void Start()
    {
        fadeImage.color = Color.black;
    }

    private void ProcessFadeToClear()
    {
        // Lerp the colour of the image between itself and transparent.
        fadeImage.color = Color.Lerp(fadeImage.color, Color.clear, fadeSpeed * Time.deltaTime);
    }

    private void ProcessFadeToBlack()
    {
        // Lerp the colour of the image between itself and black.
        fadeImage.color = Color.Lerp(fadeImage.color, Color.black, fadeSpeed * Time.deltaTime);
    }
}
