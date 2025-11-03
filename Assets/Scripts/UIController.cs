using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Player Controllers")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerSFXController playerSFXController;

    [Header("UI Controls")]
    [SerializeField] HorizontalLayoutGroup buildPrimaryUILayout;
    [SerializeField] RectOffset padding;

    [Header("UI Inputs")]
    [SerializeField] InputSystemUIInputModule inputSystemUIInputModule;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] GameObject firstSelectedGO;

    public List<Sprite> shelfIcons;
    public List<Sprite> deskIcons;
    public List<Sprite> couchIcons;
    public List<Image> slots;
    public List<Sprite> popSlotIcons;
    public List<Image> popSlots;
    public RectTransform UIbar;
    [SerializeField]public bool closed = true;
    private bool uiMode = false;
    private bool popOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closed = true;
    }

    // Update is called once per frame
    void Update()
    {
        // When Left Alt is pressed down
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            uiMode = true;
            Cursor.lockState = CursorLockMode.None;  // free the cursor
            Cursor.visible = true;                 // show the OS cursor
        }

        // When Left Alt is released
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            uiMode = false;
            Cursor.lockState = CursorLockMode.Locked; // lock it back to center
            Cursor.visible = false;                 // hide again
        }
    }

    public void DeskClick()
    {
        int index = 0;

        foreach (Image img in slots)
        {
            if (index < deskIcons.Count)
            {
                print("set image");
                img.sprite = deskIcons[index];
                img.color = new Color(1, 1, 1, 1);
                index++;
            }
            else
            {
                img.sprite = null;
            }
        }
    }

    public void ShelfClick()
    {
        int index = 0;

        foreach (Image img in slots)
        {
            if (index < shelfIcons.Count)
            {
                print("set image");
                img.sprite = shelfIcons[index];
                img.color = new Color(1, 1, 1, 1);
                index++;
            }
            else
            {
                img.sprite = null;
            }
        }
    }

    public void CouchClick()
    {
        int index = 0;

        foreach (Image img in slots)
        {
            if (index < couchIcons.Count)
            {
                print("set image");
                img.sprite = couchIcons[index];
                img.color = new Color(1, 1, 1, 1);
                index++;
            }
            else
            {
                img.sprite = null;
            }
        }
    }

    public void UItab()
    {

        /*
        The Canvas' Event System GO has a heavily linked UI controller script
        that only works with it's default Action Asset, "InputSystem_Action". We can't
        copy/paste the mapping into Player's.

        The unity requires player and UI Input Assets to be separate. 
        Since both are active by default, we need to control them such
        that only 1 is active at the same time so we don't navigate UI
        while moving the player for example.

        We need to keep Player Input enabled so we can still toggle on/off UI.
        We use a simple map switch to "disable" regular player controls but keep
        UI controls active and listening.
        */
        if (!closed)
        {
            UIbar.localPosition -= new Vector3(0, 100, 0);
            closed = true;

            inputSystemUIInputModule.enabled = false;
            playerInput.SwitchCurrentActionMap("Player");
            eventSystem.SetSelectedGameObject(null);

            playerSFXController.PlayCloseInventorySFX();
            buildPrimaryUILayout.padding = new RectOffset(0, 0, 0, 0);
        }
        else
        {
            UIbar.localPosition += new Vector3(0, 100, 0);
            closed = false;

            inputSystemUIInputModule.enabled = true;
            playerInput.SwitchCurrentActionMap("UI");
            eventSystem.SetSelectedGameObject(firstSelectedGO);


            playerSFXController.PlayOpenInventorySFX();
            buildPrimaryUILayout.padding = padding;
        }
    }

    public void SetUpPopIcons()
    {
        int index = 0;

        foreach (Image img in popSlots)
        {
            if (index < popSlotIcons.Count)
            {
                print("set pop image");
                img.sprite = popSlotIcons[index];
                img.color = new Color(1, 1, 1, 1);
                index++;
            }
            else
            {
                img.sprite = null;
            }
        }
    }
}
