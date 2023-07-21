using UnityEngine;

public class BodyPart : MonoBehaviour
{

    private Transform m_Owner;
    private Transform m_ObjectivePosition;

    private float m_DelayTime = 0.05f;
    private float m_Distance = 0.1f;

    private Vector3 m_TargetPosition;

    [SerializeField]
    private float m_Speed = 8;

    private void Update()
    {
        BodyPartMovement();
    }

    private void BodyPartMovement()
    {
        if (m_TargetPosition != null && m_ObjectivePosition != null)
        {
            m_TargetPosition = m_ObjectivePosition.position - m_ObjectivePosition.forward * m_Distance; //Current position minus forward, since it should go behind the other body part.
            m_TargetPosition += (transform.position - m_TargetPosition) * m_DelayTime; //Move smoothly to the new position with a time delay.
            m_TargetPosition.z = 0; //Don't move on the Z vector.

            transform.position = Vector3.Lerp(transform.position, m_TargetPosition, Time.deltaTime * m_Speed);
        }
    }

    public Transform GetOwner()
    {
        return m_Owner;
    }
    public Transform GetObjectivePosition()
    {
        return m_ObjectivePosition;
    }
    public void SetOwner(Transform owner)
    {
        m_Owner = owner;
    }
    public void SetObjectivePosition(Transform objectivePosition)
    {
        m_ObjectivePosition = objectivePosition;
    }
}
