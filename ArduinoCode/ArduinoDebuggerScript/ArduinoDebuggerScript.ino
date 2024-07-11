void setup() {
  Serial.begin(9600);
  Serial1.begin(9600);
  Serial.println("Arduino is ready");
}

void loop() {
  if (Serial1.available() > 0) {
    String message = Serial1.readStringUntil('\n');
    Serial.print("Received from HC-06: ");
    Serial.println(message);
    
    Serial1.print("Echo from Arduino: ");
    Serial1.println(message);
  }

  if (Serial.available() > 0) {
    String debugMessage = Serial.readStringUntil('\n');
    Serial.print("Debug message: ");
    Serial.println(debugMessage);
  }

  delay(100);
}
