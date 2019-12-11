﻿using UnityEngine;
using UnityEngine.InputSystem;

public class HunterPlayerController : MonoBehaviour
{
    public void OnInputFire(InputAction.CallbackContext context)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPos2D, Vector2.zero);

        if (hitInfo.collider != null)
        {
            SnapGridCharacter character = hitInfo.transform.gameObject.GetComponentInParent<SnapGridCharacter>();
            if (character != null)
            {
                character.Kill();
            }
        }
    }
}
