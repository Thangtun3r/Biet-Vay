using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInteraction interaction;
    public CharacterController characterController;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        GameManager.OnFreeze += FreezeMovementOnly;
        GameTransition.OnCollapseStarted += DisablePlayer;
        GameTransition.OnExpandStarted += EnablePlayer;
        SpawnPointHandler.OnPlayerSpawn += SetPlayerSpawnPoint;
    }

    private void OnDisable()
    {
        GameManager.OnFreeze -= FreezeMovementOnly;
        GameTransition.OnCollapseStarted -= DisablePlayer;
        GameTransition.OnExpandStarted -= EnablePlayer;
        SpawnPointHandler.OnPlayerSpawn -= SetPlayerSpawnPoint;
    }

    private void DisablePlayer()
    {
        movement.enabled = false;
        interaction.enabled = false;
        characterController.enabled = false;
    }

    private void EnablePlayer()
    {
        movement.enabled = true;
        interaction.enabled = true;
        characterController.enabled = true;
    }

    private void FreezeMovementOnly()
    {
        movement.IsFrozen = true;       
    }

void SetPlayerSpawnPoint(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
}