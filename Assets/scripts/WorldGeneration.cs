using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Runtime.CompilerServices;
using System.IO;
using System.Net.Security;

public class WorldGeneration : MonoBehaviour
{



    public Transform spawnPosition;
    public GameObject tile;
    public GameObject end_tile;
    public GameObject start_tile;
    public GameObject destroyed_tile;

    //navmesh
    public NavMeshSurface surface;

    //f�r Debug-Log
    public bool debugCheck;

    //zum Spawnen des Spieles
    public GameObject player;



    //f�r diamond-Step
    private List<PositionPoint> diamond_calculated_positionPoints = new List<PositionPoint>();
    private List<List<PositionPoint>> diamond_Vierecke = new List<List<PositionPoint>>();

    //f�r square-Step


    //f�r beides
    private PositionPoint[,] map;
    private int maplength;
    private List<int> offset;
    private List<int> values;

    public GameObject gameWinUI;


    // Update is called once per frame
    void Update()
    {
        if (PlayerReachedGoal())
        {
            gameWinUI.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }

    public bool PlayerReachedGoal()
    {
        if (Vector3.Distance(Last.transform.position, player.transform.position) < 1f)
        {
            Debug.LogWarning("Spieler hat Ziel erreicht");
            return true;
        }
        return false;
    }

    public GameObject NavMeshLinkScript;
    public GameObject new_tile;
    public static bool erweitert;
    public GameObject playerFallScript;

    public List<Color> mapColor = new List<Color>();

    private void Start()
    {
        Debug.LogWarning(difficulty.Difficulty.ToString());
        Time.timeScale = 1f;
        gameWinUI.SetActive(false);
        NavMeshLinkScript.GetComponent<TESTTEST>().newTile = new_tile;
        erstelleWelt(difficulty.Difficulty);

        //NavMeshLinkScript.GetComponent<TESTTEST>().deleteObjectsBetween(new Vector3 (4,0,0), new Vector3(4, 0, 10));

        //diamond_step(testmap, new PositionPoint(0, 0, 0), new PositionPoint(0, 2, 2), new PositionPoint(2, 0, 3), new PositionPoint(2, 2, 5),  testoffset, testvalues);

        surface.BuildNavMesh();
        //Vector3 startPosition = new Vector3(player.transform.position.x, player.transform.position.y - 1, player.transform.position.z);
        Vector3 startPosition = new Vector3(1, 3,1);
        //Debug.LogWarning(startPosition);
        NavMeshLinkScript.GetComponent<TESTTEST>().PlayerStartPosition = startPosition;
        mapColor.Add(Color.grey); mapColor.Add(Color.black); mapColor.Add(Color.cyan); mapColor.Add(Color.yellow);
        NavMeshLinkScript.GetComponent<TESTTEST>().Colors = mapColor;
        Debug.LogWarning("Start Position: " + startPosition.ToString());
        playerFallScript.GetComponent<PlayerFall>().initialPosition = startPosition;
        NavMeshLinkScript.GetComponent<TESTTEST>().GoalTile = Last;
        //IsPositionOnNavMesh(startPosition);
        //NavMeshLinkScript.GetComponent<TESTTEST>().ColorChangeFarbe = Color.blue;
        NavMeshLinkScript.GetComponent<TESTTEST>().destroyedTile = destroyed_tile; 
        NavMeshLinkScript.GetComponent<TESTTEST>().StartAgent(startPosition);



        if(NavMeshLinkScript.GetComponent<TESTTEST>().GoalReachable == false && erweitert)
        {

        NavMeshLinkScript.GetComponent<TESTTEST>().tile = tile;
        //NavMeshLinkScript.GetComponent<TESTTEST>().ColorChangeFarbe = Color.red;
        NavMeshLinkScript.GetComponent<TESTTEST>().erweiteterAgent();
        }

    }

    void checkGoalReachable(Vector3 start)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(start, Last.transform.position, NavMesh.AllAreas, path))
        {
            // �berpr�ft, ob der berechnete Pfad vollst�ndig ist
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                //Ziel ist f�r Spieler erreichbar
                Debug.Log("Ziel ist f�r Spieler erreichbar");
            }
            else
            {
                //Ziel ist f�r Spieler NICHT erreichbar

                Debug.Log("Ziel ist f�r Spieler NICHT erreichbar");
                //erweiterter Agent
                NavMeshLinkScript.GetComponent<TESTTEST>().erweiteterAgent();
            }
        }
    }



    public float maxVerticalDistance = 1.0f;
    public float maxHorizontalDistance = 3.0f;
    public float edgeDetectionRadius = 0.5f;


    void BEtaGenerateNavMeshLinks(Vector3 startPosition)
    {
        // Stelle sicher, dass das NavMesh auf dem NavMeshSurface erstellt wurde
        surface.BuildNavMesh();

        // Liste der Positionen im gleichen NavMesh-Bereich wie die Startposition
        List<Vector3> sameMeshPositions = new List<Vector3>();

        // Warteschlange f�r Positionen, die untersucht werden m�ssen
        Queue<Vector3> positionsToCheck = new Queue<Vector3>();
        positionsToCheck.Enqueue(startPosition);
        sameMeshPositions.Add(startPosition);

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
                if (IsPositionOnNavMesh(possiblePosition))
                {
                    // Pr�fe, ob die Position auf dem gleichen NavMeshSurface liegt
                    if (IsPositionOnSameMesh(possiblePosition, startPosition))
                    {
                        if (!sameMeshPositions.Contains(possiblePosition))
                        {
                            sameMeshPositions.Add(possiblePosition);
                            positionsToCheck.Enqueue(possiblePosition);
                        }
                    }
                    else
                    {
                        // Finde erreichbare Positionen in einem anderen NavMesh-Bereich
                        if (IsPositionReachable(currentPosition, possiblePosition))
                        {
                            // Erstelle einen NavMeshLink zwischen den beiden Bereichen
                            CreateNavMeshLink(currentPosition, possiblePosition);
                        }
                    }
                }

            }
        }
    }

    bool IsPositionOnNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        // Versuche, die Position auf dem NavMesh zu finden, innerhalb eines Radius von maxDistance
        if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
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
        NavMesh.SamplePosition(position, out hit, edgeDetectionRadius, NavMesh.AllAreas);
        NavMesh.SamplePosition(referencePosition, out NavMeshHit referenceHit, edgeDetectionRadius, NavMesh.AllAreas);

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
    }




    

    public Transform getSpawnPosition() { return spawnPosition; }

    //erstelle ein 2 dimensionales Array (Map) von Position Points
    private void createMap(List<int> values, int testlength, int startH�he, int zielH�he)
    {
        //sicherstellen, dass L�nge 2^n +1 f�r beliebiges n ist. Sollte dies nicht der Fall sein, wird der n�chstgr��te Wert genommen, der der Anforderung gen�gt
        if (!potenzVon2(testlength - 1)) { maplength = nextPotenzfor(testlength) + 1; }
        else { maplength = testlength; }
        int count = values.Count();
        map = new PositionPoint[maplength, maplength];
        for (int i = 0; i < maplength; i++)
        {
            for (int j = 0; j < maplength; j++)
            {
                if (i == 0 && j == 0 || i == 0 && j == 1 || i == 0 && j == 2 || i==1 &&j==0 || i==1 && j==1 || i==1 && j==2 || i==2 && j==0 || i==2 &&j==1 || i==2 && j==2)
                {
                    map[i, j] = new PositionPoint(i, j, values.ElementAt(startH�he));   //StartPosition
                }
                else if (i == 0 & j == maplength - 1 | i == maplength - 1 & j == 0)
                {
                    map[i, j] = new PositionPoint(i, j, values.ElementAt(UnityEngine.Random.Range(0, count)));   //weise den Eckwerten der Map einen Value zu
                }
                else if(i == maplength - 1 & j == maplength - 1 || i==maplength-3 && j==maplength-3 || i==maplength-3 && j==maplength-2 ||i==maplength-3 && j==maplength-1
                    || i == maplength - 2 && j == maplength - 3 || i == maplength - 2 && j == maplength - 2 || i == maplength - 2 && j == maplength - 1 
                    || i == maplength - 1 && j == maplength - 3 || i == maplength - 1 && j == maplength - 2)
                {
                    map[i, j] = new PositionPoint(i, j, values.ElementAt(zielH�he));
                }
                else
                {
                    map[i, j] = new PositionPoint(i, j, -100);      //-100 als Initialisierung
                }
                if (debugCheck)
                {
                    map[i, j].log();
                }

            }

        }

    }

    GameObject Last = null;
    private void createTiles(int deleteHeight1, int deleteHeight2, int deleteHeight3)
    {
        bool isSpawned = false;
        foreach (PositionPoint p in map)
        {
            if(p.getX() == maplength-1 && p.getY()==maplength-1 || p.getX() == maplength - 1 && p.getY() == maplength - 2 || p.getX() == maplength - 1 && p.getY() == maplength - 3 
            || p.getX() == maplength -2 && p.getY() == maplength - 1 || p.getX() == maplength - 2 && p.getY() == maplength - 2 || p.getX() == maplength - 2 && p.getY() == maplength - 3
            || p.getX() == maplength -3 && p.getY() == maplength - 1 || p.getX() == maplength - 3 && p.getY() == maplength - 2 || p.getX() == maplength - 3 && p.getY() == maplength - 3)
            {
                for (int i = 0; i < p.getValue(); i++)
                {
                    NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Instantiate(tile, new Vector3(p.getX(), i, p.getY()), Quaternion.identity));

                }
                if(p.getX() == maplength - 2 && p.getY() == maplength - 2)
                {
                    Last = Instantiate(end_tile, new Vector3(p.getX(), p.getValue(), p.getY()), Quaternion.identity);
                    Last.layer = 7;
                    //NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Last);
                    NavMeshLinkScript.GetComponent<TESTTEST>().GoalPositions.Add(new Vector3(p.getX(), p.getValue(), p.getY()));
                }
                else
                {
                    GameObject Top = Instantiate(end_tile, new Vector3(p.getX(), p.getValue(), p.getY()), Quaternion.identity);
                    Top.layer = 7;
                    NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Top);
                    NavMeshLinkScript.GetComponent<TESTTEST>().GoalPositions.Add(new Vector3(p.getX(), p.getValue(), p.getY()));
                }
            }
            else if(p.getX() == 0 && p.getY() == 0 || p.getX() == 0 && p.getY() == 1 || p.getX() == 0 && p.getY() == 2
            || p.getX() == 1 && p.getY() == 0 || p.getX() == 1 && p.getY() == 1 || p.getX() == 1 && p.getY() == 2
            || p.getX() == 2 && p.getY() == 0 || p.getX() == 2 && p.getY() == 1 || p.getX() == 2 && p.getY() == 2)
            {
                for (int i = 0; i < p.getValue(); i++)
                {
                    NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Instantiate(tile, new Vector3(p.getX(), i, p.getY()), Quaternion.identity));

                }

                GameObject Top = Instantiate(start_tile, new Vector3(p.getX(), p.getValue(), p.getY()), Quaternion.identity);
                Top.layer = 7;
                NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Top);
                NavMeshLinkScript.GetComponent<TESTTEST>().GoalPositions.Add(new Vector3(p.getX(), p.getValue(), p.getY()));
            }
            else
            {
                int height = p.getValue();
                if (height > 2 && height != deleteHeight1 && height != deleteHeight2 && height != deleteHeight3)
                {
                    for (int i = 0; i < height; i++)
                    {
                        NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Instantiate(tile, new Vector3(p.getX(), i, p.getY()), Quaternion.identity));

                    }
                    GameObject Top = Instantiate(tile, new Vector3(p.getX(), height, p.getY()), Quaternion.identity);
                    //Last = Top;
                    Top.layer = 7;
                    NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Top);
                    /*
                    if (!isSpawned)
                    {

                        spawnPosition.position = new Vector3(p.getX(), height + 1, p.getY());
                        //spawnPosition.rotation = Quaternion.Euler(0,210,0);
                        isSpawned = true;
                        player.transform.position = spawnPosition.position;
                        player.transform.rotation = spawnPosition.rotation;
                    }
                    */
                }
            }

                

        }
        //Debug.LogWarning(" Postion des letzten " + Last.transform.position);
        //Destroy(Last);
        //Last = Instantiate(end_tile, Last.transform.position, Quaternion.identity);
        //Last.layer = 7;

    }

    public void ReloadPlayerPos()
    {

        player.transform.position = spawnPosition.position;
        player.transform.rotation = spawnPosition.rotation;
    }



    private void LogMap()
    {
        for (int i = 0; i < maplength; i++)
        {
            for (int j = 0; j < maplength; j++)
            {
                map[i, j].log();
            }
        }
    }

    private void erstelleWelt(int diff)
    {
        if(diff == 0)
        {
            diamond_square(33, 0, 8, 0, 1,3,8); createTiles(5,5,5);
        }
        else if(diff == 1)
        {

            diamond_square(33, 0, 12, -1, 2,3,10); createTiles(5,8,8);
        }
        else if (diff == 2)
        {
            diamond_square(33, 0, 12, -2, 2,3,12); createTiles(5, 8, 9);
        }
    }




    private void diamond_square(int testlength, int minValue, int maxValue, int minOffset, int maxOffset, int startH�he, int zielH�he)
    {
        //erstelle Intervalls f�r den Wertebereich (value)
        values = new List<int>();
        for (int i = minValue; i <= maxValue; i++)
        {
            if (debugCheck) { Debug.Log("value i: " + i); }
            values.Add(i);

        }

        //erstelle Map
        createMap(values, testlength, startH�he, zielH�he);

        //erstelle Intervalls f�r den Offet Bereich 
        offset = new List<int>();
        for (int i = minOffset; i <= maxOffset; i++)
        {
            offset.Add(i);
        }


        int Abstand = (maplength - 1) / 2;
        while (Abstand > 0)
        {
            //diamond-Schritt
            for (int x = Abstand; x < maplength - 1; x = x + Abstand * 2)
            {
                for (int y = Abstand; y < maplength - 1; y = y + Abstand * 2)
                {
                    if (map[x, y].getValue() == -100) //unbearbeitet
                    {
                        map[x, y] = diamond_berechne_neuen_punkt(
                            map[x - Abstand, y + Abstand],
                            map[x + Abstand, y + Abstand],
                            map[x - Abstand, y - Abstand],
                            map[x + Abstand, y - Abstand]);

                        if (debugCheck)
                        {
                            Debug.Log("NEUER WERT:");
                            map[x, y].log();
                        }
                    }
                }
            }

            //square-Schritt
            for (int x = 0; x < maplength; x += Abstand)
            {
                for (int y = 0; y < maplength; y += Abstand)
                {
                    if (map[x, y].getValue() == -100) //noch unbearbeitet
                    {
                        if (x == 0) //ganz links
                        {
                            map[x, y].setValue(squareCalculation(
                                map[x, y + Abstand],
                                map[x + Abstand, y],
                                map[x, y - Abstand]));
                        }
                        else if (x == maplength - 1) //ganz rechts
                        {
                            map[x, y].setValue(squareCalculation(
                                map[x, y + Abstand],
                                map[x - Abstand, y],
                                map[x, y - Abstand]));
                        }
                        else if (y == 0) //ganz unten
                        {
                            map[x, y].setValue(squareCalculation(
                                map[x - Abstand, y],
                                map[x, y + Abstand],
                                map[x + Abstand, y]));
                        }
                        else if (y == maplength - 1) //ganz oben
                        {
                            map[x, y].setValue(squareCalculation(
                                map[x - Abstand, y],
                                map[x, y - Abstand],
                                map[x + Abstand, y]));
                        }
                        else //mittig, 4 Werte
                        {
                            map[x, y].setValue(squareCalculation(
                                map[x, y + Abstand],
                                map[x + Abstand, y],
                                map[x, y - Abstand],
                                map[x - Abstand, y]));
                        }
                    }
                }
            }
            Abstand = Abstand / 2;
        }


        if (debugCheck)
        {
            Debug.Log("ENDE:");
            LogMap();
        }
    }

    private int squareCalculation(PositionPoint p1, PositionPoint p2, PositionPoint p3)
    {
        int durchschnitt = (p1.getValue() + p2.getValue() + p3.getValue()) / 3;                 //durchschnitt der 3 Punkte
        int offset_val = offset.ElementAt(UnityEngine.Random.Range(0, offset.Count()));        //mit offset versehen, Count gibt anzahl der elemente wieder
        int newValue = durchschnitt + offset_val;
        if (debugCheck) { Debug.Log("offset = " + offset_val); }
        if (!values.Contains(newValue))
        {
            if (newValue < values.Min())
            {
                newValue = values.Min();
            }
            else if (newValue > values.Max())
            {
                newValue = values.Max();
            }
        }
        return newValue;
    }

    private int squareCalculation(PositionPoint p1, PositionPoint p2, PositionPoint p3, PositionPoint p4)
    {
        int durchschnitt = (p1.getValue() + p2.getValue() + p3.getValue() + p4.getValue()) / 4; //durchschnitt der 3 Punkte
        int newValue = durchschnitt + offset.ElementAt(UnityEngine.Random.Range(0, offset.Count()));        //mit offset versehen, Count gibt anzahl der elemente wieder
        if (!values.Contains(newValue))
        {
            if (newValue < values.Min())
            {
                newValue = values.Min();
            }
            else if (newValue > values.Max())
            {
                newValue = values.Max();
            }
        }
        return newValue;
    }

    public bool potenzVon2(int x)
    {
        return x > 0 && (x & (x - 1)) == 0;
    }

    public int nextPotenzfor(int x)
    {
        return 5;
    }


    //Klasse f�r Diamond und Square Schritt. Ein Punkt enth�lt die Position (x,y) und den H�henwert
    public class PositionPoint
    {
        int posX;
        int posY;
        int value;

        public PositionPoint(int x, int y, int v)
        {
            posX = x; posY = y; value = v;
        }

        public int getX() { return posX; }
        public int getY() { return posY; }
        public int getValue() { return value; }
        public void setValue(int value) { this.value = value; }

        public void log()
        {
            Debug.Log("Position: " + posX + ", " + posY + ", Wert: " + value);
        }

    }

    private void diamond_first_step()
    {
        f�gePositionPointinMapein(diamond_berechne_neuen_punkt(map[0, 0], map[0, maplength - 1], map[maplength - 1, 0], map[maplength - 1, maplength - 1]));
        Debug.Log("nach first STEP");
        LogMap();
    }

    private void diamond_point_ausViereck()
    {
        foreach (List<PositionPoint> viereck in diamond_Vierecke)
        {
            diamond_berechne_neuen_punkt(viereck.ElementAt(0), viereck.ElementAt(1), viereck.ElementAt(2), viereck.ElementAt(3));
        }
        diamond_Vierecke.Clear();
    }


    private void f�gePositionPointinMapein(PositionPoint p)
    {
        map[p.getX(), p.getY()] = p;
    }


    //erstellt zu berechnende Vierecke anhand der zuvor berechneten PositionPoints des diamond-Schritts
    private void berechneVierecke()
    {
        foreach (PositionPoint p in diamond_calculated_positionPoints)
        {
            for (int z = 0; z <= 3; z++)
            {
                diamond_Vierecke.Add(Vierreck(p, nextPointInMap(z, p)));
            }
            diamond_calculated_positionPoints.Remove(p);
        }
    }


    //gibt den n�chstgelegenen bereits ausgef�llten Position Point in der Map zur�ck
    //x=0 oben link
    //1 oben recht
    //2 unten links 
    //3 unten rechts
    private PositionPoint nextPointInMap(int x, PositionPoint p)
    {
        if (x == 2)      //links unten, immer ein schritt runter und nach links
        {
            int i = p.getX() - 1;
            int j = p.getY() - 1;
            while (i >= 0 && j >= 0)
            {
                if (map[i, j].getValue() != -100)
                {
                    return map[i, j];
                }
                else
                {
                    i--;
                    j--;
                }
            }
        }
        else if (x == 1)  //oben rechts
        {
            int i = p.getX() + 1;
            int j = p.getY() + 1;
            while (i < maplength && j < maplength)
            {
                if (map[i, j].getValue() != -100)
                {
                    return map[i, j];
                }
                else
                {
                    i++;
                    j++;
                }
            }
        }
        else if (x == 0)  //oben links (x Achse 0, y Achse MAX)
        {
            int i = p.getX() - 1;
            int j = p.getY() + 1;
            while (i >= 0 && j < maplength)
            {
                if (map[i, j].getValue() != -100)
                {
                    return map[i, j];
                }
                else
                {
                    i--;
                    j++;
                }
            }
        }
        else if (x == 3)  //unten rechts
        {
            int i = p.getX() + 1;
            int j = p.getY() - 1;
            while (i < maplength && j >= 0)
            {
                if (map[i, j].getValue() != -100)
                {
                    return map[i, j];
                }
                else
                {
                    i++;
                    j--;
                }
            }
        }
        return null;
    }

    private List<PositionPoint> Vierreck(PositionPoint p1, PositionPoint p2)
    {
        int MaxX = Math.Max(p1.getX(), p2.getX());
        int MinX = Math.Min(p1.getX(), p2.getX());
        int MaxY = Math.Max(p1.getY(), p2.getY());
        int MinY = Math.Min(p1.getY(), p2.getY());
        List<PositionPoint> vier = new List<PositionPoint>();
        PositionPoint LeftTop = map[MinX, MinY];
        PositionPoint RightTop = map[MinX, MaxY];
        PositionPoint LeftDown = map[MaxX, MinY];
        PositionPoint RightDown = map[MaxX, MaxY];

        vier.Add(LeftTop); vier.Add(RightTop); vier.Add(LeftDown); vier.Add(RightDown);
        return vier;

    }


    private PositionPoint diamond_berechne_neuen_punkt(PositionPoint leftTop, PositionPoint rightTop, PositionPoint leftDown, PositionPoint rightDown)
    {
        //berechne neue y Position, Differenz der beiden linken y-Koordinaten (oder wahlweise auch der beiden rechten) also Differenz oben und unten
        if (debugCheck) { Debug.Log("berechne PUNKT"); }
        int SumY = leftTop.getY() + leftDown.getY();
        if (SumY % 2 != 0) { Debug.Log("Diamond Schritt: y Wert nicht durch 2 teilbar"); }
        int newY = SumY / 2;

        //berechne neue x Position, also wie weit link oder rechts der neue Punkt sein soll
        int SumX = leftTop.getX() + rightTop.getX();
        if (SumX % 2 != 0) { Debug.Log("Diamond Schritt: x Wert nicht durch 2 teilbar"); }
        int newX = SumX / 2;


        //berechne Wert des neuen Punktes
        int durchschnitt = (leftTop.getValue() + rightTop.getValue() + leftDown.getValue() + rightDown.getValue()) / 4; //durchschnitt der 4 Punkte
        int newValue = durchschnitt + offset.ElementAt(UnityEngine.Random.Range(0, offset.Count()));        //mit offset versehen,                                  Count gibt anzahl der elemente wieder
        if (!values.Contains(newValue))
        {
            if (newValue < values.Min())
            {
                newValue = values.Min();
            }
            else if (newValue > values.Max())
            {
                newValue = values.Max();
            }
        }
        PositionPoint p = new PositionPoint(newX, newY, newValue);
        return p;
    }


    //hier wird der square Schritt durchgef�hrt. Es werden dazu 5 Punkte �bergeben in der Reihenfolge LeftTop, RightTop, MiddleMiddle, LeftDown, RightDown. 
    //Ahand dieser Punkte werden 4 neue Punkte berechnet
    private List<PositionPoint> square_step(int[,] map)
    {

        return null;
    }
}
