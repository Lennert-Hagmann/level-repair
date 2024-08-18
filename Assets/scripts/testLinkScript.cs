using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class testLinkScript : MonoBehaviour
{
    public NavMeshSurface s;

    public void Start()
    {
        s = GetComponent<NavMeshSurface>();
        //CreateNavMeshLink(new Vector3(0, 0, 0), new Vector3(0, 0, 3));
        Debug.Log(IsTargetReachable(new Vector3(0, 0, 0), new Vector3(0, 0, 3)));
    }
    void CreateNavMeshLink(Vector3 start, Vector3 end)
    {
        Debug.Log("link");
        // Erstelle ein neues GameObject für den NavMeshLink
        GameObject linkObject = new GameObject("NavMeshLink");
        var navMeshLink = linkObject.AddComponent<NavMeshLink>();

        // Setze die Start- und Endpunkte des Links
        navMeshLink.startPoint = start;
        navMeshLink.endPoint = end;

        // Optional: Weitere Einstellungen
        navMeshLink.width = 1.0f;
        navMeshLink.costModifier = -1;
        navMeshLink.bidirectional = true;

        // Aktualisiere den Link
        navMeshLink.UpdateLink();
    }
    bool IsTargetReachable(Vector3 start, Vector3 target)
    {
        // Erstelle ein neues NavMeshPath-Objekt
        NavMeshPath path = new NavMeshPath();

        // Versuche, einen Pfad von der Startposition zur Zielposition zu berechnen
        if (NavMesh.CalculatePath(start, target, NavMesh.AllAreas, path))
        {
            // Überprüfe, ob der Pfad vollständig ist
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
        }
        return false;
    }
}
