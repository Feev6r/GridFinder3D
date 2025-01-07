using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grid3DTest : MonoBehaviour
{
    [SerializeField] bool findDistance;
    [SerializeField] int rows = 0;
    [SerializeField] int layers = 0;
    [SerializeField] int columns = 0;
    [SerializeField] int scale = 1;

    [SerializeField] Vector3 leftBottonLocation = new Vector3(0, 0, 0);

    List<List<List<GameObject>>> GridList = new();
    List<List<List<GameObject>>> VisitedGridList = new();

    [SerializeField] public List<GameObject> Path = new();

    [SerializeField] int startX = 0;
    [SerializeField] int startZ = 0;
    [SerializeField] int startY = 0;

    [SerializeField] int endX = 2;
    [SerializeField] int endZ = 2;
    [SerializeField] int endY = 2;


    [SerializeField] GameObject GridPrefab;
    List<GameObject> TempList = new();

    private void Awake()
    {

        if (GridPrefab)
            GenerateGrid();
        else
            print("No hay Prefabs");

    }

    void Start()
    {


    }

    void Update()
    {
        UpdateVisetedList();

        

        if (findDistance)
        {

            SetDistance();

            for (int x = 0; x < GridList.Count; x++)
            {
                for (int z = 0; z < GridList[x].Count; z++)
                {
                    for (int y = 0; y < GridList[x][z].Count; y++)
                    {
                        GameObject obj = GridList[x][z][y];

                        if (obj && obj.GetComponent<Grid3DStat>().visited != -1)
                            obj.GetComponent<MeshRenderer>().material.color = Color.green;
                        else if(obj && obj.GetComponent<Grid3DStat>().visited == -1)
                            obj.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                }
            }

            SetPath();
            findDistance = false;
        }
    }

    void GenerateGrid()
    {

        for (int x = 0; x < rows; x++)
        {

            GridList.Add(new List<List<GameObject>>());

            for (int z = 0; z < columns; z++)
            {

                GridList[x].Add(new List<GameObject>());

                for (int y = 0; y < layers; y++)
                {
                    GameObject obj = Instantiate(GridPrefab, new Vector3(leftBottonLocation.x + scale * x, leftBottonLocation.y + scale * y, leftBottonLocation.z + scale * z), Quaternion.identity);
                    obj.transform.SetParent(transform);

                    obj.GetComponent<Grid3DStat>().x = x;
                    obj.GetComponent<Grid3DStat>().z = z;
                    obj.GetComponent<Grid3DStat>().y = y;


                    GridList[x][z].Add(obj);
                }
            }
        }
    }

    void InitalSetUp()
    {
        for (int x = 0; x < GridList.Count; x++)
        {
            for (int z = 0; z < GridList[x].Count; z++)
            {
                for (int y = 0; y < GridList[x][z].Count; y++)
                {
                    if (GridList[x][z][y])
                        GridList[x][z][y].GetComponent<Grid3DStat>().visited = -1;
                }
            }

        }

        GridList[startX][startZ][startY].GetComponent<Grid3DStat>().visited = 0; //donde inicia

    }

    void SetDistanceOld()
    {

        InitalSetUp();

        for (int i = 1; i < rows; i++) //en este caso puse rows, porque el maximo de step basicamente puede ser el maximos de los 3, rows, colums y layers, no da 125 Miauhs pero sigue funcionando
        {
            for (int x = 0; x < GridList.Count; x++)
            {
                for (int z = 0; z < GridList[x].Count; z++)
                {
                    for (int y = 0; y < GridList[x][z].Count; y++)
                    {
                        GameObject obj = GridList[x][z][y];

                        //busca al objeto 0 en toda la lista, setea a su alrededor 1,
                        //se vuelve a repetir, busca en toda la lista 1, setea 2 a su alrededor
                        //problema de rendimiento, si hay un maximo de 4 steps, pues que no siga de ahi pa alla. (solucionando alla arriba)
                        if (obj && obj.GetComponent<Grid3DStat>().visited == i - 1) 
                        {
                            //Debug.Log("Miauh");
                            TestDirections(obj.GetComponent<Grid3DStat>().x, obj.GetComponent<Grid3DStat>().z, obj.GetComponent<Grid3DStat>().y, i);
                        }
                    }

                }
            }
        }

       
    }

    void SetDistance()
    {
        InitalSetUp();

        for (int i = 1; i < rows * columns; i++)
        {
            for (int x = 0; x < VisitedGridList.Count; x++)
            {

                for (int z = 0; z < VisitedGridList[x].Count; z++)
                {

                    for (int y = 0; y < VisitedGridList[x][z].Count; y++)
                    {
                        GameObject obj = VisitedGridList[x][z][y];

                        if (obj.GetComponent<Grid3DStat>().visited == i - 1)
                        TestDirections(obj.GetComponent<Grid3DStat>().x, obj.GetComponent<Grid3DStat>().z, obj.GetComponent<Grid3DStat>().y, i);
                    }
                }
            }
        }
        
    }

    void UpdateVisetedList() 
    {
        VisitedGridList.Clear();

        for (int x = 0; x < GridList.Count; x++)
        {
            VisitedGridList.Add(new List<List<GameObject>>());

            for (int z = 0; z < GridList[x].Count; z++)
            {
                VisitedGridList[x].Add(new List<GameObject>());

                for (int y = 0; y < GridList[x][z].Count; y++)
                {
                    if (GridList[x][z][y] && GridList[x][z][y].GetComponent<Grid3DStat>().visited != -1)
                        VisitedGridList[x][z].Add(GridList[x][z][y]);
                }
            }
        }
    }


    void SetPath()
    {
        int step;
        int x = endX;
        int z = endZ;
        int y = endY;

        Path.Clear();

        if (GridList[endX][endZ][endY] && GridList[endX][endZ][endY].GetComponent<Grid3DStat>().visited > 0)
        {
            Path.Add(GridList[endX][endZ][endY]);
            step = GridList[endX][endZ][endY].GetComponent<Grid3DStat>().visited - 1;

        }
        else
        {
            print("No se pudo encontrar la ubicacion deseada");
            return;
        }

        for (; step > -1; step--)
        {
            TestDirections(x, z, y, step, TempList); //agrega los grids de cada step hacia atras, empezando de -1 


            GameObject TempObject = FindClosest(GridList[endX][endZ][endY].transform, TempList);
            Path.Add(TempObject);

            foreach (GameObject obj in Path)
            {
                obj.GetComponent<MeshRenderer>().material.color = Color.red;
                Debug.Log("holas");
            }

            //empezamos desde el siguente.
            x = TempObject.GetComponent<Grid3DStat>().x;
            z = TempObject.GetComponent<Grid3DStat>().z;
            y = TempObject.GetComponent<Grid3DStat>().y;

            //limpiamos
            TempList.Clear();
        }
    }
    bool TestEachDireccion(int x, int z, int y, int step, int Posibilidad, (int i, int k, int j) Value)
    {

        if (Posibilidad != 1) //NOS SALTAMOS X0, Y0, Z0
        {
            if (y + Value.j < layers && y + Value.j > -1 && z + Value.k < columns && z + Value.k > -1 && x + Value.i < rows && x + Value.i > -1 &&
                GridList[x + Value.i][z + Value.k][y + Value.j] &&
                GridList[x + Value.i][z + Value.k][y + Value.j].GetComponent<Grid3DStat>().visited == step)
            {
                //Debug.Log("true");
                return true;
            }
            else
                //Debug.Log("false");
            return false;
        }

        return false;
    }

    void TestDirections(int x, int z, int y, int step, List<GameObject> TempList = default)
    {

        int Posibilidad = 0;
        int[] valuesX = { 0, 1, -1 }; // X, X+1, X-1
        int[] valuesY = { 0, 1, -1 }; // Y, Y+1, Y-1
        int[] valuesZ = { 0, 1, -1 }; // Z, Z+1, Z-1

        for (int i = 0; i < valuesX.Length; i++)
        {
            for (int k = 0; k < valuesZ.Length; k++)
            {
                for (int j = 0; j < valuesY.Length; j++)
                {
                    Posibilidad++;

                    if (TestEachDireccion(x, z, y, -1, Posibilidad, Value: (valuesX[i], valuesZ[k], valuesY[j])) && TempList == null)//como sabremos si especifica direccion esta testeada. voy a caga
                    {
                        SetVisited(x + valuesX[i], z + valuesZ[k], y + valuesY[j], step);
                        //Debug.Log("fufa");
                    }
                    else if(TestEachDireccion(x, z, y, step, Posibilidad, Value: (valuesX[i], valuesZ[k], valuesY[j])) && TempList != null)
                    {
                        TempList.Add(GridList[x + valuesX[i]][z + valuesZ[k]][y + valuesY[j]]);
                        Debug.Log("ae");
                    }
                }

            }

        }
    }

    void SetVisited(int x, int z, int y, int step)
    {
        if (GridList[x][z][y])
        {
            GridList[x][z][y].GetComponent<Grid3DStat>().visited = step;
        }
    }

    GameObject FindClosest(Transform targetLocation, List<GameObject> list) //ojito
    {
        float currentDistance = scale * rows * columns * layers;
        int indexNumber = 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (Vector3.Distance(targetLocation.position, list[i].transform.position) < currentDistance)
            {
                currentDistance = Vector3.Distance(targetLocation.position, list[i].transform.position);
                indexNumber = i;
            }
        }

        return list[indexNumber]; //devuelve los objetos de la lista de cada step que esten mas cercanos al punto final
    }

}
