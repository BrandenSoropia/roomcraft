using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public List<Sprite> shelfIcons;
    public List<Sprite> deskIcons;
    public List<Sprite> couchIcons;
    public List<Image> slots;
    public List<Sprite> popSlotIcons;
    public List<Image> popSlots;
    public RectTransform UIbar;
    public GameObject popWindow;
    private bool closed = false;
    private bool uiMode = false;
    private bool popOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        popWindow.SetActive(false);

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
        if (!closed)
        {
            UIbar.localPosition -= new Vector3(0, 100, 0);
            closed = true;
        }
        else
        {
            UIbar.localPosition += new Vector3(0, 100, 0);
            closed = false;
        }
    }

    public void PopWindow()
    {
        if (!popOpen)
        {
            popWindow.SetActive(true);
            popOpen = true;
        }
        else
        {
            popWindow.SetActive(false);
            popOpen = false;
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
