
using UnityEngine;
using System.IO.Ports;
using Bhaptics.SDK2;

public class ButtonCubeMover : MonoBehaviour
{
    [Header("Arduino")]
    public string portName = "COM9";
    public int baudRate = 9600;

    [Header("Movement")]
    public float movementSpeed = 4f;
    public float minX = -4f;
    public float maxX = 5f;

    SerialPort sp;
    string buffer = "";

    int left = 0;
    int right = 0;

    float previousX;

    bool yesTriggered = false;

    void Start()
    {
        previousX = transform.position.x;

        try
        {
            sp = new SerialPort(portName, baudRate);
            sp.DtrEnable = true;
            sp.Open();

            Debug.Log("Arduino connected: " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Serial error: " + e.Message);
        }
    }

    void Update()
    {
        ReadSerial();

        // Continuous smooth movement
        float direction = right - left;

        Vector3 pos = transform.position;

        pos.x += direction * movementSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;

        // Trigger vibration when crossing YES position
        if (previousX > -1.8f &&
            pos.x <= -1.8f &&
            !yesTriggered)
        {
            yesTriggered = true;

            Debug.Log("YES REACHED - VIBRATION!");

            BhapticsLibrary.Play("torture");
        }

        // Reset when moving away from YES
        if (pos.x > -1.8f)
        {
            yesTriggered = false;
        }

        previousX = pos.x;
    }

    void ReadSerial()
    {
        if (sp == null || !sp.IsOpen) return;

        try
        {
            if (sp.BytesToRead > 0)
                buffer += sp.ReadExisting();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Serial read error: " + e.Message);
            return;
        }

        int newline;

        while ((newline = buffer.IndexOf('\n')) >= 0)
        {
            string line = buffer.Substring(0, newline).Trim();
            buffer = buffer.Substring(newline + 1);

            string[] parts = line.Split(',');

            if (parts.Length == 2 &&
                int.TryParse(parts[0].Trim(), out int l) &&
                int.TryParse(parts[1].Trim(), out int r))
            {
                left = l;
                right = r;
            }
        }

        if (buffer.Length > 256)
            buffer = "";
    }

    void OnDestroy()
    {
        if (sp != null && sp.IsOpen)
            sp.Close();
    }
}
