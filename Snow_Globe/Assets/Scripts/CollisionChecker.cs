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

    public bool CheckMiniatureCollision(Vector3 point, KDTree kdTree)
    {
        nearestNode = kdTree.StartSearch(point);

        // Keep this order of the vectices in the subtraction
        currentVector3 = point - nearestNode.position;

        for (int i = 0; i < 3; i++)
        {
            if (currentVector3[i] < nearestNode.radius)
            {
                // If we use bounding cubes to cover the miniatures, this would mean that the point is inside the bounding cube, thus collision with the miniature has taken place.
                // Point is outside of the miniature and should not enter the bounding cube.
                return true;
            }
        }

        // No collision if point is outside of bounding cubes.
        return false;

    }
}
