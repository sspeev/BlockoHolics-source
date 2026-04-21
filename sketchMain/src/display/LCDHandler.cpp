#include "LCDHandler.h"
#include <string.h>

LCDHandler lcdHandler;

// ── Helpers ───────────────────────────────────────────────────────────────────

// Print exactly two lines, left-aligned, padded to LCD_COLS so any old
// content is overwritten without needing a full clear (which can flicker).
void LCDHandler::print2(const char* line1, const char* line2) {
  lcd.clear();

  lcd.setCursor(0, 0);
  lcd.print(line1);

  lcd.setCursor(0, 1);
  lcd.print(line2);
}

// ── Lifecycle ─────────────────────────────────────────────────────────────────

void LCDHandler::init() {
  lcd.init();
  lcd.backlight();
  showPressToStart();
}

// ── Screens ───────────────────────────────────────────────────────────────────

void LCDHandler::showPressToStart() {
  print2("  BlockoHolics  ", " Press to start ");
}

void LCDHandler::showCountingDown() {
  print2("  Get ready...  ", "                ");
}

void LCDHandler::showPlaying() {
  lcd.clear();
  lcd.noBacklight();   // dim the LCD during play — less distraction
}

void LCDHandler::showYouWon() {
  lcd.backlight();
  print2("   You won!  GG ", "Press to restart");
}

void LCDHandler::showYouLost() {
  lcd.backlight();
  print2("   You lost.    ", "Press to restart");
}
