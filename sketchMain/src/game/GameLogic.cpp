#include "GameLogic.h"
#include "../core/Messages.h"
#include "../display/LCDHandler.h"

GameLogic game;

void GameLogic::init() {
  pinMode(buttonPin, INPUT_PULLUP);
  Serial.println(MSG_GAME_READY);  // web page: show idle, don't start timer
  waiting = true;
  displayHandler.clearAll();
  lcdHandler.showPressToStart();
}

// Called once when the player presses the button from the idle screen
void GameLogic::startGame() {

  lcdHandler.showCountingDown();
  displayHandler.playCountdown();  // blocking ~4.5 s

  // GameState init
  currentLayerCount = 0;
  currentWidth      = 4;
  currentPos        = -currentWidth;
  moveDelay         = 150;
  direction         = 1;
  buttonPressCount  = 0;
  gameOver          = false;
  gameWon           = false;
  randomSeed(analogRead(0));
  updateMaxPosition();
  Serial.println(MSG_GAME_START);  // send immediately — before any blocking calls
  Serial.flush();                  // ensure it's transmitted before countdown begins
  lcdHandler.showPlaying();
  displayHandler.updateDisplay(currentLayerCount, layers, gameOver, currentPos, currentWidth);
}

void GameLogic::update() {
  // ── Idle / waiting for button press ──────────────────────────────────────
  if (waiting) {
    if (checkButton()) {
      waiting = false;
      startGame();
    }
    return;
  }

  // ── Game-over screen ──────────────────────────────────────────────────────
  if (gameOver) {
    if (gameWon) {
      // Flash "GG" on top and bottom matrices
      displayHandler.tickWinAnimation();
    } else {
      // Blink the placed blocks
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
  Serial.println(MSG_GAME_RESET);  // web page: stop timer, show idle
  gameOver = false;
  gameWon  = false;
  waiting  = true;
  displayHandler.clearAll();
  lcdHandler.showPressToStart();
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

  // No overlap -> game over (lose)
  if (overlapBottom < overlapTop) {
    displayHandler.clearMovingBlock(currentLayerCount, currentPos, currentWidth);
    gameOver = true;
    gameWon  = false;
    lcdHandler.showYouLost();
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
    gameWon  = true;
    lcdHandler.showYouWon();
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
