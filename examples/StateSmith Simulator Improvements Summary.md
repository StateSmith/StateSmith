# StateSmith Web Simulator Improvements Summary

*This document is translated from Chinese by LLM.*

## Problem Description

In the original StateSmith Web simulator, the Events area in the right sidebar displayed all possible events, which caused the following issues:
1. When there are too many events, users have difficulty selecting the correct event
2. Transitioning from one state to another typically uses only a small number of specific events
3. Irrelevant events interfere with the user's operational experience

## Solution

Implemented dynamic event display functionality based on the current state, with irrelevant events being faded to make available events more prominent.

Added "Hide Unused" checkbox functionality, allowing users to choose to completely hide irrelevant events for a cleaner interface.

## Specific Modifications

### 1. SimWebGenerator.cs Modifications

#### 1.1 Added State-Event Mapping Data Structure
```csharp
/// <summary>
/// Mapping from states to available events
/// Key is the state name, value is the set of event names that the state can handle
/// </summary>
Dictionary<string, HashSet<string>> stateToAvailableEvents = new(StringComparer.OrdinalIgnoreCase);
```

#### 1.2 Modified CollectDiagramNames Method
Enhanced this method to collect available events for each state:
- Collect events directly defined in the state
- Collect events inherited from parent states
- Establish mapping relationship from state names to available events

#### 1.3 Added OrganizeStateEventsIntoJsObject Method
```csharp
/// <summary>
/// Convert the mapping from states to available events into JavaScript object format
/// </summary>
/// <returns>JavaScript object format string</returns>
private string OrganizeStateEventsIntoJsObject()
```

#### 1.4 Modified Generate Method
- Generate JavaScript object for state-event mapping
- Pass the mapping to HTML renderer

### 2. HtmlRenderer.cs Modifications

#### 2.1 Modified Render Method Signature
Added `stateEventsMapping` parameter:
```csharp
public static void Render(StringBuilder stringBuilder, string smName, string mocksCode, 
    string mermaidCode, string jsCode, string diagramEventNamesArray, string stateEventsMapping)
```

#### 2.2 Added CSS Styles
Added visual styles for enabled/disabled states of event buttons:
```css
button.event-button {
    transition: opacity 0.3s ease, background-color 0.3s ease;
}

button.event-button.disabled {
    opacity: 0.4;
    background-color: #f0f0f0;
    color: #999;
    cursor: not-allowed;
}

button.event-button.enabled {
    opacity: 1;
    background-color: #007bff;
    color: white;
    cursor: pointer;
}

button.event-button.enabled:hover {
    background-color: #0056b3;
}

/* Styles for hiding irrelevant events */
button.event-button.hidden {
    display: none;
}
```

#### 2.3 Added JavaScript Functionality

##### State-Event Mapping Variable
```javascript
// Mapping from states to available events
const stateEventsMapping = {{stateEventsMapping}};
```

##### updateEventButtonStates Function
```javascript
// Function to update event button states
function updateEventButtonStates(currentStateName) {
    const availableEvents = stateEventsMapping[currentStateName] || [];
    const hideIrrelevantEvents = document.getElementById('hideIrrelevantEvents').checked;
    
    diagramEventNamesArray.forEach(eventName => {
        const button = document.getElementById('button_' + eventName);
        if (button) {
            // do event always remains enabled as it is the core event for state machine operation
            const isDoEvent = eventName.toLowerCase() === 'do';
            const isAvailable = isDoEvent || availableEvents.includes(eventName);
            
            // Clear all state classes
            button.classList.remove('enabled', 'disabled', 'hidden');
            
            if (hideIrrelevantEvents && !isAvailable) {
                // If hide irrelevant events is checked and the event is not available, hide the button
                button.classList.add('hidden');
                button.disabled = true;
            } else {
                // Otherwise show the button and set appropriate state
                button.classList.add(isAvailable ? 'enabled' : 'disabled');
                button.disabled = !isAvailable;
            }
        }
    });
}
```

##### New Settings Option
Added a second checkbox in the settings dropdown menu:
```html
<div class='dropdown-item'>
  <input type='checkbox' id='hideIrrelevantEvents' name='hideIrrelevantEvents' value='Hide Unused'>
  <label for='hideIrrelevantEvents'>Hide Unused</label>
</div>
```

##### New Event Listener
```javascript
// Set up state and event listener for hide irrelevant events checkbox
document.getElementById('hideIrrelevantEvents').addEventListener('change', function() {
  // When checkbox state changes, update event button display for current state
  const currentStateName = {{smName}}.stateIdToString(sm.stateId);
  updateEventButtonStates(currentStateName);
});
```

##### Modified Event Button Creation
- Added `event-button` CSS class to event buttons
- Added click protection for disabled state

##### Modified State Entry Callback
Added event button state update in `enterState` callback:
```javascript
enterState: (mermaidName) => {
    // ... existing code ...
    
    // Update event button states
    updateEventButtonStates(mermaidName);
},
```

##### Added Initialization Code
```javascript
// Initialize event button states
const initialStateName = {{smName}}.stateIdToString(sm.stateId);
updateEventButtonStates(initialStateName);
```

## Features

### 1. Smart Event Filtering
- Dynamically display available events based on current state
- Irrelevant events are faded (reduced opacity, grayed out)
- Available events are highlighted (blue background, fully opaque)

### 2. New: Hide Irrelevant Events Functionality
- **Hide Unused checkbox**: Users can choose to completely hide irrelevant events
- **Two display modes**:
  - Default mode (checkbox unchecked): Irrelevant events are faded
  - Clean mode (checkbox checked): Irrelevant events are completely hidden
- **Real-time switching**: Users can switch between the two modes at any time
- **Clean interface**: Significantly reduces visual interference in complex state machines

### 3. Special Handling for do Events
- `do` events always remain enabled

### 4. Interaction Protection
- Disabled buttons cannot be clicked

## Testing and Validation

Tested and validated using `test-misc/c99-test/electromagnet.puml`:
- Successfully generated simulator with state-event mapping
- Event buttons correctly update based on current state
- do events always remain available
- Hide Unused checkbox functionality works properly
- Both display modes switch normally
- Visual effects meet expectations

## Usage

Use CLI tool to generate simulator:
```bash
cd src
dotnet run --project StateSmith.Cli --framework net9.0 -- run your_state_machine.puml
```

The generated `.sim.html` file will include the new smart event filtering functionality.

### Operating Instructions
1. Open the generated simulator HTML file
2. Click the "settings" icon in the upper right corner
3. You can see two checkbox options:
   - **Timestamps**: Controls whether to display timestamps
   - **Hide Unused**: Controls whether to hide irrelevant events
4. Check the "Hide Unused" checkbox to completely hide events that are not available in the current state 