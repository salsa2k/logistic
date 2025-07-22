using UnityEngine;
using System.Collections.Generic;

// AIDEV-NOTE: ScriptableObject for defining city properties and available goods
[CreateAssetMenu(fileName = "New City", menuName = "Logistics Game/City Data")]
public class CityData : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string _cityName;
    [SerializeField, TextArea(2, 4)] private string _description;
    
    [Header("Geographic Data")]
    [SerializeField] private Vector2 _position; // Position on the map in kilometers
    
    [Header("Available Goods")]
    [SerializeField] private List<GoodData> _availableGoods = new List<GoodData>();
    
    [Header("Economic Data")]
    [SerializeField, Range(0.5f, 2.0f)] private float _priceMultiplier = 1.0f; // Price modifier for contracts
    
    // Properties
    public string CityName => _cityName;
    public string Description => _description;
    public Vector2 Position => _position; // In kilometers
    public List<GoodData> AvailableGoods => _availableGoods;
    public float PriceMultiplier => _priceMultiplier;
    
    // Validation
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_cityName))
        {
            _cityName = name;
        }
        
        // Ensure price multiplier is within reasonable bounds
        _priceMultiplier = Mathf.Clamp(_priceMultiplier, 0.1f, 5.0f);
    }
    
    // Helper methods
    public bool HasGood(GoodData good)
    {
        return _availableGoods != null && _availableGoods.Contains(good);
    }
    
    public void AddAvailableGood(GoodData good)
    {
        if (good != null && !HasGood(good))
        {
            _availableGoods.Add(good);
        }
    }
    
    public void RemoveAvailableGood(GoodData good)
    {
        if (_availableGoods != null)
        {
            _availableGoods.Remove(good);
        }
    }
}