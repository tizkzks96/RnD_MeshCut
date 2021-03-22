using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using UnityEditor;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public int size = 10;
    Mesh mesh;
    Vector3[] vertices;
    public Material m;

    public float gab = 0;

    public GameObject newMeshsContainer;
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        CreateMesh();
    }
    public List<GameObject> particles;
    private void CreateMesh()
    {
        GameObject cotainer = new GameObject();
        particles = new List<GameObject>();
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3[] points = new Vector3[3];
            points[0] = mesh.vertices[mesh.triangles[i]] * size;
            points[1] = mesh.vertices[mesh.triangles[i + 1]] * size;
            points[2] = mesh.vertices[mesh.triangles[i + 2]] * size;

            Vector2[] uv = new Vector2[3];
            uv[0] = mesh.uv[mesh.triangles[i]];
            uv[1] = mesh.uv[mesh.triangles[i+1]];
            uv[2] = mesh.uv[mesh.triangles[i+2]];

            int[] newTriangles = new int[3];
            newTriangles[0] = 0;
            newTriangles[1] = 1;
            newTriangles[2] = 2;


            Mesh newMesh = new Mesh();
            newMesh.vertices = points;
            //newMesh.uv = uv;
            newMesh.triangles = newTriangles;

            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.GetComponent<MeshFilter>().mesh = newMesh;
            newObject.GetComponent<MeshRenderer>().material = m;
            newObject.transform.SetParent(cotainer.transform);
            particles.Add(newObject);
            //newObject.AddComponent<Rigidbody>();
        }
        //StartCoroutine(RePositionning(particles));
        MergeMeshByCount(4);
    }

    private void MergeMeshByCount(int count)
    {
        List<GameObject> newParticles = new List<GameObject>();
        for (int i = 0; i < particles.Count; i += count)
        {
            List<GameObject> group = new List<GameObject>();
            for (int k = 0; k < count; k++)
            {
                if(i+k < particles.Count)
                    group.Add(particles[i + k]);

            }
            newParticles.Add(MeshMerge(group));
        }
        particles.Clear();
        particles = newParticles;
    }
    List<Vector3> separatePosition;
    public void Button_AddRigidbody()
    {
        foreach (var item in particles)
        {
            Destroy(item.GetComponent<BoxCollider>());
            item.AddComponent<BoxCollider>();
            item.AddComponent<Rigidbody>();
        }
    }

    public void Button_RemoveRigidbody()
    {
        foreach (var item in particles)
        {
            Destroy(item.GetComponent<Rigidbody>());
        }
    }

    public void Button_RePositionning()
    {
        RePositionning(particles);
    }

    private void RePositionning(List<GameObject> particles)
    {
        separatePosition = new List<Vector3>();

        foreach (var item in particles)
        {
            Mesh mesh = item.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            gab = vertices[0].z * gab;
            item.transform.position += Vector3.forward * Random.Range(0, 0.5f);

            separatePosition.Add(item.transform.localPosition);
        }
    }

    private IEnumerator ReturnSeparate(List<GameObject> particles, bool isOrigin)
    {
        float time = 0;
        float maxTime = 1;

        List<Vector3> copyPositions = new List<Vector3>();
        List<Vector3> copyRotations = new List<Vector3>();

        foreach (var item in particles)
        {
            copyPositions.Add(item.transform.localPosition);
        }

        foreach (var item in particles)
        {
            copyRotations.Add(item.transform.localEulerAngles);
        }

        while (time < maxTime)
        {
            yield return null;
            time += Time.deltaTime;

            if(time > maxTime)
            {
                time = maxTime;
            }

            for (int i = 0; i < particles.Count; i++)
            {
                Vector3 newPosition;
                if (isOrigin)
                {
                    newPosition = new Vector3
                    (
                        Clamp(time / maxTime, copyPositions[i].x, 0),
                        Clamp(time / maxTime, copyPositions[i].y, 0),
                        Clamp(time / maxTime, copyPositions[i].z, 0)
                    );
                }
                else
                {
                    newPosition = new Vector3
                   (
                       Clamp(time / maxTime, copyPositions[i].x, separatePosition[i].x),
                       Clamp(time / maxTime, copyPositions[i].y, separatePosition[i].y),
                       Clamp(time / maxTime, copyPositions[i].z, separatePosition[i].z)
                   );
                }
                
                Vector3 newRotation = new Vector3
                    (
                        Clamp(time / maxTime, copyRotations[i].x, 0),
                        Clamp(time / maxTime, copyRotations[i].y, 0),
                        Clamp(time / maxTime, copyRotations[i].z, 0)
                    );
                particles[i].transform.localPosition = newPosition;
                particles[i].transform.localEulerAngles = newRotation;
            }
        }
    }


    private float Clamp(float range, float init, float target)
    {
        float value;
        var total = Mathf.Abs(target - init);
        value = total * range;
        if (target < init)
        {
            value *= -1;
        }
        return init + value;
    }

    public void Button_ReturnSeparate()
    {
        StartCoroutine(ReturnSeparate(particles, false));
    }

    public void Button_ReturnOrigin()
    {
        StartCoroutine(ReturnSeparate(particles, true));
    }


    GameObject newO;
    private GameObject MeshMerge(List<GameObject> particles)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (var item in particles)
        {
            meshFilters.Add(item.GetComponent<MeshFilter>());
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        int i = 0;
        while (i < meshFilters.Count)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        newO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newO.GetComponent<MeshRenderer>().material = m;
        newO.GetComponent<MeshFilter>().mesh = new Mesh();
        newO.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        newO.gameObject.SetActive(true);
        newO.transform.SetParent(newMeshsContainer.transform);
        return newO;
    }
    void SaveAsset(GameObject target)
    {
        var mf = target.GetComponent<MeshFilter>();
        if (mf)
        {
            var savePath = "Assets/" + target.name + ".asset";
            Debug.Log("Saved Mesh to:" + savePath);
            AssetDatabase.CreateAsset(mf.mesh, savePath);
        }
    }

    public void Button_SaveAsset()
    {
        SaveAsset(newO);
    }
    public void Button_MeshMerge()
    {
        MeshMerge(particles);
    }

    private void OnDrawGizmos()
    {
        //if (vertices.Length > 1)
        //{
        //    Gizmos.color = Color.red;
            
        //    Gizmos.color = Color.blue;

        //    for (int i = 0; i < mesh.triangles.Length; i += 3)
        //    {
        //        Gizmos.DrawLine(mesh.vertices[mesh.triangles[i]] * size, mesh.vertices[mesh.triangles[i + 1]] * size);
        //        Gizmos.DrawLine(mesh.vertices[mesh.triangles[i + 1]] * size, mesh.vertices[mesh.triangles[i + 2]] * size);
        //    }
        //}

    }

    void Update()
    {
        //for (var i = 0; i < vertices.Length; i++)
        //{
        //    vertices[i] += Vector3.up * Time.deltaTime;
        //}

        //// assign the local vertices array into the vertices array of the Mesh.
        //mesh.vertices = vertices;
        //mesh.RecalculateBounds();
    }
}
