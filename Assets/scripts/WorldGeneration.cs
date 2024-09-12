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
        NavMeshLinkScript.GetComponent<AgentScript>().newTile = new_tile;
        erstelleWelt(difficulty.Difficulty);

        //NavMeshLinkScript.GetComponent<TESTTEST>().deleteObjectsBetween(new Vector3 (4,0,0), new Vector3(4, 0, 10));

        //diamond_step(testmap, new PositionPoint(0, 0, 0), new PositionPoint(0, 2, 2), new PositionPoint(2, 0, 3), new PositionPoint(2, 2, 5),  testoffset, testvalues);

        surface.BuildNavMesh();
        //Vector3 startPosition = new Vector3(player.transform.position.x, player.transform.position.y - 1, player.transform.position.z);
        Vector3 startPosition = new Vector3(1, 3,1);
        //Debug.LogWarning(startPosition);
        NavMeshLinkScript.GetComponent<AgentScript>().PlayerStartPosition = startPosition;
        mapColor.Add(Color.blue); mapColor.Add(Color.red); mapColor.Add(Color.cyan); mapColor.Add(Color.yellow); mapColor.Add(Color.grey); mapColor.Add(Color.blue); mapColor.Add(Color.red); mapColor.Add(Color.cyan); mapColor.Add(Color.yellow); mapColor.Add(Color.grey);
        NavMeshLinkScript.GetComponent<AgentScript>().Colors = mapColor;
        Debug.LogWarning("Start Position: " + startPosition.ToString());
        playerFallScript.GetComponent<PlayerFall>().initialPosition = startPosition;
        NavMeshLinkScript.GetComponent<AgentScript>().GoalTile = Last;
        //IsPositionOnNavMesh(startPosition);
        //NavMeshLinkScript.GetComponent<TESTTEST>().ColorChangeFarbe = Color.blue;
        NavMeshLinkScript.GetComponent<AgentScript>().destroyedTile = destroyed_tile; 
        NavMeshLinkScript.GetComponent<AgentScript>().StartAgent(startPosition);



        if(NavMeshLinkScript.GetComponent<AgentScript>().GoalReachable == false && erweitert)
        {

            //NavMeshLinkScript.GetComponent<TESTTEST>().ColorChangeFarbe = Color.red;
            
            
            Stopwatch stopwatch = new Stopwatch();

            // Starte die Zeitmessung
            stopwatch.Start();

            // Der Code, dessen Ausführungszeit du messen möchtest
            NavMeshLinkScript.GetComponent<AgentScript>().erweiteterAgent();

            // Stoppe die Zeitmessung
            stopwatch.Stop();

            // Ausgabe der gemessenen Zeit in Millisekunden
            UnityEngine.Debug.Log("Ausführungszeit: " + stopwatch.Elapsed + " ms");
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
        if(diff == 0)
        {
            diamond_square(17, 0, 8, 0, 1,3,7); createTiles(5,5,5);
        }
        else if(diff == 1)
        {

            diamond_square(17, 0, 12, -1, 2,3,10); createTiles(5,8,8);
        }
        else if (diff == 2)
        {
            diamond_square(33, 0, 12, -2, 2,3,10); createTiles(4, 6, 8);
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
