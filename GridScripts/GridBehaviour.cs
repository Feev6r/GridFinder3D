using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GridBehaviour : MonoBehaviour
{
    public bool findDistance;
    public int rows = 10;
    public int columns = 10;
    public int scale = 1;
    public GameObject GridPrefab;
    public Vector3 leftBottonLocation = new Vector3 (0, 0, 0);

    public GameObject[,] GridArray;
    public int startX = 0;
    public int startY = 0;
    public int endX = 2;
    public int endY = 2;

    [SerializeField] GameObject t;
    [SerializeField] int XFinal;
    [SerializeField] int YFinal;

    public List<GameObject> Path = new List<GameObject>();

    private void Awake()
    {
        GridArray =  new GameObject[columns, rows];

        if (GridPrefab)
            GenerateGrid();
        else
            print("NO HAY PREFAPE");
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (findDistance)
        {
            foreach (GameObject obj in Path)
            {
                if(obj)
                obj.GetComponent<MeshRenderer>().material.color = Color.gray;
            }
            SetDistance();
            setPath();


            findDistance = false;
        }
    }
    void GenerateGrid()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                GameObject obj = Instantiate(GridPrefab, new Vector3(leftBottonLocation.x + scale * i, leftBottonLocation.y, leftBottonLocation.z + scale * j), Quaternion.identity);
                obj.transform.SetParent(gameObject.transform);
                //se obtienen de su gridstat x,y y se le pasan sus coordenadas
                obj.GetComponent<GridStat>().x = i; //columnas, vertical
                obj.GetComponent<GridStat>().y = j; //filas, horizontal

                GridArray[i, j] = obj;
            }
        }
    }

    void SetDistance()
    {
        initialSetUp();
        int x = startX;
        int y = startY;
        int[] testArray = new int[rows * columns];

        for (int step = 1; step < rows * columns; step++)
        {
            foreach (GameObject obj in GridArray) //empezamos en 0, buscamos al rededor, si son -1 seteamos el actual step, seguimos, empezamos en 1, comprobamos al rededor y setemaos el actual step
            {
                Debug.Log("bruh");

                if (obj && obj.GetComponent<GridStat>().visited == step - 1)
                    TestFourDirections(obj.GetComponent<GridStat>().x, obj.GetComponent<GridStat>().y, step);
            }
        }
    }

    void setPath()
    {
        int step;
        int x = endX;
        int y = endY;
        List<GameObject> tempList = new();
        Path.Clear();

        //una vez hayamos recorrido todo el grid con los steps, verificamos el punto final donde queremos llegar si existe o esta disponible 
        if (GridArray[endX, endY] && GridArray[endX, endY].GetComponent<GridStat>().visited > 0)
        {
            Path.Add(GridArray[x, y]);
                step = GridArray[x, y].GetComponent<GridStat>().visited -1; //step = punto finial.visited -1, osea si es de 5, step empezara con 4
        }
        else
        {
            print("No se pudo encontrar la ubicacion deseada");
            return;
        }
        //desde le step final hacia atras
        for (int i = step; step > -1; step--)
        {
            if (TestDireccion(x, y, step, 8))
                tempList.Add(GridArray[x - 1, y + 1]);
            if (TestDireccion(x, y, step, 7))
                tempList.Add(GridArray[x - 1, y - 1]);
            if (TestDireccion(x, y, step, 6))
                tempList.Add(GridArray[x + 1, y - 1]);
            if (TestDireccion(x, y, step, 5))
                tempList.Add(GridArray[x + 1, y + 1]);

            //---------------------------------------

            if (TestDireccion(x, y, step, 1))
                tempList.Add(GridArray[x, y + 1]);
            if (TestDireccion(x, y, step, 2))
                tempList.Add(GridArray[x + 1, y]);
            if (TestDireccion(x, y, step, 3))
                tempList.Add(GridArray[x, y - 1]);
            if (TestDireccion(x, y, step, 4))
                tempList.Add(GridArray[x - 1, y]);


            GameObject tempObj = FindClosest(GridArray[endX, endY].transform, tempList); //comparar el punto de llegada con cada grid que sea igual al step y mas cercano
            Path.Add(tempObj); //el path obtiene ese gameobject

            t = Path[0];

            XFinal = t.GetComponent<GridStat>().x;
            YFinal = t.GetComponent<GridStat>().y;


            foreach (GameObject obj in Path)
            {
                obj.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            //despues X,Y que son el punto de llegada, ya no seran el ultimo punto, sino el penultimo, para comparar desde ese punto mas cercano
            x = tempObj.GetComponent<GridStat>().x; 
            y = tempObj.GetComponent<GridStat>().y;

            tempList.Clear();
        }       

    }

    void initialSetUp()
    {
        foreach (GameObject obj in GridArray)
        {
            if(obj)
            obj.GetComponent<GridStat>().visited = -1; // no visitado
        }

        GridArray[XFinal, YFinal].GetComponent<GridStat>().visited = 0;
    }

    bool TestDireccion(int x, int y, int step, int direction)
    { 
        switch(direction) // 4 Izquierda x horizontal izquierda / / 3 abajo y vertical abajo / / 2 Derecha x horizontal derecha / / 1 arriba y vertical arriba
        {

            case 8: //Izquierda Arriba
                if (x - 1 > -1 && y + 1 < rows && GridArray[x - 1, y + 1] && GridArray[x - 1, y + 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 7://Izquierda Abajo
                if (y - 1 > -1 && x - 1 > -1 && GridArray[x - 1, y - 1] && GridArray[x - 1, y - 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 6://Derecha Abajo
                if (x + 1 < columns && y - 1 > -1 && GridArray[x + 1, y - 1] && GridArray[x + 1, y - 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 5://Derecha arriba
                if (y + 1 < rows && x + 1 < columns && GridArray[x + 1, y + 1] && GridArray[x + 1, y + 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;

            //--------------------------------------------------------------------------------------------------

            case 4:
                if (x - 1 > -1 && GridArray[x-1, y] && GridArray[x-1, y].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 3:
                if (y - 1 > -1 && GridArray[x, y - 1] && GridArray[x, y - 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 2:
                if (x + 1 < columns && GridArray[x+1, y] && GridArray[x+1, y].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 1:
                if (y + 1 < rows && GridArray[x, y + 1] && GridArray[x, y + 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
        }

        return false;
    }

    void TestFourDirections(int x, int y, int step)
    {
        if (TestDireccion(x, y, -1, 8)) //si no esta visitado izquierda Arriba
            SetVisited(x - 1, y + 1, step);

        if (TestDireccion(x, y, -1, 7)) //si no esta visitado izquierda Abajo
            SetVisited(x - 1, y - 1, step);

        if (TestDireccion(x, y, -1, 6)) //si no esta visitado Derecha Abajo
            SetVisited(x + 1, y - 1, step);

        if (TestDireccion(x, y, -1, 5)) //si no esta visitado Derecha Arriba
            SetVisited(x + 1, y + 1, step);

        //-----------------------------------------------------------------


        if (TestDireccion(x, y, -1, 1)) //si no esta visitado arriba
            SetVisited(x, y + 1, step); //setea el step ahi

        if (TestDireccion(x, y, -1, 2)) //si no esta visitado derecha
            SetVisited(x + 1, y, step);

        if (TestDireccion(x, y, -1, 3)) //si no esta visitado abajo
            SetVisited (x, y-1, step);

        if(TestDireccion(x, y, -1, 4)) //si no esta visitado inzquierda
            SetVisited(x - 1, y, step);

    }

    void SetVisited(int x, int y, int step)
    {
        if (GridArray[x, y])
        {
            GridArray[x,y].GetComponent<GridStat>().visited = step;
        }
    }

    GameObject FindClosest(Transform targetLocation, List<GameObject> list)
    {
        float currentDistance = scale * rows * columns;
        int indexNumber = 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (Vector3.Distance(targetLocation.position, list[i].transform.position) < currentDistance)
            {
                currentDistance = Vector3.Distance(targetLocation.position, list[i].transform.position);
                indexNumber = i;
            }
        }

        return list[indexNumber]; //devuelve el indice de la templist que este mas cercano
    }
}
