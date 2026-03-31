# BlockoHolics - Sketch Architecture

This document describes the modular architecture of the `sketchMain` Arduino project.

## Overview

The project has been simplified to remove the LCD and Audio components. The primary focus is now on the LED Matrix game engine:

- **MAIN PIPELINE**: `sketchMain.ino`
- **SOURCE ROOT**: `src/`
  - **CORE**: `Common.h`
  - **DISPLAY**: `DisplayHandler` (LED Matrix only)
  - **GAME**: `GameLogic`

---

## File Structure

### 1. [sketchMain.ino](file:///d:/GitHubRepos/BlockoHolics-source/sketchMain/sketchMain.ino)
The entry point of the application. It initializes the display and game engine.
Includes:
```cpp
#include "src/game/GameLogic.h"
#include "src/display/DisplayHandler.h"
```

### 2. `src/core/Common.h`
Contains shared pin definitions (Button, LED Matrix pins) and game constants.

### 3. `src/display/`
- **DisplayHandler.h / .cpp**: Manages the 4x LED matrices. Wraps the `LedControl` library.

### 4. `src/game/`
- **GameLogic.h / .cpp**: The core engine. Manages movement, collisions, and game state.

---

## How to Compile

1. Ensure you have the following libraries installed:
   - `LedControl`
2. Open `sketchMain.ino` in the Arduino IDE.
3. Select your board and port, then click **Upload**.

---

## Technical Details

- **Minimal Dependencies**: The project now only depends on the standard `LedControl` library.
- **Hardware**: Uses 4 MAX7219 LED matrix modules and 1 pushbutton.
