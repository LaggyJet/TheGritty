void setup() {
  Serial.begin(9600);
  Serial1.begin(9600);
  Serial.println("Arduino is ready");
}

void loop() {
  if (Serial1.available() > 0) {
    String message = Serial1.readStringUntil('\n');
    Serial.println(message);
  }

  delay(100);
}
