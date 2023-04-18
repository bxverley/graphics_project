// ------------------ Jonah's Version ------------------ //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeParticleSystem : MonoBehaviour
{

    List<GlobeParticles> globeParticles = new List<GlobeParticles>();

    int countOfParticles = 20;

    GameObject particleSprite;
    List<GameObject> particleSpriteList = new List<GameObject>();

    
    Vector3 gravity = new Vector3(0, -0.2f, 0);
    const float speedLimit = 0.5f;
    const float collidedSpeedLimit = 0.1f;
    const float speedScaleFactor = 0.008f;
    const float particleMoveFactor_WhenGlobeMove = 1.6f;
    const float maxCosAngleBetwGravityAndTriNorm = 0.4f;
    const float decelerationFactor = 0.01f;
    const float collideDecelerationFactor = 1.5f;
    const float triangleNormForceScaleFactor = 0.03f;
    const float gravityScaleFactor = 0.008f;
    const float awayFromBoundaryScaleFactor = 0.008f;


    SnowGlobe snowGlobe;
    KDTree triangleCenterVerticesTree, meshCollisionKDTree;
    CollisionChecker collisionChecker;
    ObjectMovement objectMovementScript;

    private Camera mainCamera;

    


    // Reused Variables
    Vector3 currentVector3 = new Vector3(0, 0, 0);
    Vector3 currentRandomVecWhenObjMove = new Vector3(0, 0, 0);
    Vector3 currentTriNormXZOnlyVec = new Vector3(0, 0, 0);
    float currentGravityDotTriNorm;
    GlobeParticles currentGlobeParticle;
    TriangleNode nearestTriangle;
    CentroidNode nearestNode;


    // Start is called before the first frame update
    public void Start()
    {

        mainCamera = Camera.main;

        snowGlobe = gameObject.GetComponent<SnowGlobe>();
        triangleCenterVerticesTree = snowGlobe.triangleCenterVerticesTree;
        meshCollisionKDTree = snowGlobe.meshCollisionKDTree;
        collisionChecker = snowGlobe.collisionChecker;
        objectMovementScript = snowGlobe.objectMovementScript;


        for (int i = 0; i < countOfParticles; i++)
        {
            currentVector3.x = -Random.Range(-0.1f, 0.1f);
            currentVector3.y = -Random.Range(-0.1f, 0.1f);
            currentVector3.z = -Random.Range(-0.1f, 0.1f);

            currentGlobeParticle = new GlobeParticles
            {
               
                directionVelocity = currentVector3,
                position = snowGlobe.meshCollisionKDTree.GetRootNode().position + currentVector3,
                speed = speedLimit * 2.0f,
                oldPositionInLocalSpace = snowGlobe.meshCollisionKDTree.GetRootNode().position + currentVector3
                
            };
            globeParticles.Add(currentGlobeParticle);


            // Reference to make a sprite to Project: https://answers.unity.com/questions/945989/how-do-i-create-my-own-custom-sprites-in-unity.html

            // Reference to create sprite in scene: https://forum.unity.com/threads/instantiating-sprites.224028/
            //                                      https://docs.unity3d.com/Manual/InstantiatingPrefabs.html
            //                                      https://answers.unity.com/questions/313398/is-it-possible-to-get-a-prefab-object-from-its-ass.html

            particleSprite = GameObject.Instantiate((UnityEngine.GameObject)Resources.Load("ParticleSpritePrefab"), currentGlobeParticle.position, Quaternion.identity);
            particleSprite.transform.localScale = Vector3.one * 0.025f;
            particleSpriteList.Add(particleSprite);

        }

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < globeParticles.Count; i++)
        {
 
            // Must convert to position in local space in order to search for nearest triangle which is in local space.
            // Current position of particle in local space.
            currentVector3 = objectMovementScript.combinedInverseTransformMatrix.MultiplyPoint3x4(globeParticles[i].position);
            nearestTriangle = (TriangleNode)snowGlobe.triangleCenterVerticesTree.StartSearch(currentVector3);

            if (globeParticles[i].latestCollidedTriangle == null)
            {
                globeParticles[i].latestCollidedTriangle = nearestTriangle;
            }

            nearestNode = snowGlobe.meshCollisionKDTree.StartSearch(nearestTriangle.position);

            // MUST NORMALIZE BEFORE COMAPRING DIRECTIONS. Reference: https://forum.unity.com/threads/vector3-dot-what-am-i-doing-wrong.973473/
            currentGravityDotTriNorm = Vector3.Dot(Vector3.Normalize(objectMovementScript.combinedInverseTransformMatrix_NoTranslation.MultiplyPoint3x4(gravity)), Vector3.Normalize(nearestTriangle.triangleNormal));


            if (globeParticles[i].speed > speedLimit ||
                (globeParticles[i].speed <= speedLimit &&
                    currentGravityDotTriNorm < maxCosAngleBetwGravityAndTriNorm &&
                    currentGravityDotTriNorm > -maxCosAngleBetwGravityAndTriNorm)) 
            {

                // As long as a force constructed using the opposite of the triangle normal vector value is not opposing the gravity vector, add to the directionVelocity of the particle.
                AddToDirectionVelocity_NonGravityForces(globeParticles[i], 
                    ((Vector3.Magnitude(nearestNode.position - nearestTriangle.position) / Vector3.Magnitude(currentVector3 - nearestTriangle.position)) *
                    triangleNormForceScaleFactor / (Vector3.Magnitude(nearestNode.position - nearestTriangle.position)) *                                                   // So spheres with larger radius would not result in the top ratio (one line above) to be very big if the particle is very close to the triangle center, and the center of the very large radius sphere is very far away from the triangle center.
                    0.5f *                                                                                                                         
                    -1.0f * nearestTriangle.triangleNormal));
            }

            else if(globeParticles[i].speed > 0)
            {
                
                currentTriNormXZOnlyVec = -1.0f * nearestTriangle.triangleNormal;           // Opposite direction of triangle normal.
                currentTriNormXZOnlyVec.y = 0f;
                AddToDirectionVelocity_NonGravityForces(globeParticles[i],
                    ((Vector3.Magnitude(nearestNode.position - nearestTriangle.position) / Vector3.Magnitude(currentVector3 - nearestTriangle.position)) *
                    triangleNormForceScaleFactor / (Vector3.Magnitude(nearestNode.position - nearestTriangle.position)) *                                                                                           // So spheres with larger radius would not result in the top ratio (one line above) to be very big if the particle is very close to the triangle center, and the center of the very large radius sphere is very far away from the triangle center.
                    0.5f *
                    currentTriNormXZOnlyVec));
            }



            currentTriNormXZOnlyVec = -1.0f * globeParticles[i].latestCollidedTriangle.triangleNormal;           // Opposite direction of triangle normal.
            currentTriNormXZOnlyVec.y = gravity.y * gravityScaleFactor;
            currentTriNormXZOnlyVec.x *= Random.Range(0.001f, 2.00f);
            currentTriNormXZOnlyVec.z *= Random.Range(0.001f, 2.00f);
            AddToDirectionVelocity_NonGravityForces(globeParticles[i], currentTriNormXZOnlyVec * speedScaleFactor);
           


            if (globeParticles[i].speed == 0)
            {
                globeParticles[i].directionVelocity = Vector3.zero;
            }

            else if (Vector3.Magnitude(globeParticles[i].directionVelocity) > globeParticles[i].speed * speedScaleFactor)
            {
                // Keep directionVelocity within a magnitude limit called speed
                globeParticles[i].directionVelocity = Vector3.Normalize(globeParticles[i].directionVelocity) * globeParticles[i].speed;
            }

            if (globeParticles[i].speed > collidedSpeedLimit)
            {
                globeParticles[i].speed -= decelerationFactor;
            }
   


            globeParticles[i].position = globeParticles[i].position + (globeParticles[i].directionVelocity * speedScaleFactor); ////////////////////////////////////////////////

            if (globeParticles[i].speed > 0)
            {
                globeParticles[i].position += gravity * gravityScaleFactor;
            }

            CheckParticlesWithinBound(globeParticles[i]);
            particleSpriteList[i].transform.position = globeParticles[i].position;

        }
    }

    private void LateUpdate()
    {
        // Causing sprites to look at camera at all times. Reference: https://www.youtube.com/watch?v=_LRZcmX_xw0
        for(int i = 0; i < particleSpriteList.Count; i++)
        {
            particleSpriteList[i].transform.LookAt(mainCamera.transform);
            particleSpriteList[i].transform.rotation = Quaternion.Euler(0f, particleSpriteList[i].transform.eulerAngles.y, 0f);
        }
    }

    public void CheckAllParticlesWhenMovement()
    {
        for (int i = 0; i < globeParticles.Count; i++)
        {
            // Current position of particle in local space.
            currentVector3 = objectMovementScript.combinedInverseTransformMatrix.MultiplyPoint3x4(globeParticles[i].position);

            // Checking if there is collision by giving the transformed vector (to local space of the globe) and the SnowGlobe meshCollisionKDTree as inputs.
            if (collisionChecker.CheckWallCollision(currentVector3, meshCollisionKDTree))
            {

                currentRandomVecWhenObjMove = globeParticles[i].oldPositionInLocalSpace - currentVector3;
                currentRandomVecWhenObjMove.x *= Random.Range(0.1f, 2.00f);
                currentRandomVecWhenObjMove.y *= Random.Range(0.1f, 2.00f);
                currentRandomVecWhenObjMove.z *= Random.Range(0.1f, 2.00f);


                // NOTE THE ORDER OF SUBTRACTION OF THE VECTORS
                AddToDirectionVelocity_NonGravityForces(globeParticles[i], (currentRandomVecWhenObjMove) * particleMoveFactor_WhenGlobeMove * particleMoveFactor_WhenGlobeMove);

                // Move in the direction opposite to the direction of globe movement
                globeParticles[i].speed += Vector3.Magnitude(globeParticles[i].oldPositionInLocalSpace - currentVector3) * particleMoveFactor_WhenGlobeMove * particleMoveFactor_WhenGlobeMove;

                // Take the latest position of the particle in local space where it did not pass the boundary, and set as the particle's position in the world space
                globeParticles[i].position = objectMovementScript.combinedTransformMatrix.MultiplyPoint3x4(globeParticles[i].oldPositionInLocalSpace);
            }

            else
            {
                // NOTE THE ORDER OF SUBTRACTION OF THE VECTORS

                currentRandomVecWhenObjMove = (currentVector3 - globeParticles[i].oldPositionInLocalSpace);
                currentRandomVecWhenObjMove.x *= Random.Range(0.1f, 2.00f);
                currentRandomVecWhenObjMove.y *= Random.Range(0.1f, 2.00f);
                currentRandomVecWhenObjMove.z *= Random.Range(0.1f, 2.00f);

                AddToDirectionVelocity_NonGravityForces(globeParticles[i], currentRandomVecWhenObjMove * particleMoveFactor_WhenGlobeMove);
                globeParticles[i].speed += Vector3.Magnitude(currentRandomVecWhenObjMove) * particleMoveFactor_WhenGlobeMove;


                // As long as a force constructed using the opposite of the triangle normal vector value is not opposing the gravity vector, add to the directionVelocity of the particle.
                AddToDirectionVelocity_NonGravityForces(globeParticles[i],
                    ((Vector3.Magnitude(nearestNode.position - nearestTriangle.position) / Vector3.Magnitude(currentVector3 - nearestTriangle.position)) *
                    0.35f *
                    -1.0f * nearestTriangle.triangleNormal));



                // Move particle with object but give a force to let it flow in the opposite direction of the object movement.
                globeParticles[i].position = objectMovementScript.combinedTransformMatrix.MultiplyPoint3x4(globeParticles[i].oldPositionInLocalSpace);

            }

            particleSpriteList[i].transform.position = globeParticles[i].position;

        }
    }

 
    void CheckParticlesWithinBound(GlobeParticles particle)
    {
        // Checking if there is collision by giving the transformed vector (to local space of the globe) and the SnowGlobe meshCollisionKDTree as inputs.
        particle.collided = collisionChecker.CheckWallCollision(objectMovementScript.combinedInverseTransformMatrix.MultiplyPoint3x4(particle.position + particle.directionVelocity * awayFromBoundaryScaleFactor), meshCollisionKDTree);
        

        if (particle.collided)
        {

            meshCollisionKDTree.RealTimeAddBoundingSpheres(triangleCenterVerticesTree, particle.position, objectMovementScript.combinedInverseTransformMatrix);

            // Further check if collided.
            particle.collided = collisionChecker.CheckWallCollision(objectMovementScript.combinedInverseTransformMatrix.MultiplyPoint3x4(particle.position + particle.directionVelocity * speedScaleFactor * 10.0f), meshCollisionKDTree);

            nearestTriangle = (TriangleNode)snowGlobe.triangleCenterVerticesTree.StartSearch(particle.position);
            particle.latestCollidedTriangle = nearestTriangle;

            if (particle.collided)
            {

                // Take the latest position of the particle in local space where it did not pass the boundary, and set as the particle's position in the world space.
                particle.position = objectMovementScript.combinedTransformMatrix.MultiplyPoint3x4(particle.oldPositionInLocalSpace); // - nearestTriangle.triangleNormal * awayFromBoundaryScaleFactor or speedScaleFactor); // currentParticlePosVec4
                // AddToDirectionVelocity_NonGravityForces(particle, -1.0f * speedScaleFactor * nearestTriangle.triangleNormal);

                particle.speed /= collideDecelerationFactor;

                if (particle.speed < 0.01f)
                {
                    particle.speed = 0;
                }

            }

            else
            {
                // If particle does not collide or go out of bound, let the position remain, but record down the latest position in local space of the globe where the particle is not out of bound.
                particle.oldPositionInLocalSpace = objectMovementScript.combinedInverseTransformMatrix.MultiplyPoint3x4(particle.position);
            }

        }
        else
        {
            // If particle does not collide or go out of bound, let the position remain, but record down the latest position in local space of the globe where the particle is not out of bound.
            particle.oldPositionInLocalSpace = objectMovementScript.combinedInverseTransformMatrix.MultiplyPoint3x4(particle.position);
        }

    }



    void AddToDirectionVelocity_NonGravityForces(GlobeParticles particle, Vector3 forceVector)
    {
        forceVector = objectMovementScript.combinedTransformMatrix_NoTranslation.MultiplyPoint3x4(forceVector);
        particle.directionVelocity += forceVector;

    }
}
