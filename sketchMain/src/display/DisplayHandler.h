#ifndef DISPLAY_HANDLER_H
#define DISPLAY_HANDLER_H

#include <LedControl.h>
#include "../core/Common.h"

class DisplayHandler {
public:
  void init();
  void clearAll();
  
  // LED Matrix functions
  void updateDisplay(int currentLayerCount, BlockLayer* layers, bool gameOver, int currentPos, int currentWidth);
  void updatePlacedBlockDisplay(int layerIndex, BlockLayer* layers);
  void clearMovingBlock(int currentLayerCount, int currentPos, int currentWidth);
  void displayMovingBlock(int currentLayerCount, int currentPos, int currentWidth);

private:
  LedControl lc = LedControl(DIN_PIN, CLK_PIN, CS_PIN, NUM_MODULES);
};

extern DisplayHandler displayHandler;

#endif
