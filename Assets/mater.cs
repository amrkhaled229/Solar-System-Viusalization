using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlanetOrbitManager : MonoBehaviour
{
    
    public List<Transform> planets; // Add the planets in this list via the Inspector
    public Transform center; // The central object (like the Sun)
    public float radius = 10f; // Radius of the circular path
    public float angleStep; // Angle increment for each planet's position
    public Camera mainCamera; // The main camera
    public float rotationDuration = 6f; // Duration for rotation in seconds
    public float rotateSpeed = 2f; // Maximum speed of rotation


    public GameObject bluetoothPanel; // Panel to hold Bluetooth device buttons
    public GameObject bluetoothButtonTemplate; // Prefab template for Bluetooth device button
    public MainMenuManager mainMenuManager;

    private int focusedPlanetIndex = 0; // Index of the focused planet
    private float rotationTime = 0f; // Timer for rotation
    private bool isRotating = false; // Rotation state
    private int rotationDirection = 1; // 1 for clockwise, -1 for counter-clockwise
    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        // Initialize socket connection
        ConnectToSocket("localhost", 5000);

        // Calculate the angle increment based on the number of planets
        angleStep = 360f / planets.Count;

        // Place each planet in its initial position on the circular path
        for (int i = 0; i < planets.Count; i++)
        {
            SetPlanetPosition(i);
        }
    }

    void Update()
    {
        // Check for incoming socket messages and handle commands
        string message = ReceiveMessage();
        if (message != null)
        {
            ProcessSocketCommand(message);
        }

        // Rotate planets around the center with easing
        if (isRotating && rotationTime < rotationDuration)
        {
            float t = rotationTime / rotationDuration;
            float easedSpeed = Mathf.SmoothStep(0f, rotateSpeed, t) * Mathf.SmoothStep(1f, 0f, t); // Ease-in and ease-out curve

            foreach (Transform planet in planets)
            {
                planet.RotateAround(center.position, Vector3.up, rotationDirection * easedSpeed * Time.deltaTime);
            }
            rotationTime += Time.deltaTime;
        }
        else if (isRotating)
        {
            isRotating = false; // End rotation once duration is reached
        }

        // Handle left and right arrow keys for changing the focused planet
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!isRotating)
            {
                isRotating = true;
                rotationTime = 0f;
                rotationDirection = 1; // Set direction to clockwise
                SendMessage("right_arrow_pressed");
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!isRotating)
            {
                isRotating = true;
                rotationTime = 0f;
                rotationDirection = -1; // Set direction to clockwise
                SendMessage("left_arrow_pressed");
            }
        }
    }

    void OnApplicationQuit()
    {
        // Close socket connection when the application quits
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }

    // Connect to the socket server
    private void ConnectToSocket(string host, int portNumber)
    {
        try
        {
            client = new TcpClient(host, portNumber);
            stream = client.GetStream();
            Debug.Log("Connected to socket server at " + host);
        }
        catch (SocketException e)
        {
            Debug.LogError("Socket connection failed: " + e.Message);
        }
    }

    private void SendMessage(string message)
    {
        if (stream != null && stream.CanWrite)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            stream.Write(msg, 0, msg.Length);
            Debug.Log("Message sent: " + message);
        }
    }

    // Public method to send "bluetooth" message, to be linked to the LoginButton
    public void SendBluetoothMessage()
    {
        SendMessage("bluetooth");
        Debug.Log("Sent 'bluetooth' message to the server.");
    }

    // Receive a message from the socket server
    private string ReceiveMessage()
    {
        try
        {
            if (stream != null && stream.DataAvailable)
            {
                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                return Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Socket receive failed: " + e.Message);
        }
        return null;
    }

    // Process incoming commands from the socket
    private void ProcessSocketCommand(string command)
    {
        if (command.StartsWith("bluetooth_names:"))
        {
            string deviceNames = command.Substring("bluetooth_names:".Length);
            string[] devices = deviceNames.Split(';');
            DisplayBluetoothDevices(devices);
        }
        else if (command == "rotate_clockwise")
        {
            if (!isRotating)
            {
                isRotating = true;
                rotationTime = 0f;
                rotationDirection = 1; // Set direction to clockwise
            }
        }
        else if (command == "rotate_counterclockwise")
        {
            if (!isRotating)
            {
                isRotating = true;
                rotationTime = 0f;
                rotationDirection = -1; // Set direction to counter-clockwise
            }
        }
        else if (command == "terminate")
        {
            // Close connection if "terminate" command is received
            if (stream != null) stream.Close();
            if (client != null) client.Close();
            Debug.Log("Connection terminated by server.");
        }
    }

    // Display Bluetooth device names as buttons on the panel
    private void DisplayBluetoothDevices(string[] devices)
    {
        // Clear any existing buttons
        foreach (Transform child in bluetoothPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each Bluetooth device
        foreach (string device in devices)
        {
            GameObject newButton = Instantiate(bluetoothButtonTemplate, bluetoothPanel.transform);

            // Set the button's text to the device name
            Text buttonText = newButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = device;
            }
            else
            {
                TextMeshProUGUI tmpButtonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpButtonText != null)
                {
                    tmpButtonText.text = device;
                }
                else
                {
                    Debug.LogError("No Text or TextMeshProUGUI component found on the button prefab.");
                }
            }

            // Add a listener to each button to call MainMenuManager.Start()
            newButton.GetComponent<Button>().onClick.AddListener(() => mainMenuManager.MoveToOriginalPosition());
            newButton.SetActive(true); // Make sure the button is active
        }
    }



    // Set the position of a planet on the circular path
    private void SetPlanetPosition(int index, float angleOffset = 0f)
    {
        float angle = (index * angleStep + angleOffset) * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        planets[index].position = new Vector3(x, planets[index].position.y, z) + center.position;
    }
}
