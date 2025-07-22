using UnityEngine;

namespace LogisticGame.Test
{
    // AIDEV-NOTE: Simple test to verify LocalizationManager can be referenced
    public class LocalizationTest : MonoBehaviour
    {
        private void Start()
        {
            // Test if LocalizationManager can be accessed
            var locManager = LogisticGame.Managers.LocalizationManager.Instance;
            
            if (locManager != null)
            {
                Debug.Log("LocalizationManager is accessible!");
            }
            else
            {
                Debug.LogWarning("LocalizationManager is not available");
            }
        }
    }
}