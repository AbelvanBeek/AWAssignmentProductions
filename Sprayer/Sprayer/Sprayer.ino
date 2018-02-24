#include <DallasTemperature.h>
#include <OneWire.h>

#include <NewPing.h>

#include <LiquidCrystal.h>

#include <EEPROM.h>
  
  const int greenLedPin = 0, redLedPin = 1;
  const int forceButtonPin = 2, delayUpButtonPin = 3, menuButtonPin = 4;
  const int motionSensorPin = 5;
  const int distanceSensorTriggerPin = 7, distanceSensorEchoPin = 6;
  const int temperatureSensorPin = 8;
  const int magneticSensorPin = 9;
  const int lightSensorPin = A0;
  const int LCDDigital1 = 12, LCDDigital2 = 13, LCDDigital3 = A5, LCDAnalog1 = 10, LCDAnalog2 = 11;
  const int toiletFreshenerPin = A1;

  const int sprayTime = 850;
  long currentTime;

  const int spraysLeft, defaultSpraysLeft = 2400;
  const unsigned short spraysleftAddress = 0, defaultSpraysAdress = 2;

  enum state {
  NOT_IN_USE,
  IN_USE_UNKNOWN,
  IN_USE_NR1,
  IN_USE_NR2,
  IN_USE_CLEAN,
  SPRAY_ONCE,
  SPRAY_TWICE,
  //SPRAY_WAIT,
  IN_MENU
  };
  enum menu_state {
  SET_DELAY,
  RESET_SPRAYS,
  EXIT
  };

void writeEEPROM(unsigned short address, unsigned short value) {
  int a = value / 256;
  int b = value % 256;
  EEPROM.write(address, a);
  EEPROM.write(address + 1, b);
}
int readEEPROM(unsigned short address){
  int a = EEPROM.read(address);
  int b = EEPROM.read(address+1);
  return a * 256 + b;
}
void decreaseSprays(){
  writeEEPROM(spraysleftAddress, readEEPROM(spraysleftAddress) - 1);
}

void setupPins(){
  //Leds
  pinMode(greenLedPin, OUTPUT);
  pinMode(redLedPin, OUTPUT);
  //Buttons
  pinMode(forceButtonPin, INPUT_PULLUP);
  pinMode(delayUpButtonPin, INPUT_PULLUP);
  pinMode(menuButtonPin, INPUT_PULLUP);
  //Sensors
  pinMode(magneticSensorPin, INPUT_PULLUP);
  pinMode(motionSensorPin, INPUT);
  //LCD
  
  //Freshener
  pinMode(toiletFreshenerPin, OUTPUT);
}

void startSpray(){
  //remember when we started spraying
  currentTime = millis();
  //start spraying
  digitalWrite(toiletFreshenerPin, HIGH)
}
void checkStopSpray(){
  //check if we've sprayed long enough
  if ((millis() - currentTime) > sprayTime)
  {
    //if so, stop spraying
    digitalWrite(toiletFreshenerPin, LOW)
  }
}
void setup() {
  // put your setup code here, to run once:
  //HandleEEPROM
}


void loop() {
  // put your main code here, to run repeatedly:
}
