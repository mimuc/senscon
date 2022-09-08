import socket
from pylsl import StreamInfo, StreamOutlet
from collections import deque
from threading import Thread

UDP_IP = "10.163.181.255"
UDP_PORT_PPG = 5005
UDP_PORT_EDA = 5006

body_status = 0
body_oxygen = 0
body_confidence = 0
body_heartRate = 0

class ConnectionInfo:
    def __init__(self, streamInfo, udpIP, udpPort, numberOfChannels, samplingrate, identifier, timeout=None):
        self.streamInfo = streamInfo
        self.udpIP = udpIP
        self.udpPort = udpPort
        self.numberOfChannels = numberOfChannels
        self.estimatedSamplingRate = samplingrate
        self.identifier = identifier
        self.timeout = timeout

class ReadThread(Thread):
    '''
    Background thread that is active during live view. Receives data from the hardware prototype and populates a ringbuffer. LiveviewTab reads this buffer when active.
    '''
    def __init__(self, connectionInfo, useLsl=None):
        Thread.__init__(self)
        self.running=False
        self.connectionInfo = connectionInfo
        self.client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.client.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, True)

        self.client_eda = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.client_eda.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, True)

        if (self.connectionInfo.timeout != None):
            self.client.settimeout(self.connectionInfo.timeout)
        self.client.bind(("", self.connectionInfo.udpPort))

        if (self.connectionInfo.timeout != None):
            self.client_eda.settimeout(self.connectionInfo.timeout)
        self.client_eda.bind(("", UDP_PORT_EDA))

        self.ringBuffer = [deque(int(self.connectionInfo.estimatedSamplingRate*10) * [0]) for i in range(0, self.connectionInfo.numberOfChannels)]

        if (useLsl != None):
            info = StreamInfo(connectionInfo.streamInfo, 'Markers', connectionInfo.numberOfChannels, connectionInfo.estimatedSamplingRate, 'int32', connectionInfo.identifier)
            self.outlet = StreamOutlet(info)

    def run(self):
        self.running = True
        while self.running:
            try:
                self.receiveData()
            except ValueError:
                print("TransmissionError")
            except socket.timeout:
                self.running = False
        self.client.close()
        # self.client_eda.close()

    def receiveData(self):
        data, addr = self.client.recvfrom(1024)
        splitted = []
        try:
            # PPG
            # [0]: PPG2 Identifier
            # [1]: Time since start in milliseconds
            # [2]: PPG Value
            splitted = data.decode("utf-8").split(';')
            if splitted[0] == "PPG2":
                splitted = [int(splitted[-1])]
                print("PPG: " + str(splitted[0]))
            else:
                # EDA
                splitted = list(map(int, splitted))
                print("EDA: " + str(splitted[0]))
                
        except:
            # TODO: What is this line doing? Is this used? Is this an old conversion?
            splitted = list(data)
            splitted = [int(((1024+2*splitted[0])*10000)/(512-splitted[0]))]

        self.outlet.push_sample(splitted)

    def addSample(self, sample, data):
        data.pop()
        data.appendleft(sample)
        self.outlet.push_sample(sample)


# # TODO: Check sample rate!
connectionInfo_ppg = ConnectionInfo("PPG", UDP_IP, UDP_PORT_PPG, 1, 100, "PPG-0")
readThread_ppg = ReadThread(connectionInfo_ppg, True)
readThread_ppg.start()

# # TODO: Check sample rate!
connectionInfo_eda = ConnectionInfo("EDA", UDP_IP, UDP_PORT_EDA, 1, 250, "EDA-0")
readThread_eda = ReadThread(connectionInfo_eda, True)
readThread_eda.start()
