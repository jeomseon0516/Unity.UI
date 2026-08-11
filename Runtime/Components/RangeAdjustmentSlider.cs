using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;

#if UNITY_EDITOR
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("com.jeomseon.ui.rangeadjustmentslider.editor")]
#endif

namespace Jeomseon.UI.Components
{
    using static UIHelper;

    [DisallowMultipleComponent]
    public sealed class RangeAdjustmentSlider : MonoBehaviour
    {
        [field: SerializeField] public Image LeftHandle { get; set; } = null;
        [field: SerializeField] public Image RightHandle { get; set; } = null;

        [field: SerializeField] public Image BackgroundBar { get; set; } = null;
        [field: SerializeField] public Image FrontBar { get; set; } = null;

        [field: SerializeField] public int DivideValue { get; set; } = 100;
        [field: SerializeField] public bool IsDivide { get; set; } = false;

        [field: SerializeField] public float HandleSizeRatio { get; set; } = 2.5f;
        [field: SerializeField] public Camera TargetCamera { get; set; } = null;

        [field: SerializeField] public UnityEvent<float> OnChangedLeftValue { get; private set; }
        [field: SerializeField] public UnityEvent<float> OnChangedRightValue { get; private set; }

        [field: SerializeField] public UnityEvent<int> OnChangedLeftIntValue { get; private set; }
        [field: SerializeField] public UnityEvent<int> OnChangedRightIntValue { get; private set; }

        public float LeftValue
        {
            get => leftValue;
            set => leftValue = Mathf.Clamp(value, 0f, rightValue);
        }

        public float RightValue
        {
            get => rightValue;
            set => rightValue = Mathf.Clamp(value, leftValue, 1f);
        }

        public int LeftIntValue
        {
            get => leftIntValue;
            set => leftIntValue = Mathf.Clamp(value, 0, rightIntValue - 1);
        }

        public int RightIntValue
        {
            get => rightIntValue;
            set => rightIntValue = Mathf.Clamp(value, leftIntValue + 1, DivideValue);
        }

        public float HandleSize => BackgroundBar ? BackgroundBar.rectTransform.rect.height * HandleSizeRatio : 0.0f;

        [SerializeField, FormerlySerializedAs("_leftValue")]
        private float leftValue = 0f;
        [SerializeField, FormerlySerializedAs("_rightValue")]
        private float rightValue = 1f;

        [SerializeField, FormerlySerializedAs("_leftIntValue")]
        private int leftIntValue = 0;
        [SerializeField, FormerlySerializedAs("_rightIntValue")]
        private int rightIntValue = 1;

        private Image _selectedHandle = null;
        private bool _isLeft = false;

        private void Awake()
        {
            if (!TargetCamera)
            {
                TargetCamera = GetComponentInParent<Camera>();
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            Init();
        }

        private void Update()
        {
            if (Mathf.Abs(LeftHandle.rectTransform.localPosition.x) > Mathf.Abs(RightHandle.rectTransform.localPosition.x))
            {
                checkIntersectHandle(LeftHandle, true);
                checkIntersectHandle(RightHandle, false);
                RightHandle.rectTransform.SetAsLastSibling();
            }
            else
            {
                checkIntersectHandle(RightHandle, false);
                checkIntersectHandle(LeftHandle, true);
                LeftHandle.rectTransform.SetAsLastSibling();
            }

            if (_selectedHandle)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    BackgroundBar.rectTransform,
                    Input.mousePosition,
                    TargetCamera,
                    out Vector2 mouseLocalPosition);
                float normalizedX = (mouseLocalPosition.x + BackgroundBar.rectTransform.rect.width * 0.5f) / BackgroundBar.rectTransform.rect.width;

                if (IsDivide)
                {
                    if (_isLeft)
                    {
                        LeftIntValue = (int)(normalizedX * DivideValue);
                    }
                    else
                    {
                        RightIntValue = (int)(normalizedX * DivideValue);
                    }
                }
                else
                {
                    if (_isLeft)
                    {
                        LeftValue = normalizedX;
                    }
                    else
                    {
                        RightValue = normalizedX;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _selectedHandle = null;
                }
            }

            if (IsDivide)
            {
                setHandleLocalPositionAndNotify(LeftHandle, GetIntValueToLocalPosition(leftIntValue), () => OnChangedLeftIntValue.Invoke(leftIntValue));
                setHandleLocalPositionAndNotify(RightHandle, GetIntValueToLocalPosition(rightIntValue), () => OnChangedRightIntValue.Invoke(rightIntValue));
            }
            else
            {
                setHandleLocalPositionAndNotify(LeftHandle, GetValueToLocalPosition(leftValue), () => OnChangedLeftValue.Invoke(leftValue));
                setHandleLocalPositionAndNotify(RightHandle, GetValueToLocalPosition(rightValue), () => OnChangedRightValue.Invoke(rightValue));
            }

            SetFrontBarSizeAndLocalPosition();
        }

        private void setHandleLocalPositionAndNotify(Image handle, Vector2 newHandlePosition, System.Action onChangedValue)
        {
            if (handle.rectTransform.localPosition.x == newHandlePosition.x) return;

            handle.rectTransform.localPosition = newHandlePosition;
            onChangedValue.Invoke();
        }

        private void checkIntersectHandle(Image handle, bool isLeft)
        {
            if (!Input.GetMouseButtonDown(0) ||
                !RectTransformUtility.RectangleContainsScreenPoint(
                    handle.rectTransform,
                    Input.mousePosition,
                    TargetCamera))
            {
                return;
            }

            _isLeft = isLeft;
            _selectedHandle = handle;
        }

        /* TODO(P2-01, api): UI Toolkit의 Slider 및 Pointer 이벤트로 대체할 수 있는 기능과
         * 런타임 uGUI에서만 필요한 동작을 구분해 이 컨트롤의 유지 범위를 결정합니다.
         */
        internal void Init()
        {
            BackgroundBar.rectTransform.sizeDelta = (transform as RectTransform).rect.size;

            Vector2 handleSize = new Vector2(BackgroundBar.rectTransform.rect.height, BackgroundBar.rectTransform.rect.height) * HandleSizeRatio;
            LeftHandle.rectTransform.sizeDelta = handleSize;
            RightHandle.rectTransform.sizeDelta = handleSize;

            SetFrontBarSizeAndLocalPosition();
        }

        internal void SetFrontBarSizeAndLocalPosition()
        {
            float frontBarWidth = RightHandle.rectTransform.localPosition.x - LeftHandle.rectTransform.localPosition.x;
            FrontBar.rectTransform.sizeDelta = new(frontBarWidth, BackgroundBar.rectTransform.sizeDelta.y);
            FrontBar.rectTransform.localPosition = new(LeftHandle.rectTransform.localPosition.x + frontBarWidth * 0.5f, 0.0f);
        }

        internal Vector2 GetValueToLocalPosition(float value)
            => new(value * BackgroundBar.rectTransform.rect.width - BackgroundBar.rectTransform.rect.width * 0.5f, 0.0f);

        internal Vector2 GetIntValueToLocalPosition(int intValue)
            => new(intValue / (float)DivideValue * BackgroundBar.rectTransform.rect.width
                - BackgroundBar.rectTransform.rect.width * 0.5f, 0.5f);
    }
}
