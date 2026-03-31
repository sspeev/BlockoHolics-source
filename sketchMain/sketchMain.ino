#include "src/game/GameLogic.h"
#include "src/display/DisplayHandler.h"

void setup() {
  
  // Initialize display components (Matrices)
  displayHandler.init();

  // Initialize game logic and state
  game.init();
}

void loop() {
  // The main pipeline simply delegates to the game engine
  game.update();
}