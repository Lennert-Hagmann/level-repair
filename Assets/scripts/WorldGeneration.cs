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
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.UIElements;
using UnityEditor.Search;
using UnityEditor;

public class WorldGeneration : MonoBehaviour
{



    public Transform spawnPosition;
    public GameObject tile;
    public GameObject end_tile;
    public GameObject start_tile;
    public GameObject destroyed_tile;

    //navmesh
    public NavMeshSurface surface;

    //für Debug-Log
    public bool debugCheck;

    //zum Spawnen des Spieles
    public GameObject player;



    //für diamond-Step
    private List<PositionPoint> diamond_calculated_positionPoints = new List<PositionPoint>();
    private List<List<PositionPoint>> diamond_Vierecke = new List<List<PositionPoint>>();

    //für square-Step


    //für beides
    private PositionPoint[,] map;
    private int maplength;
    private List<int> offset;
    private List<int> values;

    public GameObject gameWinUI;


    // Update is called once per frame
    void Update()
    {
        if (!testing)
        {
            if (PlayerReachedGoal())
            {
                gameWinUI.SetActive(true);
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
            }
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
    private bool noRepair = false;

    private static void ClearConsole()
    {
        // This method clears the console using internal Unity methods
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }


    long MeasureExecutionTime(int a)
    {
        Stopwatch stopwatch = new Stopwatch();


        // Beispielmethode, die gemessen wird
        if (a == 0)
        {
            //Generierung
            stopwatch.Start();
            erstelleWelt(difficulty.Difficulty);
            stopwatch.Stop();
        }
        else if (a == 1)
        {
            //default agent
            stopwatch.Start();
            NavMeshLinkScript.GetComponent<AgentScript>().StartAgent(playerFallScript.GetComponent<PlayerFall>().initialPosition);
            stopwatch.Stop();
        }
        else if (a == 2)
        {
            //repair agent
            stopwatch.Start();
            NavMeshLinkScript.GetComponent<AgentScript>().erweiteterAgent();
            stopwatch.Stop();
        }
        return stopwatch.ElapsedMilliseconds;
    }

    void PrintExecutionTimes(long[,] times)
    {
        for (int i = 0; i < times.GetLength(0); i++)
        {
            string line = $"Iteration {i}: ";
            for (int j = 0; j < times.GetLength(1); j++)
            {
                line += $"Messung {j + 1}: {times[i, j]} ms ";
            }
            Debug.Log(line);
        }
    }
    //für Statistiken auf true setzen
    private bool testing = true;


    public static Stopwatch stopwatch = new Stopwatch();
    public Stopwatch stopwatch2 = new Stopwatch();
    public Stopwatch stopwatch3 = new Stopwatch();
    public bool reachable;
    public static List<int> timesGeneration = new List<int>();
    public static List<int> timesDefaultAgent = new List<int>();
    public static List<int> timesRepairAgent = new List<int>();
    public static List<int> ListModifications = new List<int>();
    public static List<int> ListPlayable = new List<int>(); //0 = without repair playable, 1 = with repair playable, 2 = not playable
    private void Start()
    {

        bool playableChecked = false;
        NavMeshLinkScript.GetComponent<AgentScript>().ModifikationsTransform.Clear();


        if (!testing) { Debug.LogWarning(difficulty.Difficulty.ToString()); }
        Time.timeScale = 1f;
        gameWinUI.SetActive(false);
        NavMeshLinkScript.GetComponent<AgentScript>().newTile = new_tile;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        erstelleWelt(difficulty.Difficulty);
        stopwatch.Stop();

        //NavMeshLinkScript.GetComponent<TESTTEST>().deleteObjectsBetween(new Vector3 (4,0,0), new Vector3(4, 0, 10));

        //diamond_step(testmap, new PositionPoint(0, 0, 0), new PositionPoint(0, 2, 2), new PositionPoint(2, 0, 3), new PositionPoint(2, 2, 5),  testoffset, testvalues);

        surface.BuildNavMesh();
        //Vector3 startPosition = new Vector3(player.transform.position.x, player.transform.position.y - 1, player.transform.position.z);
        Vector3 startPosition = new Vector3(1, 3, 1);
        //Debug.LogWarning(startPosition);
        NavMeshLinkScript.GetComponent<AgentScript>().PlayerStartPosition = startPosition;
        mapColor.Add(Color.blue); mapColor.Add(Color.red); mapColor.Add(Color.cyan); mapColor.Add(Color.yellow); mapColor.Add(Color.grey); mapColor.Add(Color.blue); mapColor.Add(Color.red); mapColor.Add(Color.cyan); mapColor.Add(Color.yellow); mapColor.Add(Color.grey);
        NavMeshLinkScript.GetComponent<AgentScript>().Colors = mapColor;
        if (!testing)
        {
            Debug.Log("Start Position: " + startPosition.ToString());
        }
        playerFallScript.GetComponent<PlayerFall>().initialPosition = startPosition;
        NavMeshLinkScript.GetComponent<AgentScript>().GoalTile = Last;
        //IsPositionOnNavMesh(startPosition);
        //NavMeshLinkScript.GetComponent<TESTTEST>().ColorChangeFarbe = Color.blue;
        NavMeshLinkScript.GetComponent<AgentScript>().destroyedTile = destroyed_tile;

        Stopwatch stopwatch2 = new Stopwatch();
        stopwatch2.Start();
        NavMeshLinkScript.GetComponent<AgentScript>().StartAgent(startPosition);
        stopwatch2.Stop();
        if (testing) { ClearConsole(); }
        

        //UnityEngine.Debug.LogWarning("Generierung der Welt: " + stopwatch.ElapsedMilliseconds + " ms");
        //UnityEngine.Debug.LogWarning("Normaler Agent:" + stopwatch2.ElapsedMilliseconds + " ms");

        timesGeneration.Add((int)stopwatch.ElapsedMilliseconds);
        Debug.LogWarning("Generierung der Welt: Anzahl in Liste: " + timesGeneration.Count());
        showList(timesGeneration);

        timesDefaultAgent.Add((int)(stopwatch2.ElapsedMilliseconds));
        Debug.LogWarning("Default Agent: Anzahl in Liste: " + timesDefaultAgent.Count());
        showList(timesDefaultAgent);

        if (NavMeshLinkScript.GetComponent<AgentScript>().GoalReachable == true)
        {
            ListPlayable.Add(0);
            playableChecked = true;
        }

        if (NavMeshLinkScript.GetComponent<AgentScript>().GoalReachable == false && erweitert && !noRepair)
        {

            //NavMeshLinkScript.GetComponent<TESTTEST>().ColorChangeFarbe = Color.red;


            Stopwatch stopwatch3 = new Stopwatch();

            // Starte die Zeitmessung
            stopwatch3.Start();

            // Der Code, dessen Ausführungszeit du messen möchtest
            NavMeshLinkScript.GetComponent<AgentScript>().erweiteterAgent();

            // Stoppe die Zeitmessung
            stopwatch3.Stop();
            timesRepairAgent.Add((int)(stopwatch3.ElapsedMilliseconds));
            Debug.LogWarning("Repair Agent: Anzahl in Liste: " + timesRepairAgent.Count());
            showList(timesRepairAgent);

            if (!playableChecked)
            {
                if (NavMeshLinkScript.GetComponent<AgentScript>().checkIfGoalIsReachableFromStart())
                {
                    ListPlayable.Add(1);
                }
                else
                {
                    ListPlayable.Add(2);
                }
            }

            // Ausgabe der gemessenen Zeit in Millisekunden
            //UnityEngine.Debug.LogWarning("erweiteter Agent: " + stopwatch3.ElapsedMilliseconds + " ms");
            HashSet<Vector3> Positions = new HashSet<Vector3>();
            foreach (Transform t in NavMeshLinkScript.GetComponent<AgentScript>().ModifikationsTransform)
            {
                Positions.Add(t.position);
            }
            //UnityEngine.Debug.LogWarning("Modifikationen: " + NavMeshLinkScript.GetComponent<AgentScript>().ModifikationsTransform.Count());
            //UnityEngine.Debug.LogWarning("Modifikationen: " + Positions.Count());
            ListModifications.Add(Positions.Count());
            Debug.LogWarning("Modifikationen: Anzahl in Liste: " + ListModifications.Count());
            showListModifications(ListModifications);

        }
        if (testing)
        {
            if (timesGeneration.Count() == 1000)
            {
                //gebe Statistiken aus

                levelPlayable();

                float[] generation = MittelwertStandardabweichung(timesGeneration);
                Debug.LogWarning("Generierung der Welt: " + timesGeneration.Count() + " Objekte  ||  Durchschnitt: " + timesGeneration.Average() + "  ||  Test: " + generation[0] + "  ||  Standaradabweichung: " + generation[1]);

                float[] defaultagent = MittelwertStandardabweichung(timesDefaultAgent);
                Debug.LogWarning("Default Agent: " + timesDefaultAgent.Count() + " Objekte  ||   Durchschnitt: " + timesDefaultAgent.Average() + "  ||   Test: " + defaultagent[0] + "   ||   Standaradabweichung: " + defaultagent[1]);

                float[] repairAgent = MittelwertStandardabweichung(timesRepairAgent);
                Debug.LogWarning("Repair Ahgnt: " + timesRepairAgent.Count() + " Objekte  ||   Durchschnitt: " + timesRepairAgent.Average() + "  ||   Test: " + repairAgent[0] + "   ||   Standaradabweichung: " + repairAgent[1]);

                Debug.LogWarning("Modifikationen: Median: " + CalculateMedian(ListModifications) + "   ||  bei " + ListModifications.Count() + " Objekten   ||   Minimum: " + ListModifications.Min() + "   ||   Maximum :" + ListModifications.Max());
                EditorApplication.isPaused = true;
            }
            else
            {
                playerFallScript.GetComponent<PlayerFall>().ReloadScene();

            }
        }
        //UnityEngine.Debug.LogWarning("Goal Reachable: " + NavMeshLinkScript.GetComponent<AgentScript>().checkIfGoalIsReachableFromStart());
        //reachable = NavMeshLinkScript.GetComponent<AgentScript>().checkIfGoalIsReachableFromStart();
        


    }

    void levelPlayable()
    {
        Debug.LogWarning("Level Playable? Ja, ohne Reparatur: " + countX(ListPlayable,0) +"  ||  Ja, mit Reparatur: " + countX(ListPlayable,1) +"  || Nein: "+countX(ListPlayable,2));
    }
    int countX(List<int> a, int X)
    {
        int counter = 0;
        foreach (int i in a)
        {
            if (i == X) { counter++; }
        }
        return counter;
    }

    float CalculateMedian(List<int> values)
    {
        // Liste sortieren
        values.Sort();

        int count = values.Count;
        if (count == 0)
        {
            Debug.LogWarning("Die Liste ist leer.");
            return 0; // Oder einen anderen Wert zurückgeben, der anzeigt, dass kein Median existiert.
        }

        // Median berechnen
        if (count % 2 == 0) // Wenn die Anzahl der Elemente gerade ist
        {
            // Durchschnitt der beiden mittleren Werte
            return (values[count / 2 - 1] + values[count / 2]) / 2;
        }
        else // Wenn die Anzahl der Elemente ungerade ist
        {
            // Der mittlere Wert
            return values[count / 2];
        }
    }


    private float[] MittelwertStandardabweichung(List<int> l)
    {
        float[] a = new float[2];
        float mean = (float)l.Average();
        a[0] = mean;
        float sumOfSquaresOfDifferences = l.Select(val => (val - mean) * (val - mean)).Sum();
        float standardDeviation = Mathf.Sqrt(sumOfSquaresOfDifferences / l.Count);
        a[1] = standardDeviation;
        return a;
    }

    private void showList(List<int> a)
    {
        foreach(int i in a)
        {
            Debug.Log(i +" ms");
        }
    }
    private void showListModifications(List<int> a)
    {
        foreach (int i in a)
        {
            Debug.Log(i + " modifications");
        }
    }

    //erstelle ein 2 dimensionales Array (Map) von Position Points
    private void createMap(List<int> values, int testlength, int startHöhe, int zielHöhe)
    {
        //sicherstellen, dass Länge 2^n +1 für beliebiges n ist. Sollte dies nicht der Fall sein, wird der nächstgrößte Wert genommen, der der Anforderung genügt
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
                    map[i, j] = new PositionPoint(i, j, values.ElementAt(startHöhe));   //StartPosition
                }
                else if (i == 0 & j == maplength - 1 | i == maplength - 1 & j == 0)
                {
                    map[i, j] = new PositionPoint(i, j, values.ElementAt(UnityEngine.Random.Range(0, count)));   //weise den Eckwerten der Map einen Value zu
                }
                else if(i == maplength - 1 & j == maplength - 1 || i==maplength-3 && j==maplength-3 || i==maplength-3 && j==maplength-2 ||i==maplength-3 && j==maplength-1
                    || i == maplength - 2 && j == maplength - 3 || i == maplength - 2 && j == maplength - 2 || i == maplength - 2 && j == maplength - 1 
                    || i == maplength - 1 && j == maplength - 3 || i == maplength - 1 && j == maplength - 2)
                {
                    map[i, j] = new PositionPoint(i, j, values.ElementAt(zielHöhe));
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
        
        foreach (PositionPoint p in map)
        {
            if(p.getX() == maplength-1 && p.getY()==maplength-1 || p.getX() == maplength - 1 && p.getY() == maplength - 2 || p.getX() == maplength - 1 && p.getY() == maplength - 3 
            || p.getX() == maplength -2 && p.getY() == maplength - 1 || p.getX() == maplength - 2 && p.getY() == maplength - 2 || p.getX() == maplength - 2 && p.getY() == maplength - 3
            || p.getX() == maplength -3 && p.getY() == maplength - 1 || p.getX() == maplength - 3 && p.getY() == maplength - 2 || p.getX() == maplength - 3 && p.getY() == maplength - 3)
            {
                for (int i = 0; i < p.getValue(); i++)
                {
                    NavMeshLinkScript.GetComponent<AgentScript>().AllTiles.Add(Instantiate(tile, new Vector3(p.getX(), i, p.getY()), Quaternion.identity));

                }
                if(p.getX() == maplength - 2 && p.getY() == maplength - 2)
                {
                    Last = Instantiate(end_tile, new Vector3(p.getX(), p.getValue(), p.getY()), Quaternion.identity);
                    Last.layer = 7;
                    //NavMeshLinkScript.GetComponent<TESTTEST>().AllTiles.Add(Last);
                    NavMeshLinkScript.GetComponent<AgentScript>().GoalPositions.Add(new Vector3(p.getX(), p.getValue(), p.getY()));
                }
                else
                {
                    GameObject Top = Instantiate(end_tile, new Vector3(p.getX(), p.getValue(), p.getY()), Quaternion.identity);
                    Top.layer = 7;
                    NavMeshLinkScript.GetComponent<AgentScript>().AllTiles.Add(Top);
                    NavMeshLinkScript.GetComponent<AgentScript>().GoalPositions.Add(new Vector3(p.getX(), p.getValue(), p.getY()));
                }
            }
            else if(p.getX() == 0 && p.getY() == 0 || p.getX() == 0 && p.getY() == 1 || p.getX() == 0 && p.getY() == 2
            || p.getX() == 1 && p.getY() == 0 || p.getX() == 1 && p.getY() == 1 || p.getX() == 1 && p.getY() == 2
            || p.getX() == 2 && p.getY() == 0 || p.getX() == 2 && p.getY() == 1 || p.getX() == 2 && p.getY() == 2)
            {
                for (int i = 0; i < p.getValue(); i++)
                {
                    NavMeshLinkScript.GetComponent<AgentScript>().AllTiles.Add(Instantiate(tile, new Vector3(p.getX(), i, p.getY()), Quaternion.identity));

                }

                GameObject Top = Instantiate(start_tile, new Vector3(p.getX(), p.getValue(), p.getY()), Quaternion.identity);
                Top.layer = 7;
                NavMeshLinkScript.GetComponent<AgentScript>().AllTiles.Add(Top);
                NavMeshLinkScript.GetComponent<AgentScript>().GoalPositions.Add(new Vector3(p.getX(), p.getValue(), p.getY()));
            }
            else
            {
                int height = p.getValue();
                if (height > 2 && height != deleteHeight1 && height != deleteHeight2 && height != deleteHeight3)
                {
                    for (int i = 0; i < height; i++)
                    {
                        NavMeshLinkScript.GetComponent<AgentScript>().AllTiles.Add(Instantiate(tile, new Vector3(p.getX(), i, p.getY()), Quaternion.identity));

                    }
                    GameObject Top = Instantiate(tile, new Vector3(p.getX(), height, p.getY()), Quaternion.identity);
                    //Last = Top;
                    Top.layer = 7;
                    NavMeshLinkScript.GetComponent<AgentScript>().AllTiles.Add(Top);
                    
                }
            }

        }

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
        if (diff == 0)
        {
            diamond_square(17, 0, 8, 0, 1, 3, 7); createTiles(5, 5, 5);
        }
        else if (diff == 1)
        {

            diamond_square(17, 0, 12, -1, 2, 3, 10); createTiles(5, 8, 8);
        }
        else if (diff == 2)
        {
            diamond_square(33, 0, 12, -2, 2, 3, 10); createTiles(4, 6, 8);
        }
        else if(diff == 3)
        {
            noRepair = true;
            diamond_square(17, 0, 12, -1, 1, 3, 7); createTiles(0, 0, 0);
        }
    }




    private void diamond_square(int testlength, int minValue, int maxValue, int minOffset, int maxOffset, int startHöhe, int zielHöhe)
    {
        //erstelle Intervalls für den Wertebereich (value)
        values = new List<int>();
        for (int i = minValue; i <= maxValue; i++)
        {
            if (debugCheck) { Debug.Log("value i: " + i); }
            values.Add(i);

        }

        //erstelle Map
        createMap(values, testlength, startHöhe, zielHöhe);

        //erstelle Intervalls für den Offet Bereich 
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


    //Klasse für Diamond und Square Schritt. Ein Punkt enthält die Position (x,y) und den Höhenwert
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

}
