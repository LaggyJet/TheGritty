#include <Keypad.h>

const byte ROWS = 4;
const byte COLS = 4;

char keys[ROWS][COLS] = {
  {'1', '2', '3', 'A'},
  {'4', '5', '6', 'B'},
  {'7', '8', '9', 'C'},
  {'*', '0', '#', 'D'}
};

byte rowPins[ROWS] = {22, 24, 26, 28};
byte colPins[COLS] = {30, 32, 34, 36};

Keypad keypad = Keypad(makeKeymap(keys), rowPins, colPins, ROWS, COLS);

bool connected = false;
int fails = 0;
long lastHeartbeatTime = 0;
const int MAX_FAILS = 3;
const long MAX_HEARTBEAT_TIME = 3000;

void setup() {
  Serial.begin(9600);
  Serial1.begin(9600);
  Serial.println("Arduino is ready");

  for (int i = 2; i <= 11; i++) {
    pinMode(i, OUTPUT);
    digitalWrite(i, LOW);
  }
}

void loop() {
  CheckHeartbeatState();

  HandleIncomingBluetoothComms();

  HandleKeypadInputs();

  delay(100);
}

void CheckHeartbeatState() {
  long currentMS = millis();
  if (currentMS - lastHeartbeatTime >= MAX_HEARTBEAT_TIME) {
    lastHeartbeatTime = currentMS;
    if (connected) {
      fails++;
      if (fails >= MAX_FAILS)
        Shutdown();
    }
  }
}

void HandleIncomingBluetoothComms() {
  if (Serial1.available() > 0) {
    String message = Serial1.readStringUntil('\n');
    if (message.equals("Init"))
      Startup();
    else if (message.equals("Alive"))
      fails = 0;
    else if (message.startsWith("UpdateBar:"))
      updateLEDBar(message.substring(10).toInt());
  }
}

void HandleKeypadInputs() { 
  char key = keypad.getKey(); 
  if ((key >= '0' && key <= '9') || key == '*' || key == 'A' || key == 'B')
    Serial1.println(key);
}

void Flash() {
  for (int flash = 0; flash < 3; flash++) {
    for (int i = 2; i <= 11; i++)
      digitalWrite(i, LOW);
    delay(300); 

    for (int i = 2; i <= 11; i++)
      digitalWrite(i, HIGH);
    delay(300);
  }
}

void Startup() {
  connected = true;
  if (fails != 0)
    fails = 0;

  for (int i = 2; i <= 11; i++) {
    digitalWrite(i, HIGH);
    delay(200); 
  }

  Flash();
}

void Shutdown() {
  Flash();

  for (int i = 2; i <= 11; i++)
    digitalWrite(i, LOW);

  connected = false;
}

void updateLEDBar(int bars) {
  bars = constrain(bars, 0, 10);
  for (int i = 2; i <= 11; i++)
    digitalWrite(i, (i < 2 + bars));
}