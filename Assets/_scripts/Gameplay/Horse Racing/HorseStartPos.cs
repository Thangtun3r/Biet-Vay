using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HorseStartPos : MonoBehaviour
{
    public GameObject resultPanel;
    public TextMeshProUGUI horseNameTMP;
    public RectTransform rectTransform;
    public Horse2D horse;
    public Animator animator;
    private Vector2 initialPos;

    

    private void Start()
    {
        initialPos = rectTransform.anchoredPosition;
    }


    private void OnEnable()
    {
        AnotherBettingDay.OnRaceReset += HandleRaceReset;
    }
    
    
    private void OnDisable()
    {
        AnotherBettingDay.OnRaceReset -= HandleRaceReset;
    }
    
    
    private void HandleRaceReset()
    {
        rectTransform.anchoredPosition = initialPos;

        animator.SetFloat("Speed", 0.7f);
        animator.SetTrigger("Restart");
        resultPanel.SetActive(false);
        horseNameTMP.color = Color.white;
    }

}
