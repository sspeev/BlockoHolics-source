#ifndef GAME_LOGIC_H
#define GAME_LOGIC_H

#include "../core/Common.h"
#include "../display/DisplayHandler.h"

class GameLogic {
public:
  void init();
  void update();
  void reset();
  void handleButtonPress();

  // Public state for communication with Main Pipeling (.ino)
  bool isGameOver() const { return gameOver; }

private:
  void placeBlock();
  void updateMaxPosition();
  bool checkButton();

  int currentWidth = 4;
  int currentPos = -4;   
  int direction = 1;     
  int moveDelay = 150;   
  bool gameOver = false;
  bool gameWon = false;
  unsigned long lastMoveTime = 0;
  int maxPosition = 0;  
  int buttonPressCount = 0;
  int currentLayerCount = 0; 

  BlockLayer layers[MAX_LAYERS];
};

extern GameLogic game;

#endif
