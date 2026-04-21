#include "src/game/GameLogic.h"
#include "src/display/DisplayHandler.h"
#include "src/display/LCDHandler.h"

void setup() {
  // Initialize Serial for debugging if needed
  Serial.begin(9600);

  // Initialize display components (LED matrices)
  displayHandler.init();

  // Initialize LCD
  lcdHandler.init();

  // Initialize game logic and state
  game.init();
}

void loop() {
  // The main pipeline simply delegates to the game engine
  game.update();
}