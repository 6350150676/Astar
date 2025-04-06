using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;
    public PathMarker(MapLocation l ,float g, float h ,float f ,GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;

    }
    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        { return false; }
        else
        {
            return location.Equals(((PathMarker)obj).location);
        }
        
    }
    public override int GetHashCode()
    {
        return location.GetHashCode();
    }


}

public class FindPathAStar : MonoBehaviour
{
    public Maze maze;
    public Material closedmaterial;
    public Material openmaterial;
    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();
    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker startnode;
    PathMarker goalnode;
    PathMarker lastpos;
    bool done = false;
    void RemoveallMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach(GameObject m in markers)
        {
            Destroy(m);
        }
    }
    void beginesearch()
    {
        done = false;
        RemoveallMarkers();
        List<MapLocation> locations = new List<MapLocation>();
        for(int z = 1; z< maze.depth - 1; z++)
        {
            for(int x = 1;x< maze.width - 1; x++)
            {
                if (maze.map[x,z] != 1)
                {
                    locations.Add(new MapLocation(x, z));
                }
            }

        }locations.Shuffle();
        Vector3 startlocation = new Vector3(locations[0].x *maze.scale, 0, locations[0].z * maze.scale);
        startnode = new PathMarker(new MapLocation(locations[0].x,locations[0].z), 0, 0,0,
                                    Instantiate(start, startlocation, Quaternion.identity), null);

        Vector3 goallocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalnode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0,
                                    Instantiate(end, goallocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();
        open.Add(startnode);
        lastpos = startnode;



    }
    void search(PathMarker thisnode)
    {
        if (thisnode == null) return;
        if (thisnode.Equals(goalnode)) { done = true; return; }
        foreach (MapLocation dir in maze.directions)
        {
            MapLocation neighbour = dir + thisnode.location;
            if (maze.map[neighbour.x, neighbour.z] == 1) continue;
            if (neighbour.x < 1 || neighbour.x >= maze.width || neighbour.z < 1 || neighbour.z >= maze.depth) continue;
            if (isclosed(neighbour)) continue;

            float G = Vector2.Distance(thisnode.location.ToVector(), neighbour.ToVector()) + thisnode.G;
            float H = Vector2.Distance(neighbour.ToVector(), goalnode.location.ToVector());
            float F = G + H;

            GameObject pathblock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0, neighbour.z * maze.scale),
                                               Quaternion.identity);
            TextMesh[] values = pathblock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G:" + G.ToString("0.00");
            values[1].text = "H:" + H.ToString("0.00");
            values[2].text = "F:" + F.ToString("0.00");
            if (!updateMarker(neighbour, G, H, F, thisnode))
                open.Add(new PathMarker(neighbour, G, H, F, pathblock, thisnode));
            
        }
        open = open.OrderBy(p => p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);
        pm.marker.GetComponent<Renderer>().material = closedmaterial;
        lastpos = pm;
    }
    bool updateMarker(MapLocation pos , float g, float h, float f, PathMarker prt)
    {
        foreach(PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }return false;
    }

    bool isclosed(MapLocation marker)
    {
        foreach(PathMarker p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }return false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void Getpath()
    {
        RemoveallMarkers();
        PathMarker begin = lastpos;
        while(!startnode.Equals(begin) && begin != null)
        {
            Instantiate(pathP,new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale)
                                       , Quaternion.identity);
            begin = begin.parent;

            Instantiate(pathP, new Vector3(startnode.location.x * maze.scale, 0, startnode.location.z * maze.scale)
                                       , Quaternion.identity);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) beginesearch();
        if (Input.GetKeyDown(KeyCode.C) && !done) search(lastpos);
        if (Input.GetKeyDown(KeyCode.M)) Getpath();
    }
}
