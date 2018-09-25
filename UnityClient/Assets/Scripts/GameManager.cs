using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Shared;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static NetworkManager _networkManager;

    private static long _lastUpdateTick;

    private static int _updateIntervalTicks = 30;

    private static WorldDescription _currentDesctiption;

    private static List<GameObject> _fieldCells = new List<GameObject>();

    private static Dictionary<Guid, UnitView> _unitSprites = new Dictionary<Guid, UnitView>();

    private bool _isSelecting = false;
    private Vector3 _mousePosition;


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
            // Здесь будет обновлено положение юнитов. По-хорошему, 
            // на клиенте стоило бы соорудить интерполяцию и свое "локальное"
            // перемещение.
	        UpdateWorldState(_networkManager.LastDescription);
	    }

        if (Input.GetMouseButtonDown(0))
        {
            _isSelecting = true;
            _mousePosition = Input.mousePosition;
        }


        if (Input.GetMouseButtonUp(0))
        {
            var selectionRect = GetScreenRect(_mousePosition, Input.mousePosition);

            foreach (var unit in _unitSprites)
            {
                Bounds bounds = unit.Value.Bounds;

                Vector3 screenPoint = Camera.main.WorldToScreenPoint(
                    new Vector3(
                        bounds.min.x,
                        bounds.min.y,
                        1));

                Vector3 screenPoint2 = Camera.main.WorldToScreenPoint(
                    new Vector3(
                        bounds.max.x,
                        bounds.max.y,
                        1));

                Rect unitScreenRect = GetScreenRect(
                    screenPoint,
                    screenPoint2);

                if (unitScreenRect.Overlaps(selectionRect) || unitScreenRect.Contains(selectionRect.center) || selectionRect.Contains(unitScreenRect.center))
                {
                    unit.Value.IsSelected = true;
                }
                else
                {
                    unit.Value.IsSelected = false;
                }
            }

            _isSelecting = false;
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
        if (_isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect(_mousePosition, Input.mousePosition);
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

    private void SendSelectedToPoint(Point point)
    {
        var args =new MoveArguments() { Position = point, Units = _unitSprites.Where(us => us.Value.IsSelected).Select(us => us.Key).ToArray() };

        _networkManager.SendCommand(args);
    }


    private void UpdateWorldState(WorldDescription newDescription)
    {
        Camera camera = Camera.main;
        int newSize = newDescription.WorldSize;
        float minSize = Math.Min(camera.rect.width, camera.rect.height);

        float cellZPosition = 30;
        float unitZPosition = 29;

        if (_currentDesctiption == null || newDescription.WorldSize != _currentDesctiption.WorldSize)
        {
            foreach(var cell in _fieldCells)
            {
                Destroy(cell);
            }

            _fieldCells.Clear();

            for (int i = 1; i <= newSize; i++)
            {
                for (int j = 1; j <= newSize; j++)
                {

                    Sprite fieldSprite = Resources.Load<Sprite>("Sprites/FieldCell");

                    var obj = new GameObject();

                    var rend = obj.AddComponent<SpriteRenderer>() ;
                    rend.sprite = fieldSprite;

                    obj.AddComponent<CircleCollider2D>();

                    var cellView = obj.AddComponent<CellView>();
                    cellView.Point = new Point(i - 1, j - 1);
                    cellView.Clicked += Cell_Clicked;

                    obj.transform.localScale = new Vector3(
                        0.1f * fieldSprite.rect.width * ( minSize / (newSize + 1)),
                        0.1f * fieldSprite.rect.height * ( minSize / (newSize + 1)),
                        1);

                    obj.transform.position = camera.ViewportToWorldPoint(new Vector3(i / (float)(newSize + 1) , j / (float)(newSize + 1), cellZPosition));

                    _fieldCells.Add(obj);
                }
            }
        }

        _currentDesctiption = newDescription;

        foreach (var unit in _currentDesctiption.Units)
        {
            if (!_unitSprites.ContainsKey(unit.UnitId))
            {
                var view = new UnitView();
                view.IsSelected = false;

                view.SetScale(new Vector3(
                    0.9f * 2 * (minSize / (newSize + 1)),
                    0.9f * 2 * (minSize / (newSize + 1)),
                    1));

                _unitSprites.Add(unit.UnitId, view);
            }

            _unitSprites[unit.UnitId].SetPosition(camera.ViewportToWorldPoint(
                new Vector3((unit.Position.X + 1) / (float)(newSize + 1),
                    (unit.Position.Y + 1) / (float)(newSize + 1), unitZPosition)));

            if (unit.Destination.HasValue)
            {
                _unitSprites[unit.UnitId].SetState(UnitState.Moving);
            }
            else
            {
                _unitSprites[unit.UnitId].SetState(UnitState.Stopped);
            }
        }

        // Здесь возможно удаление исчезнувших юнитов, но в
        // нашем случае это не нужно.
    }

    private void Cell_Clicked(object sender, EventArgs e)
    {
        var view = sender as CellView;

        if (view == null)
        {
            return;
        }

        SendSelectedToPoint(view.Point);
    }
}
