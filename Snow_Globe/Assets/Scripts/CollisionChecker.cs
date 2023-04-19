using UnityEngine;

public class CollisionChecker 
{
    private CentroidNode nearestNode;
    private Vector3 currentVector3;

    public bool CheckWallCollision(Vector3 point, KDTree kDTree)
    {
        nearestNode = kDTree.StartSearch(point);

        currentVector3 = point - nearestNode.position;

        if(Vector3.Magnitude(currentVector3) < nearestNode.radius)
        {
            // No collision if point is within bounding sphere.
            return false;
        }

        else
        {
            return true;
        }

    }
}
