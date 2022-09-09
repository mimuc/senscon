#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

#define WIFI_SSID "SensCon"
#define WIFI_PASS "wearedoingresearch."
unsigned int UDP_PORT = 5550;
IPAddress SendIP(192,168,0,255);
WiFiUDP UDP;

int PULSE_SENSOR_PIN = A0; // D5

int Signal;                // Store incoming ADC data. Value can range from 0-1024

long myTime;

float frequencyHz = 250;

void setup() {
  Serial.begin(9600);

//-------------------------------------
  // Begin WiFi
  WiFi.begin(WIFI_SSID, WIFI_PASS);
   
  // Connecting to WiFi...
  Serial.print("Connecting to ");
  Serial.print(WIFI_SSID);
  // Loop continuously while WiFi is not connected
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(1000);
    Serial.print(".");
  }
   
  // Connected to WiFi
  Serial.println();
  Serial.print("Connected! IP address: ");
  Serial.println(WiFi.localIP());

  // Begin listening to UDP port
  UDP.begin(UDP_PORT);
  Serial.print("Listening on UDP port ");
  Serial.println(UDP_PORT);
//--------------------------------------
}

void loop() {
  myTime = millis();
  Signal = analogRead(PULSE_SENSOR_PIN); // Read the sensor value
  //Serial.println(Signal);                // Send the signal value to serial plotter
  String sendString = "PPG2;" + String(myTime) + ";" + String(Signal);
  //Serial.println(sendString);
  UDP.beginPacket(SendIP, UDP_PORT);
  UDP.print(sendString);
  UDP.endPacket();
  float t = 1000.0/frequencyHz - (millis() - myTime);
  //Serial.println(t);
  if (t > 0.0){
    Serial.println("delay " + String(t));
    delay(t);
  }
}
