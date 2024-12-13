using UnityEngine;

namespace _TW_Framework
{
    public struct Pyramid
    {
        private const float SAFE_THRESHOLD = 0.01f;

        private Camera _camera;
        private float _maxDistance;

        private Vector2 _screenStartPoint;
        private Vector2 _screenEndPoint;
        private (Vector2, Vector2) _screenInterpolatedPoints;

        public bool IsValid()
        {
            return _camera != null && _screenStartPoint != Vector2.zero && _screenEndPoint != Vector2.zero;
        }

        public Pyramid(Camera camera, float maxDistance, Vector2 screenStartPoint, Vector2 screenEndPoint)
        {
            _camera = camera;
            _maxDistance = maxDistance;

            _screenStartPoint = screenStartPoint;
            _screenEndPoint = screenEndPoint;
            _screenInterpolatedPoints = (new Vector2(screenStartPoint.x, screenEndPoint.y), new Vector2(screenEndPoint.x, screenStartPoint.y));
        }

        public Mesh GenerateMesh()
        {
            if (((_screenInterpolatedPoints.Item1 - _screenStartPoint).magnitude <= SAFE_THRESHOLD) ||
                ((_screenInterpolatedPoints.Item2 - _screenStartPoint).magnitude <= SAFE_THRESHOLD))
            {
                return null;
            }

            Vector3 originPos = _camera.transform.position;

            Vector3 startPos = _camera.ScreenToWorldPoint(new Vector3(_screenStartPoint.x, _screenStartPoint.y, _maxDistance));
            Vector3 endPos = _camera.ScreenToWorldPoint(new Vector3(_screenEndPoint.x, _screenEndPoint.y, _maxDistance));
            Vector3 interpolatedPos0 = _camera.ScreenToWorldPoint(new Vector3(_screenInterpolatedPoints.Item1.x, _screenInterpolatedPoints.Item1.y, _maxDistance));
            Vector3 interpolatedPos1 = _camera.ScreenToWorldPoint(new Vector3(_screenInterpolatedPoints.Item2.x, _screenInterpolatedPoints.Item2.y, _maxDistance));

            Vector3[] vertices = new Vector3[]
            {
                originPos,

                startPos,
                interpolatedPos0,
                endPos,
                interpolatedPos1
            };

            // 0 1 2 - 

            int[] triangles;
            if (startPos.x - endPos.x > 0f && startPos.y - endPos.y > 0f ||
                startPos.x - endPos.x < 0f && startPos.y - endPos.y < 0f)
            {
                triangles = new int[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 1,

                    1, 3, 2,
                    3, 1, 4,
                };
            }
            else
            {
                triangles = new int[]
                {
                    0, 2, 1,
                    0, 3, 2,
                    0, 4, 3,
                    0, 1, 4,

                    1, 2, 3,
                    3, 4, 1,
                };
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            return mesh;
        }

        public bool TryGenerateMesh(out Mesh mesh)
        {
            mesh = GenerateMesh();
            return mesh != null;
        }
    }
}