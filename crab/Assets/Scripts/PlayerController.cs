using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Animator charAnim, cameraShake;

    Vector3 velocity;
    [SerializeField] Rigidbody myRigidBody;
    Transform myTransform;

    int moveInt;

    [SerializeField] SoundManagerLogic mySoundManager;

    [SerializeField] AudioSource[] movementSounds;

    AudioSource currentSound, nextSound;

    private void Start()
    {
        myTransform = transform;
        moveInt = 0;
    }

    public void Move(Vector3 _velocity)
    {
        velocity = new Vector3(_velocity.x, velocity.y, _velocity.y);
    }

    public void LookAt(float _rotation)
    {
        myTransform.eulerAngles = new Vector3(0, -_rotation+90, 0);
    }

    public void FixedUpdate()
    {
        var pos = myRigidBody.position + velocity * Time.fixedDeltaTime;
        pos.x = Mathf.Clamp(pos.x, -4.3f, 4.3f);
        pos.z = Mathf.Clamp(pos.z, -5.65f, 0.55f);
        if (!GameManager.levelFailed)
            myRigidBody.MovePosition(pos);
    }

    private void Update()
    {
        // set animations
        if (velocity.magnitude <= 0.5f && velocity.magnitude >= 0.05f && moveInt != 1)
        {
            charAnim.SetTrigger("walk");
            SetCurrentSound();
            moveInt = 1;
            nextSound = movementSounds[moveInt - 1];
            StartCoroutine(CrossFade(currentSound, nextSound));
        }
        else if (velocity.magnitude <= 2f && velocity.magnitude >= 0.5f && moveInt != 2)
        {
            charAnim.SetTrigger("run");
            SetCurrentSound();
            moveInt = 2;
            nextSound = movementSounds[moveInt - 1];
            StartCoroutine(CrossFade(currentSound, nextSound));
        }
        else if (velocity.magnitude > 2 && moveInt != 3)
        {
            charAnim.SetTrigger("sprint");
            SetCurrentSound();
            moveInt = 3;
            nextSound = movementSounds[moveInt - 1];
            StartCoroutine(CrossFade(currentSound, nextSound));
        }
        else if (velocity.magnitude < 0.1f && moveInt != 0)
        {
            charAnim.SetTrigger("idle");
            SetCurrentSound();
            int prevInt = moveInt;
            moveInt = 0;
            StartCoroutine(FadeOut(currentSound, prevInt));
        }
    }

    void SetCurrentSound()
    {
        switch (moveInt)
        {
            case 0: // idle
                currentSound = null;
                break;
            default:
                currentSound = movementSounds[moveInt - 1];
                break;
        }
    }

    IEnumerator CrossFade(AudioSource oldSound, AudioSource newSound)
    {
        float timer = 0, totalTime = 6;
        newSound.Play();

        float newSoundStart = newSound.volume;
        float oldSoundStart = 1;
        if (oldSound != null)
            oldSoundStart = oldSound.volume;

        while (timer <= totalTime)
        {
            if (oldSound != null)
                oldSound.volume = Mathf.Lerp(oldSoundStart, 0, timer / totalTime);
            newSound.volume = Mathf.Lerp(newSoundStart, 1,  timer / totalTime);

            yield return new WaitForFixedUpdate();
            timer++;
        }
        newSound.volume = 1;
        if (oldSound != null)
        {
            oldSound.volume = 0;
            oldSound.Stop();
        }
    }

    IEnumerator FadeOut(AudioSource mySound, int pastInt)
    {
        float timer = 0, totalTime = 6;
        if (mySound == null)
            mySound = movementSounds[0];

        while (timer <= totalTime)
        {
            if (moveInt != 0 && moveInt == pastInt)
                break;
            mySound.volume = Mathf.Lerp(1, 0, timer / totalTime);

            yield return new WaitForFixedUpdate();
            timer++;
        }
        if (moveInt == 0 || moveInt != pastInt)
        {
            mySound.volume = 0;
            mySound.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Page" && !GameManager.levelFailed)
        {
            mySoundManager.Play("CrabSquish"); // squash sound
            StartCoroutine(WaitForJingle());
            GameManager.levelFailed = true;
            StartCoroutine(SquishChar());
        }
    }

    IEnumerator WaitForJingle()
    {
        yield return new WaitForSeconds(0.15f);
        mySoundManager.Play("LosingJingle"); // losing jingle
    }

    IEnumerator SquishChar()
    {
        // GameManager.pageTurnSpeed
        float timer = 0, totalTime = 8;  // 12 - 9
        Vector3 startingSize = myTransform.localScale;
        Vector3 endSize = new Vector3(startingSize.x, 0, startingSize.z);
        while (timer <= totalTime)
        {
            myTransform.localScale = Vector3.Lerp(startingSize, endSize, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }
}
