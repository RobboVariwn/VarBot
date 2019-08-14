using UnityEngine;

namespace Varwin.VRInput
{
    [RequireComponent(typeof(ArcMaterialHolder))]
    public class Arc : MonoBehaviour
    {
        public int SegmentCount = 60;
        public float Thickness = 0.01f;

        [Tooltip("The amount of time in seconds to predict the motion of the projectile.")]
        public float ArcDuration = 3.0f;

        [Tooltip("The amount of time in seconds between each segment of the projectile.")]
        public float SegmentBreak = 0.025f;

        [Tooltip("The speed at which the line segments of the arc move.")]
        public float ArcSpeed = 0.2f;

        public Material MaterialRay;

        public LayerMask TraceLayerMask;

        public Vector3 PlayerTeleportTransformCandidate;

        public ControllerInteraction.ControllerHand ControllerHand;

        //Private data
        private LineRenderer[] lineRenderers;
        private float arcTimeOffset = 0.0f;
        private float prevThickness = 0.0f;
        private int prevSegmentCount = 0;
        private bool showArc = true;
        private Vector3 startPos;
        private Vector3 projectileVelocity;
        private bool useGravity = true;
        private Transform arcObjectsTransfrom;
        private bool arcInvalid = false;
        private float scale = 1.5f;
        private static readonly int ColorShader = Shader.PropertyToID("_Color");
        private Color _defaultColor = Color.cyan;

        private Color _validColor = new Color(0 / 255f,
            255 / 255f,
            86 / 255f,
            0.45f);

        private Color _invalidColor = new Color(255 / 255f,
            0 / 255f,
            92 / 255f,
            0.45f);

        //-------------------------------------------------
        void Start()
        {
            arcTimeOffset = Time.time;
            MaterialRay = new Material(GetComponent<ArcMaterialHolder>().arc);
            MaterialRay.SetColor(ColorShader, _defaultColor);
            MaterialRay.renderQueue = 3001;
        }


        //-------------------------------------------------
        void Update()
        {
            //scale arc to match player scale
            //scale = transform.lossyScale.x;
            if (Thickness != prevThickness || SegmentCount != prevSegmentCount)
            {
                CreateLineRendererObjects();
                prevThickness = Thickness;
                prevSegmentCount = SegmentCount;
            }
        }


