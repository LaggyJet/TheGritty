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
  if (Serial1.available() > 0) {
    String message = Serial1.readStringUntil('\n');
    Serial.println(message);
    if (message.equals("Init"))
      Startup();
    if (message.startsWith("UpdateBar:")) {
      int bars = message.substring(10).toInt();
      updateLEDBar(bars);
    }
  }

  delay(100);
}

void Startup() {
  for (int i = 2; i <= 11; i++)
    digitalWrite(i, HIGH);
}

void updateLEDBar(int bars) {
  bars = constrain(bars, 0, 10);
  for (int i = 2; i <= 11; i++)
    digitalWrite(i, (i < 2 + bars));
}
