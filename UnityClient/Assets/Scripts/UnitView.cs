using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{
    class UnitView
    {
        private GameObject _unitObject;
        private GameObject _parentObject;
        private UnitState _state;

        private Animator _animation;

        public UnitView()
        {
            Sprite unitSprite = Resources.Load<Sprite>("Sprites/UnitSprite");

            _parentObject = new GameObject();
            _unitObject = new GameObject();

            _unitObject.transform.parent = _parentObject.transform;

            var rend = _unitObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            rend.sprite = unitSprite;

            _animation = _unitObject.AddComponent<Animator>();
            _animation.runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animations/UnitAnimationController");

            SetState(UnitState.Stopped);
        }

        public void SetState(UnitState state)
        {
            switch (state)
            {
                case UnitState.Moving:
                {
                    if (_animation.GetBool("Stop"))
                    {
                        _animation.SetBool("Stop", false);
                    }
                        break;
                }
                case UnitState.Stopped:
                {
                    if (!_animation.GetBool("Stop"))
                    {
                        _animation.SetBool("Stop", true);
                    }

                    break;
                }
            }
        }

        public Bounds Bounds
        {
            get
            {
                var bounds = _unitObject.GetComponent<SpriteRenderer>().bounds;
                bounds.Expand(_parentObject.transform.localScale);
                return bounds;
            }
        }

        public void SetPosition(Vector3 position)
        {
            _parentObject.transform.position = position;
        }

        public void SetScale(Vector3 scale)
        {
            _parentObject.transform.localScale = new Vector3(
                scale.x,
                scale.y,
                1);
        }



        public bool IsSelected
        {
            get
            {
                return _unitObject.GetComponent<SpriteRenderer>().color == Color.green;
            }

            set
            {
                _unitObject.GetComponent<SpriteRenderer>().color = value? Color.green : Color.gray;
            }
        }
    }
}
