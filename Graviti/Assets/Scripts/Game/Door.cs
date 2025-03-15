using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private List<PressureButton> requiredButtons;

    [Header("Visual Settings")]
    [SerializeField] private Sprite closedDoorSprite;
    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private SpriteRenderer doorSprite;

    [Header("Level Complete Settings")]
    [SerializeField] private GameObject levelCompletePanel; 

    private bool isOpen = false;

    private void Start()
    {
        if (!doorSprite) doorSprite = GetComponent<SpriteRenderer>();

        if (levelCompletePanel)
            levelCompletePanel.SetActive(false);

        foreach (var button in requiredButtons)
        {
            button.onButtonActivated.AddListener(CheckDoorState);
            button.onButtonDeactivated.AddListener(CheckDoorState);
        }

        UpdateDoorVisual();
    }

    private void CheckDoorState()
    {
        isOpen = true;
        foreach (var button in requiredButtons)
        {
            if (!button.IsActivated())
            {
                isOpen = false;
                break;
            }
        }

        UpdateDoorVisual();
    }

    private void UpdateDoorVisual()
    {
        if (doorSprite)
        {
            doorSprite.sprite = isOpen ? openDoorSprite : closedDoorSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            AudioManager.instance.StopMusic();
            AudioManager.instance.PlayLevelCompleteSFX();
            if (levelCompletePanel)
                levelCompletePanel.SetActive(true);

            other.gameObject.SetActive(false);
        }
    }
}
