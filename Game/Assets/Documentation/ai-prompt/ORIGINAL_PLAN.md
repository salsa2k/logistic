## Logistics Game Implementation Plan

I need a plan to implement a logistics game.

## Project Context
The logistics game is a 2D top-down/side scroller vehicle simulation game where players can manage contracts, vehicles, and shopping. Players will complete contracts by transporting goods between cities while managing their vehicle's speed and gas levels.

## Visual Layout of the main menu screen

### Description
The main menu screen, which will have buttons for "New Game", "Load Game", "Settings", "Credits", and "Exit".

### Layout
- The main menu will be a simple layout with buttons for each option.
- The "New Game" button will start a new game session.
- The "Load Game" button will allow the player to load a previously saved game state.
- The "Settings" button will open a settings menu where the player can adjust game settings.
- The "Credits" button will display the game credits.
- The "Exit" button will allow the player to quit the game.
- The main menu will have a background image that represents the game theme.
- The buttons will be arranged in a vertical list, with the "New Game" button at the top and the "Exit" button at the bottom.

## Visual Layout of the main game scene

### Description
The main game scene will have a top menu bar with buttons for "Contracts", "Vehicles", "Shopping", and "Company". It will also display the player's credits and a settings button.

- The left top widget will show the path of the selected vehicle, using phaser.
- The right bottom widget will display a side scroller view of the game world with the vehicle's position using phaser.
- The top-down map will provide a bird's eye view of the game world using phaser.
- All 3 phasers will be 3 different instances of the phaser game engine, each with its own canvas (like views).

## Save/Load Game

### Description
The game will support saving and loading the game state, allowing players to continue their progress later.

### Business Logic
- The game state will include the player's current vehicle, contracts, cities visited.
- The player can save the game at any time, and the game will automatically save when the player completes a contract or purchases a vehicle.
- The player can load the game from the main menu, which will restore the game state to the last saved point.

## New Game

### Description
The new game button will start a new game session, resetting all game state and allowing the player to begin fresh.

### Business Logic
- When the player clicks the "New Game" button, the game will reset all game state, including contracts, vehicles, and cities.
- The game will generate a new set of contracts for the player to start with.
- The player will start with 100.000 credits.
- The player must choose a Company Name.
- The player must choose a logo.
- The logo can be 3 different predefined logos. (Art/UI/Company)
- The player can choose a color for the vehicles.
- The colors can be chosen from a predefined palette.

## Load Game

### Description
The load game button will allow the player to load a previously saved game state.

### Business Logic
- When the player clicks the "Load Game" button, the game will display a list of saved game states.
- The player can select a saved game state to load, which will restore the game state to the point when the game was saved.
- The game will display a confirmation dialog before loading the game state to prevent accidental loading.

## Main menu Settings

### Description
The settings button will allow the player to adjust game settings, such as sound volume, metrical system, and language.

### Layout
- A dropdown menu to select the game language.
- The language options will include English and Brazilian Portuguese.
- A toggle switch to change between metric and imperial units.

### Business Logic
- The language settings should be applied immediately.
- The player can reset the settings to default values at any time.


## Game Credits

### Description
It should display how much credits the player has, which can be used to purchase vehicles and refuel them.

### Layout
- The credits will be displayed in the top right corner of the screen.
- The credits will be represented as a number with a currency symbol (e.g., "C100.000")
- The credits will be updated in real-time as the player earns or spends money.
- The credits color will be yellow to indicate the player's current balance.

## Top Menu Bar

### Description
The top menu bar will contain two buttons: "Contracts" and "Vehicles". These buttons will allow the user to switch between the contracts view and the vehicles view.

### Layout
- The top menu bar will be a box widget that spans the top of the screen.
- The items in the top menu bar will be buttons that can be clicked to switch between views.
- The "Contracts" button will display the contracts view, where the user can see the available contracts and select one to start.
- The "Vehicles" button will display the vehicles view, where the user can see the available vehicles and select one to view its details.
- The "Shopping" button will allow the user to purchase vehicles.

## Left Top Widget

### Description
User starts from a city (A), to the city (B), and on the road between them, there could have a gas station (G) and a police checkpoint (P), or even another city (C).

### Layout
- The top widget will display a side view (straight line) of the path of a selected vehicle. Where (S) is 0% and (E) is 100% of the path.
- When the vehicle is moving a dot representing the vehicle will move along the path horizontally.
- The path will be represented as a straight line with the following elements:
  - S: Start of the path
  - C: City
  - G: Gas Station
  - P: Police Checkpoint
  - E: End of the path

## Right Bottom Widget

