import socket
from ssl import SSLSyscallError
import sys

client_ppg = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
client_ppg.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
client_ppg.bind(("", 5006))

# client_eda = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
# client_eda.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
# client_eda.bind(("", 5006))

# [Status, Oxygen, Confidence, Heart Rate]

while True:
    # data_ppg, addr = client_ppg.recvfrom(1024)
    # udp_data_ppg = data_ppg.decode("utf-8")
    # splitted = udp_data_ppg.split(';')
    # splitted = list(map(int, splitted))
    # print(splitted)

    data_eda, addr = client_ppg.recvfrom(1024)
    try:
        udp_data_eda = data_eda.decode("utf-8")
        print(udp_data_eda)
    except:
        print("jo")    