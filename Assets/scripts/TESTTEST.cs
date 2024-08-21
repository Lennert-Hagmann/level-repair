using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;
using UnityEngine.UIElements;

public class TESTTEST : MonoBehaviour
{

    public NavMeshSurface surface;
    public GameObject player;
    public Vector3 PlayerStartPosition;
    public GameObject GoalTile;
    private void Start()
    {

        //Debug.Log(isAObjectinMap(new Vector3(1, 0, 2)));
        //RemoveObjectsAbove(new Vector3(2,2, 1));
        //deleteObjectsBetween(new Vector3(0,0,0), new Vector3(2,1,3));
        //surface.BuildNavMesh();
        //Vector3 start = new Vector3 (0, 0.6f, 0);
        //Vector3 end = new Vector3 (0,0.6f,3);
        //Debug.Log("Ist Weg erreichbar von " + start.ToString() + " zu " + end.ToString() + "? Antwort: "+ IsPathClear(start, end, collisionMask));
        //Vector3 startPosition = new Vector3(0, 0, 0); //player.transform.position;
        //Debug.Log(IsPositionReachableOnNavMesh(startPosition, new Vector3(1, 1, 2)));

        // Stelle sicher, dass das NavMesh auf dem NavMeshSurface erstellt wurde
        //surface.BuildNavMesh();

        //GenerateNavMeshLinks(startPosition);
    }


    //In Bearbeitung
    public void erweiteterAgent(Vector3 startPosition)
    {
        Debug.LogWarning("ERWEITERTER AGENT");
        List<Vector3> l = new List<Vector3>();
        if (!GoalReachable)
        {

            l = durchquereNavMeshGebiet(startPosition);
        }
        if(!GoalReachable)
        {
            constructNavMeshLinksToReachablePositions(l, true);

        }
    }

    //überprüft, ob bei der Verbindung zwischen beiden Blöcken ein Block im Weg ist. ein Raycast 1,1 Einheiten weiter oben und 2,1 Einheiten weiter oben verwendet. 
    //1,1 Einheiten, da ansonsten der Übergang von (0,0,0) zu (1,1,0) als blockiert angesehen wird. Sollte sich 1 Einheit überhalb des Blocks etwas befinden, dann versperrt dies sowieso nicht den Weg, 
    //da der Spieler einfach draufspringen kann
    public bool IsPathClear(Vector3 p1, Vector3 p2, LayerMask collisionMask)
    {
        //verschiebe nach oben, damit er nicht mit den Objekten auf Ebene 0 oder 1 kollidiert (diese sind für den Spieler überquerbar)
        Vector3 startPoint = new Vector3(p1.x, p1.y + 1.1f, p1.z);
        Vector3 endPoint = new Vector3(p2.x, p2.y + 1.1f, p2.z);
        // Berechne die Richtung vom Startpunkt zum Endpunkt
        Vector3 direction = endPoint - startPoint;

        // Berechne die Distanz zwischen den beiden Punkten
        float distance = direction.magnitude;

        // Führe den Raycast mit der Layer Mask durch
        RaycastHit hit;
        for(int height=0; height<=1; height++)
        {
            Vector3 startPosition = new Vector3(startPoint.x, startPoint.y + height, startPoint.z); 

            if (Physics.Raycast(startPosition, direction.normalized, out hit, distance, collisionMask))
            {
                // Wenn der Raycast auf etwas trifft, bedeutet das, dass der Weg blockiert ist
                Debug.Log("Weg blockiert von: " + hit.collider.name);
                return false;
            }
        }
        

        // Wenn der Raycast auf nichts trifft, ist der Weg frei
        //Debug.Log("Weg ist frei.von " + p1.ToString() + " zu " + p2.ToString());
        return true;
    }

    //Liste zum Speichern aller von WorldGeneration erstellten Tiles,
    //Diese Liste wird dazu genutzt, später die Objekte zu suchen, dessen Farbe geändert werden soll.
    //Dazu muss auf die bereits erstellten Objekte zugegriffen werden. Deshalb werden diese in dieser Liste gespeichert.
    public List<GameObject> AllTiles;