### Description
The right bottom widget will display a side scroller view of the game world, showing the vehicle's centered on the screen, with the road, gas station, police checks, and cities image scrolling horizontally as the vehicle moves.

### Layout
- The current speed of the vehicle will be displayed in the top left corner with a small control to increase or decrease the speed.
- The current gas level will be displayed in the top right corner.
- The vehicle sprite will be centered on the screen and will move horizontally as the vehicle moves.
- The road sprite will be a repeating image that scrolls horizontally as the vehicle moves.
- The gas station sprite will be displayed when the vehicle is near a gas station.
- The police checkpoint sprite will be displayed when the vehicle is near a police checkpoint.
- The city sprite will be displayed when the vehicle is near a city.
- The road sprite should be be moving faster or slower depending on the vehicle's speed to give the illusion of movement.
- ETA (Estimated Time of Arrival) will be displayed at the top of the widget, showing the estimated time it will take for the vehicle to reach its destination.
- STATUS will be displayed at the bottom of the widget, showing the current status of the vehicle (e.g., "Moving", "Stopped", "Waiting for Contract Completion").

### Business Logic
- If the vehicle speed is increased, the vehicle will move faster along the road.
- If the vehicle speed is decreased, the vehicle will move slower along the road.

## Top Down map

### Description
The top down map will provide a bird's eye view of the game world, showing the positions of the vehicles and cities.

### Layout
- The map will be displayed in the top right corner of the screen.
- The vehicle will be represented by a dot on the map.
- The vehicle dot will move along the map as the vehicle moves in the game world.
- The cities will be represented by larger dots on the map.
- The map will be scrollable, allowing the player to see areas of the game world that are not currently visible on the main screen.
- If the vehicle dot is clicked, the Left Top Widget and the Right Bottom Widget will be updated to show the vehicle's path and position in the game world.

### Business Logic
- The map should be generated based on the available cities and the locations of the vehicles.
- The map should not show gas stations or police checkpoints, as they are not relevant to the player's navigation.
- The map should update in real-time as the vehicle moves.
- When the vehicle is assigned for a contract, it will follow the road from the origin city to the destination city. Use a pathfinding algorithm to calculate the best route between the cities.
- If the vehicle is selected, show the vehicle's path on the map, and highlight the vehicle dot.

### Goods

### Description
Goods are the items that the player will transport between cities as part of contracts.

### Business Logic
- Goods will have a name, description, icon, and weight.
- The player can only transport goods that fit within the weight capacity of their vehicle.
- All calculations should be made in metric units (kilograms for weight, kilometers for distance and liters for volume).
- This is the list of goods available in the game:
  - Shopping Goods
  - Perishable Goods
  - Fragile Goods
  - Heavy Goods
  - Hazardous Materials

## Contracts

### Description
Contracts are tasks that the player can complete to earn rewards.

### Business Logic
- If the vehicle has arrived at its destination, the city should be flagged as visited.
- Generate 8 random contracts at the start of the game.
- Each contract will have a title, description, cargo size, reward, origin city, and destination city.
- The first rule of generate contracts is that the origin city and destination city must be different.
- The second rule is that the origin city must be a city that the player has visited before.
- The third rule is that contracts should be generated based on the goods available in the origin city.
- If the player is with negative credits, the player will not be able to accept new contracts until the credits are positive again.
- Player cannont accept a contract if they do not have a vehicle in the city where the contract is available.
- All calculations should be made in metric units (kilograms for weight, kilometers for distance and liters for volume).

## Cities

### Description
Cities are the locations in the game world where the player can start and end contracts.

### Business Logic
- Cities will have a name, description, and a list of goods that can be transported to and from them.
- Contracts will be generated based on the goods available in the origin city.
- Connect the cities with roads to allow vehicles to travel between them.
- Spread the cities across the game world to create a realistic map.
- Generate a set of 10 cities with different names, descriptions, and available goods.
  - Port Vireo
  - Brunholt
  - Calderique
  - Nordhagen
  - Arelmoor
  - Sundale Ridge
  - Veltrona
  - Duskwell
  - New Halvern
  - Eastmere Bay

## Vehicles

### Description
Vehicles are the means by which the player will transport goods between cities. Create a set of vehicles with different names, descriptions, speed limits, fuel capacities, weight capacities, icon, and prices.

