using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageLogic : MonoBehaviour
{
    public int turnState;
    Transform myTransform;
    Vector3 endingAngle, endingPosition;

    public bool move;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = transform;
        move = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            PageTurnFSM();
            move = false;
        }
    }

    void PageTurnFSM()
    {
        switch (turnState)
        {
            case 0: // 0->1
                endingPosition = new Vector3(-0.02f, -4.344f, 0);
                StartCoroutine(MovePage(endingPosition));
                break;
            case 1: // 1->2
                endingPosition = new Vector3(0.076f, -4.29f, 0);
                StartCoroutine(MovePage(endingPosition));
                break;
            case 2: // 2->3
                endingAngle = new Vector3(0, 0, 0);
                StartCoroutine(TurnPage(endingAngle));
                break;
            case 3: // 3->4
                endingPosition = new Vector3(0.076f, -4.4f, 0);
                StartCoroutine(MovePage(endingPosition));
                break;
            case 4: // 4->5
                endingPosition = new Vector3(0.076f, -4.51f, 0);
                StartCoroutine(MovePage(endingPosition));
                break;
            default:
                //Destroy(gameObject);
                break;
        }
        turnState++;
    }

    IEnumerator TurnPage(Vector3 endingAngle)
    {
        Vector3 startingAngle = myTransform.localEulerAngles;
        float timer = 0, totalTime = GameManager.pageTurnSpeed * ((startingAngle.z-endingAngle.z)/121.55f);
        while (timer <= totalTime)
        {
            myTransform.localEulerAngles = Vector3.Lerp(startingAngle, endingAngle, timer/totalTime);
            if ((int)timer == ((int)totalTime - 4))
            {
                GameManager.moveBottom = true;
            }
            yield return new WaitForFixedUpdate();
            timer++;            
        }
        myTransform.localEulerAngles = endingAngle;
        // turn off mesh collider
        if (turnState == 3)
        {
            MeshCollider myMesh = GetComponentInChildren<MeshCollider>();
            GameManager.bottomsTriggered = true;
            if (myMesh != null)
                myMesh.enabled = false;
            else
                Debug.Log("error!");
        }
    }

    IEnumerator MovePage(Vector3 endingPosition)
    {
        Vector3 startingPos = myTransform.localPosition;
        float timer = 0, totalTime = GameManager.pageTurnSpeed * (1 / 121.55f);
        if (turnState == 0 || turnState == 1)
            yield return new WaitForSeconds(5f / 60f);
        while (timer <= totalTime)
        {
            myTransform.localPosition = Vector3.Lerp(startingPos, endingPosition, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        myTransform.localPosition = endingPosition;
    }
}
