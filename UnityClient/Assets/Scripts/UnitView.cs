using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    class UnitView
    {
        private GameObject _unitObject;

        private GameObject _selectionObject;

        public UnitView()
        {
            Sprite unitSprite = Resources.Load<Sprite>("Sprites/UnitSprite");

            _unitObject = new GameObject();

            var rend = _unitObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;

            rend.sprite = unitSprite;


            Sprite selectionSprite = Resources.Load<Sprite>("Sprites/SelectionSprite");

            _selectionObject = new GameObject();

            var rend2 = _selectionObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            rend2.sprite = selectionSprite;

        }

        public Bounds Bounds
        {
            get
            {
                var bounds = _unitObject.GetComponent<SpriteRenderer>().bounds;
                bounds.Expand(_unitObject.transform.localScale);
                return bounds;
            }
        }

        public void SetPosition(Vector3 position)
        {
            _unitObject.transform.position = position;
            _selectionObject.transform.position = position + Vector3.forward;
        }

        public void SetScale(Vector3 scale)
        {
            _unitObject.transform.localScale = new Vector3(
                0.9f * scale.x,
                0.9f * scale.y,
                1);

            _selectionObject.transform.localScale = new Vector3(
                scale.x,
                scale.y,
                1);
        }

        public bool IsSelected
        {
            get
            {
                return _selectionObject.activeSelf;
            }

            set
            {
                 _selectionObject.SetActive(value);
            }
        }
    }
}