### Business Logic
- Vehicles will have a name, description, speed limit, current speed, fuel capacity, current fuel level, weight capacity, icon and current weight.
- The player can purchase vehicles from the shopping view.
- Vehicles can be used to complete contracts by transporting goods between cities.
- Weight affects the vehicle's speed and fuel consumption.
- If the vehicle is out of fuel, it will stop moving until the player refuels it.
- Once is refueled, the vehicle will resume its path.
- When the vehicle is out of fuel, a modal dialog will appear asking the player to emergency refuel the vehicle.
- Many vehicles can be at the same city at the same time.
- All calculations should be made in metric units (kilograms for weight, kilometers for distance and liters for volume).
- Create vehicles for different types of goods, such as small, medium, and large vehicles.
- Vehicles available in the game:
  - Van
    - Name: "Cargo Van"
    - Description: "A small van for transporting light goods."
    - Speed Limit: 100 km/h
    - Fuel Capacity: 50 liters
    - Weight Capacity: 1000 kg
    - Icon: CargoVanIcon
    - Price: 20.000 credits
  - Truck
    - Name: "Delivery Truck"
    - Description: "A medium truck for transporting medium goods."
    - Speed Limit: 90 km/h
    - Fuel Capacity: 100 liters
    - Weight Capacity: 2000 kg
    - Icon: DeliveryTruckIcon
    - Price: 40.000 credits
  - Lorry
    - Name: "Heavy Lorry"
    - Description: "A large lorry for transporting heavy goods."
    - Speed Limit: 80 km/h
    - Fuel Capacity: 200 liters
    - Weight Capacity: 5000 kg
    - Icon: HeavyLorryIcon
    - Price: 80.000 credits
  - Refrigerated Truck
    - Name: "Refrigerated Truck"
    - Description: "A truck for transporting perishable goods."
    - Speed Limit: 85 km/h
    - Fuel Capacity: 120 liters
    - Weight Capacity: 3000 kg
    - Icon: RefrigeratedTruckIcon
    - Price: 60.000 credits
  - Flatbed Truck
    - Name: "Flatbed Truck"
    - Description: "A truck for transporting heavy and oversized goods."
    - Speed Limit: 75 km/h
    - Fuel Capacity: 150 liters
    - Weight Capacity: 7000 kg
    - Icon: FlatbedTruckIcon
    - Price: 100.000 credits
  - Tanker Truck
    - Name: "Tanker Truck"
    - Description: "A truck for transporting hazardous materials."
    - Speed Limit: 70 km/h
    - Fuel Capacity: 180 liters
    - Weight Capacity: 6000 kg
    - Icon: TankerTruckIcon
    - Price: 120.000 credits

## Gas Stations

### Description
Gas stations are locations in the game world where the player can refuel their vehicle. There's 3 brands of gas stations, each with a different price per unit of fuel.

### Layout
- Gas stations will be represented as icons on the road in the Right Bottom Widget.
- If the player click on the gast station, it will open a modal dialog showing the price of fuel and a button to refuel the vehicle.

### Business Logic
- Gas stations will have a price per unit of fuel.
- When player select to refuel the vehicle, it will consume credits based on the price per unit of fuel and the amount of fuel needed to fill the vehicle's tank.
- It will ONLY consume the credits when the vehicle passes by the gas station, not when the player check on the refuel checkbox.
- Gas stations will be randomly placed on the road between cities. It could be placed every 1000 units of distance, or every 2000 units of distance, depending on the game settings.
- Be careful to don't overlap with police checkpoints, as they will be placed on the road between cities.
- The cost will be debated from the player's credits when the vehicle passes by the gas station that is CHECKED for refuel.
- If the player does not have enough credits to refuel the vehicle, the player credits should go negative.
- If the vehicle is marked to refuel, it will stop for a few seconds to refuel, and then resume its path.
- The Top Bottom Widget will show the gas station sprite behind the vehicle, to give the illusion the vehicle is parking at the gas station.
- All calculations should be made in metric units (kilograms for weight, kilometers for distance and liters for volume).

## Emeregency Refuel Modal Dialog

### Description
The emergency refuel modal dialog will appear when the vehicle is out of fuel, allowing the player to refuel the vehicle.

### Layout
- The modal dialog will have a title, description, and a button to refuel the vehicle.
- The title will be "Emergency Refuel".
- The description will inform the player that the vehicle is out of fuel and needs to be refueled.
- The button will be labeled "Refuel" and will allow the player to refuel the vehicle using credits.

### Business Logic
- It will cost 20% more of the closest gas station price per unit of fuel.
- It will fill the maximum of 20% of the vehicle's fuel capacity.
- After the player clicks on the "Refuel" button, the vehicle will be refueled and the modal dialog will close and the vehicle will start moving again.
- The cost will be debated from the player's credits when the player clicks on the "Refuel" button.
- If the player does not have enough credits to refuel the vehicle, the player credits should go negative.
- All calculations should be made in metric units (kilograms for weight, kilometers for distance and liters for volume).

