using System;
using System.Collections;
using System.Collections.Generic;
using Shared;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static NetworkManager _networkManager;

    private static long _lastUpdateTick;

    private static int _updateIntervalTicks = 30;

    private static WorldDescription _currentDesctiption;

    private static List<GameObject> _fieldCells = new List<GameObject>();

    bool isSelecting = false;
    Vector3 mousePosition1;


	void Start () {
        _networkManager = new NetworkManager();
        _networkManager.Start();
	}
	
	// Update is called once per frame
	void Update () {

	    if (Environment.TickCount - _lastUpdateTick > _updateIntervalTicks)
	    {
            _networkManager.RequestUpdateWorldState();
            _lastUpdateTick = Environment.TickCount;
	    }
		
        /// Здесь лучше бы использовать переопределенный Equals.
	    if (_currentDesctiption != _networkManager.LastDescription)
	    {
	        UpdateWorldState(_networkManager.LastDescription);
	    }

        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
        }
    }


    static Texture2D _whiteTexture;

    public static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect(mousePosition1, Input.mousePosition);
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }


    private void UpdateWorldState(WorldDescription newDescription)
    {
        Camera camera = Camera.main;

        if (_currentDesctiption == null || newDescription.WorldSize != _currentDesctiption.WorldSize)
        {
            foreach(var cell in _fieldCells)
            {
                Destroy(cell);
            }

            _fieldCells.Clear();

            int newSize = newDescription.WorldSize;
            float minSize = Math.Min(camera.rect.width, camera.rect.height);


            for (int i = 1; i <= newSize; i++)
            {
                for (int j = 1; j <= newSize; j++)
                {

                    Sprite fieldSprite = Resources.Load<Sprite>("Sprites/FieldCell");

                    GameObject obj = new GameObject();

                    SpriteRenderer rend = obj.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;

                    rend.sprite = fieldSprite;

                    obj.transform.localScale = new Vector3(
                        0.1f * fieldSprite.rect.width * ( minSize / (newSize + 1)),
                        0.1f * fieldSprite.rect.height * ( minSize / (newSize + 1)),
                        1);

                    obj.transform.position = camera.ViewportToWorldPoint ( new Vector3(i / (float)(newSize + 1) , j / (float)(newSize + 1), 30));
                }
            }
        }

            _currentDesctiption = newDescription;
    }
}
