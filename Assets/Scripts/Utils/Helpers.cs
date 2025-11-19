using UnityEngine;

namespace ArrowPath.Utils
{
    public static class Helpers
    {
        /// <summary>
        /// Maps a value from one range to another
        /// </summary>
        /// <param name="value">the value to map</param>
        /// <param name="minVal">the minimum of the original range</param>
        /// <param name="maxVal">the maximum of the original range</param>
        /// <param name="minMap">the minimum of the target range</param>
        /// <param name="maxMap">the maximum of the target range</param>
        /// <returns></returns>
        public static float Map(float value, float minVal, float maxVal, float minMap, float maxMap)
        {
            if (value < minVal) value = minVal;
            if (value > maxVal) value = maxVal;

            value = (value - minVal) / (maxVal - minVal) * (maxMap - minMap) + minMap;
            return value;
        }
        
        /// <summary>
        /// Returns the distance of a point - line.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float Distance(Vector2 point, Vector2 p1, Vector2 p2)
        {
            var _v = p2 - p1; // Çubuk vektörü (P1 -> P2)
            var _w = point - p1;         // P1'den fareye vektör (P1 -> P0)

            var _c1 = Vector2.Dot(_w, _v); // w'nin v üzerindeki izdüşümünün skaler çarpanı
            var _c2 = Vector2.Dot(_v, _v); // v'nin karesel uzunluğu
            
            // İzdüşüm parametresini hesapla
            var _b = _c1 / _c2;

            Vector2 _closestPointOnSegment;

            if (_b < 0) // En yakın nokta stick.pointA'dır (segmentin başlangıcı)
            {
                _closestPointOnSegment = p1;
            }
            else if (_b > 1) // En yakın nokta stick.pointB'dir (segmentin sonu)
            {
                _closestPointOnSegment = p2;
            }
            else // En yakın nokta segmentin üzerindedir (pointA ve pointB arasında)
            {
                _closestPointOnSegment = p1 + _v * _b;
            }

            // Fare pozisyonundan segment üzerindeki en yakın noktaya olan mesafeyi hesapla
            var _distance = Vector2.Distance(point, _closestPointOnSegment);

            return _distance;
        }
    }
}