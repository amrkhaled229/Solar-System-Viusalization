using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public Camera mainCamera; // Assign your main camera here
    public Vector3 targetPosition = new Vector3(2.17f, 0.97f, -44.81f); // Define the target position
    public Vector3 originalPosition = new Vector3(0.77f, 5.400533f, -52.27061f); // Original camera position
    public float moveDuration = 2f; // Duration for the entire movement
    public Button startButton; // Assign the Start button here
    public TextMeshProUGUI planetInfoText;  // UI Text to show planet info

    public GameObject mainMenuCanvas;
    private bool isMoving = false;
    private bool isAtTarget = false; // Track if the camera is at the target position
    private Vector3 startPosition; // Starting position of the camera for each move
    private Vector3 endPosition; // Ending position of the camera for each move
    private float elapsedTime = 0f; // Time elapsed since movement started

    public GameObject[] planets; // Array to hold planet objects for hover animation
    private float hoverSpeed = 2f; // Speed of the hover animation
    private float hoverHeight = 0.005f; // Height of the hover animation

    private List<string> planetInfoList = new List<string>() // List of 8 strings with detailed planet information
{
    "Planet Information: Neptune\n\n" +
    "Neptune, the eighth and farthest known planet from the Sun, is a dark, cold, and windy ice giant. " +
    "It has a dynamic atmosphere with winds that can reach up to 2,100 km/h (1,300 mph). Neptune’s " +
    "deep blue color is due to the presence of methane in its atmosphere. It has 14 known moons, " +
    "with Triton being the largest.",

    "Planet Information: Venus\n\n" +
    "Venus is the second planet from the Sun and has a thick, toxic atmosphere " +
    "composed mainly of carbon dioxide, with clouds of sulfuric acid. It is the hottest planet " +
    "in the solar system, with surface temperatures reaching 465°C (869°F). Venus rotates slowly " +
    "and in the opposite direction to most other planets.",

    "Planet Information: Earth\n\n" +
    "Earth is the third planet from the Sun and the only one known to support life. " +
    "It has a rich atmosphere composed mostly of nitrogen and oxygen, a water-rich surface, " +
    "and diverse ecosystems. Earth’s climate and distance from the Sun make it ideal for " +
    "hosting life.",

    "Planet Information: Mercury\n\n" +
    "Mercury is the smallest planet in our solar system and closest to the Sun. " +
    "It has a thin atmosphere of oxygen, sodium, hydrogen, helium, and potassium, " +
    "but cannot support life as we know it. Mercury has extreme temperatures, reaching up to " +
    "430°C (800°F) during the day and dropping to -180°C (-290°F) at night.",



    "Planet Information: Mars\n\n" +
    "Mars, known as the Red Planet, is the fourth planet from the Sun. Its surface is " +
    "covered in iron oxide dust, giving it a reddish appearance. Mars has a thin atmosphere " +
    "composed mostly of carbon dioxide. Scientists are exploring Mars for signs of past or " +
    "present life and potential human colonization.",

    "Planet Information: Jupiter\n\n" +
    "Jupiter is the largest planet in our solar system, a gas giant with a mass more than " +
    "twice that of all other planets combined. It has a thick atmosphere primarily made of " +
    "hydrogen and helium. Jupiter is known for its Great Red Spot, a massive storm larger " +
    "than Earth that has been raging for centuries.",

    "Planet Information: Saturn\n\n" +
    "Saturn is the sixth planet from the Sun and is famous for its prominent ring system, " +
    "composed of ice and rock particles. Like Jupiter, it is a gas giant with a composition " +
    "mainly of hydrogen and helium. Saturn has at least 83 moons, with Titan being the largest " +
    "and possessing a thick, nitrogen-rich atmosphere.",

    "Planet Information: Uranus\n\n" +
    "Uranus is the seventh planet from the Sun and has a unique tilt, rotating on its side " +
    "at an angle of 98 degrees. It is an ice giant with an atmosphere mostly composed of hydrogen, " +
    "helium, and methane, which gives it a blue color. Uranus has 27 known moons and faint rings."

};

    private int currentInfoIndex = 0; // Track the current index of planet info displayed

    private float cooldownDuration = 2f; // Duration of cooldown in seconds
    private float lastInputTime = -2f; // Tracks the last time an arrow key was pressed

    public void Start()
    {
        // Set up the Start button to trigger the MoveToOriginalPosition function
        startButton.onClick.AddListener(MoveToOriginalPosition);
        planetInfoText.gameObject.SetActive(false); // Hide planet info text initially
        UpdatePlanetInfoText(); // Display initial text

    }

    void Update()
    {
        // Toggle camera movement and planet info on spacebar press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleCameraPosition();
        }

        // Check if the camera should move towards the target position
        if (isMoving)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the interpolation factor with ease-in and ease-out
            float t = elapsedTime / moveDuration;
            t = t * t * (3f - 2f * t); // Smoothstep function for ease-in and ease-out

            // Interpolate between start and end positions
            mainCamera.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            // Stop moving if the camera has reached the end position
            if (elapsedTime >= moveDuration)
            {
                isMoving = false;
            }
        }

        // Check for left or right arrow key inputs to cycle through planet info, with cooldown
        if (Time.time - lastInputTime >= cooldownDuration)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentInfoIndex = (currentInfoIndex + 1) % planetInfoList.Count;
                UpdatePlanetInfoText();
                lastInputTime = Time.time; // Reset the cooldown timer
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentInfoIndex = (currentInfoIndex - 1 + planetInfoList.Count) % planetInfoList.Count;
                UpdatePlanetInfoText();
                lastInputTime = Time.time; // Reset the cooldown timer
            }
        }
        AnimateCurrentPlanet();
    }
    void AnimateCurrentPlanet()
    {
        if (planets.Length > 0 && currentInfoIndex < planets.Length)
        {
            GameObject currentPlanet = planets[currentInfoIndex];
            float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            Vector3 newPosition = currentPlanet.transform.position;
            newPosition.y += hoverOffset;
            currentPlanet.transform.position = newPosition;
        }
    }
    void ToggleCameraPosition()
    {
        // Toggle between original and target positions
        isAtTarget = !isAtTarget;
        startPosition = mainCamera.transform.position;
        endPosition = isAtTarget ? targetPosition : originalPosition;
        elapsedTime = 0f;
        isMoving = true;

        // Toggle planet info text visibility
        planetInfoText.gameObject.SetActive(isAtTarget);
        if (isAtTarget)
        {
            UpdatePlanetInfoText(); // Show current info when camera moves to target
        }
    }

    public void MoveToOriginalPosition()
    {
        // Move the camera to the original position when the start button is clicked
        startButton.gameObject.SetActive(false); // Hide the Start button
        startPosition = mainCamera.transform.position;
        endPosition = originalPosition;
        elapsedTime = 0f;
        isMoving = true;

        // Hide planet info text if showing
        planetInfoText.gameObject.SetActive(false);
        isAtTarget = false; // Set flag to indicate camera is at the original position

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }
    }

    void UpdatePlanetInfoText()
    {
        // Update the displayed text based on the current index
        planetInfoText.text = planetInfoList[currentInfoIndex];
    }
}
