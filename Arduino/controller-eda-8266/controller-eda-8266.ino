#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

// Set WiFi credentials
#define WIFI_SSID "SensCon"
#define WIFI_PASS "wearedoingresearch."

const int GSR =  A0; //D8
unsigned int UDP_PORT = 5551;
IPAddress SendIP(192,168,0,255);
WiFiUDP UDP;

int sensorValue=0;

long myTime;
float frequencyHz = 250;

void setup() {
  // Setup serial portg
  Serial.begin(9600);
//  pinMode(GSR, OUTPUT);
  Serial.println();
   
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
}
   
void loop() {
  myTime = millis();
  sensorValue=analogRead(GSR);;
  Serial.println(sensorValue);
  String sendString = "EDA;" + String(myTime) + ";" + String(sensorValue);
  // Send return packet
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
