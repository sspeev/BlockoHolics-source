#include "GameLogic.h"
#include "../core/Messages.h"

GameLogic game;

void GameLogic::init() {
  pinMode(buttonPin, INPUT_PULLUP);
  Serial.println(MSG_GAME_START);
  
  // GameState init
  currentLayerCount = 0;
  currentWidth = 4;
  currentPos = -currentWidth;
  randomSeed(analogRead(0));
  updateMaxPosition();
  
  displayHandler.updateDisplay(currentLayerCount, layers, gameOver, currentPos, currentWidth);
}

void GameLogic::update() {
  if (gameOver) {
    // Blink matrices
    static bool blinkState = false;
    static unsigned long lastBlinkTime = 0;
    if (millis() - lastBlinkTime > 500) {
      lastBlinkTime = millis();
      blinkState = !blinkState;
      if (blinkState) {
        displayHandler.updateDisplay(currentLayerCount, layers, gameOver, currentPos, currentWidth);
      } else {
        displayHandler.clearAll();
      }
    }
    
    if (checkButton()) {
      reset();
    }
    return;
  }

  // Motion
  unsigned long currentTime = millis();
  if (currentTime - lastMoveTime > moveDelay) {
    lastMoveTime = currentTime;
    displayHandler.clearMovingBlock(currentLayerCount, currentPos, currentWidth);
    currentPos += direction;

    // Reflect at bounds
    if (currentPos < -currentWidth) {
      int overshoot = (-currentWidth) - currentPos;
      currentPos = -currentWidth + overshoot;
      direction = -direction;
    } else if (currentPos > maxPosition) {
      int overshoot = currentPos - maxPosition;
      currentPos = maxPosition - overshoot - (currentWidth - 1);
      direction = -direction;
    }
    displayHandler.displayMovingBlock(currentLayerCount, currentPos, currentWidth);
  }

  if (checkButton()) {
    handleButtonPress();
  }
}

void GameLogic::reset() {
  Serial.println(MSG_GAME_RESET);
  gameOver = false;
  gameWon = false;
  currentLayerCount = 0;
  currentWidth = 4;
  currentPos = -currentWidth;
  moveDelay = 150;
  direction = 1;
  buttonPressCount = 0;
  updateMaxPosition();
  displayHandler.updateDisplay(currentLayerCount, layers, gameOver, currentPos, currentWidth);
}

void GameLogic::handleButtonPress() {
  placeBlock();
}

void GameLogic::placeBlock() {
  buttonPressCount++;
  if (buttonPressCount == 4) moveDelay = 120;
  else if (buttonPressCount == 8) moveDelay = 90;
  else if (buttonPressCount == 12) moveDelay = 60;

  // First layer: no previous to compare
  if (currentLayerCount == 0) {
    layers[0].position = currentPos;
    layers[0].width = currentWidth;
    layers[0].startCol = 0;
    layers[0].colWidth = blockColumns;
    currentLayerCount = 1;

    updateMaxPosition();
    currentPos = random(-currentWidth, maxPosition + 1);

    displayHandler.updatePlacedBlockDisplay(0, layers);
    return;
  }

  // Overlap with previous layer
  int prevPos = layers[currentLayerCount - 1].position;
  int prevWidth = layers[currentLayerCount - 1].width;
  int overlapTop = max(prevPos, currentPos);
  int overlapBottom = min(prevPos + prevWidth - 1, currentPos + currentWidth - 1);

  // No overlap -> game over
  if (overlapBottom < overlapTop) {
    displayHandler.clearMovingBlock(currentLayerCount, currentPos, currentWidth);
    gameOver = true;
    gameWon = false;
    Serial.println(MSG_GAME_OVER);
    return;
  }

  // Save overlap as new layer
  layers[currentLayerCount].position = overlapTop;
  layers[currentLayerCount].width = overlapBottom - overlapTop + 1;
  layers[currentLayerCount].startCol = currentLayerCount * blockColumns;
  layers[currentLayerCount].colWidth = blockColumns;

  // Update moving block height and count
  currentWidth = overlapBottom - overlapTop + 1;
  currentLayerCount++;

  // Win condition: 16 layers (32 columns)
  int totalUsedCols = currentLayerCount * blockColumns;
  if (totalUsedCols >= 32) {
    displayHandler.updatePlacedBlockDisplay(currentLayerCount - 1, layers);
    gameOver = true;
    gameWon = true;
    Serial.println(MSG_GAME_WIN);
    return;
  }

  updateMaxPosition();
  currentPos = random(-currentWidth, maxPosition + 1);

  displayHandler.updatePlacedBlockDisplay(currentLayerCount - 1, layers);
}

void GameLogic::updateMaxPosition() {
  maxPosition = 7 + currentWidth;
}

bool GameLogic::checkButton() {
  if (digitalRead(buttonPin) == LOW) {
    delay(20);
    if (digitalRead(buttonPin) == LOW) {
      while (digitalRead(buttonPin) == LOW) {}  // wait release
      return true;
    }
  }
  return false;
}