        //-------------------------------------------------
        private void CreateLineRendererObjects()
        {
            //Destroy any existing line renderer objects
            if (arcObjectsTransfrom != null)
            {
                Destroy(arcObjectsTransfrom.gameObject);
            }

            GameObject arcObjectsParent = new GameObject("ArcObjects");
            arcObjectsTransfrom = arcObjectsParent.transform;
            arcObjectsTransfrom.SetParent(this.transform);

            //Create new line renderer objects
            lineRenderers = new LineRenderer[SegmentCount];

            for (int i = 0; i < SegmentCount; ++i)
            {
                GameObject newObject = new GameObject("LineRenderer_" + i);
                newObject.transform.SetParent(arcObjectsTransfrom);

                lineRenderers[i] = newObject.AddComponent<LineRenderer>();

                lineRenderers[i].receiveShadows = false;
                lineRenderers[i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                lineRenderers[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                lineRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderers[i].material = MaterialRay;
#if (UNITY_5_4)
                lineRenderers[i].SetWidth(thickness, thickness);
#else
                lineRenderers[i].startWidth = Thickness * scale;
                lineRenderers[i].endWidth = Thickness * scale;
#endif
                lineRenderers[i].enabled = false;
            }
        }


        //-------------------------------------------------
        public void SetArcData(
            Vector3 position,
            Vector3 velocity,
            bool gravity,
            bool pointerAtBadAngle)
        {
            startPos = position;
            projectileVelocity = velocity;
            useGravity = gravity;

            if (arcInvalid && !pointerAtBadAngle)
            {
                arcTimeOffset = Time.time;
            }

            arcInvalid = pointerAtBadAngle;
        }


        //-------------------------------------------------
        public void Show()
        {
            showArc = true;

            if (lineRenderers == null)
            {
                CreateLineRendererObjects();
            }
        }


        //-------------------------------------------------
        public void Hide()
        {
            //Hide the line segments if they were previously being shown
            if (showArc)
            {
                HideLineSegments(0, SegmentCount);
            }

            showArc = false;
        }


        //-------------------------------------------------
        // Draws each segment of the arc individually
        //-------------------------------------------------
        public bool DrawArc(out RaycastHit hitInfo)
        {
            float timeStep = ArcDuration / SegmentCount;

            float currentTimeOffset = (Time.time - arcTimeOffset) * ArcSpeed;

            //Reset the arc time offset when it has gone beyond a segment length
            if (currentTimeOffset > (timeStep + SegmentBreak))
            {
                arcTimeOffset = Time.time;
                currentTimeOffset = 0.0f;
            }

            float segmentStartTime = currentTimeOffset;

            float arcHitTime = FindProjectileCollision(out hitInfo);

            if (arcInvalid)
            {
                //Only draw first segment
                lineRenderers[0].enabled = true;
                lineRenderers[0].SetPosition(0, GetArcPositionAtTime(0.0f));
                lineRenderers[0].SetPosition(1, GetArcPositionAtTime(arcHitTime < timeStep ? arcHitTime : timeStep));

                HideLineSegments(1, SegmentCount);
            }
            else
            {
                //Draw the first segment outside the loop if needed
                int loopStartSegment = 0;

                if (segmentStartTime > SegmentBreak)
                {
                    float firstSegmentEndTime = currentTimeOffset - SegmentBreak;

                    if (arcHitTime < firstSegmentEndTime)
                    {
                        firstSegmentEndTime = arcHitTime;
                    }

                    DrawArcSegment(0, 0.0f, firstSegmentEndTime);

                    loopStartSegment = 1;
                }

                bool stopArc = false;
                int currentSegment = 0;

                if (segmentStartTime < arcHitTime)
                {
                    for (currentSegment = loopStartSegment; currentSegment < SegmentCount; ++currentSegment)
                    {
                        //Clamp the segment end time to the arc duration
                        float segmentEndTime = segmentStartTime + timeStep;

                        if (segmentEndTime >= ArcDuration)
                        {
                            segmentEndTime = ArcDuration;
                            stopArc = true;
                        }

                        if (segmentEndTime >= arcHitTime)
                        {
                            segmentEndTime = arcHitTime;
                            stopArc = true;
                        }

                        DrawArcSegment(currentSegment, segmentStartTime, segmentEndTime);

                        segmentStartTime += timeStep + SegmentBreak;

                        //If the previous end time or the next start time is beyond the duration then stop the arc
                        if (stopArc || segmentStartTime >= ArcDuration || segmentStartTime >= arcHitTime)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    currentSegment--;
                }

                //Hide the rest of the line segments
                HideLineSegments(currentSegment + 1, SegmentCount);
            }

            return arcHitTime != float.MaxValue;
        }


        //-------------------------------------------------
        private void DrawArcSegment(int index, float startTime, float endTime)
        {
            lineRenderers[index].enabled = true;
            lineRenderers[index].SetPosition(0, GetArcPositionAtTime(startTime));
            lineRenderers[index].SetPosition(1, GetArcPositionAtTime(endTime));
        }


        //-------------------------------------------------
        public void SetColor(Color color)
        {
            for (int i = 0; i < SegmentCount; ++i)
            {
#if (UNITY_5_4)
                lineRenderers[i].SetColors(color, color);
#else
                lineRenderers[i].startColor = color;
                lineRenderers[i].endColor = color;
#endif
            }
        }


        private const int MaxHits = 500; //It'll fail sometime but eh not my fault

        RaycastHit[] _hits = new RaycastHit[MaxHits];

        //-------------------------------------------------
        private float FindProjectileCollision(out RaycastHit hitInfo)
        {
            float timeStep = ArcDuration / SegmentCount;
            float segmentStartTime = 0.0f;

            hitInfo = new RaycastHit();

            Vector3 segmentStartPos = GetArcPositionAtTime(segmentStartTime);

            ControllerInteraction.ControllerSelf controller =
                InputAdapter.Instance.PlayerController.Nodes.GetControllerReference(ControllerHand).Controller;

            for (int i = 0; i < SegmentCount; ++i)
            {
                float segmentEndTime = segmentStartTime + timeStep;
                Vector3 segmentEndPos = GetArcPositionAtTime(segmentEndTime);

                var size = Physics.RaycastNonAlloc(segmentStartPos,
                    segmentEndPos - segmentStartPos,
                    _hits,
                    (segmentEndPos - segmentStartPos).magnitude,
                    TraceLayerMask);

                if (size > 0)
                {
                    for (int j = 0; j < size; j++)
                    {
                        hitInfo = _hits[j];

                        if (controller.CheckIfColliderPresent(hitInfo.collider))
                        {
                            continue;
                        }

                        if (!hitInfo.transform.CompareTag("TeleportArea") || arcInvalid)
                        {
                            MaterialRay.SetColor(ColorShader, _invalidColor);
                            DrawCross(hitInfo.point, _invalidColor, 0.5f);
                            PlayerTeleportTransformCandidate = Vector3.zero;
                        }
                        else
                        {
                            MaterialRay.SetColor(ColorShader, _validColor);
                            PlayerTeleportTransformCandidate = hitInfo.point;
                        }

                        float segmentDistance = Vector3.Distance(segmentStartPos, segmentEndPos);
                        float hitTime = segmentStartTime + (timeStep * (hitInfo.distance / segmentDistance));

                        return hitTime;
                    }
                }

                segmentStartTime = segmentEndTime;
                segmentStartPos = segmentEndPos;
            }

            return float.MaxValue;
        }

        private static void DrawCross(Vector3 origin, Color crossColor, float size)
        {
            Vector3 line1Start = origin + (Vector3.right * size);
            Vector3 line1End = origin - (Vector3.right * size);

            Debug.DrawLine(line1Start, line1End, crossColor);

            Vector3 line2Start = origin + (Vector3.up * size);
            Vector3 line2End = origin - (Vector3.up * size);

            Debug.DrawLine(line2Start, line2End, crossColor);

            Vector3 line3Start = origin + (Vector3.forward * size);
            Vector3 line3End = origin - (Vector3.forward * size);

            Debug.DrawLine(line3Start, line3End, crossColor);
        }


        //-------------------------------------------------
        public Vector3 GetArcPositionAtTime(float time)
        {
            Vector3 gravity = useGravity ? Physics.gravity : Vector3.zero;

            Vector3 arcPos = startPos + ((projectileVelocity * time) + (0.5f * time * time) * gravity) * scale;

            return arcPos;
        }


        //-------------------------------------------------
        private void HideLineSegments(int startSegment, int endSegment)
        {
            if (lineRenderers != null)
            {
                for (int i = startSegment; i < endSegment; ++i)
                {
                    lineRenderers[i].enabled = false;
                }
            }
        }

        public bool IsArcValid() => !arcInvalid;
    }
}
