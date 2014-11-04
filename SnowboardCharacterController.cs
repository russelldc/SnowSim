using UnityEngine;
using System.Collections;
//using System;

public class SnowboardCharacterController : MonoBehaviour
{
    private const float LengthOfSnowboard = 1.5f;
    private const float HalfSnowboardLength = LengthOfSnowboard / 2.0f;
    private const float HeightOfRider = 2.0f;

    public float m_minForwardVelocity;
    public float m_maxForwardVelocity;
    public float m_maxAccelerationOnGround;
    public float m_slopeAccelerationDivisor;
    public float m_verticalNudge;
    public float m_turnRate;
    public float m_forwardFriction;
    public float m_leanFactor;
	public int points;

	public static float m_JoyY;
	public static float m_JoyX;

    private float m_mountainAngle;
    private float m_mountainSlope;
    private Vector3 m_offset;
    private Vector3 m_snowboardPosition;
    private float m_forwardVelocity;
    private float m_heading;
    private float m_turnAngle;
    public ParticleEmitter m_leftEmitter;
    public ParticleEmitter m_rightEmitter;
    public GameObject m_rearEmitter;
    protected Vector3 m_velocity;
    protected Transform m_myTransform;

    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(25, 25, 400, 25), "Slope: " + m_mountainSlope);
        GUI.Label(new Rect(25, 50, 400, 25), "Angle: " + m_mountainAngle);
        GUI.Label(new Rect(25, 75, 400, 25), "Snowboard Velocity: " + m_forwardVelocity);
        GUI.Label(new Rect(25, 100, 400, 25), "Offset: " + m_offset);
        GUI.Label(new Rect(25, 125, 400, 25), "Position: " + m_snowboardPosition);
        GUI.Label(new Rect(25, 150, 400, 25), "Heading: " + m_heading);
        GUI.Label(new Rect(25, 175, 400, 25), "Turn: " + m_turnAngle);
		GUI.Label(new Rect(25, 200, 400, 25), "FPS: " + (int)(1.0f / Time.smoothDeltaTime));
		GUI.Label(new Rect(25, 225, 400, 25), "JoyX: " + m_JoyX);
		GUI.Label(new Rect(25, 250, 400, 25), "JoyY: " + m_JoyY);
    }

    void Reset()
    {
        m_verticalNudge = 0.025f;
        m_minForwardVelocity = 0.5f;
        m_maxForwardVelocity = 20.0f;
        m_maxAccelerationOnGround = 5.0f;
        m_forwardFriction = 0.25f;
        m_slopeAccelerationDivisor = 10.0f;
        m_turnRate = 45.0f;
        m_leanFactor = 1.0f;
		points = 0;
    }

    void Start()
    {
        m_forwardVelocity = 0.0f;
        m_heading = 0.0f;
        m_myTransform = transform;
        m_rearEmitter.particleEmitter.emit = true;
		points = 0;
    }

    public void Turn(float angle)
    {
        m_turnAngle = angle * m_turnRate;
    }

	public void LeanTurn(float m_JoyY)
	{
		m_turnAngle = m_JoyY * m_turnRate;
	}

	public void Effort(float effortDirection)
	{
		m_forwardVelocity += effortDirection / 50;
	}

    void Update()
    {
        // calculate the slope of the snowboard
        // sample either end of the snowboard and determine the slope of the line, use that slope to angle the snowboard

        // calculate the slope of the mountain
        // sample the four corners of the snowboard and determine the slope based on the normal of the polygon, use that normal to calculate the acceleration and the lean of the snowboard
        Quaternion myRot = m_myTransform.rotation;
        Quaternion currentHeadingRotation = Quaternion.AngleAxis(myRot.eulerAngles.y, Vector3.up);

        Vector3 ray = Vector3.down; //transform.TransformDirection(Vector3.down);
        Vector3 frontEndOfSnowboard = m_myTransform.position + (currentHeadingRotation * new Vector3(0.0f, HeightOfRider, -HalfSnowboardLength));
        Vector3 backEndOfSnowboard = m_myTransform.position + (currentHeadingRotation * new Vector3(0.0f, HeightOfRider, HalfSnowboardLength));
        Vector3 middleOfSnowboard = m_myTransform.position + new Vector3(0.0f, HeightOfRider, 0.0f);
        RaycastHit m_raycastFront;
        Physics.Raycast(frontEndOfSnowboard, ray, out m_raycastFront);
        RaycastHit m_raycastBack;
        Physics.Raycast(backEndOfSnowboard, ray, out m_raycastBack);
        RaycastHit m_raycastMiddle;
        Physics.Raycast(middleOfSnowboard, ray, out m_raycastMiddle);

        float mountainSlope = (m_raycastBack.point.y - m_raycastFront.point.y) / LengthOfSnowboard;
        m_mountainSlope = mountainSlope;
        float mountainAngle = Mathf.Atan(mountainSlope) * Mathf.Rad2Deg;
        m_mountainAngle = mountainAngle;

        // calculate acceleration and forward velocity based on slope of mountain
        float accelerationFromAngle = mountainAngle / m_slopeAccelerationDivisor;
        //float acceleration = Mathf.Min(mountainAngle / m_slopeAccelerationDivisor, m_maxAccelerationOnGround);
        m_forwardVelocity += accelerationFromAngle * Time.deltaTime;
        m_forwardVelocity = Mathf.Clamp(m_forwardVelocity, m_minForwardVelocity, m_maxForwardVelocity);
        m_forwardVelocity -= m_forwardFriction * Time.deltaTime;

		// Turn using balance board
		//LeanTurn (-m_JoyY);

		// Forward effort
		if (m_JoyX >= 0.5)
			Effort (-1);
		else if (m_JoyX <= -0.5)
			Effort (1);


        // calculate new heading from amount of turn
        m_heading += m_turnAngle * Time.deltaTime;

        // calculate rotation based on slope of mountain and snowboard heading and turn amount
        Quaternion pitchQuat = Quaternion.AngleAxis(m_mountainAngle, Vector3.left);

        Quaternion yawQuat = Quaternion.AngleAxis(m_heading, Vector3.up);

        Quaternion rollQuat = Quaternion.AngleAxis(m_turnAngle * m_leanFactor, Vector3.forward);

        m_myTransform.rotation = yawQuat * pitchQuat * rollQuat;

        // update position based on forward velocity
        Vector3 pos = m_myTransform.position;

        // my horizontal position updates based on my heading and my velocity
        Vector3 forwardVelocity = Vector3.forward * -m_forwardVelocity * Time.deltaTime;
        Vector3 headingOffset = yawQuat * forwardVelocity;
        m_offset = headingOffset;
        pos += headingOffset;
        pos.y = m_raycastMiddle.point.y + m_verticalNudge;
        m_myTransform.position = pos;
        m_snowboardPosition = pos;
    }

}