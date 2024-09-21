import socket
import threading
import time

class udp_parser :
    def __init__(self,ip,port):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        recv_address = (ip,port)
        self.sock.bind(recv_address)
        self.data_size=65535 

        self.raw_data=None
        self.sender=None
        
        thread = threading.Thread(target=self.recv_udp_data)
        thread.daemon = True 
        thread.start() 

    
    def recv_udp_data(self):
        while True :
            self.raw_data, self.sender = self.sock.recvfrom(self.data_size)
   
       
            
    def get_data(self) :
        return self.raw_data

    def __del__(self):
        self.sock.close()
        print('del')



test=udp_parser('192.168.0.22',12345)
while True:
    print(test.get_data(),test.sender)
    time.sleep(1)