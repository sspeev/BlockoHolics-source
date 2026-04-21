#ifndef LCD_HANDLER_H
#define LCD_HANDLER_H

// ==== LCD wiring ====
// Uses hardware I2C — pins are fixed by the Arduino hardware:
//   SDA = A4,  SCL = A5
// (A6 is analog-input only and cannot be used for I2C)
// Common I2C addresses: 0x27 (PCF8574T) or 0x3F (PCF8574AT)
// Change LCD_I2C_ADDR below if your backpack uses a different address.

#include <LiquidCrystal_I2C.h>

#define LCD_I2C_ADDR  0x27
#define LCD_COLS      16
#define LCD_ROWS      2

class LCDHandler {
public:
  void init();

  void showPressToStart();           // Idle screen
  void showCountingDown();           // Shown while countdown plays
  void showPlaying();                // Blank — nothing shown during gameplay
  void showYouWon();                 // Win screen
  void showYouLost();                // Lose screen

private:
  LiquidCrystal_I2C lcd = LiquidCrystal_I2C(LCD_I2C_ADDR, LCD_COLS, LCD_ROWS);

  // Helper: clear and print two lines (pad with spaces to LCD_COLS)
  void print2(const char* line1, const char* line2);
};

extern LCDHandler lcdHandler;

#endif
