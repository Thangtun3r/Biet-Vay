using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInteraction interaction;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
    }

    private void OnEnable()
    {
        GameTransition.OnCollapseStarted += DisablePlayer;
        GameTransition.OnExpandStarted += EnablePlayer;
    }

    private void OnDisable()
    {
        GameTransition.OnCollapseStarted -= DisablePlayer;
        GameTransition.OnExpandStarted -= EnablePlayer;
    }

    private void DisablePlayer()
    {
        movement.enabled = false;
        interaction.enabled = false;
    }

    private void EnablePlayer()
    {
        movement.enabled = true;
        interaction.enabled = true;
    }
}