using UnityEngine;

public class MiningMechanic : MonoBehaviour
{
    public GameObject heldObject; // The object the player is holding, needs a tag
    public string miningTag = "MineTool"; // The tag that allows mining
    public int indentRadius = 10; // Radius of the circular indent
    public float indentDepth = 0.1f; // Depth of the indent (increased for better visibility)
    public LayerMask terrainLayer; // LayerMask for terrain
    public KeyCode miningKey = KeyCode.Q; // Set the mining key directly (default to "Q")

    private Terrain terrain;
    private TerrainData terrainData;

    private void Start()
    {
        // Cache the terrain reference
        terrain = Terrain.activeTerrain;
        terrainData = terrain.terrainData;

        // Check if terrain is found
        if (terrain == null || terrainData == null)
        {
            Debug.LogError("No active terrain found. Make sure your terrain is properly set up.");
        }
    }

    void Update()
    {
        // Call the mining mechanic if the player is holding the correct tool and presses the assigned mining key
        HandleMiningMechanic();
    }

    void HandleMiningMechanic()
    {
        // Check if the player is holding the correct object and presses the assigned mining key
        if (heldObject != null && heldObject.CompareTag(miningTag) && Input.GetKey(miningKey))
        {
            // Perform raycast from the camera to where the player is clicking
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
            {
                // Check if the hit object is terrain
                if (hit.collider.gameObject == terrain.gameObject)
                {
                    Debug.Log("Terrain hit at position: " + hit.point); // Log hit position
                    CreateIndent(hit.point); // Call the function to create the indent
                }
                else
                {
                    Debug.LogWarning("Raycast hit an object that is not terrain.");
                }
            }
            else
            {
                Debug.LogWarning("Raycast did not hit any object.");
            }
        }
    }

    // Function to create the circular indent on the terrain
    void CreateIndent(Vector3 point)
    {
        // Get terrain world position
        Vector3 terrainPos = terrain.transform.position;

        // Calculate normalized X and Z positions relative to the terrain
        int x = (int)(((point.x - terrainPos.x) / terrainData.size.x) * terrainData.heightmapResolution);
        int z = (int)(((point.z - terrainPos.z) / terrainData.size.z) * terrainData.heightmapResolution);

        // Make sure we're within bounds of the heightmap
        if (x < 0 || z < 0 || x >= terrainData.heightmapResolution || z >= terrainData.heightmapResolution)
        {
            Debug.LogError("Hit point is out of terrain bounds.");
            return;
        }

        // Get the heightmap area surrounding the impact point
        int radius = indentRadius;
        float[,] heights = terrainData.GetHeights(x - radius, z - radius, radius * 2, radius * 2);

        // Get the height at the clicked position
        float heightAtPoint = terrain.SampleHeight(point);

        // Modify the terrain heights to create a circular indent
        for (int i = 0; i < radius * 2; i++)
        {
            for (int j = 0; j < radius * 2; j++)
            {
                // Calculate the distance from the center of the indent
                float dist = Vector2.Distance(new Vector2(i, j), new Vector2(radius, radius));

                // If within the circle, lower the height gradually
                if (dist < radius)
                {
                    // Calculate the new height relative to the height at the clicked position
                    // Use indentDepth to create a proper indent effect without creating pillars
                    float newHeight = Mathf.Max(heights[i, j] - indentDepth * (1 - dist / radius), 0); // Ensure we don't go below 0

                    // Set the height only if it results in a valid value
                    heights[i, j] = newHeight;
                }
            }
        }

        // Apply the modified heights back to the terrain
        terrainData.SetHeights(x - radius, z - radius, heights);
    }
}