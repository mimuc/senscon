#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

#define WIFI_SSID "HCUM"
#define WIFI_PASS "wearedoingresearch."
unsigned int UDP_PORT = 5005;
IPAddress SendIP(10,163,181,255);
WiFiUDP UDP;

int PULSE_SENSOR_PIN = A0; // D5

int Signal;                // Store incoming ADC data. Value can range from 0-1024

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
  Signal = analogRead(PULSE_SENSOR_PIN); // Read the sensor value
  // Serial.println(Signal);                // Send the signal value to serial plotter
  String sendString = String(millis()) + ";" + String(Signal);
  UDP.beginPacket(SendIP, UDP_PORT);
  UDP.print(sendString);
  UDP.endPacket();
}
