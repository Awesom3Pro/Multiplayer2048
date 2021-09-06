using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT,
    Tap
}
public class InputManager : MonoBehaviour
{
    private static InputManager instance;

    public static InputManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<InputManager>();
            }
            return instance;
        }
    }

    public Action<Direction> OnTouchReceived;

    public event Action OnIdleTimerCompleted;

    private float minMoveDistance = 50f;

    private Vector2 startPos;

    private bool onTouch;

    private readonly float idleTime = 10f;

    private float idleElapsedTime;

    private bool idleTimeActionInvoked = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !onTouch)
        {
            startPos = Input.mousePosition;

            onTouch = true;

            idleElapsedTime = 0f;
        }
        if (onTouch)
        {
            if (Mathf.Abs(Input.mousePosition.magnitude - startPos.magnitude) > minMoveDistance)
            {
                idleElapsedTime = 0f;

                Vector2 endPos = Input.mousePosition;

                Vector2 difference = endPos - startPos;

                if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
                {
                    if (difference.x > 0)
                    {
                        OnTouchReceived?.Invoke(Direction.RIGHT);

                        onTouch = false;
                    }
                    else
                    {
                        OnTouchReceived?.Invoke(Direction.LEFT);

                        onTouch = false;
                    }
                }
                else
                {
                    if (difference.y > 0)
                    {
                        OnTouchReceived?.Invoke(Direction.UP);

                        onTouch = false;
                    }
                    else
                    {
                        OnTouchReceived?.Invoke(Direction.DOWN);

                        onTouch = false;
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (onTouch)
            {
                OnTouchReceived?.Invoke(Direction.Tap);
            }
            onTouch = false;

            idleElapsedTime = 0f;
        }

        idleElapsedTime += Time.deltaTime;
        if(!idleTimeActionInvoked && idleElapsedTime >= idleTime)
        {
            Debug.Log("Player has reached idle timeout");

            OnIdleTimerCompleted?.Invoke();

            idleTimeActionInvoked = true;
        }
    }
}
