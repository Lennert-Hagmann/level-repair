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
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class AgentScript : MonoBehaviour
{

    public NavMeshSurface surface;
    public GameObject player;
    public Vector3 PlayerStartPosition;
    public GameObject GoalTile;

    private bool statistics = true; //für Statistiken, entfernt andere Logs
    

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
        int b;
        if(difficulty.Difficulty == 2) { b = 32; }
        else { b = 16; }
        return (0 <= p.x && p.x <= b  && 0 <= p.z && p.z <= b);
    }

    public List<Vector3> reachablePositions; 


    //speichert die zwischenpos beim erweiterten Sprung
    public Vector3 zwischenpos;
    //gibt die erste Position zurück, die durch einen Doppelsprung erreicht werden kann
    Vector3 erweiteterSprung(Vector3 p)
    {
        if (!statistics) { Debug.Log("erweiteter Sprung mit Startposition " + p.ToString()); }    
        Vector3 finalPosition = new Vector3(100, 100, 100); 
        Vector3 Test = new Vector3(100, 100, 100);
        for (int height = 1; height >= 1; height--)
        {
            
            for (int x = 0; x <= 3; x++)
            {

                // Positive x zuerst
                //int positiveX = x;  // z.B. 1, 2, 3
                    for (int z = 0; z <= 3; z++)
                    {
                        // Positive Z zuerst
                        if (z != 0 || x!=0) // Vermeidet den Fall z = 0 0der x=0
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
                // Negative x 
                int negativeX = -x;  // z.B. -1, -2, -3
                    for (int z = 0; z <= 3; z++)
                    {
                        // Positive Z zuerst
                        if (z != 0 || x !=0) // Vermeidet den 0 Fall
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
        if (!statistics) { Debug.LogWarning("kein erweiterter Sprung gefunden"); }
        return Test;
    }



    Vector3 erweiteterSprung2(Vector3 zwischenpos, Vector3 startPos)
    {
        if (!statistics) { Debug.Log("erweiteter Sprung mit Zwischenposition " + zwischenpos.ToString()); }
        Vector3 endPos;
        for (int height = 1; height >= -1; height--)
        {
            
            for (int x = 0; x <= 3; x++)
            {
                // Positive x zuerst
                
                    int positiveX = x;  // z.B. 1, 2, 3
                    for (int z = 0; z <= 3; z++)
                    {
                        if (z != 0 || x != 0) 
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
                        if (z != 0 || x!=0) 
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




    private List<Vector3> untersuchtePos = new List<Vector3>();

    //Iteration des erweiterten Agenten
    private int Iteration = 1;
    public void erweiteterAgent()
    {

        ColorChangeFarbe = Colors.First();
        Colors.Remove(Colors.First());
        //Debug.LogWarning("ERWEITERTER AGENT mit Iteration " + Iteration);
        reachablePositions.Add(PlayerStartPosition);
        Vector3 closestPos = FindClosestPosition(GoalTile.transform.position, reachablePositions); //NavMeshs, die nur 1 Position enthalten werden nicht berücksichtigt
        checkIfGoalIsReachable(closestPos);
        if (!statistics) { Debug.Log("Nächste Position: " + closestPos.ToString()); }


        List<Vector3> NavMesh = FindNavMeshFromPos(closestPos);                                                                                                                                                                                     //Fehler bei closestPos = (0,0,0)
        bool gefunden = false;
        bool ZielErreichbar = false;
        bool NichtReparierbar = false;
        Vector3 newPos = new Vector3();
        if (!GoalReachable)
        {
            int counter = 0;
            while ((gefunden == false && counter < 10) && !NichtReparierbar)
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
                            if (!statistics) { Debug.Log("Neue Position gefunden " + SprungPos.ToString() + " über Zwischenposition: " + zwischenpos.ToString()); }
                            reachablePositions.Add(SprungPos);
                            if (ZielErreichbar)
                            {
                                if (SprungPos == GoalTile.transform.position)
                                {

                                    gefunden = true;
                                    ErweitertCreateNavMeshLink(position, SprungPos, zwischenpos);
                                    newPos = SprungPos;
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                gefunden = true;
                                try
                                {
                                    ErweitertCreateNavMeshLink(position, SprungPos, zwischenpos);
                                    newPos = SprungPos;

                                }
                                catch(Exception ex) { Debug.LogException(ex); }
                            }
                        }
                    }

                }
                /*
                if (gefunden == false)
                {
                    //mach dreifachSprung
                    //aus Zeitgründen nicht fertiggestellt
                    
                    foreach (Vector3 position in sortedNavMesh)
                    {
                        Vector3[] loru = new Vector3[] {
                        new Vector3(position.x, position.y+1, position.z+1),
                        new Vector3(position.x+1, position.y+1, position.z),
                        new Vector3(position.x, position.y+1, position.z-1),
                        new Vector3(position.x-1, position.y+1, position.z),
                        };

                        foreach (Vector3 p in loru)
                        {
                            if (!gefunden)
                            {
                                Vector3 SprungPos = erweiteterSprung3(p, position);
                                if (SprungPos.x != 100 || SprungPos.y != 100 || SprungPos.z != 100)
                                {
                                    AllTiles.Add(Instantiate(newTile, p, Quaternion.identity));
                                    AllTiles.Add(Instantiate(newTile, position, Quaternion.identity));
                                    RemoveObjectsAbove(p);
                                    ErweitertCreateNavMeshLink(p, SprungPos, zwischenpos);
                                    gefunden = true;
                                    if (!statistics) { Debug.LogWarning("Dreifachsprung " + zwischenpos.ToString()); }
                                    reachablePositions.Add(SprungPos);
                                    surface.BuildNavMesh();

                                    List<Vector3> n = new List<Vector3>();
                                    n.Add(zwischenpos);
                                    ListOfNavMeshs.Add(n);
                                    List<Vector3> na = new List<Vector3>();
                                    na.Add(SprungPos);
                                    ListOfNavMeshs.Add(na);

                                }
                            }

                        }


                    }
                    
                }
                if (gefunden == false)
                {
                    if (!statistics) { Debug.LogWarning("Kein Dreifachsprung"); }
                }
                 */
                counter++;
                if(gefunden == false)
                {
                    //suche neue Position
                    closestPos = FindClosestPosition(GoalTile.transform.position, reachablePositions);
                    if (closestPos == Vector3.zero)
                    {
                        //Nicht durch Doppelsprung reparierbar
                        NichtReparierbar = true;
                    }
                    else
                    {
                        NavMesh = FindNavMeshFromPos(closestPos);

                    }
                }
                
            }
            if(gefunden == false) { Debug.LogWarning("kein Feld gefunden"); }


        }
        if (NichtReparierbar) { Debug.LogWarning("Level nicht durch Doppelsprung reparierbar"); }
        if (!GoalReachable && !NichtReparierbar)
        {
            Agent(newPos);

        }
        if (!GoalReachable && !NichtReparierbar)
        {
            if (Iteration < 6)
            {
                Iteration++;
                surface.BuildNavMesh();
                erweiteterAgent();
            }

        }

    }

    //gibt true zrück, wenn sich keine der Vektoren übereinander befinden
    private bool NichtÜbereinander(Vector3 a, Vector3 b, Vector3 c)
    {
        if (a.x == b.x && a.z == b.z) { if (!statistics) { Debug.LogWarning("Übereinander " + a.ToString() + b.ToString()); } return false; }
        if (a.x == c.x && a.z == c.z) {
            if (!statistics) { Debug.LogWarning("Übereinander " + a.ToString() + c.ToString()); } return false; }
        if (b.x == c.x && b.z == c.z) {
            if (!statistics) { Debug.LogWarning("Übereinander " + b.ToString() + c.ToString()); } return false; }
        if (!statistics) { Debug.LogWarning("Nicht Übereinander " + a.ToString() + b.ToString() + c.ToString()); } return true;
    }


    //für Dreifachsprung
    Vector3 erweiteterSprung23(Vector3 zwischenpos, Vector3 startPos, Vector3 ursprünglicherStart)
    {
        if (!statistics) { Debug.Log("erweiteter Sprung mit Zwischenposition " + zwischenpos.ToString()); }
        Vector3 endPos;
        for (int height = 1; height >= -1; height--)
        {
            
            for (int x = 0; x <= 3; x++)
            {
                // Positive x zuerst

                int positiveX = x;  
                for (int z = 0; z <= 3; z++)
                {
                    if (z != 0 || x != 0) 
                    {
                        // Positive Z
                        endPos = new Vector3(zwischenpos.x + positiveX, zwischenpos.y + height, zwischenpos.z + z);
                        if (IsValidPosition(endPos, ursprünglicherStart, zwischenpos))
                        {
                            return endPos;
                        }

                        // Negative Z nach positive Z
                        int negativeZ = -z;
                        endPos = new Vector3(zwischenpos.x + positiveX, zwischenpos.y + height, zwischenpos.z + negativeZ);
                        if (IsValidPosition(endPos, ursprünglicherStart, zwischenpos))
                        {
                            return endPos;
                        }
                    }
                }

                // Negative x nach positive x
                int negativeX = -x;
                for (int z = 0; z <= 3; z++)
                {
                    if (z != 0 || x != 0) 
                    {
                        // Positive Z
                        endPos = new Vector3(zwischenpos.x + negativeX, zwischenpos.y + height, zwischenpos.z + z);
                        if (IsValidPosition(endPos, ursprünglicherStart, zwischenpos))
                        {
                            return endPos;
                        }

                        // Negative Z nach positive Z
                        int negativeZ = -z;
                        endPos = new Vector3(zwischenpos.x + negativeX, zwischenpos.y + height, zwischenpos.z + negativeZ);
                        if (IsValidPosition(endPos, ursprünglicherStart, zwischenpos))
                        {
                            return endPos;
                        }
                    }
                }

            }
            // Direkte Tests für größere Sprünge (X, Z = ±4)
            Vector3[] directions = new Vector3[] {
            new Vector3(4, 0, 0), new Vector3(-4, 0, 0),
            new Vector3(0, 0, 4), new Vector3(0, 0, -4)
        };

            foreach (var dir in directions)
            {
                endPos = zwischenpos + new Vector3(dir.x, height, dir.z);
                if (IsValidPosition(endPos, ursprünglicherStart, zwischenpos))
                {
                    return endPos;
                }
            }
        }
        return new Vector3(100, 100, 100);
    }

    bool IsValidPosition(Vector3 endPos, Vector3 ursprünglicherStart, Vector3 zwischenpos)
    {
        return IsPositionOnNavMesh(endPos)
            && !IsPositionReachableOnNavMesh(ursprünglicherStart, endPos)
            && NichtÜbereinander(endPos, zwischenpos, ursprünglicherStart);
    }


    //DreifachSprung
    Vector3 erweiteterSprung3(Vector3 p, Vector3 start)
    {
        if (!statistics)
        {
            Debug.Log("erweiteter Sprung mit Startposition " + p.ToString());
        }
        Vector3 finalPosition = new Vector3(100, 100, 100);
        Vector3 Test = new Vector3(100, 100, 100);
        for (int height = 1; height >= 1; height--)
        {
            
            for (int x = 0; x <= 3; x++)
            {
                // Positive x zuerst

                int positiveX = x;  // z.B. 1, 2, 3
                for (int z = 0; z <= 3; z++)
                {
                    // Positive Z zuerst
                    if (z != 0 || x != 0) // Vermeidet den Fall z = 0
                    {
                        zwischenpos = new Vector3(p.x + x, p.y + height, p.z + z);
                        if (IstImSpielBereich(zwischenpos))
                        {
                            finalPosition = erweiteterSprung23(zwischenpos, p, start);
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
                            finalPosition = erweiteterSprung23(zwischenpos, p,start);
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
                    if (z != 0 || x != 0) 
                    {
                        zwischenpos = new Vector3(p.x + negativeX, p.y + height, p.z + z);
                        if (IstImSpielBereich(zwischenpos))
                        {
                            finalPosition = erweiteterSprung23(zwischenpos, p, start);
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
                            finalPosition = erweiteterSprung23(zwischenpos, p, start);
                            if (finalPosition.x != Test.x || finalPosition.y != Test.y || finalPosition.z != Test.z)
                            {
                                return finalPosition;
                            }
                        }
                    }


                }

            }

            Vector3[] directions = new Vector3[] {
            new Vector3(4, 0, 0), new Vector3(-4, 0, 0),
            new Vector3(0, 0, 4), new Vector3(0, 0, -4)
            };

            foreach (var dir in directions)
            {
                Vector3 zwischenpos = p + new Vector3(dir.x, height, dir.z);
                if (IstImSpielBereich(zwischenpos))
                {
                    finalPosition = erweiteterSprung23(zwischenpos, p, start);
                    if (!finalPosition.Equals(new Vector3(100, 100, 100)))
                    {
                        return finalPosition;
                    }
                }
            }
        }
        if (!statistics)
        {
            Debug.LogWarning("kein erweiterter Sprung gefunden");
        }
        return Test;
    }


    //gibt das NavMesh Gebiet zurück, in dem die Position liegt, die dem Ziel am nächsten ist
    List<Vector3> FindNavMeshFromPos(Vector3 pos)
    {
        foreach(List<Vector3> NavMesh in ListOfNavMeshs) 
        {
            if(NavMesh.Contains(pos)) { return NavMesh; }
        }
        Debug.LogWarning("Fehler bei FindNavMeshFromPos");
        return null;
    }


    //gibt Position zurück, die dem Ziel am nächsten liegt
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
            if (distance < smallestDistance && !untersuchtePos.Contains(position))
            {
                smallestDistance = distance;
                closestPosition = position;
            }
        }
        List<Vector3> list = FindNavMeshFromPos(closestPosition);
        if(list == null)
        {
            return Vector3.zero;
        }

        foreach (Vector3 position in FindNavMeshFromPos(closestPosition))
        {
            untersuchtePos.Add(position);
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
        try
        {
            /*
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
            */

            Vector3 startPosition = new Vector3(startPoint.x, startPoint.y, startPoint.z);

            if (Physics.Raycast(startPosition, direction.normalized, out hit, distance, collisionMask))
            {
                // Wenn der Raycast auf etwas trifft, bedeutet das, dass der Weg blockiert ist
                Debug.Log("Weg blockiert von: " + hit.collider.name);
                return false;
            }
        }
        catch (Exception e) { Debug.Log(e); }


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
                if (!checkedPositions.Contains(possiblePosition) &&!reachablePositions.Contains(possiblePosition))
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
                    }
                }
                checkedPositions.Add(possiblePosition);
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
                if (!statistics)
                {
                    Debug.LogWarning("Ziel ist für Spieler erreichbar von Position : " + pos.ToSafeString());
                }
                CreateNavMeshLink(pos, GoalTile.transform.position);
                GoalReachable = true;

            }
            else
            {
                //Debug.LogWarning("Ziel ist für Spieler NICHT erreichbar");
            }
        }
    }


    //für Automatisierte Statistik, zum Überprüfen ob korrekt gearbeitet wurde
    public bool checkIfGoalIsReachableFromStart()
    {

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(PlayerStartPosition, GoalTile.transform.position, NavMesh.AllAreas, path))
        {
            // Überprüft, ob der berechnete Pfad vollständig ist
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;

            }
            else
            {
                return false;
            }
        }
        return false;
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

    //färbt alle Position in der Liste mit Farbe
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
    /*
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
    */

    //durchlaufe mögliche durch Sprung erreichbare Felder
    //Spieler kann im Bereich 3x3 alle Felder erreichen, egal ob Höhe 1, 0 oder -1
    //in einer zentrale Richtung kann er sogar 4 Felder weit springen
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

    //normaler Agent
    public void Agent(Vector3 startPosition)
    {

        if (!statistics)
        {
            Debug.Log("AA aus TEST");
        }
        if (!GoalReachable)
        {
            l = durchquereNavMeshGebiet(startPosition);
        }
        if(!GoalReachable) 
        { 
            foreach(Vector3 position in l)
            {
                untersucheFelderDurchSprungErreichbar(position);
            }
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

            // neues Gamobjekt, NavMeshLink
            GameObject linkObject = new GameObject("NavMeshLink");
            var navMeshLink = linkObject.AddComponent<NavMeshLink>();

            // Setze die Start- und Endpunkte des Links
            navMeshLink.startPoint = start;
            navMeshLink.endPoint = end;

            // Optional: Weitere Einstellungen
            navMeshLink.width = 1.0f;
            navMeshLink.bidirectional = true;

            // Aktualisiere den Link
            navMeshLink.UpdateLink();

            //Position als nächstes überprüfen
            positionsToCheck.Enqueue(end);

            //Debug.Log("NEUER NavMesh Link, jetzt Iteration mit NEUEM Punkt: " + end.ToString());

            Agent(end);
        }

    }


    void ErweitertCreateNavMeshLink(Vector3 start, Vector3 end, Vector3 zwischenposition)
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
                check(zwischenposition, start, end);
                surface.BuildNavMesh();


    }

    public GameObject newTile;

    void ForceNavMeshLink(Vector3 start, Vector3 end)
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
    }


        //überprüft, ob ein Block erstellt oder entfernt werden muss
        void check(Vector3 zwischenpos, Vector3 startpos, Vector3 endpos)
    {
        if (isAObjectinMap(zwischenpos))
        {
            deleteObjectsBetween(startpos, zwischenpos);
            RemoveObjectsAbove(zwischenpos);
            deleteObjectsBetween(zwischenpos, endpos);
            //surface.BuildNavMesh();
            ForceNavMeshLink(startpos, endpos); //für anschließenden Test für Statistik
            //entferne die Blöcke oberhalb und alle, die in der NavLink Linie liegen
        }
        else
        {
            //zerstöre alle Objekte, die in der NavLink Linie liegen
            deleteObjectsBetween(startpos, zwischenpos);
            modifications++;
            GameObject ob = Instantiate(newTile, zwischenpos, Quaternion.identity); AllTiles.Add(ob);
            ModifikationsTransform.Add(ob.transform);
            //unterhalb auch noch mit Objekten füllen
            deleteObjectsBetween(zwischenpos, endpos);
        }
        surface.RemoveData();
        surface.BuildNavMesh() ;
    }

    void RemoveObjectsAbove(Vector3 position)
    {
        if (!statistics)
        {
            Debug.LogWarning("Remove Objects Above " + position.ToString());
        }
        RaycastHit hit;
        List<Vector3> positions = new List<Vector3>();

        // Führe einen vertikalen Raycast nach oben durch
        while (Physics.Raycast(position, Vector3.up, out hit, Mathf.Infinity))
        {
            if (!statistics)
            {
                Debug.Log("Entferne Objekt oberhalb der Position: " + hit.collider.name);
            }

            positions.Add(hit.collider.gameObject.transform.position);


            // Entferne das getroffene Objekt
            Destroy(hit.collider.gameObject.transform.parent.gameObject); 
            

            // Aktualisiere die Position, um direkt hinter dem entfernten Objekt zu starten
            position = hit.point + Vector3.up * 0.01f; // Ein kleiner Abstand, um sicherzustellen, dass nicht immer das gleiche Objekt getroffen wird
        }

        List<Vector3> CreatedPositions = new List<Vector3>();
        foreach (Vector3 pos in positions)
        {
            if (!CreatedPositions.Contains(pos))
            {
                CreatedPositions.Add(pos);
                GameObject ob = Instantiate(destroyedTile, pos, Quaternion.identity);
                ModifikationsTransform.Add(ob.transform);
                modifications++;
            }
        }
        if (!statistics)
        {
            Debug.Log("Alle Objekte oberhalb der Position wurden entfernt.");
        }
    }

    public HashSet<Transform> ModifikationsTransform = new HashSet<Transform>();

    public GameObject destroyedTile;

    public void deleteObjectsBetween(Vector3 startpos, Vector3 endpos)
    {
        List<Vector3> positions = new List<Vector3>();
        bool blockiert = true;
        if (startpos.x != endpos.x && startpos.z != endpos.z)
        {
            //links und rechts wird auch gelöscht
            for (float x = 0.5f; x >= -0.5f; x = x - 0.5f)
            {
                for (int height = 0; height <= 3; height++)
                {
                    //verschiebe Vektor nach oben
                    Vector3 start = new Vector3(startpos.x + x, startpos.y + 1.1f + height, startpos.z);
                    Vector3 end = new Vector3(endpos.x + x, endpos.y + 1.1f + height, endpos.z);
                    Vector3 direction = end - start;
                    float distance = direction.magnitude;
                    RaycastHit hit;

                    int safetyCounter = 40;  // Begrenze die Anzahl der Versuche
                    while (blockiert && safetyCounter > 0)
                    {
                        if (!statistics)
                        {
                            Debug.Log("TT");
                        }
                        if (Physics.Raycast(start, direction.normalized, out hit, distance, collisionMask))
                        {
                            try
                            {
                                if (!statistics)
                                {
                                    Debug.Log("Weg blockiert von: " + hit.collider.name);
                                }
                                if (hit.collider.gameObject == destroyedTile || hit.collider.gameObject == null || hit.collider == null || hit.collider.gameObject.transform.parent.gameObject.transform == GoalTile.transform || hit.collider.gameObject.transform.parent.gameObject == GoalTile) { }
                                else
                                {
                                    //zerstöre das getroffene Objekte
                                    Destroy(hit.collider.gameObject.transform.parent.gameObject);
                                    if (!statistics)
                                    {
                                        Debug.Log("Entferne Objekt: " + hit.collider.gameObject.transform.position);
                                    }
                                    start = hit.point + direction.normalized * 0.1f;
                                    direction = end - start;
                                    distance = direction.magnitude;
                                    positions.Add(hit.collider.gameObject.transform.position);
                                }
                            }
                            //um Tests nicht abzubrechen
                            catch (System.NullReferenceException e) { Debug.LogError("Eine NullReferenceException ist aufgetreten: " + e.Message); }
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

                int safetyCounter = 50;  // Begrenze die Anzahl der Versuche
                while (blockiert && safetyCounter > 0)
                {
                    if (!statistics)
                    {
                        Debug.Log("TT");
                    }
                    if (Physics.Raycast(start, direction.normalized, out hit, distance, collisionMask))
                    {
                        if (!statistics)
                        {
                            Debug.Log("Weg blockiert von: " + hit.collider.name);
                        }
                        if (hit.collider.gameObject == null || hit.collider == null || hit.collider.gameObject.transform.parent.gameObject == null || hit.collider.gameObject.transform.parent.gameObject == GoalTile) { }
                        else
                        {
                            Destroy(hit.collider.gameObject.transform.parent.gameObject);
                            if (!statistics)
                            {
                                Debug.Log("Entferne Objekt: " + hit.collider.gameObject.transform.position);
                            }
                            start = hit.point + direction.normalized * 0.1f;
                            direction = end - start;
                            distance = direction.magnitude;
                            positions.Add(hit.collider.gameObject.transform.position);
                        }
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
        //füge an den gelöschten Stellen andere Objekte ein, um dem Spieler die Löschung anzuzeigen
        List<Vector3> CreatedPositions = new List<Vector3>();
        foreach (Vector3 pos in positions)
        {
            if (!CreatedPositions.Contains(pos))
            {
                CreatedPositions.Add(pos);
                GameObject ob = Instantiate(destroyedTile, pos, Quaternion.identity);
                modifications++;
                ModifikationsTransform.Add(ob.transform);
                Collider objectCollider = ob.GetComponent<Collider>();
                if (objectCollider != null)
                {
                    objectCollider.enabled = false; // Deaktiviert den Collider
                }
                ob.GetComponent<Collider>().enabled = false;
                ob.GetComponentInChildren<Collider>().enabled = false;
            }
        }
    }

    public int modifications;



    bool isAObjectinMap(Vector3 pos)
    {
        Vector3 boxSize = new Vector3(0.9f, 0.9f, 0.9f);
        Collider[] hitColliders = Physics.OverlapBox(pos, boxSize / 2);

        if (hitColliders.Length > 0)
        {
            if (!statistics)
            {
                Debug.Log("Ein Block ist an der Position.");
            }
            return true;
        }
        else
        {
            if (!statistics)
            {
                Debug.Log("Kein Block an der Position.");
            }
            return false;
        }
    }
}
