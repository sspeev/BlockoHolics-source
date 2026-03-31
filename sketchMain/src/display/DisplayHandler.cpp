#include "DisplayHandler.h"

DisplayHandler displayHandler;

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