## Police Checkpoints

### Description
Police checkpoints are locations in the game world where if the player is speeding, they will be fined.

### Business Logic
- Police checkpoints will be represented as icons on the road in the Right Bottom Widget.
- If the player is speeding when passing by a police checkpoint, a modal dialog will appear informing the player that they have been fined.
- The fine is a predefined amount of credits, which can be adjusted in the game settings.
- Police checkpoints will be randomly placed on the road between cities. It could be placed every 1000 units of distance, or every 2000 units of distance, depending on the game settings.
- Be carefuel to don't overlap with gas stations, as they will be placed on the road between cities.
- If the player does not have enough credits to refuel the vehicle, the player credits should go negative.
- The Top Bottom Widget will show the police checkpoint sprite behind the vehicle, to give the illusion the vehicle was passing by the checkpoint.
- All calculations should be made in metric units (kilograms for weight, kilometers for distance and liters for volume).

## Contracts List View

### Description
The contracts list view will allow the user to see the available contracts and select one to work, cancel or complete it.

### Layout
- The contracts list view will be a scrollable list of contracts.
- Each contract item should have a title, description, cargo size, type of goods, reward, the origin city, and the destination city.
- If the user selects a contract, it should open a contract card with the contract's details.
- It should have a filter to show only the contracts that are not accepted.
- It should have a filter to show only the contracts that are accepted but not completed.
- It should have a filter to show only the contracts that are ready to be completed.

## Contract Card

### Description
The contract card will display the details of a selected contract.

### Layout
- The contract card will be a widget that appears when a contract is selected.
- The contract card should display the contract's title, description, cargo size, type of goods, reward, origin city, and destination city.
- The contract card should also have a button to accept the contract.
- The contract card should have a button to cancel the contract if it is not accepted yet.
- The contract card should have a button to complete the contract if it is accepted and the vehicle has arrived at its destination.

### Business Logic
- If the contract is accepted, it should have a button to complete the contract if the vehicle has arrived at its destination.
- Player cannot accept a contract if they do not have a vehicle in the city where the contract is available.
- Player cannot accepet contracts carrying goods if they do not have the license for that type of goods.

## Vehicles View

### Description
The vehicles view will allow the user to see the owned available vehicles and select one to view its details.

### Layout
- The vehicles view will be a scrollable list of vehicles.
- Each vehicle item should have a name, description, speed limit, current speed, fuel capacity, current fuel level, weight capacity, current weight.
- If the user select a vehicle and the vehicle is not moving, it should open a vehicle widget card with the vehicle's details.
- If the user selects a vehicle and the vehicle is moving, it should open a vehicle widget card with the vehicle's details and load the vehicle's path in the Left Top Widget and the Right Bottom Widget.
- It should have a button to move the vehicle to a different city.

### Business Logic
- The vehicles view will allow the player to see the details of their vehicles and their current status.
- The player can select a vehicle to view its details, and if the vehicle is moving,
  the Left Top Widget and the Right Bottom Widget will be updated to show the vehicle's path and position in the game world.
- On the map, the vehicle will be represented by a dot that moves along the path of the vehicle.
- On the map, the vehicle will be represented by a dot that moves along the path of the vehicle, and if selected, the dot will be highlighted.
- By clicking on the button to move the vehicle, the player will be able to choose a city to move the vehicle to.

## Move Vehicle Modal Dialog

### Description
The move vehicle modal dialog will allow the player to choose a city to move the vehicle to.

### Layout
- The modal dialog will have a title, description, and a list of cities to choose from.
- The title will be "Move Vehicle".
- The description will inform the player that they can choose a city to move the vehicle to.
- The list of cities will be displayed as a scrollable list, with each city having a name and description.
- The player can select a city from the list, and a button to confirm the move will be displayed.

### Business Logic
- When the player selects a city and clicks the "Move" button, the vehicle will be moved to the selected city.
- The vehicle will be moved to the selected city, and the Left Top Widget and Right Bottom Widget will be updated to show the vehicle's new position in the game world.
- The player can only choose a city that they have visited before.
- Vehicles cannot move to the same city they are currently in.
- Moving vehicles will not cost any credits, but it will take time to move the vehicle to the new city.
- Moving a vehicle consumes fuel based on the distance between the current city and the destination city.
- Gas stations and police checkpoint has to be considered when moving the vehicle, as the vehicle will stop at gas stations to refuel and will be fined if speeding at police checkpoints.