    //überprüft, ob die Target Position von der Start Position aus erreichbar ist (Test über Navmesh)
    bool IsPositionReachableOnNavMesh(Vector3 start, Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();

        // Berechnet den Pfad vom Startpunkt zum Zielpunkt
        if (NavMesh.CalculatePath(start, target, NavMesh.AllAreas, path))
        {
            // Überprüft, ob der berechnete Pfad vollständig ist
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;  // Position target ist erreichbar
            }
        }

        return false;  // Position target ist nicht erreichbar
    }


    //wird dazu genutzt, um zu checken, ob Ojekte auf einem gleichen NavMesh Bereich liegen
    //In Methode IsPositionOnSameMesh
    public float edgeDetectionRadius = 0.2f;


    
    // Warteschlange für Positionen, die untersucht werden müssen
    Queue<Vector3> positionsToCheck = new Queue<Vector3>();

    //bekommt eine Startposition und gibt ein Array zurück, welches alle Positionen enthält, 
    //welche in demselben NavMesh-Bereich liegen wie die Startposition. 
    //Also alle Bereiche, die durch NavMesh von der Startposition aus erreichbar sind
    List<Vector3> durchquereNavMeshGebiet(Vector3 startPosition)
    {
        //In dieser Liste werden alle Positionen gespeichert, die auf einem NavMesh Bereich liegen
        List<Vector3> sameMeshPositions = new List<Vector3>();

        positionsToCheck.Enqueue(startPosition);
        sameMeshPositions.Add(startPosition);

        //alle Positionen werden hier gespeichert, die überprüft wurden und außerhalb des NavMesh Bereichs liegen
        List<Vector3> checkedPositions = new List<Vector3>();

        while (positionsToCheck.Count > 0)
        {
            // Nimm die nächste Position aus der Warteschlange
            Vector3 currentPosition = positionsToCheck.Dequeue();

            // Finde benachbarte Positionen auf den Kanten des aktuellen Bereichs
            Vector3[] possiblePositions = new Vector3[]
            {
                currentPosition + new Vector3(1, 0, 0), // rechts
                currentPosition + new Vector3(-1, 0, 0), // links
                currentPosition + new Vector3(0, 0, 1), // vorwärts
                currentPosition + new Vector3(0, 0, -1), // rückwärts
                //oben
                currentPosition + new Vector3(1, 1, 0), // rechts
                currentPosition + new Vector3(-1, 1, 0), // links
                currentPosition + new Vector3(0, 1, 1), // vorwärts
                currentPosition + new Vector3(0, 1, -1), // rückwärts
                //unten
                currentPosition + new Vector3(1, -1, 0), // rechts
                currentPosition + new Vector3(-1, -1, 0), // links
                currentPosition + new Vector3(0, -1, 1), // vorwärts
                currentPosition + new Vector3(0, -1, -1), // rückwärts
            };

            foreach (Vector3 possiblePosition in possiblePositions)
            {
                //wenn Position noch nicht überprüft (sonst entweder außerhalb oder in der gleichen NavMesh)
                if (!checkedPositions.Contains(possiblePosition) && !sameMeshPositions.Contains(possiblePosition))
                {
                    //wenn Position begehbar ist
                    if (IsPositionOnNavMesh(possiblePosition))
                    {
                        //Debug.Log(possiblePosition.ToString() + " ist auf einer NavMesh");

                        // Prüfe, ob die Position auf dem gleichen NavMeshSurface liegt
                        if (IsPositionOnSameMesh(possiblePosition, startPosition))
                        {
                            //Debug.Log(possiblePosition.ToString() + " ist auf GLEICHER NavMesh");
                            sameMeshPositions.Add(possiblePosition);
                            positionsToCheck.Enqueue(possiblePosition);
                        }
                        else
                        {
                            checkedPositions.Add(possiblePosition);
                        }
                    }
                }


            }
        }
        //färbe bei Bedarf die Objekte, die auf demselben NavMesh Bereich liegen
        if (ColorChange)
        {
            changeColor(sameMeshPositions);
        }
        checkIfGoalIsReachable();
        return sameMeshPositions;
    }

    void checkIfGoalIsReachable()
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(PlayerStartPosition, GoalTile.transform.position, NavMesh.AllAreas, path))
        {
            // Überprüft, ob der berechnete Pfad vollständig ist
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                //Ziel ist für Spieler erreichbar
                Debug.Log("Ziel ist für Spieler erreichbar");
                GoalReachable = true;
            }
            else
            {
                
            }
        }
    }



    public bool ColorChange;
    public Color ColorChangeFarbe;
    public bool markOnlyNewPositions;
    public List<Vector3> ColorChangedPositions;


    public bool GoalReachable = false;


    //färbt alle Position in der Liste mit der übergebenen Farbe
    void changeColor(List<Vector3> positions)
    {
        foreach (Vector3 position in positions)
        {
            if (markOnlyNewPositions && ColorChangedPositions.Contains(position))
            {
                //nothing
            }
            else
            {
                GameObject tile = returnObjectAtPosition(position);
                Transform childTransform = tile.transform.GetChild(0);
                GameObject childGameObject = childTransform.gameObject;
                Renderer ren = childGameObject.GetComponent<Renderer>();
                // Zugriff auf den Renderer des Objekts
                if (ren != null)
                {
                    // Ändern der Farbe des Materials
                    ren.material.SetColor("_Color", ColorChangeFarbe);
                    if (markOnlyNewPositions)
                    {
                        ColorChangedPositions.Add(position);
                    }
                    //Debug.Log("BLAU GEFÄRBT an Positon: " + tile.transform.position);
                }
                else
                {
                    Debug.LogWarning("Kein Renderer gefunden! Dieses Objekt kann keine Farbe ändern.");
                }
            }

        }
    }


    //Hilfsmethode für changeColor, gibt das Objekt an der übergebenen Position zurück
    GameObject returnObjectAtPosition(Vector3 position)
    {
        foreach (GameObject obj in AllTiles)
        {
            // Überprüfen, ob die Position des Objekts innerhalb der Toleranz liegt
            if (Vector3.Distance(obj.transform.position, new Vector3(position.x+0.5f, position.y, position.z+0.5f)) < 1f)
            {
                //Debug.Log("Objekt GEFUNDEN!!! " + position);
                return obj; // GameObject gefunden
            }
        }
        Debug.LogWarning("FEHLER bei Objekt Suche mit Objekt: " + position);
        return null;
    }



    //testet, ob ein NavMesh Link zwischen den Positionen möglich ist.
    //Dazu darf die Zielposition nicht bereits erreichbar sein, aber muss auf einer NavMesh liegen 
    //und der Weg zwischen beiden Positionen muss frei sein
    bool isNavMeshLinkPossible(Vector3 start, Vector3 end)
    {
        //if(!reachablePositions.Contains(end) && IsPositionOnNavMesh(end) && !IsPositionReachableOnNavMesh(start, end))
        if(IsPositionOnNavMesh(end) && !IsPositionReachableOnNavMesh(start, end))
        {
            
            if (IsPathClear(start, end, collisionMask))
            {
                return true;
            }
        };
        return false;
    }

    public LayerMask collisionMask;  // Layer, die als Hindernisse zählen




    //erreiche alle durch Sprung erreichbaren Positionen, die auf einer anderen NavMesh liegen
    //dazu wird eine Liste übergeben, die alle Positionen innerhalb einer NavMesh Umgebung enthält. Der Umgebung, die aktuell untersucht wird
    void constructNavMeshLinksToReachablePositions(List<Vector3> positionsInSameNavMesh, Boolean erweiterterAgent)
    {
        foreach (Vector3 position in positionsInSameNavMesh)
        {

            Boolean erweitert = erweiterterAgent;
            //falls in der 2. Iteration nichts passiert, sollte eine 3. Iteration durchgeführt werden, also mit 2 zwischenpositionen
            if (erweitert)
            {
                Vector3 zwischenposition = new Vector3(0,0,0);
                for (int height = -1; height <= 1; height++)
                {
                    zwischenposition = new Vector3(position.x - 4, position.y + height, position.z);
                    untersucheFelderDurchErweitertenSprungErreichbar(position, zwischenposition);
                    zwischenposition = new Vector3(position.x + 4, position.y + height, position.z);
                    untersucheFelderDurchErweitertenSprungErreichbar(position, zwischenposition);
                    zwischenposition = new Vector3(position.x , position.y + height, position.z + 4);
                    untersucheFelderDurchErweitertenSprungErreichbar(position, zwischenposition);
                    zwischenposition = new Vector3(position.x , position.y + height, position.z - 4);
                    untersucheFelderDurchErweitertenSprungErreichbar(position, zwischenposition);
                    /*
                    for (int x = -3; x <= 3; x++)
                    {
                        for (int z = -3; z <= 3; z++)
                        {
                            if ((((x == -1 && z == 0) || (x == 0 && z == -1) || (x == 0 && z == 0) || (x == 0 && z == 1) || (x == 1 && z == 0)) && height == 0) || (x == 0 && z == 0 && height == 0))
                            {
                                //do nothing
                            }
                            else
                            {
                                zwischenposition = new Vector3(position.x + x, position.y + height, position.z + z);
                                untersucheFelderDurchErweitertenSprungErreichbar(position, zwischenposition);
                            }
                        }
                    }
                    */





                }

            }
            else
            {
                untersucheFelderDurchSprungErreichbar(position);
            }
        }
    }

    //durchlaufe mögliche durch Sprung erreichbare Felder
    //Spieler kann im Bereich 3x3 alle Felder erreichen, egal ob Höhe 1, 0 oder -1
    //in einer zentrale Richtung kann er sogar 4 Felder weit springen
    //höhe
    void untersucheFelderDurchSprungErreichbar(Vector3 position)
    {
        for (int height = -1; height <= 1; height++)
        {
            CreateNavMeshLink(position, new Vector3(position.x - 4, position.y + height, position.z));
            CreateNavMeshLink(position, new Vector3(position.x + 4, position.y + height, position.z));
            CreateNavMeshLink(position, new Vector3(position.x, position.y + height, position.z + 4));
            CreateNavMeshLink(position, new Vector3(position.x, position.y + height, position.z - 4));
            for (int x = -3; x <= 3; x++)
            {
                for (int z = -3; z <= 3; z++)
                {
                    if ((((x == -1 && z == 0) || (x == 0 && z == -1) || (x == 0 && z == 0) || (x == 0 && z == 1) || (x == 1 && z == 0)) && height == 0) || (x == 0 && z == 0 && height == 0))
                    {
                        //do nothing
                    }
                    else
                    {
                        CreateNavMeshLink(position, new Vector3(position.x + x, position.y + height, position.z + z));
                    }
                }
            }
        }
    }

    void untersucheFelderDurchErweitertenSprungErreichbar(Vector3 position, Vector3 zwischenposition)
    {
        for (int height = -1; height <= 1; height++)
        {
            ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x - 4, zwischenposition.y + height, zwischenposition.z), zwischenposition);
            ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + 4, zwischenposition.y + height, zwischenposition.z), zwischenposition);
            ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height, zwischenposition.z + 4), zwischenposition);
            ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height, zwischenposition.z - 4), zwischenposition);
            for (int x = -3; x <= 3; x++)
            {
                for (int z = -3; z <= 3; z++)
                {
                    if ((((x == -1 && z == 0) || (x == 0 && z == -1) || (x == 0 && z == 0) || (x == 0 && z == 1) || (x == 1 && z == 0)) && height == 0) || (x == 0 && z == 0 && height == 0))
                    {
                        //do nothing
                    }
                    else
                    {
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + x, zwischenposition.y + height, zwischenposition.z + z), zwischenposition);
                    }
                }
            }
        }
    }


    public void Agent(Vector3 startPosition)
    {
        List<Vector3> l = new List<Vector3>();
        if (!GoalReachable)
        {
            l = durchquereNavMeshGebiet(startPosition);
        }
        if(!GoalReachable) 
        { 
            constructNavMeshLinksToReachablePositions(l, false); 
        }




    }

    private void DebugShow(List<Vector3> positions)
    {
        foreach(Vector3 po in positions)
        {
            Debug.Log(po.ToString());
        }
    }

    bool IsPositionOnNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        //verschiebe Position nach Oben, da NavMesh an Oberfläche ist
        Vector3 PositionAbove = new Vector3(position.x, position.y+0.5f, position.z);
        // Versuche, die Position auf dem NavMesh zu finden, innerhalb eines Radius von maxDistance
        if (NavMesh.SamplePosition(PositionAbove, out hit, 0.2f, NavMesh.AllAreas))
        {
            // Wenn ein Punkt gefunden wurde, liegt die Position innerhalb der NavMeshSurface
            //Debug.Log("NAVMESH");
            return true;
        }
        return false;
    }

    bool IsPositionOnSameMesh(Vector3 position, Vector3 referencePosition)
    {
        NavMeshHit hit; 
        Vector3 positionAbove = new Vector3(position.x, position.y + 0.5f, position.z); 
        Vector3 referencePositionAbove = new Vector3(referencePosition.x, referencePosition.y + 0.5f, referencePosition.z);
        NavMesh.SamplePosition(positionAbove, out hit, edgeDetectionRadius, NavMesh.AllAreas);
        NavMesh.SamplePosition(referencePositionAbove, out NavMeshHit referenceHit, edgeDetectionRadius, NavMesh.AllAreas);

        // Prüfe, ob beide Positionen im gleichen NavMesh-Bereich liegen
        return hit.mask == referenceHit.mask;
    }

    

    void CreateNavMeshLink(Vector3 start, Vector3 end)
    {
        if (isNavMeshLinkPossible(start, end))
        {
            //Debug.Log("link");
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
            positionsToCheck.Enqueue(end);
            //Debug.Log("NEUER NavMesh Link, jetzt Iteration mit NEUEM Punkt: " + end.ToString());
            Agent(end);
        }

    }

    bool erweitertenLinkGesetzt = false;
    public GameObject tile;

    void ErweitertCreateNavMeshLink(Vector3 start, Vector3 end, Vector3 zwischenposition)
    {
        if (!erweitertenLinkGesetzt)
        {
            //Link möglich
            if (IsPositionOnNavMesh(end) && !IsPositionReachableOnNavMesh(start, end))
            {
                // Erstelle ein neues GameObject für den NavMeshLink
                GameObject linkObject = new GameObject("NavMeshLink");
                var navMeshLink = linkObject.AddComponent<NavMeshLink>();

                // Setze die Start- und Endpunkte des Links
                navMeshLink.startPoint = start;
                navMeshLink.endPoint = zwischenposition;

                // Optional: Weitere Einstellungen
                navMeshLink.width = 1.0f;
                navMeshLink.costModifier = -1;
                navMeshLink.bidirectional = true;

                // Aktualisiere den Link
                navMeshLink.UpdateLink();
                positionsToCheck.Enqueue(end);

                // Erstelle ein neues GameObject für den NavMeshLink
                GameObject linkObject2 = new GameObject("NavMeshLink");
                var navMeshLink2 = linkObject2.AddComponent<NavMeshLink>();

                // Setze die Start- und Endpunkte des Links
                navMeshLink2.startPoint = zwischenposition;
                navMeshLink2.endPoint = end;

                // Optional: Weitere Einstellungen
                navMeshLink2.width = 1.0f;
                navMeshLink2.costModifier = -1;
                navMeshLink2.bidirectional = true;

                // Aktualisiere den Link
                navMeshLink2.UpdateLink();
                positionsToCheck.Enqueue(end);
                erweitertenLinkGesetzt = true;
                check(zwischenposition, start, end);
                Agent(end);
            }
        }
    }

    public GameObject newTile;

    //überprüft, ob ein Block erstellt oder entfernt werden muss
    void check(Vector3 zwischenpos, Vector3 startpos, Vector3 endpos)
    {
        if (isAObjectinMap(zwischenpos))
        {
            deleteObjectsBetween(startpos, zwischenpos);
            RemoveObjectsAbove(zwischenpos);
            deleteObjectsBetween(zwischenpos, endpos);
            //entferne die Blöcke oberhalb und alle, die in der NavLink Linie liegen
        }
        else
        {
            //zerstöre alle Objekte, die in der NavLink Linie liegen
            deleteObjectsBetween(startpos, zwischenpos);
            Instantiate(newTile, zwischenpos, Quaternion.identity);
            //unterhalb auch noch mit Objekten füllen
            deleteObjectsBetween(zwischenpos, endpos);
        }
    }

    void RemoveObjectsAbove(Vector3 position)
    {
        RaycastHit hit;

        // Führe einen vertikalen Raycast nach oben durch
        while (Physics.Raycast(position, Vector3.up, out hit, Mathf.Infinity))
        {
            Debug.Log("Entferne Objekt oberhalb der Position: " + hit.collider.name);

            // Entferne das getroffene Objekt
            Destroy(hit.collider.gameObject);

            // Aktualisiere die Position, um direkt hinter dem entfernten Objekt zu starten
            position = hit.point + Vector3.up * 0.01f; // Ein kleiner Abstand, um sicherzustellen, dass du nicht immer das gleiche Objekt triffst
        }

        Debug.Log("Alle Objekte oberhalb der Position wurden entfernt.");
    }


    public GameObject destroyedTile;

    public void deleteObjectsBetween(Vector3 startpos, Vector3 endpos)
    {

        
        

        bool blockiert = true;
        for (int height = 0; height <= 2; height++)
        {
            //verschiebe nach oben, damit er nicht mit den Objekten auf Ebene 0 oder 1 kollidiert (diese sind für den Spieler überquerbar)
            Vector3 start = new Vector3(startpos.x, startpos.y + 1.1f + height, startpos.z);
            Vector3 end = new Vector3(endpos.x, endpos.y + 1.1f + height, endpos.z);

            // Berechne die Richtung vom Startpunkt zum Endpunkt
            Vector3 direction = end - start;

            // Berechne die Distanz zwischen den beiden Punkten
            float distance = direction.magnitude;
            // Führe den Raycast mit der Layer Mask durch
            RaycastHit hit;
            while (blockiert)
            {
                Debug.Log("TT");
                if (Physics.Raycast(start, direction.normalized, out hit, distance, collisionMask))
                {
                    Debug.Log("Weg blockiert von: " + hit.collider.name);
                    Destroy(hit.collider.gameObject);
                    Debug.Log("Entferne Objekt: " + hit.collider.gameObject.transform.position);
                    start = hit.point + direction.normalized * 0.01f;
                    Instantiate(destroyedTile, hit.collider.gameObject.transform.position, Quaternion.identity);
                }
                else
                {
                    blockiert = false;
                }
            }
            blockiert = true;
        }
    }

    bool isThereAObject(Vector3 pos)
    {
        //verschiebe nach oben, damit er nicht mit den Objekten auf Ebene 0 oder 1 kollidiert (diese sind für den Spieler überquerbar)
        Vector3 pos2 = new Vector3(pos.x, pos.y + 0.2f, pos.z);
        // Berechne die Richtung vom Startpunkt zum Endpunkt
        Vector3 direction = pos2 - pos;

        // Berechne die Distanz zwischen den beiden Punkten
        float distance = direction.magnitude;

        // Führe den Raycast mit der Layer Mask durch
        RaycastHit hit;
        if (Physics.Raycast(pos, direction.normalized, out hit, distance, collisionMask))
        {
                // Wenn der Raycast auf etwas trifft, bedeutet das, dass der Weg blockiert ist
                Debug.Log("Weg blockiert von: " + hit.collider.name);
                return true;
        }
        return false;
    }

    bool isAObjectinMap(Vector3 pos)
    {
        Vector3 boxSize = new Vector3(0.9f, 0.9f, 0.9f);
        Collider[] hitColliders = Physics.OverlapBox(pos, boxSize / 2);

        if (hitColliders.Length > 0)
        {

            Debug.Log("Ein Block ist an der Position.");
            return true;
        }
        else
        {
            Debug.Log("Kein Block an der Position.");
            return false;
        }
    }
}
