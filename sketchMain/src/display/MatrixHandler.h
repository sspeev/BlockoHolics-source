#ifndef DISPLAY_HANDLER_H
#define DISPLAY_HANDLER_H

#include <LedControl.h>
#include "../core/Common.h"

class DisplayHandler {
public:
  void init();
  void clearAll();
  void playCountdown();        // 3-2-1-GO animation, runs before game starts
  void tickWinAnimation();     // non-blocking GG flash, call every loop() tick after winning

  // LED Matrix functions
  void updateDisplay(int currentLayerCount, BlockLayer* layers, bool gameOver, int currentPos, int currentWidth);
  void updatePlacedBlockDisplay(int layerIndex, BlockLayer* layers);
  void clearMovingBlock(int currentLayerCount, int currentPos, int currentWidth);
  void displayMovingBlock(int currentLayerCount, int currentPos, int currentWidth);

private:
  void showPatternOnChip(int chip, const byte pattern[8]);
  void flashAll(int times, int delayMs);
  void showCountdownDigit(int chip, const byte digit[8]);

  LedControl lc = LedControl(DIN_PIN, CLK_PIN, CS_PIN, NUM_MODULES);
};

extern DisplayHandler displayHandler;

#endif
