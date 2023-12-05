using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class code : MonoBehaviour
{
    List<Vector3> sourcePoints = new List<Vector3>
    {
        new Vector3(0f, 1f, 2f),
        new Vector3(0f, 2f, 5f),
        new Vector3(3f, 1f, 2f),
        new Vector3(1f, 2f, 1f),
        new Vector3(0f, 1f, 1f)
    };

    List<Vector3> targetPoints = new List<Vector3>
    {
        new Vector3(10.98f, 5.09f, 3.57f),
        new Vector3(13.79f, 3.69f, 3.97f),
        new Vector3(11.85f, 6.07f, 0.87f),
        new Vector3(11.16f, 6.75f, 3.12f),
        new Vector3(10.25f, 5.77f, 3.59f)
    };


    // create a list
    List<GameObject> transformedPointsGameObject = new List<GameObject>();
    List<GameObject> sourcePointsGameObject = new List<GameObject>();
    List<GameObject> targetPointsGameObject = new List<GameObject>();

    void Start()
    {
        CreateGameObjects(sourcePoints, 1);
        CreateGameObjects(targetPoints, 2);
        rigidTransformation(sourcePoints, targetPoints);
    }

    private void Update()
    {
        // move transformedPointsGameObject automatically from sourcePoints to targetPoints slowly
    }

    void rigidTransformation(List<Vector3> sourcePoints, List<Vector3> targetPoints)
    {

        // we will use ransac here
        // we will pick random 3 points from source and target
        //  calculate the transformation matrix
        //  apply the transformation matrix to the source points
        //  calculate the error between the transformed points and the target points
        //  calculate the mean error
        // then repeat this process until we find the best transformation matrix
        // we assume that at least %50 of the points are inliers
        // if the mean error is less than the previous mean error, we will update the best transformation matrix

        var combinations = GetCombinations(sourcePoints, 3);
        var combinations2 = GetCombinations(targetPoints, 3);

        // create gameobjectList for transformed points
        float bestMeanError = 1000;

        foreach (var point in sourcePoints)
        {
            // create a sphere for each point
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.transform.position = point;
            gameObject.GetComponent<Renderer>().material.color = Color.gray;

            transformedPointsGameObject.Add(gameObject);
        }   

        // Print the combinations
        foreach (var sourcePointsComb in combinations)
        {
            foreach (var targetPointsComb in combinations2)
            {
                // print the combinations
                //print(string.Join(", ", sourcePointsComb));
                //print(string.Join(", ", targetPointsComb));

                //use these combination, add to chosenPointsSource and chosenPointsTarget

                //create vector3 list size of 3	
                List<Vector3> chosenPointsSource = new List<Vector3>();
                List<Vector3> chosenPointsTarget = new List<Vector3>();

                chosenPointsSource.Add(sourcePointsComb.ElementAt(0));
                chosenPointsSource.Add(sourcePointsComb.ElementAt(1));
                chosenPointsSource.Add(sourcePointsComb.ElementAt(2));

                chosenPointsTarget.Add(targetPointsComb.ElementAt(0));
                chosenPointsTarget.Add(targetPointsComb.ElementAt(1));
                chosenPointsTarget.Add(targetPointsComb.ElementAt(2));

                //print the chosen points
                //print("chosenPointsSource: ");
                //print(string.Join(", ", chosenPointsSource));
                //print("chosenPointsTarget: ");
                //print(string.Join(", ", chosenPointsTarget));


                // calculate transformation matrix between the two matrices
                Matrix4x4 transformationMatrix = calculateTransformationMatrix(chosenPointsSource, chosenPointsTarget);

                List<Vector3> transformedPoints = new List<Vector3>();

                float error = 0;

                // apply the transformation matrix to the first matrix and calculate the error
                for (int i = 0; i < sourcePoints.Count; i++)
                {
                    // create a vector3 from the matrix
                    Vector3 transformedPoint = transformationMatrix.MultiplyPoint(sourcePoints[i]);
                    transformedPoints.Add(transformedPoint);

                    error += Vector3.Distance(transformedPoints[i], targetPoints[i]);
                }
                // calculate the mean error
                float meanError = error / transformedPoints.Count;

                // if the mean error is less than the previous mean error, we will update the best transformation matrix
                if (meanError < bestMeanError)
                {
                    bestMeanError = meanError;
                    foreach (var transformedPoint in transformedPoints)
                    {
                        // update the position of the point
                        transformedPointsGameObject[transformedPoints.IndexOf(transformedPoint)].transform.position = transformedPoint;
                    }
                }

                if (bestMeanError < 0.1)
                {
                    return;
                }
            }
        }

        print("best mean error is: " + bestMeanError);
    }

    /*
     * calculate the transformation matrix between the two matrices
     */
    private Matrix4x4 calculateTransformationMatrix(List<Vector3> chosenPointsSource, List<Vector3> chosenPointsTarget)
    {
        int numPoints = chosenPointsSource.Count;

        Vector3 centroidSource = Vector3.zero;
        Vector3 centroidTarget = Vector3.zero;

        for (int i = 0; i < numPoints; i++)
        {
            centroidSource += chosenPointsSource[i];
            centroidTarget += chosenPointsTarget[i];
        }

        centroidSource /= numPoints;
        centroidTarget /= numPoints;

        List<Vector3> centeredPointsSource = new List<Vector3>();
        List<Vector3> centeredPointsTarget = new List<Vector3>();

        for (int i = 0; i < numPoints; i++)
        {
            centeredPointsSource.Add(chosenPointsSource[i] - centroidSource);
            centeredPointsTarget.Add(chosenPointsTarget[i] - centroidTarget);
        }

        Matrix4x4 H = Matrix4x4.zero;

        for (int i = 0; i < numPoints; i++)
        {
            H.m00 += centeredPointsSource[i].x * centeredPointsTarget[i].x;
            H.m01 += centeredPointsSource[i].x * centeredPointsTarget[i].y;
            H.m02 += centeredPointsSource[i].x * centeredPointsTarget[i].z;

            H.m10 += centeredPointsSource[i].y * centeredPointsTarget[i].x;
            H.m11 += centeredPointsSource[i].y * centeredPointsTarget[i].y;
            H.m12 += centeredPointsSource[i].y * centeredPointsTarget[i].z;

            H.m20 += centeredPointsSource[i].z * centeredPointsTarget[i].x;
            H.m21 += centeredPointsSource[i].z * centeredPointsTarget[i].y;
            H.m22 += centeredPointsSource[i].z * centeredPointsTarget[i].z;
        }

        var R = H;

        // reflection case if the determinant is smaller than 0
        if (Matrix4x4.Determinant(R) < 0)
        {
            Vector4 reflectionRow = R.GetRow(2);
            reflectionRow *= -1;
            R.SetRow(2, reflectionRow);
        }

        var t = -R.MultiplyPoint3x4(centroidSource) + centroidTarget;

        Matrix4x4 transformationMatrix = Matrix4x4.TRS(t, Quaternion.identity, Vector3.one);
        return transformationMatrix;
    }

    private static Matrix4x4 OuterProduct(Vector3 a, Vector3 b)
    {
        return new Matrix4x4(
            new Vector4(a.x * b.x, a.x * b.y, a.x * b.z, 0),
            new Vector4(a.y * b.x, a.y * b.y, a.y * b.z, 0),
            new Vector4(a.z * b.x, a.z * b.y, a.z * b.z, 0),
            new Vector4(0, 0, 0, 0)
        );
    }


    void CreateGameObjects(List<Vector3> points, int type)
    {
        for (int i = 0; i < points.Count; i++)
        {
            // create a sphere for each point
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //print(points[i, 0] + " " + points[i, 1] + " " + points[i, 2]);
            //set the position of the point
            gameObject.transform.position = points[i];
            // set the scale of the point
            //gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            // set the color of the point
            if (type == 1)
            {
                // source points are yellow
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                sourcePointsGameObject.Add(gameObject);
            }
            else if (type == 2)
            {
                // target points are blue
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
                targetPointsGameObject.Add(gameObject);
            }

        }
    }

    static IEnumerable<IEnumerable<T>> GetCombinations<T>(List<T> source, int size)
    {
        if (size == 1)
        {
            return source.Select(item => new List<T> { item });
        }

        return GetCombinations(source, size - 1)
            .SelectMany(item => source.Where(element => !item.Contains(element)),
                        (item, element) => item.Concat(new[] { element }));
    }

}
