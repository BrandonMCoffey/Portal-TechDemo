using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utility {
    public static class ConeCast {
        public static RaycastHit[] ConeCastAll(Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle)
        {
            RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, maxDistance);
            List<RaycastHit> coneCastHitList = new List<RaycastHit>();

            if (sphereCastHits.Length > 0) {
                foreach (var hit in sphereCastHits) {
                    Vector3 directionToHit = hit.point - origin;
                    float angleToHit = Vector3.Angle(direction, directionToHit);

                    if (angleToHit < coneAngle) {
                        coneCastHitList.Add(hit);
                    }
                }
            }

            return coneCastHitList.ToArray();
        }
    }
}