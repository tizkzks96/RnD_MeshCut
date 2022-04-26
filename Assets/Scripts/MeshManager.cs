using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEditor;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public GameObject target;
    public int size = 10;
    public Mesh mesh;
    public Material m;

    public float gab = 0;

    public GameObject newMeshsContainer;

    List<Vector3> separatePosition;

    public List<GameObject> particles;

    private GameObject newO;

    void Start()
    {
        mesh = target.GetComponent<MeshFilter>().sharedMesh;

    }

    private void OnDrawGizmos()
    {
        var mesh = target.GetComponent<MeshFilter>().sharedMesh;
        Gizmos.color = Color.red;

        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(mesh.vertices[mesh.triangles[0]], 100);

        Gizmos.DrawLine(mesh.vertices[mesh.triangles[0]] * size, mesh.vertices[mesh.triangles[1]] * size);
        Gizmos.DrawLine(mesh.vertices[mesh.triangles[1]] * size, mesh.vertices[mesh.triangles[2]] * size);
        Gizmos.DrawLine(mesh.vertices[mesh.triangles[2]] * size, mesh.vertices[mesh.triangles[0]] * size);

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Gizmos.DrawLine(mesh.vertices[mesh.triangles[i]] * size, mesh.vertices[mesh.triangles[i + 1]] * size);
            Gizmos.DrawLine(mesh.vertices[mesh.triangles[i + 1]] * size, mesh.vertices[mesh.triangles[i + 2]] * size);
        }
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

    private void CreateMesh_Near()
    {
       
    }

    private void CreateMesh()
    {
        GameObject cotainer = new GameObject();
        particles = new List<GameObject>();
        Debug.Log(mesh.triangles.Length);
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3[] points = new Vector3[3]
            {
                mesh.vertices[mesh.triangles[i]] * size,
                mesh.vertices[mesh.triangles[i + 1]] * size,
                mesh.vertices[mesh.triangles[i + 2]] * size
            };

            Vector2[] uv = new Vector2[3] 
            {
                mesh.uv[mesh.triangles[i]],
                mesh.uv[mesh.triangles[i + 1]],
                mesh.uv[mesh.triangles[i + 2]]
            };

            int[] newTriangles = new int[3]
            {
                0, 1, 2
            };

            Mesh newMesh = new Mesh
            {
                vertices = points,
                uv = uv,
                triangles = newTriangles
            };
            newMesh.RecalculateNormals();

            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.GetComponent<MeshFilter>().mesh = newMesh;
            newObject.GetComponent<MeshRenderer>().material = m;
            newObject.transform.SetParent(cotainer.transform);
            particles.Add(newObject);
        }
    }

    private void MergeMeshByCount(int minCount, int maxCount)
    {
        List<GameObject> newParticles = new List<GameObject>();
        int count;
        for (int i = 0; i < particles.Count; i += count)
        {
            count = Random.Range(minCount, maxCount);
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

    private void Separate(List<GameObject> particles)
    {
        separatePosition = new List<Vector3>();

        foreach (var item in particles)
        {
            Mesh mesh = item.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            gab = vertices[0].z * gab;
            item.transform.position += item.transform.parent.forward * Random.Range(0, 1f);

            separatePosition.Add(item.transform.localPosition);
        }
    }

    private IEnumerator ReturnSeparate(List<GameObject> particles, bool isOrigin)
    {
        Button_RemoveRigidbody();

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
                if (isOrigin || separatePosition == null)
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

    private void SaveAsset(GameObject target)
    {
        var mf = target.GetComponent<MeshFilter>();
        if (mf)
        {
            var savePath = "Assets/" + target.name + ".asset";
            Debug.Log("Saved Mesh to:" + savePath);
            AssetDatabase.CreateAsset(mf.mesh, savePath);
        }
    }

    //인접한 mesh 우선순위로 재정렬
    public void Button_ReArragne()
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        Dictionary<int, float> distances = new Dictionary<int, float>();
        List<GameObject> newdistances = new List<GameObject>();

        var flag = localToWorld.MultiplyPoint3x4(particles[0].GetComponent<MeshFilter>().mesh.vertices[0]);

        for (int i = 0; i < particles.Count; i++)
        {
            var pos = localToWorld.MultiplyPoint3x4(particles[i].GetComponent<MeshFilter>().mesh.vertices[0]);
            distances.Add(i, Vector3.Distance(flag, pos));
        }
        StringBuilder sb = new StringBuilder();
        // Use OrderBy method.
        foreach (var item in distances.OrderBy(i => i.Value))
        {
            newdistances.Add(particles[item.Key]);
            sb.AppendLine(item.ToString());
        }
        Debug.Log(sb);
        particles = newdistances;

        //하이어라키에서 재정렬
        foreach (var item in particles)
        {
            item.transform.SetParent(newMeshsContainer.transform.parent);
        }
        foreach (var item in particles)
        {
            item.transform.SetParent(newMeshsContainer.transform);
        }
    }

    public void Button_SaveAsset()
    {
        MeshMerge(particles);

        SaveAsset(newO);
    }

    public void Button_MeshMerge()
    {
        MeshMerge(particles);
    }

    public void Button_ReturnSeparate()
    {
        StartCoroutine(ReturnSeparate(particles, false));
    }

    public void Button_ReturnOrigin()
    {
        StartCoroutine(ReturnSeparate(particles, true));
    }

    public void Button_AddRigidbody()
    {
        foreach (var item in particles)
        {
            if (item.GetComponent<Rigidbody>() == false)
            {
                Destroy(item.GetComponent<BoxCollider>());
                item.AddComponent<BoxCollider>();
                item.AddComponent<Rigidbody>();
            }
        }
    }

    public void Button_RemoveRigidbody()
    {
        foreach (var item in particles)
        {
            if (item.GetComponent<Rigidbody>())
            {
                Destroy(item.GetComponent<Rigidbody>());
            }
        }
    }

    public void Button_RePositionning()
    {
        Separate(particles);
    }

    public void Button_CreateMesh()
    {
        CreateMesh();
    }

    public void Button_CreateMesh_Near()
    {
        CreateMesh_Near();
    }

    public void Button_MergeMesh()
    {
        MergeMeshByCount(5, 10);
    }
}

class Triangle
{
    public Mesh mesh;

    public Triangle(Mesh mesh)
    {
        this.mesh = mesh;
    }
}