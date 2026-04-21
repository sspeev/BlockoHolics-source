// I2C Scanner — upload this to find your LCD's address.
// Open Serial Monitor at 9600 baud.
#include <Wire.h>

void setup() {
  Wire.begin();
  Serial.begin(9600);
  Serial.println("Scanning I2C bus...");

  byte found = 0;
  for (byte addr = 8; addr < 127; addr++) {
    Wire.beginTransmission(addr);
    if (Wire.endTransmission() == 0) {
      Serial.print("Device found at 0x");
      if (addr < 16) Serial.print("0");
      Serial.println(addr, HEX);
      found++;
    }
  }
  if (found == 0) Serial.println("No I2C devices found. Check wiring.");
  else Serial.println("Done.");
}

void loop() {}
