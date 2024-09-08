using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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

    private bool ZielMitDoppelsprungerreichbar(Vector3 p)
    {
        Vector3 zielposition = GoalTile.transform.position;
        if(Math.Abs(p.y - zielposition.y) < 2 && Math.Abs(p.x - zielposition.x) < 6 && Math.Abs(p.z - zielposition.z) < 6)
        {
            return true;
        }
        return false;
    }

    

    bool IstImSpielBereich(Vector3 p)
    {
        return (0 <= p.x && p.x <= 32  && 0 <= p.z && p.z <= 32);
    }
    //In Bearbeitung

    public List<Vector3> reachablePositions; 


    //speichert die zwischenpos beim erweiterten Sprung
    public Vector3 zwischenpos;
    //gibt die erste Position zurück, die durch einen Doppelsprung erreicht werden kann
    Vector3 erweiteterSprung(Vector3 p)
    {
        
        if(Vector3.Distance(p, GoalTile.transform.position) < 3)
        Debug.Log("erweiteter Sprung mit Startposition "+ p.ToString());    
        Vector3 finalPosition = new Vector3(100, 100, 100); 
        Vector3 Test = new Vector3(100, 100, 100);
        for (int height = 1; height >= 1; height--)
        {
            /*for (int x = 3; x >= -3; x--)
            {
                for (int z = 3; z >= -3; z--)
                {
                    if ((((x == -1 && z == 0) || (x == 0 && z == -1) || (x == 0 && z == 0) || (x == 0 && z == 1) || (x == 1 && z == 0)) && height == 0) || (x == 0 && z == 0 && height == 0))
                    {
                        //do nothing
                    }
                    else
                    {
                        zwischenpos = new Vector3(p.x + x, p.y + height, p.z + z);
                        if (IstImSpielBereich(zwischenpos))
                        {
                            finalPosition = erweiteterSprung2(zwischenpos, p);
                            if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                            {
                                return finalPosition;
                            }
                        }
                        

                    }
                }
            }*/
            for (int x = 0; x <= 3; x++)
            {
                // Positive x zuerst
                
                    int positiveX = x;  // z.B. 1, 2, 3
                    for (int z = 0; z <= 3; z++)
                    {
                        // Positive Z zuerst
                        if (z != 0 || x!=0) // Vermeidet den Fall z = 0
                        {
                            zwischenpos = new Vector3(p.x + x, p.y + height, p.z + z);
                            if (IstImSpielBereich(zwischenpos))
                            {
                                finalPosition = erweiteterSprung2(zwischenpos, p);
                                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                                {
                                    return finalPosition;
                                }
                            }
                            // Negative Z nach positive z
                            int negativeZ = -z;
                            zwischenpos = new Vector3(p.x + x, p.y + height, p.z + negativeZ);
                            if (IstImSpielBereich(zwischenpos))
                            {
                                finalPosition = erweiteterSprung2(zwischenpos, p);
                                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                                {
                                    return finalPosition;
                                }
                            }
                        }


                    }
                    // Negative x nach positive x
                    int negativeX = -x;  // z.B. -1, -2, -3
                    for (int z = 0; z <= 3; z++)
                    {
                        // Positive Z zuerst
                        if (z != 0 || x !=0) // Vermeidet den Fall i = 0
                        {
                            zwischenpos = new Vector3(p.x + negativeX, p.y + height, p.z + z);
                            if (IstImSpielBereich(zwischenpos))
                            {
                                finalPosition = erweiteterSprung2(zwischenpos, p);
                                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                                {
                                    return finalPosition;
                                }
                            }
                            // Negative Z nach positive z
                            int negativeZ = -z;
                            zwischenpos = new Vector3(p.x + x, p.y + height, p.z + negativeZ);
                            if (IstImSpielBereich(zwischenpos))
                            {
                                finalPosition = erweiteterSprung2(zwischenpos, p);
                                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                                {
                                    return finalPosition;
                                }
                            }
                        }


                    }
                
            }

            //Schleifenende


            zwischenpos = new Vector3(p.x - 4, p.y + height, p.z);
            if (IstImSpielBereich(zwischenpos))
            {
                finalPosition = erweiteterSprung2(zwischenpos, p);
                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                {
                    return finalPosition;
                }
            }
            zwischenpos = new Vector3(p.x + 4, p.y + height, p.z);
            if (IstImSpielBereich(zwischenpos))
            {
                finalPosition = erweiteterSprung2(zwischenpos, p);
                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                {
                    return finalPosition;
                }
            }
            zwischenpos = new Vector3(p.x, p.y + height, p.z + 4);
            if (IstImSpielBereich(zwischenpos))
            {
                finalPosition = erweiteterSprung2(zwischenpos, p);
                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                {
                    return finalPosition;
                }
            }
            zwischenpos = new Vector3(p.x, p.y + height, p.z - 4);
            if (IstImSpielBereich(zwischenpos))
            {
                finalPosition = erweiteterSprung2(zwischenpos, p);
                if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                {
                    return finalPosition;
                }
            }
        }
        Debug.LogWarning("kein erweiterter Sprung gefunden");
        return Test;
    }
    Vector3 erweiteterSprung2(Vector3 zwischenpos, Vector3 startPos)
    {
        Debug.Log("erweiteter Sprung mit Zwischenposition " + zwischenpos.ToString());
        Vector3 endPos;
        for (int height = 1; height >= -1; height--)
        {
            /*for (int x = 3; x >= -3; x--)
            {
                for (int z = 3; z >= -3; z--)
                {
                    endPos = new Vector3(zwischenpos.x + x, zwischenpos.y + height, zwischenpos.z + z);
                    if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
                    {
                        return endPos;
                    }
                }
            }*/
            for (int x = 0; x <= 3; x++)
            {
                // Positive x zuerst
                
                    int positiveX = x;  // z.B. 1, 2, 3
                    for (int z = 0; z <= 3; z++)
                    {
                        if (z != 0 || x != 0) // Vermeidet den Fall z = 0
                        {
                            // Positive Z
                            endPos = new Vector3(zwischenpos.x + positiveX, zwischenpos.y + height, zwischenpos.z + z);
                            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
                            {
                                return endPos;
                            }

                            // Negative Z nach positive Z
                            int negativeZ = -z;
                            endPos = new Vector3(zwischenpos.x + positiveX, zwischenpos.y + height, zwischenpos.z + negativeZ);
                            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
                            {
                                return endPos;
                            }
                        }
                    }

                    // Negative x nach positive x
                    int negativeX = -x;
                    for (int z = 0; z <= 3; z++)
                    {
                        if (z != 0 || x!=0) // Vermeidet den Fall z = 0
                        {
                            // Positive Z
                            endPos = new Vector3(zwischenpos.x + negativeX, zwischenpos.y + height, zwischenpos.z + z);
                            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
                            {
                                return endPos;
                            }

                            // Negative Z nach positive Z
                            int negativeZ = -z;
                            endPos = new Vector3(zwischenpos.x + negativeX, zwischenpos.y + height, zwischenpos.z + negativeZ);
                            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
                            {
                                return endPos;
                            }
                        }
                    }
                
            }
            endPos = new Vector3(zwischenpos.x - 4, zwischenpos.y + height, zwischenpos.z);
            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
            {
                return endPos;
            }
            endPos = new Vector3(zwischenpos.x + 4, zwischenpos.y + height, zwischenpos.z);
            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
            {
                return endPos;
            }
            endPos = new Vector3(zwischenpos.x, zwischenpos.y + height, zwischenpos.z + 4);
            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
            {
                return endPos;
            }
            endPos = new Vector3(zwischenpos.x, zwischenpos.y + height, zwischenpos.z - 4);
            if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
            {
                return endPos;
            }
        }
        return new Vector3(100, 100, 100);
    }

    //gibt die erste Position zurück, die durch einen Doppelsprung erreicht werden kann
    Vector3 Sprung1(Vector3 p)
    {
        Vector3 zero = Vector3.zero;
        for(int height =1; height>=-1; height--)
        {
            for (int x = 3; x >= -3; x--)
            {
                zwischenpos = new Vector3(p.x + x, p.y + height, p.z);
                Vector3 a = Sprung2(zwischenpos,p);
                if (a != zero)
                {
                    return a;
                }
            }
            for (int z = 3; z >= -3; z--)
            {
                zwischenpos = new Vector3(p.x, p.y + height, p.z+z);
                Vector3 a = Sprung2(zwischenpos,p);
                if (a != zero)
                {
                    return a;
                }
            }


        }
        return zero;
    }

    Vector3 Sprung2(Vector3 zwischenpos, Vector3 startPos) { 
        Vector3 endPos;
        
        for (int height = 1; height >= -1; height--)
        {
            for (int x = 3; x >= -3; x--)
            {
                endPos= new Vector3(zwischenpos.x + x, zwischenpos.y + height, zwischenpos.z);
                if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
                {
                    return endPos;
                }
            }
            for (int z = 3; z >= -3; z--)
            {
                endPos = new Vector3(zwischenpos.x, zwischenpos.y + height, zwischenpos.z+z);
                if (IsPositionOnNavMesh(endPos) && !IsPositionReachableOnNavMesh(startPos, endPos))
                {
                    return endPos;
                }
            }


        }
        return Vector3.zero;
    }


    private int Iteration = 1;
    public void erweiteterAgent()
    {

        ColorChangeFarbe = Colors.First();
        Colors.Remove(Colors.First());
        Debug.LogWarning("ERWEITERTER AGENT mit Iteration " + Iteration);
        reachablePositions.Add(PlayerStartPosition);
        Vector3 closestPos = FindClosestPosition(GoalTile.transform.position, reachablePositions); //NavMeshs, die nur 1 Position enthalten werden nicht berücksichtigt
        Debug.Log("Nächste Position: " + closestPos.ToString());


        //Debug.Log("Positionen in VavMesh Gebiet:");
        List<Vector3> NavMesh = FindNavMeshFromPos(closestPos);     //Fehler bei closestPos = (0,0,0)
        //foreach(Vector3 pos in NavMesh)
        //{
        //    Debug.Log(pos.ToString());
        //}
        bool gefunden = false;
        bool ZielErreichbar = false;
        //List<Vector3> newPosDurchErweitertenSprung = new List<Vector3>();
        Vector3 newPos = new Vector3();
        if (!GoalReachable)
        {
            List<Vector3> sortedNavMesh = NavMesh
            .OrderByDescending(v => v.x)  // Sortiere nach der X-Koordinate absteigend
            .ThenByDescending(v => v.z)   // Sortiere nach der Z-Koordinate absteigend
            .ToList();
            foreach (Vector3 position in sortedNavMesh)
            {

                if (ZielMitDoppelsprungerreichbar(position)) { ZielErreichbar = true; }
                if (gefunden == false)
                {
                    Vector3 SprungPos = erweiteterSprung(position);
                    if (SprungPos.x != 100 || SprungPos.y != 100 || SprungPos.z != 100)
                    {
                        Debug.Log("Neue Position gefunden " + SprungPos.ToString() + " über Zwischenposition: " + zwischenpos.ToString());
                        reachablePositions.Add(SprungPos);
                        if (ZielErreichbar)
                        {
                            if (SprungPos == GoalTile.transform.position)
                            {

                                gefunden = true;
                                ErweitertCreateNavMeshLink(position, SprungPos, zwischenpos);
                                //newPosDurchErweitertenSprung.Add(SprungPos);
                                newPos = SprungPos;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            gefunden = true;
                            ErweitertCreateNavMeshLink(position, SprungPos, zwischenpos);
                            //newPosDurchErweitertenSprung.Add(SprungPos);
                            newPos = SprungPos;
                        }
                    }
                }

            }
            if (gefunden == false)
            {
                //kein neues Objekt erreicht worden, beende Agent
                //mach dreifachSprung

                foreach(Vector3 position in sortedNavMesh)
                {
                    Vector3[] loru = new Vector3[] {
                    new Vector3(position.x, position.y+1, position.z+1),
                    new Vector3(position.x+1, position.y+1, position.z),
                    new Vector3(position.x, position.y+1, position.z-1),
                    new Vector3(position.x-1, position.y+1, position.z),
                    };

                    foreach(Vector3 p in loru) 
                    {
                        if (!gefunden)
                        {
                            Vector3 SprungPos = erweiteterSprung(p);
                            if (SprungPos.x != 100 || SprungPos.y != 100 || SprungPos.z != 100)
                            {
                                AllTiles.Add(Instantiate(newTile, p, Quaternion.identity));
                                ErweitertCreateNavMeshLink(p, SprungPos, zwischenpos);
                                gefunden = true;
                                Debug.LogWarning("Dreifachsprung " +zwischenpos.ToString());
                            }
                        }
                        
                    }

                    
                }
            }
            if(gefunden == false)
            {
                Debug.LogWarning("Kein Dreifachsprung");
            }

        }
        Agent(newPos);
        if (!GoalReachable)
        {
            if (Iteration < 4)
            {
                Iteration++;
                erweiteterAgent();
            }
            
        }

    }

    private Vector3 erweiteterSprungOhneA(Vector3 p, Vector3 a)
    {
        bool korrekt = false;
        Vector3 t = new Vector3();
        /*
        while (!korrekt)
        {
            t = erweiteterSprung(p);
            if (t.x == a.x && t.z == a.z || t.x == zwischenpos.x && t.z == zwischenpos.z)
            {
                korrekt = true;
            }
        }
        */
        return t;
        
    }

    private Vector3 Drittel(Vector3 start, Vector3 v)
    {
        float x = v.x / 3;
        float y = v.y / 3;
        float z = v.z / 3;
        return new Vector3(start.x + x, start.y + y,start.z + z);
    }
    private Vector3 ZweiDrittel(Vector3 start, Vector3 v)
    {
        float x = v.x *(2/3);
        float y = v.y * (2 / 3);
        float z = v.z * (2 / 3);
        return new Vector3(start.x + x, start.y + y, start.z + z);
    }

    private Vector3 dreifachSprung(List<Vector3> pos)
    {
        foreach(Vector3 v in pos)
        {
            Debug.LogWarning("DreifachSprung mit Vektor " + v.ToString());
            Vector3[] Richtungen = new Vector3[]
            {
            new Vector3(3, 3, 3),
            new Vector3(3, 3, -3),
            new Vector3(-3, 3, -3),
            new Vector3(-3, 3, 3),
            new Vector3(3, -3, 3),
            new Vector3(3, -3, -3),
            new Vector3(-3, -3, -3),
            new Vector3(-3, -3, 3)
            };

            foreach (Vector3 sprung in Richtungen)
            {
                Vector3 final = new Vector3(v.x + sprung.x, v.y + sprung.y, v.z + sprung.z);
                if (IsPositionOnNavMesh(final) && !IsPositionReachableOnNavMesh(v, final))
                {
                    RemoveObjectsAbove(v);
                    Vector3 temp = Drittel(v,sprung);
                    Debug.LogWarning("HH Ist "+temp.ToString() + " ein Drittel von "+v.ToString());
                    Vector3 temp2 = ZweiDrittel(v,sprung);
                    Debug.LogWarning("HH Ist " + temp2.ToString() + " zwei Drittel von " + v.ToString());
                    RemoveObjectsAbove(temp);
                    RemoveObjectsAbove(temp2);
                    RemoveObjectsAbove(final);
                    CreateNavMeshLink(v, temp);
                    CreateNavMeshLink(temp, temp2);
                    CreateNavMeshLink(temp2, final);
                    return final;
                }
            }
        }
        return new Vector3(100, 100, 100);
    }

    List<Vector3> FindNavMeshFromPos(Vector3 pos)
    {
        foreach(List<Vector3> NavMesh in ListOfNavMeshs) 
        {
            if(NavMesh.Contains(pos)) { return NavMesh; }
        }
        Debug.LogWarning("Fehler bei FindNavMeshFromPos");
        return null;
    }

    public Vector3 FindClosestPosition(Vector3 targetPosition, List<Vector3> positions)
    {
        // Initialisiere die Variablen für die nächste Position und den kleinsten Abstand
        Vector3 closestPosition = Vector3.zero;
        float smallestDistance = Mathf.Infinity;

        // Durchlaufe jede Position in der Liste
        foreach (Vector3 position in positions)
        {
            // Berechne den Abstand zwischen der aktuellen Position und der Zielposition
            float distance = Vector3.Distance(targetPosition, position);

            // Wenn dieser Abstand kleiner ist als der bisher kleinste Abstand,
            // speichere diese Position und den neuen kleinsten Abstand
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestPosition = position;
            }
        }

        // Gib die nächste Position zurück
        return closestPosition;
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

    List<List<Vector3>> ListOfNavMeshs = new List<List<Vector3>>();

    
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
                if (!checkedPositions.Contains(possiblePosition) && !sameMeshPositions.Contains(possiblePosition)&&!reachablePositions.Contains(possiblePosition))
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
                            reachablePositions.Add(possiblePosition);
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
        checkIfGoalIsReachable(startPosition);
        ListOfNavMeshs.Add(sameMeshPositions);
        return sameMeshPositions;
    }

    void checkIfGoalIsReachable(Vector3 pos)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(pos, GoalTile.transform.position, NavMesh.AllAreas, path))
        {
            // Überprüft, ob der berechnete Pfad vollständig ist
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                //Ziel ist für Spieler erreichbar
                Debug.LogWarning("Ziel ist für Spieler erreichbar");
                GoalReachable = true;

            }
            else
            {
                //Debug.LogWarning("Ziel ist für Spieler NICHT erreichbar");
            }
        }
    }



    public static bool ColorChange;
    public Color ColorChangeFarbe;
    public  List<Color> Colors;
    public bool markOnlyNewPositions;
    public List<Vector3> ColorChangedPositions;
    public List<Vector3> GoalPositions;


    public bool GoalReachable = false;

    public void StartAgent(Vector3 pos)
    {

        ColorChangeFarbe = Colors.First();
        Colors.Remove(Colors.First());
        Agent(pos);
    }

    //färbt alle Position in der Liste mit der übergebenen Farbe
    void changeColor(List<Vector3> positions)
    {
        foreach (Vector3 position in positions)
        {
            if (markOnlyNewPositions && ColorChangedPositions.Contains(position) || GoalPositions.Contains(position))
            {
                //nothing
            }
            else
            {
                GameObject tile = returnObjectAtPosition(position);
                if(tile.transform.GetChild(0) != null)
                {
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
                else
                {
                    Debug.LogWarning(tile.transform.position.ToString() +" konnte nicht gefunden werden! (ist null)");
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
            CreateNavMeshLink(position, new Vector3(position.x - 4, position.y + height, position.z));
            CreateNavMeshLink(position, new Vector3(position.x + 4, position.y + height, position.z));
            CreateNavMeshLink(position, new Vector3(position.x, position.y + height, position.z + 4));
            CreateNavMeshLink(position, new Vector3(position.x, position.y + height, position.z - 4));
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


    List<Vector3> l = new List<Vector3>();
    public void Agent(Vector3 startPosition)
    {

        Debug.Log("AA aus TEST");
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

    void CreateNavMeshLink2(Vector3 start, Vector3 end, List<Vector3> list)
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
            //positionsToCheck.Enqueue(end);
            //Debug.Log("NEUER NavMesh Link, jetzt Iteration mit NEUEM Punkt: " + end.ToString());
            list.Add(end);

        }

    }

    void CreateNavMeshLinkMitBeidenAgenten(Vector3 start, Vector3 end)
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
            LinkGesetzt = true;
            //Debug.Log("NEUER NavMesh Link, jetzt Iteration mit NEUEM Punkt: " + end.ToString());
            List<Vector3> l = new List<Vector3>();
            if (!GoalReachable)
            {
                l = durchquereNavMeshGebiet(end);
            }
            if (!GoalReachable)
            {
                constructNavMeshLinksToReachablePositions(l, false);
            }
            if (!GoalReachable)
            {
                constructNavMeshLinksToReachablePositions(l, true);
            }

            
        }

    }
    bool LinkGesetzt = false;
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
                //erweitertenLinkGesetzt = true;
                check(zwischenposition, start, end);
                
                pos = end;
                surface.BuildNavMesh();
                
            }
        }
    }

    public GameObject newTile;
    public Vector3 pos;

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
            AllTiles.Add(Instantiate(newTile, zwischenpos, Quaternion.identity));
            //unterhalb auch noch mit Objekten füllen
            deleteObjectsBetween(zwischenpos, endpos);
        }
        surface.RemoveData();
        surface.BuildNavMesh() ;
    }

    void RemoveObjectsAbove(Vector3 position)
    {
        Debug.LogWarning("Remove Objects Above " + position.ToString());
        RaycastHit hit;
        List<Vector3> positions = new List<Vector3>();

        // Führe einen vertikalen Raycast nach oben durch
        while (Physics.Raycast(position, Vector3.up, out hit, Mathf.Infinity))
        {
            Debug.Log("Entferne Objekt oberhalb der Position: " + hit.collider.name);

            positions.Add(hit.collider.gameObject.transform.position);


            // Entferne das getroffene Objekt
            Destroy(hit.collider.gameObject.transform.parent.gameObject); 
            

            // Aktualisiere die Position, um direkt hinter dem entfernten Objekt zu starten
            position = hit.point + Vector3.up * 0.01f; // Ein kleiner Abstand, um sicherzustellen, dass du nicht immer das gleiche Objekt triffst
        }

        List<Vector3> CreatedPositions = new List<Vector3>();
        foreach (Vector3 pos in positions)
        {
            if (!CreatedPositions.Contains(pos))
            {
                CreatedPositions.Add(pos);
                GameObject ob = Instantiate(destroyedTile, pos, Quaternion.identity);
            }
        }

        Debug.Log("Alle Objekte oberhalb der Position wurden entfernt.");
    }


    public GameObject destroyedTile;

    public void deleteObjectsBetween(Vector3 startpos, Vector3 endpos)
    {

        
        
        List<Vector3> positions = new List<Vector3>();
        bool blockiert = true;
        if(startpos.x !=  endpos.x && startpos.z != endpos.z) 
        {
            //links und rechts wird auch gelöscht
            for (int x = 1; x >= -1; x = x - 1)
            {
                for (int height = 0; height <= 3; height++)
                {
                    //verschiebe nach oben, damit er nicht mit den Objekten auf Ebene 0 oder 1 kollidiert (diese sind für den Spieler überquerbar)
                    Vector3 start = new Vector3(startpos.x + x, startpos.y + 1.1f + height, startpos.z);
                    Vector3 end = new Vector3(endpos.x + x, endpos.y + 1.1f + height, endpos.z);

                    // Berechne die Richtung vom Startpunkt zum Endpunkt
                    Vector3 direction = end - start;

                    // Berechne die Distanz zwischen den beiden Punkten
                    float distance = direction.magnitude;
                    // Führe den Raycast mit der Layer Mask durch
                    RaycastHit hit;

                    int safetyCounter = 100;  // Begrenze die Anzahl der Versuche
                    while (blockiert && safetyCounter > 0)
                    {
                        Debug.Log("TT");
                        if (Physics.Raycast(start, direction.normalized, out hit, distance, collisionMask))
                        {
                            Debug.Log("Weg blockiert von: " + hit.collider.name);
                            Destroy(hit.collider.gameObject.transform.parent.gameObject);
                            Debug.Log("Entferne Objekt: " + hit.collider.gameObject.transform.position);
                            start = hit.point + direction.normalized * 0.1f;
                            direction = end - start;
                            distance = direction.magnitude;
                            positions.Add(hit.collider.gameObject.transform.position);
                        }
                        else
                        {
                            blockiert = false;
                        }
                        safetyCounter--;
                    }

                    if (safetyCounter == 0)
                    {
                        Debug.LogError("Endlosschleife verhindert");
                        blockiert = false;
                    }
                    blockiert = true;
                }
            }
        }
        else
        {
            for (int height = 0; height <= 3; height++)
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

                int safetyCounter = 100;  // Begrenze die Anzahl der Versuche
                while (blockiert && safetyCounter > 0)
                {
                    Debug.Log("TT");
                    if (Physics.Raycast(start, direction.normalized, out hit, distance, collisionMask))
                    {
                        Debug.Log("Weg blockiert von: " + hit.collider.name);
                        Destroy(hit.collider.gameObject.transform.parent.gameObject);
                        Debug.Log("Entferne Objekt: " + hit.collider.gameObject.transform.position);
                        start = hit.point + direction.normalized * 0.1f;
                        direction = end - start;
                        distance = direction.magnitude;
                        positions.Add(hit.collider.gameObject.transform.position);
                    }
                    else
                    {
                        blockiert = false;
                    }
                    safetyCounter--;
                }

                if (safetyCounter == 0)
                {
                    Debug.LogError("Endlosschleife verhindert");
                    blockiert = false;
                }
                blockiert = true;
            }
        }
        
        
        List<Vector3> CreatedPositions = new List<Vector3>();
        foreach(Vector3 pos in positions)
        {
            if (!CreatedPositions.Contains(pos))
            {
                CreatedPositions.Add(pos);
                GameObject ob = Instantiate(destroyedTile, pos, Quaternion.identity);
                Collider objectCollider = ob.GetComponent<Collider>();
                if (objectCollider != null)
                {
                    Debug.LogWarning("TESTTESTTEST");
                    objectCollider.enabled = false; // Deaktiviert den Collider
                }
                ob.GetComponent<Collider>().enabled = false;
                ob.GetComponentInChildren<Collider>().enabled = false;
            }
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
