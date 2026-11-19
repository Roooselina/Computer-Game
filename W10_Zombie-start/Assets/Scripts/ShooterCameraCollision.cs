using UnityEngine;
using Unity.Cinemachine;

// Retracts the orbit camera before Cinemachine evaluates its final position.
public class ShooterCameraCollision : CinemachineExtension {
    public Transform player;
    public LayerMask obstacleMask = ~0;
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
        if (stage != CinemachineCore.Stage.Body || !player) return;
        Vector3 origin = player.position + Vector3.up * 1.4f;
        Vector3 offset = state.RawPosition - origin;
        float distance = offset.magnitude;
        if (distance > .01f && Physics.SphereCast(origin, .2f, offset / distance, out RaycastHit hit,
            distance, obstacleMask, QueryTriggerInteraction.Ignore))
            state.RawPosition = origin + offset.normalized * Mathf.Max(.05f, hit.distance - .1f);
    }
}
