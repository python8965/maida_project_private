import socket
import time

SERVER_ADDRESS = ('127.0.0.1',12345)
client_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# 텍스트 파일에서 리스트 불러오기
def load_list_from_file(filename):
    with open(filename, 'r') as file:
        return [line.strip() for line in file]

# 리스트에서 값을 하나씩 소켓을 통해 전송
def send_list_over_socket(values):
    for value in values:
        client_socket.sendto(value.encode(), SERVER_ADDRESS)
        time.sleep(0.1)  # 0.1초 대기
    client_socket.sendto("End".encode(), SERVER_ADDRESS)
    # print(f"Sent: {value}")
    time.sleep(2.0)  # 0.1초 대기

# 리스트를 불러오고 전송 실행
filename = 'list_values_yjy_hand_flip.txt'
loaded_list = load_list_from_file(filename)
print("done")
while True:
    send_list_over_socket(loaded_list)
