using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{

    public NavMeshSurface surface;
    public GameObject o;
    private NavMeshObstacle obstacle;
    // Start is called before the first frame update
    void Start()
    {
        surface.BuildNavMesh();
        surface.RemoveData();
        obstacle = o.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;  // Carving erlaubt dynamische Anpassung des NavMesh
        obstacle.shape = NavMeshObstacleShape.Box;  // Form des Hindernisses (Box oder Kapsel)
        obstacle.size = new Vector3(2f, 1f, 2f);    // Größe des Hindernisses
        Destroy(o);
        surface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
