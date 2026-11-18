
using UnityEngine;
using System.IO.Ports;
using Bhaptics.SDK2;

public class ArduinoHapticTest : MonoBehaviour
{
    [Header("Serial")]
    public string portName = "COM9";
    public int baudRate = 9600;

    SerialPort sp;
    string buffer = "";

    int left = 0;
    int right = 0;

    // Remember previous button states
    int previousLeft = 0;
    int previousRight = 0;

    void Start()
    {
        try
        {
            sp = new SerialPort(portName, baudRate);
            sp.DtrEnable = true;
            sp.Open();

            Debug.Log("Arduino connected on " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Could not open " + portName + ": " + e.Message);
        }
    }

    void Update()
    {
        ReadSerial();

        // Left button: trigger only when pressed
        if (left == 1 && previousLeft == 0)
        {
            Debug.Log("LEFT BUTTON PRESSED!");
            BhapticsLibrary.Play("badhit");
        }

        // Right button: trigger only when pressed
        if (right == 1 && previousRight == 0)
        {
            Debug.Log("RIGHT BUTTON PRESSED!");
            BhapticsLibrary.Play("badhit");
        }

        // Save current button states
        previousLeft = left;
        previousRight = right;
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

        int lastNewline = buffer.LastIndexOf('\n');

        if (lastNewline < 0)
        {
            if (buffer.Length > 256) buffer = "";
            return;
        }

        string complete = buffer.Substring(0, lastNewline);
        buffer = buffer.Substring(lastNewline + 1);

        string[] lines = complete.Split('\n');

        foreach (string line in lines)
        {
            string[] parts = line.Trim().Split(',');

            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int l) &&
                int.TryParse(parts[1], out int r))
            {
                left = l;
                right = r;
            }
        }
    }

    void OnDestroy()
    {
        if (sp != null && sp.IsOpen)
            sp.Close();
    }
}
