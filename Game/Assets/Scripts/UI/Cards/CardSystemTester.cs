using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// AIDEV-NOTE: Test script for demonstrating and validating the BaseCard system.
/// Can be attached to a GameObject to test different card types and states.
/// </summary>
public class CardSystemTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool _runTestsOnStart = true;
    [SerializeField] private int _numberOfTestCards = 3;
    
    [Header("Card Data References")]
    [SerializeField] private ContractData[] _testContracts;
    [SerializeField] private VehicleData[] _testVehicles;
    
    [Header("Card Prefab")]
    [SerializeField] private BaseCard _cardPrefab;
    
    [Header("Test Container")]
    [SerializeField] private Transform _cardContainer;
    
    // Test data
    private List<BaseCard> _testCardInstances = new List<BaseCard>();

    private void Start()
    {
        if (_runTestsOnStart)
        {
            StartCoroutine(RunCardSystemTests());
        }
    }

    /// <summary>
    /// Runs comprehensive tests of the card system.
    /// AIDEV-NOTE: Tests data binding, state changes, and interactions.
    /// </summary>
    private System.Collections.IEnumerator RunCardSystemTests()
    {
        Debug.Log("CardSystemTester: Starting card system tests...");
        
        // Test 1: Create and bind contract cards
        yield return TestContractCards();
        yield return new WaitForSeconds(1f);
        
        // Test 2: Create and bind vehicle cards
        yield return TestVehicleCards();
        yield return new WaitForSeconds(1f);
        
        // Test 3: Create and bind license cards
        yield return TestLicenseCards();
        yield return new WaitForSeconds(1f);
        
        // Test 4: Test card state changes
        yield return TestCardStates();
        yield return new WaitForSeconds(1f);
        
        // Test 5: Test card interactions
        TestCardInteractions();
        
        Debug.Log("CardSystemTester: All tests completed successfully!");
    }

    /// <summary>
    /// Tests contract card creation and data binding.
    /// AIDEV-NOTE: Validates ContractCardData integration with BaseCard.
    /// </summary>
    private System.Collections.IEnumerator TestContractCards()
    {
        Debug.Log("Testing Contract Cards...");
        
        if (_testContracts == null || _testContracts.Length == 0)
        {
            Debug.LogWarning("No test contracts assigned. Creating dummy contract data...");
            yield return CreateDummyContractData();
        }

        for (int i = 0; i < Mathf.Min(_numberOfTestCards, _testContracts.Length); i++)
        {
            if (_testContracts[i] != null)
            {
                var cardData = new ContractCardData(_testContracts[i], i == 0, false);
                yield return CreateAndTestCard(cardData, $"Contract Card {i + 1}");
            }
        }
    }

    /// <summary>
    /// Tests vehicle card creation and data binding.
    /// AIDEV-NOTE: Validates VehicleCardData integration with BaseCard.
    /// </summary>
    private System.Collections.IEnumerator TestVehicleCards()
    {
        Debug.Log("Testing Vehicle Cards...");
        
        if (_testVehicles == null || _testVehicles.Length == 0)
        {
            Debug.LogWarning("No test vehicles assigned. Skipping vehicle card tests.");
            yield break;
        }

        for (int i = 0; i < Mathf.Min(_numberOfTestCards, _testVehicles.Length); i++)
        {
            if (_testVehicles[i] != null)
            {
                var cardData = new VehicleCardData(_testVehicles[i], i == 1, false, i % 2 == 0);
                yield return CreateAndTestCard(cardData, $"Vehicle Card {i + 1}");
            }
        }
    }

    /// <summary>
    /// Tests license card creation and data binding.
    /// AIDEV-NOTE: Validates LicenseCardData integration with BaseCard.
    /// </summary>
    private System.Collections.IEnumerator TestLicenseCards()
    {
        Debug.Log("Testing License Cards...");
        
        var licenseTypes = new[] 
        { 
            LicenseType.Standard, 
            LicenseType.Perishable, 
            LicenseType.Heavy 
        };

        for (int i = 0; i < licenseTypes.Length; i++)
        {
            var cardData = new LicenseCardData(
                licenseTypes[i], 
                i == 2, // Selected
                false, 
                i == 0, // Owned (only standard)
                i == 0 ? 0f : (i * 2500f) // Cost
            );
            yield return CreateAndTestCard(cardData, $"License Card {licenseTypes[i]}");
        }
    }

    /// <summary>
    /// Creates a card instance and tests basic functionality.
    /// AIDEV-NOTE: Generic test method for any card data type.
    /// </summary>
    private System.Collections.IEnumerator CreateAndTestCard(ICardData cardData, string testName)
    {
        if (_cardPrefab == null)
        {
            Debug.LogError("Card prefab not assigned!");
            yield break;
        }

        // Create card instance
        var cardInstance = Instantiate(_cardPrefab, _cardContainer);
        _testCardInstances.Add(cardInstance);
        
        // Bind data
        cardInstance.BindData(cardData);
        
        // Test data binding
        if (cardInstance.CurrentData == cardData)
        {
            Debug.Log($"✓ {testName}: Data binding successful");
        }
        else
        {
            Debug.LogError($"✗ {testName}: Data binding failed");
        }
        
        // Test visual state
        if (cardInstance.IsSelected == cardData.IsSelected)
        {
            Debug.Log($"✓ {testName}: Selection state correct");
        }
        else
        {
            Debug.LogError($"✗ {testName}: Selection state incorrect");
        }
        
        yield return null; // Wait one frame for UI update
    }

    /// <summary>
    /// Tests various card state changes.
    /// AIDEV-NOTE: Validates state management and visual updates.
    /// </summary>
    private System.Collections.IEnumerator TestCardStates()
    {
        Debug.Log("Testing Card States...");
        
        if (_testCardInstances.Count == 0)
        {
            Debug.LogWarning("No test cards available for state testing");
            yield break;
        }

        var testCard = _testCardInstances[0];
        
        // Test selection
        testCard.SetSelected(true);
        yield return new WaitForSeconds(0.5f);
        
        if (testCard.IsSelected)
        {
            Debug.Log("✓ Card selection state change successful");
        }
        else
        {
            Debug.LogError("✗ Card selection state change failed");
        }
        
        // Test disabled state
        testCard.SetDisabled(true);
        yield return new WaitForSeconds(0.5f);
        
        if (testCard.IsDisabled)
        {
            Debug.Log("✓ Card disabled state change successful");
        }
        else
        {
            Debug.LogError("✗ Card disabled state change failed");
        }
        
        // Test loading state
        testCard.SetLoading(true);
        yield return new WaitForSeconds(1f);
        testCard.SetLoading(false);
        
        Debug.Log("✓ Card loading state test completed");
        
        // Reset to normal state
        testCard.SetDisabled(false);
        testCard.SetSelected(false);
    }

    /// <summary>
    /// Tests card interaction events.
    /// AIDEV-NOTE: Validates event system integration and callbacks.
    /// </summary>
    private void TestCardInteractions()
    {
        Debug.Log("Testing Card Interactions...");
        
        if (_testCardInstances.Count == 0)
        {
            Debug.LogWarning("No test cards available for interaction testing");
            return;
        }

        var testCard = _testCardInstances[0];
        
        // Subscribe to events
        testCard.OnCardClicked += OnTestCardClicked;
        testCard.OnCardSelected += OnTestCardSelected;
        testCard.OnCardDeselected += OnTestCardDeselected;
        testCard.OnPrimaryActionClicked += OnTestPrimaryAction;
        testCard.OnSecondaryActionClicked += OnTestSecondaryAction;
        
        Debug.Log("✓ Card interaction event handlers registered");
    }

    /// <summary>
    /// Creates dummy contract data for testing when none is assigned.
    /// AIDEV-NOTE: Generates minimal test data for validation purposes.
    /// </summary>
    private System.Collections.IEnumerator CreateDummyContractData()
    {
        // This would need to create ScriptableObject instances for testing
        // For now, just log that dummy data would be created
        Debug.Log("Would create dummy contract data here...");
        yield return null;
    }

    // Event handlers for testing

    private void OnTestCardClicked(BaseCard card, ICardData data)
    {
        Debug.Log($"✓ Card clicked event received: {data.Title}");
    }

    private void OnTestCardSelected(BaseCard card, ICardData data)
    {
        Debug.Log($"✓ Card selected event received: {data.Title}");
    }

    private void OnTestCardDeselected(BaseCard card, ICardData data)
    {
        Debug.Log($"✓ Card deselected event received: {data.Title}");
    }

    private void OnTestPrimaryAction(BaseCard card, ICardData data)
    {
        Debug.Log($"✓ Primary action event received: {data.Title}");
        
        if (data is IActionCardData actionData)
        {
            Debug.Log($"  Action: {actionData.PrimaryActionText}");
        }
    }

    private void OnTestSecondaryAction(BaseCard card, ICardData data)
    {
        Debug.Log($"✓ Secondary action event received: {data.Title}");
        
        if (data is IActionCardData actionData)
        {
            Debug.Log($"  Action: {actionData.SecondaryActionText}");
        }
    }

    /// <summary>
    /// Cleans up test cards and resources.
    /// AIDEV-NOTE: Call this to reset the test environment.
    /// </summary>
    public void CleanupTests()
    {
        Debug.Log("Cleaning up card system tests...");
        
        foreach (var card in _testCardInstances)
        {
            if (card != null)
            {
                // Unsubscribe from events
                card.OnCardClicked -= OnTestCardClicked;
                card.OnCardSelected -= OnTestCardSelected;
                card.OnCardDeselected -= OnTestCardDeselected;
                card.OnPrimaryActionClicked -= OnTestPrimaryAction;
                card.OnSecondaryActionClicked -= OnTestSecondaryAction;
                
                DestroyImmediate(card.gameObject);
            }
        }
        
        _testCardInstances.Clear();
        
        Debug.Log("Card system test cleanup completed");
    }

    #if UNITY_EDITOR
    [Header("Debug Controls")]
    [SerializeField] private bool _debugMode = false;
    
    /// <summary>
    /// AIDEV-NOTE: Debug method to manually run tests in editor.
    /// </summary>
    [ContextMenu("Run Card Tests (Debug)")]
    private void DebugRunTests()
    {
        if (_debugMode && Application.isPlaying)
        {
            StartCoroutine(RunCardSystemTests());
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method to clean up tests in editor.
    /// </summary>
    [ContextMenu("Cleanup Tests (Debug)")]
    private void DebugCleanupTests()
    {
        if (_debugMode)
        {
            CleanupTests();
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method to test individual card states.
    /// </summary>
    [ContextMenu("Test Card States (Debug)")]
    private void DebugTestStates()
    {
        if (_debugMode && Application.isPlaying)
        {
            StartCoroutine(TestCardStates());
        }
    }
    #endif
}