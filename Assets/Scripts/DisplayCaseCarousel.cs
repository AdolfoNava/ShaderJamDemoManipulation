﻿using System.Collections.Generic;
using UnityEngine;

namespace AdrianMiasik
{
    public class DisplayCaseCarousel : MonoBehaviour
    {
        // References
        [SerializeField] private DisplayCase displayPrefab = null;
        [SerializeField] private MaterialList materials = null;
        [SerializeField] private GameObject modelPrefab = null;
        [SerializeField] private DisplayCaseSelector displayCaseSelector = null;
        
        // TODO: Create inspector which only shows this after init
        [SerializeField] private Vector3 targetPosition = Vector3.zero; // Position where we want the selected object to go to
        [SerializeField] private Vector3 itemOffset = Vector3.up; // How much to offset the model inside the display case?
        [SerializeField] private Vector3 displayOffset = Vector3.right; // How much to offset the entire display case?
        [SerializeField] private float animationDuration = 0.5f;
        
        // TODO: Create inspector which makes this a read-only
        private bool isInitialized;
        private readonly List<DisplayCase> displays = new List<DisplayCase>();
        private bool isAnimating;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private float accumulatedTime;

        /// <summary>
        /// Invoked when the DisplayModel selection changes
        /// </summary>
        public delegate void OnDisplayChange(DisplayCase _previousDisplay, DisplayCase _currentDisplay);
        public OnDisplayChange onDisplayChange;

        public delegate void OnClick(DisplayCaseCarousel _clickedCarousel);
        public OnClick onClick;

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("This carousel has already been initialized");
                return;
            }
            
            GenerateDisplays(displayPrefab, modelPrefab, materials.materials.Count);

            // Swap materials on each model
            for (int _i = 0; _i < displays.Count; _i++)
            {
                DisplayCase _display = displays[_i];
                _display.ChangeModelMaterial(materials.materials[_i]);
            }

            targetPosition = transform.position;
            
            displayCaseSelector.Initialize(displays);
            displayCaseSelector.onSelectionChange += OnSelectionChange;

            isInitialized = true;
        }

        /// <summary>
        /// Destroys any cached game objects
        /// </summary>
        [ContextMenu("Clean Up")]
        public void CleanUp()
        {
            foreach (DisplayCase _displayCase in displays)
            {
                DestroyImmediate(_displayCase.gameObject);
            }
            
            Clear();
        }

        private void Clear()
        {
            displays.Clear();
            targetPosition = Vector3.zero;
            
            displayCaseSelector.onSelectionChange -= OnSelectionChange;
            displayCaseSelector.Clear();
            
            isInitialized = false;
        }
        
        private void OnSelectionChange(DisplayCase _previousDisplay, DisplayCase _currentDisplay)
        {
            // Ignore clicks on the same display case
            if (_currentDisplay == _previousDisplay)
            {
                return;
            }

            startPosition = transform.position;
            endPosition = displayOffset * (displayCaseSelector.GetCurrentIndex() * -1);
            accumulatedTime = 0;

            isAnimating = true;
            onDisplayChange?.Invoke(_previousDisplay, _currentDisplay);
        }

        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            if (!isAnimating)
            {
                return;
            }

            accumulatedTime += Time.deltaTime;
            if (accumulatedTime > animationDuration)
            {
                accumulatedTime = animationDuration;
                isAnimating = false;
            }

            float _t = accumulatedTime / animationDuration;
            _t = _t * _t * (3 - 2 * _t); // Smoothstep formula
            transform.position = Vector3.Lerp(startPosition, targetPosition + endPosition, _t);
        }

        // TODO: Utilize
        public void SetTargetPosition(Vector3 _targetPosition)
        {
            startPosition = transform.position;
            targetPosition = _targetPosition;
            isAnimating = true;
            accumulatedTime = 0;
        }

        /// <summary>
        /// Creates and caches a list of DisplayCases
        /// </summary>
        /// <param name="_displayPrefab">What display case do you want to generate?</param>
        /// <param name="_modelToSpawnInside">What GameObject do you want to generate inside the display case?</param>
        /// <param name="_quantity">How many displays would you like to generate?</param>
        /// <returns></returns>
        private void GenerateDisplays(DisplayCase _displayPrefab, GameObject _modelToSpawnInside, int _quantity)
        {
            displays.Clear();

            for (int _i = 0; _i < _quantity; _i++)
            {
                // Create a display
                DisplayCase _displayCase = Instantiate(_displayPrefab, transform);
                _displayCase.Initialize(_modelToSpawnInside, itemOffset);
                _displayCase.SetDisplayPosition(_displayCase.GetDisplayWorldPosition() + displayOffset * _i);
                _displayCase.onClick += OnDisplayCaseClick;

                // Cache display
                displays.Add(_displayCase);
            }
        }
        
        public void NextDisplay()
        {
            displayCaseSelector.NextItem();
        }

        public void PreviousDisplay()
        {
            displayCaseSelector.PreviousItem();
        }
        
        public DisplayCase GetSelectedDisplayModel()
        {
            return displayCaseSelector.GetCurrentItem();
        }

        private void OnDisplayCaseClick(DisplayCase _displayCase)
        {
            displayCaseSelector.Select(_displayCase);
            onClick?.Invoke(this);
        }

        /// <summary>
        /// Returns the cached display cases
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DisplayCase> GetDisplayCases()
        {
            return displays;
        }
    }
}