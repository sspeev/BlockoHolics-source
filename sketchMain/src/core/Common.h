#ifndef COMMON_H
#define COMMON_H

#include <Arduino.h>

// ==== PIN DEFINITIONS ====
const int buttonPin = 11;    // Button input (active LOW)

// ==== DISPLAY SETTINGS ====
// LedControl: DIN=6, CLK=5, CS=3, 4 modules
const int DIN_PIN = 6;
const int CLK_PIN = 5;
const int CS_PIN = 3;
const int NUM_MODULES = 4;

// ==== GAME CONSTANTS ====
const int blockColumns = 2;  // Each layer uses 2 columns
const int MAX_LAYERS = 32;

// ==== SHARED DATA STRUCTURES ====
struct BlockLayer {
  int position;  // top row
  int width;     // height in rows
  int startCol;  // start column (0..31)
  int colWidth;  // always 2
};

#endif
