#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

// Set WiFi credentials
#define WIFI_SSID "HCUM"
#define WIFI_PASS "wearedoingresearch."

const int GSR =  A0; //D8
unsigned int UDP_PORT = 5006;
IPAddress SendIP(10,163,181,255);
WiFiUDP UDP;

int sensorValue=0;
int gsr_average=0;

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
  long sum=0;
  sensorValue=analogRead(GSR);
  String sensorValueString = String(sensorValue);
  Serial.println(sensorValueString);

  // Send return packet
  UDP.beginPacket(SendIP, UDP_PORT);
  UDP.print(sensorValueString);
  UDP.endPacket();

}
