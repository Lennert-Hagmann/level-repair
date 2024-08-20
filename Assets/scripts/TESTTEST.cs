using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private void Start()
    {

        
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

    public void erweiteterAgent(Vector3 startPosition)
    {
        List<Vector3> l = durchquereNavMeshGebiet(startPosition);
        constructNavMeshLinksToReachablePositions(l,true);
    }

    //�berpr�ft, ob bei der Verbindung zwischen beiden Bl�cken ein Block im Weg ist. ein Raycast 1,1 Einheiten weiter oben und 2,1 Einheiten weiter oben verwendet. 
    //1,1 Einheiten, da ansonsten der �bergang von (0,0,0) zu (1,1,0) als blockiert angesehen wird. Sollte sich 1 Einheit �berhalb des Blocks etwas befinden, dann versperrt dies sowieso nicht den Weg, 
    //da der Spieler einfach draufspringen kann
    public bool IsPathClear(Vector3 p1, Vector3 p2, LayerMask collisionMask)
    {
        Vector3 startPoint = new Vector3(p1.x, p1.y + 1.1f, p1.z);
        Vector3 endPoint = new Vector3(p2.x, p2.y + 1.1f, p2.z);
        // Berechne die Richtung vom Startpunkt zum Endpunkt
        Vector3 direction = endPoint - startPoint;

        // Berechne die Distanz zwischen den beiden Punkten
        float distance = direction.magnitude;

        // F�hre den Raycast mit der Layer Mask durch
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
        Debug.Log("Weg ist frei.von " + p1.ToString() + " zu " + p2.ToString());
        return true;
    }


    public List<GameObject> AllTiles;

    //TT
    bool IsPositionReachableOnNavMesh(Vector3 start, Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();

        // Berechnet den Pfad vom Startpunkt zum Zielpunkt
        if (NavMesh.CalculatePath(start, target, NavMesh.AllAreas, path))
        {
            // �berpr�ft, ob der berechnete Pfad vollst�ndig ist
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;  // Position A ist erreichbar
            }
        }

        return false;  // Position A ist nicht erreichbar
    }



    public float maxVerticalDistance = 1.0f;
    public float maxHorizontalDistance = 3.0f;
    public float edgeDetectionRadius = 0.2f;


    //bekommt eine Startposition und gibt ein Array zur�ck, welches alle Positionen enth�lt, welche in demselben NavMesh-Bereich liegen wie die Startposition. Also alle Bereiche, die durch NavMesh von der Startposition aus erreichbar sind
    // Warteschlange f�r Positionen, die untersucht werden m�ssen
    Queue<Vector3> positionsToCheck = new Queue<Vector3>();
    List<Vector3> durchquereNavMeshGebiet(Vector3 startPosition)
    {
        List<Vector3> sameMeshPositions = new List<Vector3>();

        
        positionsToCheck.Enqueue(startPosition);
        sameMeshPositions.Add(startPosition);


        List<Vector3> checkedPositions = new List<Vector3>();

        while (positionsToCheck.Count > 0)
        {
            // Nimm die n�chste Position aus der Warteschlange
            Vector3 currentPosition = positionsToCheck.Dequeue();

            // Finde benachbarte Positionen auf den Kanten des aktuellen Bereichs
            Vector3[] possiblePositions = new Vector3[]
            {
                currentPosition + new Vector3(1, 0, 0), // rechts
                currentPosition + new Vector3(-1, 0, 0), // links
                currentPosition + new Vector3(0, 0, 1), // vorw�rts
                currentPosition + new Vector3(0, 0, -1), // r�ckw�rts
                //oben
                currentPosition + new Vector3(1, 1, 0), // rechts
                currentPosition + new Vector3(-1, 1, 0), // links
                currentPosition + new Vector3(0, 1, 1), // vorw�rts
                currentPosition + new Vector3(0, 1, -1), // r�ckw�rts
                //unten
                currentPosition + new Vector3(1, -1, 0), // rechts
                currentPosition + new Vector3(-1, -1, 0), // links
                currentPosition + new Vector3(0, -1, 1), // vorw�rts
                currentPosition + new Vector3(0, -1, -1), // r�ckw�rts
            };

            foreach (Vector3 possiblePosition in possiblePositions)
            {
                if (!checkedPositions.Contains(possiblePosition) && !sameMeshPositions.Contains(possiblePosition))
                {
                    if (IsPositionOnNavMesh(possiblePosition))
                    {
                        Debug.Log(possiblePosition.ToString() + " ist auf einer NavMesh");
                        // Pr�fe, ob die Position auf dem gleichen NavMeshSurface liegt
                        if (IsPositionOnSameMesh(possiblePosition, startPosition))
                        {

                            Debug.Log(possiblePosition.ToString() + " ist auf GLEICHER NavMesh");
                            sameMeshPositions.Add(possiblePosition);
                            positionsToCheck.Enqueue(possiblePosition);
                        }
                        else
                        {
                            checkedPositions.Add(possiblePosition);
                            // Finde erreichbare Positionen in einem anderen NavMesh-Bereich
                            //if (IsPositionReachable(currentPosition, possiblePosition))
                            //{
                            // Erstelle einen NavMeshLink zwischen den beiden Bereichen
                            //CreateNavMeshLink(currentPosition, possiblePosition);
                            //}
                        }
                    }
                }


            }
            //
        }
        if (ColorChange)
        {
            changeColorToBlue(sameMeshPositions);
        }
        return sameMeshPositions;
    }

    public bool ColorChange;

    void changeColorToBlue(List<Vector3> positions)
    {
        foreach (Vector3 position in positions)
        {
            GameObject tile = returnObjectAtPosition(position);
            Transform childTransform = tile.transform.GetChild(0);
            GameObject childGameObject = childTransform.gameObject;
            Renderer ren = childGameObject.GetComponent<Renderer>();
            // Zugriff auf den Renderer des Objekts
            if (ren != null)
            {
                // �ndern der Farbe des Materials
                ren.material.SetColor("_Color", Color.blue);
                //Debug.Log("BLAU GEF�RBT an Positon: " + tile.transform.position);
            }
            else
            {
                Debug.LogWarning("Kein Renderer gefunden! Dieses Objekt kann keine Farbe �ndern.");
            }
        }
    }

    GameObject returnObjectAtPosition(Vector3 position)
    {
        foreach (GameObject obj in AllTiles)
        {
            // �berpr�fen, ob die Position des Objekts innerhalb der Toleranz liegt
            if (Vector3.Distance(obj.transform.position, new Vector3(position.x+0.5f, position.y, position.z+0.5f)) < 1f)
            {
                //Debug.Log("Objekt GEFUNDEN!!! " + position);
                return obj; // GameObject gefunden
            }
        }
        Debug.LogWarning("FEHLER bei Objekt Suche mit Objekt: " + position);

        return null;
    }


    bool isNavMeshLinkPossible(Vector3 start, Vector3 end)
    {
        if(!reachablePositions.Contains(end) && IsPositionOnNavMesh(end) && !IsPositionReachableOnNavMesh(start, end))
        {
            
            if (IsPathClear(start, end, collisionMask))
            {
                return true;
            }
        };
        return false;
    }

    public LayerMask collisionMask;  // Layer, die als Hindernisse z�hlen
    public float height = 2f;  // H�he, die abgedeckt werden soll
    public int rayCount = 5;   // Anzahl der Raycasts in der H�he



    bool CheckForCollisionWithNarrowRaycast(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        direction.Normalize();

        // F�hre mehrere schmale Raycasts entlang der vertikalen Linie durch
        for (int i = 0; i < rayCount; i++)
        {
            float currentHeight = i * (height / (rayCount - 1));
            Vector3 rayStart = start + Vector3.up * currentHeight;

            // Ein schmaler Raycast wird an der aktuellen H�he ausgef�hrt
            if (Physics.Raycast(rayStart, direction, out RaycastHit hit, distance, collisionMask))
            {
                Debug.Log("Kollision auf H�he " + currentHeight + " mit " + hit.collider.name);
                return true;
            }
        }

        return false;
    }

    public bool IsPathClear(Vector3 startPoint, Vector3 endPoint)
    {
        // Berechne die Richtung vom Startpunkt zum Endpunkt
        Vector3 direction = endPoint - startPoint;

        // Berechne die Distanz zwischen den beiden Punkten
        float distance = direction.magnitude;

        // F�hre den Raycast durch
        RaycastHit hit;
        if (Physics.Raycast(startPoint, direction.normalized, out hit, distance))
        {
            // Wenn der Raycast auf etwas trifft, bedeutet das, dass der Weg blockiert ist
            Debug.Log("Weg blockiert von: " + hit.collider.name);
            return false;
        }

        // Wenn der Raycast auf nichts trifft, ist der Weg frei
        Debug.Log("Weg ist frei.");
        return true;
    }

    bool ObjectBetween2Points(Vector3 start, Vector3 end)
    {
        return false;
        bool hasCollision = IsPathClear(start, end);
        if (hasCollision)
        {
            Debug.Log("Es gibt eine Kollision auf dem Weg zwischen Position A und Position B.");
            return true;
        }
        else
        {
            Debug.Log("Kein Hindernis auf dem Weg zwischen Position A und Position B.");
            return false;
        }
    }


    //erreiche alle durch Sprung erreichbaren Positionen, die auf einer anderen NavMesh liegen
    // Finde erreichbare Positionen anderer NavMesh Bereiche innerhalb einer Begrenzung
    //for NavMeshLink Konstruktion, beinhaltet alle Positionen, zu denen ein NavMeshLink m�glich ist. Also, durch einen Sprung erreichbar
    List<Vector3> reachablePositions = new List<Vector3>();
    void constructNavMeshLinksToReachablePositions(List<Vector3> positionsInSameNavMesh, Boolean erweiterterAgent)
    {
        foreach (Vector3 position in positionsInSameNavMesh)
        {

            Boolean erweitert = erweiterterAgent;
            //falls in der 2. Iteration nichts passiert, sollte eine 3. Iteration durchgef�hrt werden, also mit 2 zwischenpositionen
            if (erweitert)
            {
                Vector3 zwischenposition = new Vector3(0,0,0);
                for (int height = -1; height <= 1; height++)
                {
                    zwischenposition = new Vector3(position.x - 4, position.y + height, position.z);
                    for (int height2 = -1; height2 <= 1; height2++)
                    {
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x - 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z + 4), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z - 4), zwischenposition);
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
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + x, zwischenposition.y + height2, zwischenposition.z + z), zwischenposition);
                                }
                            }
                        }
                    }
                    zwischenposition = new Vector3(position.x + 4, position.y + height, position.z);
                    for (int height2 = -1; height2 <= 1; height2++)
                    {
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x - 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z + 4), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z - 4), zwischenposition);
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
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + x, zwischenposition.y + height2, zwischenposition.z + z), zwischenposition);
                                }
                            }
                        }
                    }
                    zwischenposition = new Vector3(position.x , position.y + height, position.z + 4);
                    for (int height2 = -1; height2 <= 1; height2++)
                    {
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x - 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z + 4), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z - 4), zwischenposition);
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
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + x, zwischenposition.y + height2, zwischenposition.z + z), zwischenposition);
                                }
                            }
                        }
                    }
                    zwischenposition = new Vector3(position.x , position.y + height, position.z - 4);
                    for (int height2 = -1; height2 <= 1; height2++)
                    {
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x - 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + 4, zwischenposition.y + height2, zwischenposition.z), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z + 4), zwischenposition);
                        ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height2, zwischenposition.z - 4), zwischenposition);
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
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + x, zwischenposition.y + height2, zwischenposition.z + z), zwischenposition);
                                }
                            }
                        }
                    }
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
                                for (int height3 = -1; height3 <= 1; height3++)
                                {
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x - 4, zwischenposition.y + height3, zwischenposition.z), zwischenposition);
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + 4, zwischenposition.y + height3, zwischenposition.z), zwischenposition);
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height3, zwischenposition.z + 4), zwischenposition);
                                    ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x, zwischenposition.y + height3, zwischenposition.z - 4), zwischenposition);
                                    for (int x1 = -3; x <= 3; x++)
                                    {
                                        for (int z1 = -3; z <= 3; z++)
                                        {
                                            if ((((x1 == -1 && z1 == 0) || (x1 == 0 && z1 == -1) || (x1 == 0 && z1 == 0) || (x1 == 0 && z1 == 1) || (x1 == 1 && z1 == 0)) && height == 0) || (x1 == 0 && z1 == 0 && height == 0))
                                            {
                                                //do nothing
                                            }
                                            else
                                            {
                                                ErweitertCreateNavMeshLink(position, new Vector3(zwischenposition.x + x1, zwischenposition.y + height3, zwischenposition.z + z1), zwischenposition);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }





                }

            }
            else
            {
                //durchlaufe m�gliche durch Sprung erreichbare Felder
                //Spieler kann im Bereich 3x3 alle Felder erreichen, egal ob H�he 1, 0 oder -1
                //in einer zentrale Richtung kann er sogar 4 Felder weit springen
                //h�he
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


                /*
                Debug.Log("Durchlauf mit Position: " + position.ToString()); //DEBUG
                for (int distance = 2; distance <= 4; distance++)
                {
                    for (int height = -1; height <= 1; height++)
                    {
                        Vector3 p1 = new Vector3(position.x + distance, position.y + height, position.z);
                        Vector3 p2 = new Vector3(position.x - distance, position.y + height, position.z);
                        Vector3 p3 = new Vector3(position.x, position.y + height, position.z + distance);
                        Vector3 p4 = new Vector3(position.x, position.y + height, position.z - distance);
                        CreateNavMeshLink(position, p1);
                        CreateNavMeshLink(position, p2);
                        CreateNavMeshLink(position, p3);
                        CreateNavMeshLink(position, p4);

                        if (distance == 1) //durchlaufe diagonal
                        {
                            Vector3 p5 = new Vector3(position.x - distance, position.y + height, position.z - distance);
                            Vector3 p6 = new Vector3(position.x + distance, position.y + height, position.z + distance);
                            CreateNavMeshLink(position, p5);
                            CreateNavMeshLink(position, p6);
                        }
                        else if (distance == 2)
                        {
                            Vector3 p5 = new Vector3(position.x - distance, position.y + height, position.z - distance);
                            Vector3 p6 = new Vector3(position.x - 1, position.y + height, position.z - distance);
                            Vector3 p7 = new Vector3(position.x - distance, position.y + height, position.z - 1);
                            Vector3 p8 = new Vector3(position.x + distance, position.y + height, position.z + distance);
                            Vector3 p9 = new Vector3(position.x + 1, position.y + height, position.z + distance);
                            Vector3 p10 = new Vector3(position.x + distance, position.y + height, position.z + 1);
                            CreateNavMeshLink(position, p5);
                            CreateNavMeshLink(position, p6);
                            CreateNavMeshLink(position, p7);
                            CreateNavMeshLink(position, p8);
                            CreateNavMeshLink(position, p9);
                            CreateNavMeshLink(position, p10);
                        }
                        else if (distance == 3)
                        {
                            Vector3 p5 = new Vector3(position.x - distance, position.y + height, position.z - distance);
                            Vector3 p6 = new Vector3(position.x - 1, position.y + height, position.z - distance);
                            Vector3 p7 = new Vector3(position.x - 2, position.y + height, position.z - distance);
                            Vector3 p8 = new Vector3(position.x - distance, position.y + height, position.z - 1);
                            Vector3 p9 = new Vector3(position.x - distance, position.y + height, position.z - 2);
                            Vector3 p10 = new Vector3(position.x + distance, position.y + height, position.z + distance);
                            Vector3 p11 = new Vector3(position.x + 1, position.y + height, position.z + distance);
                            Vector3 p12 = new Vector3(position.x + 2, position.y + height, position.z + distance);
                            Vector3 p13 = new Vector3(position.x + distance, position.y + height, position.z + 1);
                            Vector3 p14 = new Vector3(position.x + distance, position.y + height, position.z + 2);
                            CreateNavMeshLink(position, p5);
                            CreateNavMeshLink(position, p6);
                            CreateNavMeshLink(position, p7);
                            CreateNavMeshLink(position, p8);
                            CreateNavMeshLink(position, p9);
                            CreateNavMeshLink(position, p10);
                            CreateNavMeshLink(position, p11);
                            CreateNavMeshLink(position, p12);
                            CreateNavMeshLink(position, p13);
                            CreateNavMeshLink(position, p14);
                        }
                    }
                }
                */
                DebugShow(reachablePositions);



            }
        }
    }


    public void GenerateNavMeshLinks(Vector3 startPosition)
    {
        
        List<Vector3> l = durchquereNavMeshGebiet(startPosition);
        constructNavMeshLinksToReachablePositions(l, false);




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
        //verschiebe Position nach Oben, da NavMesh an Oberfl�che ist
        Vector3 PositionAbove = new Vector3(position.x, position.y+0.5f, position.z);
        // Versuche, die Position auf dem NavMesh zu finden, innerhalb eines Radius von maxDistance
        if (NavMesh.SamplePosition(PositionAbove, out hit, 0.2f, NavMesh.AllAreas))
        {
            // Wenn ein Punkt gefunden wurde, liegt die Position innerhalb der NavMeshSurface
            Debug.Log("NAVMESH");
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

        // Pr�fe, ob beide Positionen im gleichen NavMesh-Bereich liegen
        return hit.mask == referenceHit.mask;
    }

    bool IsPositionReachable(Vector3 from, Vector3 to)
    {
        float verticalDistance = Mathf.Abs(to.y - from.y);
        float horizontalDistance = Vector3.Distance(new Vector3(to.x, 0, to.z), new Vector3(from.x, 0, from.z));
        Debug.Log("VERTIKALE DISTANZ: " + verticalDistance + " ,   HORIZONTALE DISTANZ: " + horizontalDistance);
        return verticalDistance <= maxVerticalDistance && horizontalDistance <= maxHorizontalDistance && horizontalDistance > 0;
    }

    void CreateNavMeshLink(Vector3 start, Vector3 end)
    {
        if (isNavMeshLinkPossible(start, end))
        {
            reachablePositions.Add(end);
            Debug.Log("link");
            // Erstelle ein neues GameObject f�r den NavMeshLink
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
            Debug.Log("NEUER NavMesh Link, jetzt Iteration mit NEUEM Punkt: " + end.ToString());
            GenerateNavMeshLinks(end);
        }

    }

    void ErweitertCreateNavMeshLink(Vector3 start, Vector3 end, Vector3 zwischenposition)
    {

        //Wenn Link m�glich
        //Mach Zwischenposition erreichbar (entweder durch Hinzuf�gen eines Blocks oder durch Hinwegnahme anderer Bl�cke), f�rbe diesen Pink
        //Link von start zu zwischen und von zwischen zu end
        if (isNavMeshLinkPossible(start, end))
        {
            reachablePositions.Add(end);
            Debug.Log("link");
            // Erstelle ein neues GameObject f�r den NavMeshLink
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
            Debug.Log("NEUER NavMesh Link, jetzt Iteration mit NEUEM Punkt: " + end.ToString());
            GenerateNavMeshLinks(end);
        }

    }
}
