using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeParticles
{
    public Vector3 position;
    public float speed = 0.1f;
    public Vector3 directionVelocity;
    public Vector3 oldPositionInLocalSpace;
    public bool collided = false;
    public TriangleNode latestCollidedTriangle;
}
