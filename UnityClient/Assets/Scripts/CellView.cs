using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    class CellView : MonoBehaviour
    {

        public CellView()
        {
        }

        public Point Point { get; set; }

        public event EventHandler<EventArgs> Clicked;

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
                RaycastHit2D rayHit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (rayHit.collider != null)
                {
                    if (rayHit.collider.gameObject == gameObject)
                    {
                        OnClicked();
                    }
                }
            }
        }

        protected virtual void OnClicked()
        {
            var clicked = Clicked;
            if (clicked != null)
            {
                clicked(this, EventArgs.Empty);
            }
        }
    }
}
