using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Material material;
    // Start is called before the first frame update
    void Start()
    {
        
        
        //StartCoroutine(AutoCut());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                GameObject objectHit = hit.transform.gameObject;

                GameObject[] objects = MeshCut.Cut(objectHit, hit.point, Vector3.down, material);
                
                for (int i = 0; i < objects.Length; i++)
                {
                    Debug.Log($"{i}" + objects[i].name);
                    MeshSaverEditor.SaveMesh(objects[i].GetComponent<MeshFilter>().mesh, i.ToString(), true, true);
                }

                //Test a = objects[1].AddComponent<Test>();
                //a.material = material;
                MeshCollider m = objects[1].AddComponent<MeshCollider>();
                m.convex = true;
                //Rigidbody r = objects[0].AddComponent<Rigidbody>();
                //r.drag = 10;
                //r.AddForce(Vector3.forward * 10000);
                Destroy(objectHit.GetComponent<MeshCollider>());
                MeshCollider mc = objectHit.AddComponent<MeshCollider>();
                mc.convex = true;
            }
        }
    }

    public IEnumerator AutoCut()
    {
        
        yield return new WaitForSecondsRealtime(1f);
        GameObject[] gameObjects = MeshCut.Cut(gameObject, transform.position, Vector3.down, material);
        Test a = gameObjects[1].AddComponent<Test>();

    }
}
