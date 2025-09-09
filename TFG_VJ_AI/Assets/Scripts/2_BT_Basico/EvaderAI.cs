using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace BT_Basico
{
    public class EvaderAI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform threat;
        [SerializeField] private Material MaterialObjetivo;
        [SerializeField] private Material MaterialDefault;

        [Header("Rangos")]
        [SerializeField] private float detectionRadius = 12f;
        [SerializeField] private float resumeRadius = 16f;
        [SerializeField] private float waypointTol = 0.3f;

        [Header("Velocidades")]
        [SerializeField] private float patrolSpeed = 3.5f;
        [SerializeField] private float evadeSpeed = 6.5f;

        [Header("Evade (muestrado direcciones)")]
        [SerializeField] private float fleeDistance = 8f;
        [SerializeField] private float repathInterval = 0.25f;
        [SerializeField] private int samples = 16;
        [SerializeField] private float maxSpreadAngle = 120f;

        [Header("Runway (huida hacia adelante)")]
        [SerializeField] private float runwayMax = 25f;
        [SerializeField] private float runwayStep = 2f;
        [SerializeField] private float minRunwayAccept = 13f;

        [Header("Stickiness / Anti-U-turn")]
        [SerializeField] private float minStickTime = 0.9f;
        [SerializeField] private float minStickAdvance = 1.5f;
        [SerializeField] private float minScoreImprovement = 3f;
        [SerializeField] private float maxTurnEarlyDeg = 110f;

        [Header("Waypoint Seguro - Anti Ping-Pong")]
        [SerializeField] private float safeWpSwitchCooldown = 1.5f;
        [SerializeField] private float safeWpMinImprovement = 5f;
        private float lastSafeWpSwitchTime = -999f;

        private NavMeshAgent agent;
        private int wpIndex = 0;
        private int ultimoResaltado = -1;

        private enum Mode { Navigate, Evade }
        private Mode mode = Mode.Navigate;

        private float repathTimer = 0f;
        private float stuckTimer = 0f;

        private Vector3 currentEvadeTarget;
        private bool hasEvadeTarget = false;
        private float lastTargetChangeTime = -999f;
        private float lastRemainingToTarget = Mathf.Infinity;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.autoBraking = false;
            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 10;
        }

        void Start()
        {
            agent.speed = patrolSpeed;
            if (waypoints != null && waypoints.Length > 0)
            {
                agent.SetDestination(waypoints[wpIndex].position);
                ResaltarSolo(wpIndex);
            }
        }

        void Update()
        {
            float d = threat ? Vector3.Distance(transform.position, threat.position) : Mathf.Infinity;

            if (mode == Mode.Navigate)
            {
                if (d < detectionRadius) SwitchToEvade();
                else Navigate();
            }
            else
            {
                if (d > resumeRadius) SwitchToNavigate();
                else Evade();
            }

            if (agent.velocity.sqrMagnitude < 0.05f) stuckTimer += Time.deltaTime;
            else stuckTimer = 0f;

            if (stuckTimer > 1.25f)
            {
                repathTimer = 0f;
                fleeDistance *= 1.15f;
                maxSpreadAngle = Mathf.Min(170f, maxSpreadAngle + 10f);
                stuckTimer = 0f;
            }
        }

        private void Navigate()
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointTol && waypoints.Length > 0)
            {
                wpIndex = (wpIndex + 1) % waypoints.Length;
                ResaltarSolo(wpIndex);
                agent.SetDestination(waypoints[wpIndex].position);
            }
        }

        private void Evade()
        {
            repathTimer -= Time.deltaTime;

            if (hasEvadeTarget)
            {
                float advanced = lastRemainingToTarget - agent.remainingDistance;
                bool stickAlive = (Time.time - lastTargetChangeTime) < minStickTime && advanced < minStickAdvance;
                if (stickAlive) return;
            }

            if (repathTimer > 0f) return;

            if (TryGetBestFleePoint(out var best, out float bestScore, out Vector3 bestDir))
            {
                if (!hasEvadeTarget)
                {
                    Debug.Log($"<color=cyan>[EvaderAI]</color> Primer destino de huida elegido. Score: {bestScore:F2}");
                    CommitNewTarget(best);
                }
                else
                {
                    float curScore = ScoreOf(currentEvadeTarget);
                    float gain = bestScore - curScore;

                    Vector3 curDir = (currentEvadeTarget - transform.position); curDir.y = 0f;
                    float turnDeg = Vector3.Angle(curDir, bestDir);

                    if ((turnDeg <= maxTurnEarlyDeg && gain >= minScoreImprovement) ||
                        (turnDeg > maxTurnEarlyDeg && gain >= minScoreImprovement * 2f))
                    {
                        Debug.Log($"<color=magenta>[EvaderAI]</color> Cambio de destino: mejora {gain:F2} | Giro: {turnDeg:F1}° | Nuevo Score: {bestScore:F2}");
                        CommitNewTarget(best);
                    }
                }
            }
            else if (TryGetSafestWaypoint(out var wpPos))
            {
                bool same = agent.hasPath && Vector3.Distance(agent.destination, wpPos) <= 0.5f;
                float curScore = ScoreOf(currentEvadeTarget);
                float candScore = ScoreOf(wpPos);
                float gain = candScore - curScore;
                bool cooldownOk = (Time.time - lastSafeWpSwitchTime) >= safeWpSwitchCooldown;

                if (!same && (!hasEvadeTarget || (cooldownOk && gain >= safeWpMinImprovement)))
                {
                    Debug.Log($"<color=blue>[EvaderAI]</color> Waypoint seguro elegido: {wpPos} | Mejora: {gain:F2}");
                    CommitNewTarget(wpPos);
                    lastSafeWpSwitchTime = Time.time;
                }
                else
                {
                    Debug.Log($"<color=yellow>[EvaderAI]</color> Manteniendo waypoint seguro. Gain: {gain:F2}, Cooldown: {(Time.time - lastSafeWpSwitchTime):F2}s");
                }
            }

            repathTimer = repathInterval;
        }

        private void CommitNewTarget(Vector3 p)
        {
            currentEvadeTarget = p;
            hasEvadeTarget = true;
            lastTargetChangeTime = Time.time;
            agent.SetDestination(currentEvadeTarget);
            lastRemainingToTarget = agent.remainingDistance;

            float distToThreat = threat ? Vector3.Distance(p, threat.position) : -1f;
            float runway = EstimateRunway(p, (p - transform.position), runwayMax, runwayStep);

            Debug.Log($"<color=green>[EvaderAI]</color> Nuevo destino: {p} | Dist. a amenaza: {distToThreat:F1}m | Runway: {runway:F1}m");
        }

        private bool TryGetBestFleePoint(out Vector3 best, out float bestScore, out Vector3 bestDir)
        {
            best = transform.position;
            bestDir = transform.forward;
            bestScore = float.NegativeInfinity;
            if (!threat) return false;

            Vector3 away = (transform.position - threat.position);
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f) away = transform.forward;
            away.Normalize();

            float currentDist = Vector3.Distance(transform.position, threat.position);
            const float MIN_AWAY_DOT = 0.35f;
            const float MIN_DIST_GAIN = 1.0f;
            const float TURN_BLOCK_DOT = -0.3f;

            Vector3 moveDir = agent.velocity.sqrMagnitude > 0.05f ? agent.velocity.normalized : transform.forward;

            for (int i = 0; i < samples; i++)
            {
                float t = (samples == 1) ? 0f : i / (samples - 1f);
                float angle = Mathf.Lerp(-maxSpreadAngle, maxSpreadAngle, t);
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * away;
                Vector3 candidate = transform.position + dir.normalized * fleeDistance;

                float awayDot = Vector3.Dot(dir.normalized, away);
                if (awayDot < MIN_AWAY_DOT) continue;
                float moveDot = Vector3.Dot(dir.normalized, moveDir);
                if (moveDot < TURN_BLOCK_DOT) continue;
                if (!NavMesh.SamplePosition(candidate, out var hit, fleeDistance, NavMesh.AllAreas)) continue;

                var path = new NavMeshPath();
                if (!agent.CalculatePath(hit.position, path)) continue;
                if (path.status != NavMeshPathStatus.PathComplete) continue;
                if (NavMesh.Raycast(transform.position, hit.position, out var _, NavMesh.AllAreas)) continue;

                float distToThreat = Vector3.Distance(hit.position, threat.position);
                if (distToThreat < currentDist + MIN_DIST_GAIN) continue;
                float runway = EstimateRunway(hit.position, dir, runwayMax, runwayStep);
                if (runway < minRunwayAccept) continue;

                float pathLen = PathLength(path);
                float score = 2.5f * runway + 0.7f * (distToThreat - currentDist) + 0.1f * pathLen + 0.6f * awayDot + 0.3f * moveDot;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = hit.position;
                    bestDir = dir;
                }
            }
            return bestScore > float.NegativeInfinity;
        }

        private float EstimateRunway(Vector3 from, Vector3 dir, float maxRun, float step)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) return 0f;
            dir.Normalize();

            float reached = 0f;
            int areaMask = NavMesh.AllAreas;

            for (float d = step; d <= maxRun + 0.001f; d += step)
            {
                Vector3 target = from + dir * d;
                if (!NavMesh.SamplePosition(target, out var hit, step * 0.6f, areaMask))
                    break;

                var path = new NavMeshPath();
                if (!NavMesh.CalculatePath(from, hit.position, areaMask, path) ||
                    path.status != NavMeshPathStatus.PathComplete)
                    break;

                reached = d;
            }
            return reached;
        }

        private float ScoreOf(Vector3 target)
        {
            if (!threat) return 0f;
            Vector3 dir = target - transform.position; dir.y = 0f;
            float currentDist = Vector3.Distance(transform.position, threat.position);
            float distToThreat = Vector3.Distance(target, threat.position);
            float runway = EstimateRunway(target, dir, runwayMax, runwayStep);
            return 2.5f * runway + 0.7f * (distToThreat - currentDist);
        }

        private bool TryGetSafestWaypoint(out Vector3 pos)
        {
            pos = transform.position;
            if (waypoints == null || waypoints.Length == 0) return false;

            float best = float.NegativeInfinity;
            bool found = false;

            foreach (var wp in waypoints)
            {
                var path = new NavMeshPath();
                if (!agent.CalculatePath(wp.position, path) || path.status != NavMeshPathStatus.PathComplete)
                    continue;

                float score = Vector3.Distance(wp.position, threat.position);
                if (score > best)
                {
                    best = score;
                    pos = wp.position;
                    found = true;
                }
            }
            return found;
        }

        private float PathLength(NavMeshPath path)
        {
            float len = 0f;
            var c = path.corners;
            for (int i = 1; i < c.Length; i++) len += Vector3.Distance(c[i - 1], c[i]);
            return len;
        }

        private void SwitchToEvade()
        {
            mode = Mode.Evade;
            agent.speed = evadeSpeed;
            repathTimer = 0f;
            hasEvadeTarget = false;
            lastRemainingToTarget = Mathf.Infinity;
        }

        private void SwitchToNavigate()
        {
            mode = Mode.Navigate;
            agent.speed = patrolSpeed;
            hasEvadeTarget = false;

            if (waypoints != null && waypoints.Length > 0)
            {
                agent.SetDestination(waypoints[wpIndex].position);
                if (ultimoResaltado != wpIndex) ResaltarSolo(wpIndex);
            }
            fleeDistance = Mathf.Clamp(fleeDistance, 4f, 20f);
            maxSpreadAngle = Mathf.Clamp(maxSpreadAngle, 60f, 170f);
        }

        public void SetWaypoints(Transform[] wps) => waypoints = wps;
        public void SetThreat(Transform t) => threat = t;
        private void ResaltarSolo(int idxObjetivo)
        {
            if (waypoints == null || waypoints.Length == 0) return;
            if (ultimoResaltado >= 0 && ultimoResaltado < waypoints.Length)
            {
                var rendPrev = waypoints[ultimoResaltado]?.GetComponent<Renderer>();
                if (rendPrev != null && MaterialDefault != null)
                    rendPrev.sharedMaterial = MaterialDefault;
            }
            var rend = waypoints[idxObjetivo]?.GetComponent<Renderer>();
            if (rend != null && MaterialObjetivo != null)
                rend.sharedMaterial = MaterialObjetivo;
            ultimoResaltado = idxObjetivo;
        }
    }
}