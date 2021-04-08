using UnityEngine;

namespace Assets.Scripts.Utility {
    public static class CameraUtility {
        private static readonly Vector3[] CubeCornerOffsets =
        {
            new Vector3(1, 1, 1),
            new Vector3(-1, 1, 1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, -1, -1),
            new Vector3(-1, 1, -1),
            new Vector3(1, -1, -1),
            new Vector3(1, 1, -1),
            new Vector3(1, -1, 1),
        };

        // http://wiki.unity3d.com/index.php/IsVisibleFrom
        public static bool VisibleFromCamera(Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }

        public static bool BoundsOverlap(MeshFilter nearObject, MeshFilter farObject, Camera camera)
        {
            var near = GetScreenRectFromBounds(nearObject, camera);
            var far = GetScreenRectFromBounds(farObject, camera);

            // ensure far object is indeed further away than near object
            if (far.ZMax > near.ZMin) {
                // Doesn't overlap on x axis
                if (far.XMax < near.XMin || far.XMin > near.XMax) {
                    return false;
                }
                // Doesn't overlap on y axis
                if (far.YMax < near.YMin || far.YMin > near.YMax) {
                    return false;
                }
                // Overlaps
                return true;
            }
            return false;
        }

        // With thanks to http://www.turiyaware.com/a-solution-to-unitys-camera-worldtoscreenpoint-causing-ui-elements-to-display-when-object-is-behind-the-camera/
        public static MinMax3D GetScreenRectFromBounds(MeshFilter renderer, Camera mainCamera)
        {
            MinMax3D minMax = new MinMax3D(float.MaxValue, float.MinValue);

            var localBounds = renderer.sharedMesh.bounds;
            bool anyPointIsInFrontOfCamera = false;

            for (int i = 0; i < 8; i++) {
                Vector3 localSpaceCorner = localBounds.center + Vector3.Scale(localBounds.extents, CubeCornerOffsets[i]);
                Vector3 worldSpaceCorner = renderer.transform.TransformPoint(localSpaceCorner);
                Vector3 viewportSpaceCorner = mainCamera.WorldToViewportPoint(worldSpaceCorner);

                if (viewportSpaceCorner.z > 0) {
                    anyPointIsInFrontOfCamera = true;
                } else {
                    // If point is behind camera, it gets flipped to the opposite side
                    // So clamp to opposite edge to correct for this
                    viewportSpaceCorner.x = (viewportSpaceCorner.x <= 0.5f) ? 1 : 0;
                    viewportSpaceCorner.y = (viewportSpaceCorner.y <= 0.5f) ? 1 : 0;
                }

                // Update bounds with new corner point
                minMax.AddPoint(viewportSpaceCorner);
            }

            // All points are behind camera so just return empty bounds
            if (!anyPointIsInFrontOfCamera) {
                return new MinMax3D();
            }

            return minMax;
        }

        public struct MinMax3D {
            public float XMin;
            public float XMax;
            public float YMin;
            public float YMax;
            public float ZMin;
            public float ZMax;

            public MinMax3D(float min, float max)
            {
                this.XMin = min;
                this.XMax = max;
                this.YMin = min;
                this.YMax = max;
                this.ZMin = min;
                this.ZMax = max;
            }

            public void AddPoint(Vector3 point)
            {
                XMin = Mathf.Min(XMin, point.x);
                XMax = Mathf.Max(XMax, point.x);
                YMin = Mathf.Min(YMin, point.y);
                YMax = Mathf.Max(YMax, point.y);
                ZMin = Mathf.Min(ZMin, point.z);
                ZMax = Mathf.Max(ZMax, point.z);
            }
        }
    }
}