using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField]
    public GameObject target;
    public GameObject container;
    public GameObject tempContainer;
    private Coroutine isRun = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(isRun == null)
            {
                isRun = StartCoroutine(RunRotateObject());
            }
            else
            {
                StopCoroutine(isRun);
                StartCoroutine(RunRotateObject());
            }

        }

        if(Input.GetMouseButtonUp(0) && isRun != null)
        {
            StopCoroutine(isRun);
            isRun = null;
        }
    }

    public IEnumerator RunRotateObject()
    {
        yield return null;

        Vector2 initCenter = Input.mousePosition;
        Vector2 currentCenter;
        float x;
        float y;
        target.transform.SetParent(tempContainer.transform);
        container.transform.LookAt(Camera.main.transform, Vector3.up);
        target.transform.SetParent(container.transform);

        Vector3 initRotate = container.transform.eulerAngles;
        while (true)
        {
            yield return null;
            currentCenter = Input.mousePosition;

            x = initCenter.x - currentCenter.x;
            y = initCenter.y - currentCenter.y;
            container.transform.eulerAngles = new Vector3(initRotate.x + y, initRotate.y + x, initRotate.z);
        }
    }
}