## Shopping View

### Description
The shopping view will allow the user to purchase vehicles and licenses for transporting goods.

### Layout
- The shopping view will have 2 buttons to act as tabs: "Vehicles" and "Licenses".
- The "Vehicles" tab will display a list of available vehicles for purchase.
  - The shopping view will be a list of available vehicles for purchase.
  - Each vehicle item should have a name, description, speed, fuel capacity, weight capacity, and price.
  - The user can click on a vehicle to purchase it, the player must choose a city to purchase from.
  - The user can only choose a city that they have visited before.
  - By buying a vehicle, the player will be able to use it in the vehicles view.
- The "Licenses" tab will display a list of available licenses for purchase.
  - The shopping view will also display a list of available licenses for purchase.
  - Each license item should have a name, description, and price.
  - The user can click on a license to purchase it.
  - By buying a license, the player will be able to transport goods of that type.

## Vehicle Card

### Description
The vehicle card will display the details of a selected vehicle.

### Layout
- The vehicle card will be a widget that appears when a vehicle is selected.
- The vehicle card should display the vehicle's name, description, speed limit, current speed, fuel capacity, current fuel level, weight capacity, and current weight.
- It should show a button to show the vehicle path in the Left Top Widget and the Right Bottom Widget if the vehicle is moving.
- It should show the status of the vehicle (moving, stopped or wait to complete the contract).
- It should show a button to complete the contract if the vehicle has a contract and has arrived at its destination.

### Business Logic
- The vehicle card will allow the player to see the details of the vehicle and its current status.
- If the vehicle arrived at its destination, the player can complete the contract.

## Roads

### Description
Roads are the paths that vehicles will take to transport goods between cities.

### Business Logic
- Roads will be auto-generated when a new game is created based on the available cities.
- The system will calculate the path between the origin and destination cities.
- The roads should be represented on the map to show the path of the vehicles.
- Each road can have a speed limit, which will affect the policy checkpoints.
- The min limit of the roads can be 80km/h and the max limit can be 110km/h.
- All calculations should be made in metric units (kilograms for weight, kilometers for distance and liters for volume).
- Road can have curves, intersections, and other features to make the game world more realistic.
- The roads should be represented as a series of points that the vehicle will follow.

## Notifications Snackbar

### Description
The notifications snackbar will display messages to the player, such as contract completion, vehicle refueling, and police fines.

### Layout
- The notifications snackbar will be a small widget that appears at the bottom center of the screen.
- The snackbar will display a message for a few seconds and then disappear.

### Business Logic
- The snackbar will be used to inform the player about important events in the game, such as contract completion, vehicle refueling, and police fines.
- The snackbar will be triggered by events in the game, such as contract completion, vehicle refueling, and police fines.

## Quit Menu

### Description
The quit menu will allow the player to exit the game or return to the main menu.

### Layout
- The quit menu will be a simple icon in the top right corner of the screen.
- When clicked, it will display a confirmation dialog with options to "Exit Game" or "Cancel".

### Business Logic
- If the player clicks "Exit Game", all game state will be saved, and the player will be returned to the main menu.
- If the player clicks "Cancel", the quit menu will close and the player will return to the game.

## Main menu Exit

### Description
The exit game button will allow the player to quit the game by closing the application.

## Main Game Settings

### Description
A modal dialog will allow the player to adjust game settings, such as metrical system, and language.

### Layout
- The settings dialog will have a title, description, and a button to save the settings.
- It should have a dropdown menu to select the game language.
- The language options will include English and Brazilian Portuguese.
- It should have a toggle to switch between metric and imperial systems.

### Business Logic
- The settings dialog will allow the player to adjust the game language.
- The language settings should be applied immediately.
- The player can reset the settings to default values at any time.

##  Main menu Credits

### Description
A scene showing the credits of the game, including the developers, artists, and other contributors.

### Layout
- The credits scene will display a list of names and roles of the contributors.
- A button to return to the main menu.

### Business Logic
- The credits scene will be accessible from the main menu.
- The player can return to the main menu by clicking the "Back" button.

## Company View

### Description
The company view will allow the player to see their company information, such as the company name, logo, and credits.

### Layout
- The company view will display the company name, logo, and credits in a simple layout.
- The company logo and name cannot be changed after the game is started.
- It should have a scrollable list of licenses for goods transportation, such as hazardous materials, fragile goods, and perishable goods.

### Business Logic
- The company view will allow the player to see their company information, such as the company name, logo, and credits.
- The player can view the licenses for goods transportation, which will be required to transport certain types of goods.
- The player can purchase licenses from the shopping view.