using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public GameObject bluetoothPanel; // Reference to the panel for displaying Bluetooth names
    public GameObject buttonPrefab;   // Prefab for dynamically creating buttons for each Bluetooth device

    public void OnLoginButtonClick()
    {
        // Check if the SocketManager instance exists and send the "bluetooth" message
        if (SocketManager.Instance != null)
        {
            SocketManager.Instance.SendMessage("bluetooth");
            Debug.Log("Sent 'bluetooth' command to the server.");
        }
        else
        {
            Debug.LogError("SocketManager instance is missing or not connected.");
        }
    }

    private void Update()
    {
        // Check for incoming Bluetooth names
        string message = SocketManager.Instance.ReceiveMessage();
        if (message != null && message.StartsWith("bluetooth_names:"))
        {
            string[] names = message.Substring("bluetooth_names:".Length).Split(';');
            DisplayBluetoothNames(names);
        }
    }

    private void DisplayBluetoothNames(string[] names)
    {
        // Clear existing buttons
        foreach (Transform child in bluetoothPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each Bluetooth name
        foreach (string name in names)
        {
            GameObject newButton = Instantiate(buttonPrefab, bluetoothPanel.transform);
            newButton.GetComponentInChildren<Text>().text = name; // Set button text
            newButton.SetActive(true); // Show the button
        }
    }
}
