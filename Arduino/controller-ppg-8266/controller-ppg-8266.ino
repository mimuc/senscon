/*
  This example sketch gives you exactly what the SparkFun Pulse Oximiter and
  Heart Rate Monitor is designed to do: read heart rate and blood oxygen levels.
  This board requires I-squared-C connections but also connections to the reset
  and mfio pins. When using the device keep LIGHT and CONSISTENT pressure on the
  sensor. Otherwise you may crush the capillaries in your finger which results
  in bad or no results. A summary of the hardware connections are as follows:
  SDA -> SDA
  SCL -> SCL
  RESET -> PIN 4
  MFIO -> PIN 5

  Author: Elias Santistevan
  Date: 8/2019
  SparkFun Electronics

  If you run into an error code check the following table to help diagnose your
  problem:
  1 = Unavailable Command
  2 = Unavailable Function
  3 = Data Format Error
  4 = Input Value Error
  5 = Try Again
  255 = Error Unknown
*/

#include <SparkFun_Bio_Sensor_Hub_Library.h>
#include <Wire.h>
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

#define WIFI_SSID "HCUM"
#define WIFI_PASS "wearedoingresearch."
unsigned int UDP_PORT = 5005;
IPAddress SendIP(10,163,181,255);
WiFiUDP UDP;

// Reset pin, MFIO pin
int resPin = 2; // D4
int mfioPin = 14; // D5

// Takes address, reset pin, and MFIO pin.
SparkFun_Bio_Sensor_Hub bioHub(resPin, mfioPin);

bioData body;
// ^^^^^^^^^
// What's this!? This is a type (like int, byte, long) unique to the SparkFun
// Pulse Oximeter and Heart Rate Monitor. Unlike those other types it holds
// specific information on your heartrate and blood oxygen levels. BioData is
// actually a specific kind of type, known as a "struct".
// You can choose another variable name other than "body", like "blood", or
// "readings", but I chose "body". Using this "body" varible in the
// following way gives us access to the following data:
// body.heartrate  - Heartrate
// body.confidence - Confidence in the heartrate value
// body.oxygen     - Blood oxygen level
// body.status     - Has a finger been sensed?


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

  Wire.begin();
  int result = bioHub.begin();
  if (result == 0) // Zero errors!
    Serial.println("Sensor started!");
  else
    Serial.println("Could not communicate with the sensor!!!");

  Serial.println("Configuring Sensor....");
  int error = bioHub.configBpm(MODE_ONE); // Configuring just the BPM settings.
  if (error == 0) { // Zero errors!
    Serial.println("Sensor configured.");
  }
  else {
    Serial.println("Error configuring sensor.");
    Serial.print("Error: ");
    Serial.println(error);
  }

  // Data lags a bit behind the sensor, if you're finger is on the sensor when
  // it's being configured this delay will give some time for the data to catch
  // up.
  Serial.println("Loading up the buffer with data....");
  delay(4000);

}

void loop() {

  // Information from the readBpm function will be saved to our "body"
  // variable.
  body = bioHub.readBpm();
  Serial.print("Heartrate: ");
  Serial.println(body.heartRate);
  Serial.print("Confidence: ");
  Serial.println(body.confidence);
  Serial.print("Oxygen: ");
  Serial.println(body.oxygen);
  Serial.print("Status: ");
  Serial.println(body.status);
  // Slow it down or your heart rate will go up trying to keep up
  // with the flow of numbers

  String sendString = String(body.status) + ";" + String(body.oxygen) + ";" + String(body.confidence) + ";" + String(body.heartRate);
//  String sendString = "Body.Status:" + String(body.status);
//  sendString = "Body.Oxygen:" + String(body.oxygen);
//  sendString = "Body.Confidence:" + String(body.confidence);
//  sendString = "Body.HeartRate:" + String(body.heartRate);
  UDP.beginPacket(SendIP, UDP_PORT);
  UDP.print(sendString);
  UDP.endPacket();
}
