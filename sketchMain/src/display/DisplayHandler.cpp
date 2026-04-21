#include "DisplayHandler.h"

DisplayHandler displayHandler;

// ── Countdown digit bitmaps (8-row × 8-col, MSB = leftmost column) ──────────

static const byte DIGIT_3[8] = 
{
  0b00000000,
  0b11111110,
  0b11111110,
  0b10010010,
  0b10000010,
  0b11000110,
  0b11000110,
  0b00000000
};

static const byte DIGIT_2[8] = 
{
  0b00000000,
  0b01100110,
  0b11110110,
  0b10010010,
  0b10010010,
  0b11011110,
  0b01001110,
  0b00000000
};

static const byte DIGIT_1[8] = 
{
  0b00000000,
  0b00000010,
  0b00000010,
  0b11111110,
  0b11111110,
  0b01000010,
  0b00000010,
  0b00000000
};

static const byte DIGIT_G[8] = {
  0b00000000,
  0b00101100,
  0b01101110,
  0b01001010,
  0b01000010,
  0b01111110,
  0b00111100,
  0b00000000 
};

static const byte DIGIT_O[8] = {
  0b00000000,
  0b00111100,
  0b01111110,
  0b01000010,
  0b01000010,
  0b01111110,
  0b00111100,
  0b00000000 
};

static const byte DIGIT_I[8] = {
  0b00000000,
  0b00000000,
  0b00000000,
  0b00000000,
  0b01111010,
  0b00000000,
  0b00000000,
  0b00000000 
};

// ── Private helpers ──────────────────────────────────────────────────────────

void DisplayHandler::showPatternOnChip(int chip, const byte pattern[8]) {
  for (int row = 0; row < 8; row++) {
    lc.setRow(chip, row, pattern[row]);
  }
}

// Flash all four matrices N times for dramatic effect
void DisplayHandler::flashAll(int times, int delayMs) {
  const byte full = 0b11111111;
  for (int t = 0; t < times; t++) {
    for (int i = 0; i < NUM_MODULES; i++)
      for (int r = 0; r < 8; r++)
        lc.setRow(i, r, full);
    delay(delayMs);
    clearAll();
    delay(delayMs);
  }
}

// Show a digit on a specific chip, all other chips blank
void DisplayHandler::showCountdownDigit(int chip, const byte digit[8]) {
  clearAll();
  showPatternOnChip(chip, digit);
}

// ── Public countdown sequence ────────────────────────────────────────────────

void DisplayHandler::playCountdown() {
  // Brief flash to signal something is about to happen
  flashAll(2, 80);
  delay(200);

  showCountdownDigit(3, DIGIT_3);
  delay(900);
  showCountdownDigit(2, DIGIT_2);
  delay(900);
  showCountdownDigit(1, DIGIT_1);
  delay(900);
  clearAll();
  delay(150);

  // ---- G  O  !  each on chip 1 (middle matrix) ----
  // ---- G O ! all at once: each on its own chip ----
  clearAll();
  showPatternOnChip(3, DIGIT_G);
  showPatternOnChip(2, DIGIT_O);
  showPatternOnChip(1, DIGIT_I);
  delay(900);
  
  clearAll();
  delay(100);
}

// ── Win animation (non-blocking) ─────────────────────────────────────────────

// Call this every loop() tick while gameWon is true.
// Flashes "GG" on the top (chip 0) and bottom (chip 3) matrices simultaneously.
void DisplayHandler::tickWinAnimation() {
  static bool    ggVisible       = false;
  static unsigned long lastToggle = 0;
  const unsigned long INTERVAL    = 350;   // ms per blink half-cycle

  if (millis() - lastToggle >= INTERVAL) {
    lastToggle = millis();
    ggVisible  = !ggVisible;

    clearAll();
    if (ggVisible) {
      showPatternOnChip(0, DIGIT_G);
      showPatternOnChip(1, DIGIT_G);
    }
  }
}


void DisplayHandler::init() {
  // Matrices init
  for (int i = 0; i < NUM_MODULES; i++) {
    lc.shutdown(i, false);
    lc.setIntensity(i, 8);
    lc.clearDisplay(i);
  }
}

void DisplayHandler::clearAll() {
  for (int i = 0; i < NUM_MODULES; i++) lc.clearDisplay(i);
}

void DisplayHandler::updateDisplay(int currentLayerCount, BlockLayer* layers, bool gameOver, int currentPos, int currentWidth) {
  clearAll();

  // Draw placed layers
  for (int i = 0; i < currentLayerCount; i++) {
    int startCol = layers[i].startCol;
    int colWidth = layers[i].colWidth;
    for (int colOffset = 0; colOffset < colWidth; colOffset++) {
      int currentCol = startCol + colOffset;
      int module = currentCol / 8;
      int col = 7 - (currentCol % 8);
      if (module >= NUM_MODULES) continue;
      for (int j = 0; j < layers[i].width; j++) {
        int row = layers[i].position + j;
        if (row >= 0 && row < 8) lc.setLed(module, row, col, true);
      }
    }
  }

  // Draw moving block if playing
  if (!gameOver) {
    displayMovingBlock(currentLayerCount, currentPos, currentWidth);
  }
}

void DisplayHandler::updatePlacedBlockDisplay(int layerIndex, BlockLayer* layers) {
  int startCol = layers[layerIndex].startCol;
  int colWidth = layers[layerIndex].colWidth;
  for (int colOffset = 0; colOffset < colWidth; colOffset++) {
    int currentCol = startCol + colOffset;
    int module = currentCol / 8;
    int col = 7 - (currentCol % 8);
    if (module >= NUM_MODULES) continue;

    // clear that column
    for (int row = 0; row < 8; row++) lc.setLed(module, row, col, false);
    // redraw the layer pixels
    for (int j = 0; j < layers[layerIndex].width; j++) {
      int row = layers[layerIndex].position + j;
      if (row >= 0 && row < 8) lc.setLed(module, row, col, true);
    }
  }
}

void DisplayHandler::displayMovingBlock(int currentLayerCount, int currentPos, int currentWidth) {
  int startCol = currentLayerCount * blockColumns;
  for (int colOffset = 0; colOffset < blockColumns; colOffset++) {
    int currentCol = startCol + colOffset;
    int module = currentCol / 8;
    int col = 7 - (currentCol % 8);
    if (module >= NUM_MODULES) continue;
    for (int j = 0; j < currentWidth; j++) {
      int row = currentPos + j;
      if (row >= 0 && row < 8) lc.setLed(module, row, col, true);
    }
  }
}

void DisplayHandler::clearMovingBlock(int currentLayerCount, int currentPos, int currentWidth) {
  int startCol = currentLayerCount * blockColumns;
  for (int colOffset = 0; colOffset < blockColumns; colOffset++) {
    int currentCol = startCol + colOffset;
    int module = currentCol / 8;
    int col = 7 - (currentCol % 8);
    if (module >= NUM_MODULES) continue;
    for (int j = 0; j < currentWidth; j++) {
      int row = currentPos + j;
      if (row >= 0 && row < 8) lc.setLed(module, row, col, false);
    }
  }
}
