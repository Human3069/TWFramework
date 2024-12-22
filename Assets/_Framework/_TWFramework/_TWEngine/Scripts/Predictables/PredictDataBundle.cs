using _KMH_Framework;
using _TW_Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PredictDataBundle", menuName = "Scriptable Objects/PredictDataBundle")]
public class PredictDataBundle : ScriptableObject
{
    [SerializeField]
    private List<PredictableData> dataList = new List<PredictableData>();

    public static PredictDataBundle GetPredictData(PoolerType type)
    {
        string resourceName = type.ToString() + "PredictData";
        PredictDataBundle predictData = Resources.Load<PredictDataBundle>(resourceName);

        return predictData;
    }

    private (PredictableData, PredictableData) GetPredictableTwoPointDatas(float distance)
    {
        if (distance > 650 || distance < 0)
        {
            throw new System.NotImplementedException("distance : " + distance);
        }
        else
        {
            PredictableData overData = dataList.Find(data => data.Distance > distance);
            
            int moreThanIndex = dataList.IndexOf(overData);
            PredictableData lessData = dataList[moreThanIndex - 1];

            return (lessData, overData);
        }
    }

    private float GetLinearProjectileSpeed(float distance)
    {
        if (distance > 650 || distance < 0)
        {
            throw new System.NotImplementedException("distance : " + distance);
        }
        else
        {
            (PredictableData lessData, PredictableData overData) = GetPredictableTwoPointDatas(distance);
            PredictableData firstData = dataList[0];

            float distanceNormal = Mathf.InverseLerp(lessData.Distance, overData.Distance, distance);
            float linearProjectileSpeed = (Mathf.Lerp(lessData.SpeedOnHit, overData.SpeedOnHit, distanceNormal) + firstData.SpeedOnHit) * 0.5f;

            return linearProjectileSpeed;
        }
    }

    private float GetPredictedGravityHeightAmount(float distance)
    {
        (PredictableData lessData, PredictableData overData) = GetPredictableTwoPointDatas(distance);

        float distanceNormal = Mathf.InverseLerp(lessData.Distance, overData.Distance, distance);
        float heightAmount = Mathf.Lerp(lessData.AdditionalHeight, overData.AdditionalHeight, distanceNormal);

        return heightAmount;
    }

    // Predicts velocity and gravity
    public Vector3 GetPredictedPosition(Vector3 firePos, UnitHandler targetUnit)
    {
        float distance = (targetUnit.MiddlePos - firePos).magnitude;
        float lerpedSpeed = GetLinearProjectileSpeed(distance);
        Vector3 predictedVelocity = Vector3Ex.GetPredictPosition(firePos, targetUnit.MiddlePos, targetUnit.Velocity, lerpedSpeed);
        Vector3 predictedGravity = new Vector3(0f, GetPredictedGravityHeightAmount(distance), 0f);

        Vector3 predictPos = predictedVelocity + predictedGravity;
        return predictPos;
    }

    // Predicts velocity and gravity
    public Vector3 GetPredictedPosition(Vector3 firePos, Rigidbody targetRigidbody)
    {
        float distance = (targetRigidbody.centerOfMass - firePos).magnitude;
        float lerpedSpeed = GetLinearProjectileSpeed(distance);
        Vector3 predictedVelocity = Vector3Ex.GetPredictPosition(firePos, targetRigidbody.centerOfMass, targetRigidbody.linearVelocity, lerpedSpeed);
        Vector3 predictedGravity = new Vector3(0f, GetPredictedGravityHeightAmount(distance), 0f);

        Vector3 predictPos = predictedVelocity + predictedGravity;
        return predictPos;
    }

    // Only Gravity Prediction
    public Vector3 GetPredictedPosition(Vector3 firePos, Vector3 targetPos)
    {
        float distance = (targetPos - firePos).magnitude;
        Vector3 predictedGravity = new Vector3(0f, GetPredictedGravityHeightAmount(distance), 0f);

        Vector3 predictPos = targetPos + predictedGravity;
        return predictPos;
    }
}
