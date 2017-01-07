using UnityEngine;
using System.Collections;
using Pathfinding;
using Pathfinding.RVO;

public class ActorPathFinding
{
    public Transform targetPosition;
    private Seeker seeker;
    private CharacterController controller;
    public Path path;
    public float speed = 2;
    public float nextWaypointDistance = 3;
    private int currentWaypoint = 0;
    public float repathRate = 1f;
    private float lastRepath = -9999;
     
    public bool targetReached
    {
        get;
        protected set;
    }

    private bool canMove;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seeker"></param>
    public void Init( Seeker seeker, CharacterController controller,Transform targetPosition)
    { 
        this.seeker = seeker;
        this.controller = controller;
        this.targetPosition = targetPosition;
    }
    public void OnPathComplete(Path p)
    {
        Debug.Log("A path was calculated. Did it fail with an error? " + p.error);
        // Path pooling. To avoid unnecessary allocations paths are reference counted.
        // Calling Claim will increase the reference count by 1 and Release will reduce
        // it by one, when it reaches zero the path will be pooled and then it may be used
        // by other scripts. The ABPath.Construct or Seeker.StartPath methods will
        // take a path from the pool if possible. See also the documentation page about path pooling.
        p.Claim(this);
        if (!p.error)
        {
            if (path != null) path.Release(this);
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
        else
        {
            p.Release(this);
        }
    }
    public void Update()
    {
      /*  if (Time.time - lastRepath > repathRate && seeker.IsDone())
        {
            lastRepath = Time.time + Random.value * repathRate * 0.5f;
            // Start a new path to the targetPosition, call the the OnPathComplete function
            // when the path has been calculated (which may take a few frames depending on the complexity)
            seeker.StartPath(controller.transform.position, targetPosition.position, OnPathComplete);
        }
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            return;
        }
        if (currentWaypoint > path.vectorPath.Count) return;
        if (currentWaypoint == path.vectorPath.Count)
        {
            Debug.Log("End Of Path Reached");
            currentWaypoint++;
            return;
        }
        // Direction to the next waypoint
        Vector3 dir = (path.vectorPath[currentWaypoint] - controller.transform.position).normalized;
        dir *= speed;
        // Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
        controller.SimpleMove(dir);
        // The commented line is equivalent to the one below, but the one that is used
        // is slightly faster since it does not have to calculate a square root
        //if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
        if ((controller.transform.position - path.vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance)
        {
            currentWaypoint++;
            return;
        }
        */
    }
}
