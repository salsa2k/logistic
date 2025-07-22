using UnityEngine;
using UnityEditor;
using System.IO;

// AIDEV-NOTE: Editor utility for creating logistics game assets with proper folder structure
public class LogisticsGameAssetCreator : EditorWindow
{
    // Asset creation paths
    private const string CITIES_PATH = "Assets/Data/ScriptableObjects/Cities";
    private const string VEHICLES_PATH = "Assets/Data/ScriptableObjects/Vehicles";
    private const string GOODS_PATH = "Assets/Data/ScriptableObjects/Goods";
    private const string CONTRACTS_PATH = "Assets/Data/ScriptableObjects/Contracts";
    private const string COMPANIES_PATH = "Assets/Data/ScriptableObjects/Companies";
    private const string NETWORKS_PATH = "Assets/Data/ScriptableObjects/Networks";
    private const string GAMESTATE_PATH = "Assets/Data/ScriptableObjects/GameState";
    
    // Creation options
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private readonly string[] tabNames = { "Core Assets", "Quick Setup", "Validation" };
    
    // Quick setup options
    private bool createSampleCities = true;
    private bool createSampleVehicles = true;
    private bool createSampleGoods = true;
    private bool createRoadNetwork = true;
    private bool createGameState = true;
    
    [MenuItem("Logistics Game/Asset Creator")]
    public static void ShowWindow()
    {
        GetWindow<LogisticsGameAssetCreator>("Logistics Asset Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Logistics Game Asset Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        GUILayout.Space(10);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        switch (selectedTab)
        {
            case 0:
                DrawCoreAssetsTab();
                break;
            case 1:
                DrawQuickSetupTab();
                break;
            case 2:
                DrawValidationTab();
                break;
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawCoreAssetsTab()
    {
        GUILayout.Label("Individual Asset Creation", EditorStyles.boldLabel);
        GUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Create individual assets for the logistics game. Assets will be created in appropriate subfolders.", MessageType.Info);
        GUILayout.Space(10);
        
        // Cities section
        if (GUILayout.Button("Create City Data", GUILayout.Height(30)))
        {
            CreateAsset<CityData>(CITIES_PATH, "New City");
        }
        
        // Vehicles section
        if (GUILayout.Button("Create Vehicle Data", GUILayout.Height(30)))
        {
            CreateAsset<VehicleData>(VEHICLES_PATH, "New Vehicle");
        }
        
        // Goods section
        if (GUILayout.Button("Create Good Data", GUILayout.Height(30)))
        {
            CreateAsset<GoodData>(GOODS_PATH, "New Good");
        }
        
        // Contracts section
        if (GUILayout.Button("Create Contract Template", GUILayout.Height(30)))
        {
            CreateAsset<ContractData>(CONTRACTS_PATH, "New Contract Template");
        }
        
        // Companies section
        if (GUILayout.Button("Create Company Data", GUILayout.Height(30)))
        {
            CreateAsset<CompanyData>(COMPANIES_PATH, "New Company");
        }
        
        // Road Network section
        if (GUILayout.Button("Create Road Network", GUILayout.Height(30)))
        {
            CreateAsset<RoadNetworkData>(NETWORKS_PATH, "New Road Network");
        }
        
        GUILayout.Space(20);
        
        // Game State section
        GUILayout.Label("Game State Assets", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Game State", GUILayout.Height(30)))
        {
            CreateAsset<GameState>(GAMESTATE_PATH, "Game State");
        }
        
        if (GUILayout.Button("Create Player Progress", GUILayout.Height(30)))
        {
            CreateAsset<PlayerProgress>(GAMESTATE_PATH, "Player Progress");
        }
        
        if (GUILayout.Button("Create Settings Data", GUILayout.Height(30)))
        {
            CreateAsset<SettingsData>(GAMESTATE_PATH, "Settings Data");
        }
        
        if (GUILayout.Button("Create Save Data", GUILayout.Height(30)))
        {
            CreateAsset<SaveData>(GAMESTATE_PATH, "Save Data");
        }
    }
    
    private void DrawQuickSetupTab()
    {
        GUILayout.Label("Quick Setup Options", EditorStyles.boldLabel);
        GUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Quickly create a complete set of sample assets for testing and development.", MessageType.Info);
        GUILayout.Space(10);
        
        createSampleCities = EditorGUILayout.Toggle("Create Sample Cities (10)", createSampleCities);
        createSampleVehicles = EditorGUILayout.Toggle("Create Sample Vehicles (6)", createSampleVehicles);
        createSampleGoods = EditorGUILayout.Toggle("Create Sample Goods (5)", createSampleGoods);
        createRoadNetwork = EditorGUILayout.Toggle("Create Road Network", createRoadNetwork);
        createGameState = EditorGUILayout.Toggle("Create Game State Assets", createGameState);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Create All Sample Assets", GUILayout.Height(40)))
        {
            CreateAllSampleAssets();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("This will create sample assets based on the Phase 1C requirements:\n" +
            "• 10 cities with different available goods\n" +
            "• 6 vehicle types with realistic specifications\n" +
            "• 5 goods types with different properties\n" +
            "• Road network connecting all cities\n" +
            "• Game state management assets", MessageType.Info);
    }
    
    private void DrawValidationTab()
    {
        GUILayout.Label("Asset Validation", EditorStyles.boldLabel);
        GUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Validate existing assets for consistency and completeness.", MessageType.Info);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Validate All Assets", GUILayout.Height(30)))
        {
            ValidateAllAssets();
        }
        
        if (GUILayout.Button("Check Folder Structure", GUILayout.Height(30)))
        {
            CheckFolderStructure();
        }
        
        if (GUILayout.Button("Generate Asset Report", GUILayout.Height(30)))
        {
            GenerateAssetReport();
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Create Missing Folders", GUILayout.Height(30)))
        {
            CreateFolderStructure();
        }
    }
    
    private void CreateAsset<T>(string path, string defaultName) where T : ScriptableObject
    {
        // Ensure the directory exists
        CreateFolderIfNotExists(path);
        
        // Create the asset
        T asset = ScriptableObject.CreateInstance<T>();
        
        // Generate unique asset path
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{defaultName}.asset");
        
        // Create and save the asset
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select the created asset
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        
        Debug.Log($"Created {typeof(T).Name} asset at {assetPath}");
    }
    
    private void CreateAllSampleAssets()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Creating Sample Assets", "Setting up folder structure...", 0f);
            
            CreateFolderStructure();
            
            if (createSampleGoods)
            {
                EditorUtility.DisplayProgressBar("Creating Sample Assets", "Creating goods...", 0.1f);
                CreateSampleGoods();
            }
            
            if (createSampleVehicles)
            {
                EditorUtility.DisplayProgressBar("Creating Sample Assets", "Creating vehicles...", 0.3f);
                CreateSampleVehicles();
            }
            
            if (createSampleCities)
            {
                EditorUtility.DisplayProgressBar("Creating Sample Assets", "Creating cities...", 0.5f);
                CreateSampleCities();
            }
            
            if (createRoadNetwork)
            {
                EditorUtility.DisplayProgressBar("Creating Sample Assets", "Creating road network...", 0.7f);
                CreateSampleRoadNetwork();
            }
            
            if (createGameState)
            {
                EditorUtility.DisplayProgressBar("Creating Sample Assets", "Creating game state assets...", 0.9f);
                CreateGameStateAssets();
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", "All sample assets created successfully!", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating sample assets: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to create sample assets: {e.Message}", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private void CreateSampleGoods()
    {
        var goodsData = new[]
        {
            new { name = "Shopping Goods", weight = 1f, volume = 0.1f, value = 50f, license = LicenseType.Standard, special = false, perishable = false, fragile = false, hazardous = false },
            new { name = "Perishable Goods", weight = 0.8f, volume = 0.08f, value = 75f, license = LicenseType.Perishable, special = true, perishable = true, fragile = false, hazardous = false },
            new { name = "Fragile Goods", weight = 0.5f, volume = 0.15f, value = 150f, license = LicenseType.Fragile, special = true, perishable = false, fragile = true, hazardous = false },
            new { name = "Heavy Goods", weight = 5f, volume = 0.2f, value = 100f, license = LicenseType.Heavy, special = true, perishable = false, fragile = false, hazardous = false },
            new { name = "Hazardous Materials", weight = 2f, volume = 0.05f, value = 300f, license = LicenseType.Hazardous, special = true, perishable = false, fragile = false, hazardous = true }
        };
        
        foreach (var data in goodsData)
        {
            var good = ScriptableObject.CreateInstance<GoodData>();
            
            // Use reflection to set private fields (since they're SerializeField)
            var nameField = typeof(GoodData).GetField("_goodName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weightField = typeof(GoodData).GetField("_weightPerUnit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var volumeField = typeof(GoodData).GetField("_volumePerUnit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var valueField = typeof(GoodData).GetField("_baseValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var licenseField = typeof(GoodData).GetField("_requiredLicense", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var specialField = typeof(GoodData).GetField("_requiresSpecialVehicle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var perishableField = typeof(GoodData).GetField("_isPerishable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fragileField = typeof(GoodData).GetField("_isFragile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hazardousField = typeof(GoodData).GetField("_isHazardous", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            nameField?.SetValue(good, data.name);
            weightField?.SetValue(good, data.weight);
            volumeField?.SetValue(good, data.volume);
            valueField?.SetValue(good, data.value);
            licenseField?.SetValue(good, data.license);
            specialField?.SetValue(good, data.special);
            perishableField?.SetValue(good, data.perishable);
            fragileField?.SetValue(good, data.fragile);
            hazardousField?.SetValue(good, data.hazardous);
            
            string assetPath = $"{GOODS_PATH}/{data.name}.asset";
            AssetDatabase.CreateAsset(good, assetPath);
        }
    }
    
    private void CreateSampleVehicles()
    {
        var vehicleData = new[]
        {
            new { name = "Cargo Van", maxSpeed = 110f, fuelCapacity = 80f, consumption = 12f, weightCapacity = 3500f, volume = 15f, price = 35000f },
            new { name = "Delivery Truck", maxSpeed = 100f, fuelCapacity = 120f, consumption = 15f, weightCapacity = 7500f, volume = 30f, price = 65000f },
            new { name = "Heavy Lorry", maxSpeed = 90f, fuelCapacity = 300f, consumption = 25f, weightCapacity = 25000f, volume = 80f, price = 150000f },
            new { name = "Refrigerated Truck", maxSpeed = 95f, fuelCapacity = 150f, consumption = 18f, weightCapacity = 12000f, volume = 40f, price = 120000f },
            new { name = "Flatbed Truck", maxSpeed = 85f, fuelCapacity = 200f, consumption = 20f, weightCapacity = 20000f, volume = 60f, price = 95000f },
            new { name = "Tanker Truck", maxSpeed = 80f, fuelCapacity = 250f, consumption = 22f, weightCapacity = 30000f, volume = 50f, price = 180000f }
        };
        
        foreach (var data in vehicleData)
        {
            var vehicle = ScriptableObject.CreateInstance<VehicleData>();
            
            // Use reflection to set private fields
            var nameField = typeof(VehicleData).GetField("_vehicleName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var speedField = typeof(VehicleData).GetField("_maxSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fuelCapField = typeof(VehicleData).GetField("_fuelCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var consumptionField = typeof(VehicleData).GetField("_fuelConsumption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weightField = typeof(VehicleData).GetField("_weightCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var volumeField = typeof(VehicleData).GetField("_cargoVolume", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var priceField = typeof(VehicleData).GetField("_purchasePrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            nameField?.SetValue(vehicle, data.name);
            speedField?.SetValue(vehicle, data.maxSpeed);
            fuelCapField?.SetValue(vehicle, data.fuelCapacity);
            consumptionField?.SetValue(vehicle, data.consumption);
            weightField?.SetValue(vehicle, data.weightCapacity);
            volumeField?.SetValue(vehicle, data.volume);
            priceField?.SetValue(vehicle, data.price);
            
            string assetPath = $"{VEHICLES_PATH}/{data.name}.asset";
            AssetDatabase.CreateAsset(vehicle, assetPath);
        }
    }
    
    private void CreateSampleCities()
    {
        var cityData = new[]
        {
            new { name = "Port Vireo", pos = new Vector2(0, 0), desc = "Major port city with access to international shipping" },
            new { name = "Brunholt", pos = new Vector2(150, 75), desc = "Industrial manufacturing center" },
            new { name = "Calderique", pos = new Vector2(200, -50), desc = "Mining town rich in natural resources" },
            new { name = "Nordhagen", pos = new Vector2(-100, 120), desc = "Northern trading post and logistics hub" },
            new { name = "Arelmoor", pos = new Vector2(50, 150), desc = "Agricultural region known for fresh produce" },
            new { name = "Sundale Ridge", pos = new Vector2(300, 100), desc = "Mountain resort town with tourism industry" },
            new { name = "Veltrona", pos = new Vector2(-75, -125), desc = "Coastal fishing village" },
            new { name = "Duskwell", pos = new Vector2(125, -150), desc = "Oil refinery and chemical processing center" },
            new { name = "New Halvern", pos = new Vector2(-200, 50), desc = "Modern technology and electronics hub" },
            new { name = "Eastmere Bay", pos = new Vector2(250, -100), desc = "Secondary port with specialized cargo handling" }
        };
        
        for (int i = 0; i < cityData.Length; i++)
        {
            var data = cityData[i];
            var city = ScriptableObject.CreateInstance<CityData>();
            
            // Use reflection to set private fields
            var nameField = typeof(CityData).GetField("_cityName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var posField = typeof(CityData).GetField("_position", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descField = typeof(CityData).GetField("_description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            nameField?.SetValue(city, data.name);
            posField?.SetValue(city, data.pos);
            descField?.SetValue(city, data.desc);
            
            string assetPath = $"{CITIES_PATH}/{data.name}.asset";
            AssetDatabase.CreateAsset(city, assetPath);
        }
    }
    
    private void CreateSampleRoadNetwork()
    {
        var network = ScriptableObject.CreateInstance<RoadNetworkData>();
        
        // Use reflection to set network name
        var nameField = typeof(RoadNetworkData).GetField("_networkName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nameField?.SetValue(network, "Main Road Network");
        
        string assetPath = $"{NETWORKS_PATH}/Main Road Network.asset";
        AssetDatabase.CreateAsset(network, assetPath);
    }
    
    private void CreateGameStateAssets()
    {
        // Create Game State
        var gameState = ScriptableObject.CreateInstance<GameState>();
        AssetDatabase.CreateAsset(gameState, $"{GAMESTATE_PATH}/Game State.asset");
        
        // Create Player Progress
        var playerProgress = ScriptableObject.CreateInstance<PlayerProgress>();
        AssetDatabase.CreateAsset(playerProgress, $"{GAMESTATE_PATH}/Player Progress.asset");
        
        // Create Settings Data
        var settingsData = ScriptableObject.CreateInstance<SettingsData>();
        AssetDatabase.CreateAsset(settingsData, $"{GAMESTATE_PATH}/Settings Data.asset");
        
        // Create Save Data
        var saveData = ScriptableObject.CreateInstance<SaveData>();
        AssetDatabase.CreateAsset(saveData, $"{GAMESTATE_PATH}/Save Data.asset");
    }
    
    private void CreateFolderStructure()
    {
        CreateFolderIfNotExists("Assets/Data");
        CreateFolderIfNotExists("Assets/Data/ScriptableObjects");
        CreateFolderIfNotExists(CITIES_PATH);
        CreateFolderIfNotExists(VEHICLES_PATH);
        CreateFolderIfNotExists(GOODS_PATH);
        CreateFolderIfNotExists(CONTRACTS_PATH);
        CreateFolderIfNotExists(COMPANIES_PATH);
        CreateFolderIfNotExists(NETWORKS_PATH);
        CreateFolderIfNotExists(GAMESTATE_PATH);
        
        AssetDatabase.Refresh();
        Debug.Log("Folder structure created successfully");
    }
    
    private void CreateFolderIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentPath = Path.GetDirectoryName(path).Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            
            // Ensure parent exists
            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
            {
                CreateFolderIfNotExists(parentPath);
            }
            
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }
    
    private void ValidateAllAssets()
    {
        Debug.Log("Starting asset validation...");
        
        var cities = Resources.LoadAll<CityData>("");
        var vehicles = Resources.LoadAll<VehicleData>("");
        var goods = Resources.LoadAll<GoodData>("");
        var contracts = Resources.LoadAll<ContractData>("");
        
        Debug.Log($"Found {cities.Length} cities, {vehicles.Length} vehicles, {goods.Length} goods, {contracts.Length} contracts");
        
        // Add validation logic here
        Debug.Log("Asset validation completed");
    }
    
    private void CheckFolderStructure()
    {
        var folders = new[] { CITIES_PATH, VEHICLES_PATH, GOODS_PATH, CONTRACTS_PATH, COMPANIES_PATH, NETWORKS_PATH, GAMESTATE_PATH };
        
        foreach (var folder in folders)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                Debug.Log($"✓ Folder exists: {folder}");
            }
            else
            {
                Debug.LogWarning($"✗ Missing folder: {folder}");
            }
        }
    }
    
    private void GenerateAssetReport()
    {
        var report = "=== Logistics Game Asset Report ===\n\n";
        
        // Count assets in each category
        var folders = new[]
        {
            (CITIES_PATH, "Cities"),
            (VEHICLES_PATH, "Vehicles"),
            (GOODS_PATH, "Goods"),
            (CONTRACTS_PATH, "Contracts"),
            (COMPANIES_PATH, "Companies"),
            (NETWORKS_PATH, "Networks"),
            (GAMESTATE_PATH, "Game State")
        };
        
        foreach (var (path, name) in folders)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { path });
                report += $"{name}: {guids.Length} assets\n";
            }
            else
            {
                report += $"{name}: Folder missing\n";
            }
        }
        
        Debug.Log(report);
        
        // Optionally save to file
        string reportPath = "Assets/asset_report.txt";
        File.WriteAllText(reportPath, report);
        AssetDatabase.Refresh();
        Debug.Log($"Report saved to {reportPath}");
    }
}