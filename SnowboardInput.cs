using UnityEngine;

//using System;

public class SnowboardInput : MonoBehaviour
{
    private SnowboardCharacterController m_snowboardController;
    void Start()
    {
        m_snowboardController = this.GetComponent<SnowboardCharacterController>();
    }

    void Update()
    {
        float turnAngle = Input.GetAxis("Horizontal");
        m_snowboardController.Turn(turnAngle);
		float effortDirection = Input.GetAxis ("Vertical");
		m_snowboardController.Effort(effortDirection);
    }

}
