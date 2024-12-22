using _KMH_Framework;
using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    [ExecuteAlways]
    public class PredictableShooter : MonoBehaviour
    {
        [SerializeField]
        protected PoolerType type;
        [SerializeField]
        protected Transform shootPoint;

        [Space(10)]
        [SerializeField]
        protected float maxDistance = 1000;

        [Space(10)]
        [SerializeField]
        protected List<PredictableData> predictableDataList = new List<PredictableData>();

        protected Vector3 straightPredictPoint;

        protected bool isFiring = false;
        protected float timer = 0f;

        protected void Awake()
        {
            if (Application.isPlaying == true)
            {
                gizmoPosList.Clear();
            }
        }

        protected void Update()
        {
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, maxDistance) == true)
            {
                straightPredictPoint = hit.point;
                Debug.DrawLine(shootPoint.position, hit.point, Color.red);
            }
            else
            {
                straightPredictPoint = shootPoint.position + shootPoint.forward * maxDistance;
                Debug.DrawLine(shootPoint.position, straightPredictPoint, Color.green);
            }
            

            if (Application.isPlaying == true)
            {
                if (Input.GetMouseButtonDown(0) == true && isFiring == false)
                {
                    isFiring = true;
                    Shoot();
                }

                if (isFiring == true)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer = 0f;
                }
            }
        }

        protected void Shoot()
        {
            BulletHandler shootedBullet = type.EnablePool<BulletHandler>(OnBeforeShoot);
        }

        protected void OnBeforeShoot(BulletHandler shootedBullet)
        {
            shootedBullet.OnHitAction = OnShootedBulletHit;

            shootedBullet.transform.position = shootPoint.position;
            shootedBullet.transform.rotation = shootPoint.rotation;
        }

        protected List<Vector3> gizmoPosList = new List<Vector3>();

        protected void OnShootedBulletHit(BulletHandler bullet, RaycastHit hit)
        {
            gizmoPosList.Add(hit.point);

            float distance = (straightPredictPoint - shootPoint.position).magnitude;
            float additionalHeight = Mathf.Abs((hit.point - straightPredictPoint).magnitude);
            float speedOnHit = bullet.GetComponent<Rigidbody>().linearVelocity.magnitude;

            float floatTime = timer;

            Debug.Log("additionalHeight : " + additionalHeight + ", spdOnHit : " + speedOnHit + ", distance : " + distance + ", floatTime : " + floatTime);
            Debug.DrawLine(straightPredictPoint, hit.point, Color.blue, 10f);

            PredictableData predictableData = new PredictableData(distance, floatTime, speedOnHit, additionalHeight);
            predictableDataList.Add(predictableData);

            isFiring = false;
        }

        protected void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            foreach (Vector3 gizmoPos in gizmoPosList)
            {
                Gizmos.DrawSphere(gizmoPos, 0.1f);
            }
        }
    }
}