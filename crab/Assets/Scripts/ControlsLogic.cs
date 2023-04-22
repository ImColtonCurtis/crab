using System.Collections;
using System.Collections.Generic;
using Unity.Services.Mediation.Samples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsLogic : MonoBehaviour
{
    bool touchedDown;

    Vector3 anchorPos, targetPos;
    Vector2 moveInput;
    float targetAngle, anchorRadius = 0.0192f/2.6f; // set anchor radius
    float moveSpeed = 4.3f; // add aceleration to the movement
    [SerializeField] PlayerController controller;

    [SerializeField] GameObject noIcon;

    [SerializeField] Animator soundAnim;

    int cheatCounter;

    void Awake()
    {
        touchedDown = false;

        cheatCounter = 0;

        if (PlayerPrefs.GetInt("SoundStatus", 1) == 1)
        {
            noIcon.SetActive(false);
            AudioListener.volume = 1;
        }
        else
        {
            noIcon.SetActive(true);
            AudioListener.volume = 0;
        }
    }

    void OnTouchDown(Vector3 point)
    {
        if (!touchedDown)
        {
            if (ShowAds.poppedUp)
            {
                if (point.x <= 0)
                    ShowAds.shouldShowRewardedAd = true;
                else
                    ShowAds.dontShow = true;
            }
            else
            {
                // cheat: top-right, top-right, top-left, bottom-right
                // top right tap
                if (!GameManager.levelStarted && (cheatCounter == 0 || cheatCounter == 1) && point.x >= 0.03f && point.y >= 8f)
                {
                    cheatCounter++;
                }
                // top left tap
                else if (!GameManager.levelStarted && (cheatCounter == 2) && point.x <= -0.03f && point.y >= 8f)
                {
                    cheatCounter++;
                }
                // bottom right tap
                else if (!GameManager.levelStarted && (cheatCounter == 3) && point.x >= 0.03f && point.y <= 7.92f)
                {
                    cheatCounter = 0;
                    if (!GameManager.cheatOn)
                        GameManager.cheatOn = true;
                    else
                        GameManager.cheatOn = false;
                }

                else if (!GameManager.levelStarted && point.x <= -0.01f && point.y <= 7.92f) // bottom left button clicked
                {
                    if (PlayerPrefs.GetInt("SoundStatus", 1) == 1)
                    {
                        PlayerPrefs.SetInt("SoundStatus", 0);
                        noIcon.SetActive(true);
                        AudioListener.volume = 0;
                    }
                    else
                    {
                        PlayerPrefs.SetInt("SoundStatus", 1);
                        noIcon.SetActive(false);
                        AudioListener.volume = 1;
                    }
                    soundAnim.SetTrigger("Blob");
                }
                else
                {
                    touchedDown = true;
                    if (!GameManager.levelFailed)
                    {
                        if (!GameManager.levelStarted)
                            GameManager.levelStarted = true;

                        anchorPos = new Vector3(point.x, point.y, anchorPos.z);
                        targetPos = new Vector3(point.x, point.y, targetPos.z);
                    }
                }
            }
        }
    }

    void OnTouchStay(Vector3 point)
    {
        if (!touchedDown && !GameManager.levelFailed)
        {
            touchedDown = true;
            anchorPos = new Vector3(point.x, point.y, anchorPos.z);
            targetPos = new Vector3(point.x, point.y, targetPos.z);
        }
        if (touchedDown && !GameManager.levelFailed)
        {
            targetPos = new Vector3(point.x, point.y, targetPos.z);
            targetAngle = Mathf.Rad2Deg * (Mathf.Atan2(targetPos.y - anchorPos.y, targetPos.x - anchorPos.x));
            if (targetAngle < 0)
                targetAngle += 360;

            Vector3 moveDirection = new Vector3(Mathf.Cos(Mathf.Deg2Rad * targetAngle), Mathf.Sin(Mathf.Deg2Rad * targetAngle), targetPos.z);
            moveInput = new Vector2(Mathf.Clamp(moveDirection.x, -1, 1)* Mathf.Clamp(Mathf.Abs(targetPos.x - anchorPos.x) / anchorRadius, 0, 1), 
                Mathf.Clamp(moveDirection.y, -1, 1)* Mathf.Clamp(Mathf.Abs(targetPos.y - anchorPos.y) / anchorRadius, 0, 1));

            Vector3 moveVelocity = moveInput * moveSpeed;
            controller.Move(moveVelocity);
            controller.LookAt(targetAngle);

            // if finger position is greater than (anchorRadius * 2), then reset anchor to be (anchorRadius)
            if (Vector3.Distance(anchorPos, targetPos) >= (anchorRadius * 2))
               anchorPos = Vector3.Lerp(anchorPos, targetPos, 0.5f);
        }
    }

    void OnTouchUp()
    {
        if (touchedDown)
        {
            touchedDown = false;

            moveInput = Vector2.zero;
            Vector3 moveVelocity = moveInput.normalized * moveSpeed;
            controller.Move(moveVelocity);
        }
    }

    void OnTouchExit()
    {
        if (touchedDown)
        {
            touchedDown = false;

            moveInput = Vector2.zero;
            Vector3 moveVelocity = moveInput.normalized * moveSpeed;
            controller.Move(moveVelocity);
        }
    }
}
