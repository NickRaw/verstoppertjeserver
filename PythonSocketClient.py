import socket
import json

HOST = '127.0.0.1'  # The server's hostname or IP address
PORT = 34000        # The port used by the server
dataString = '{"connectionType":#CONNECTIONTYPE,"clientname":#CLIENTNAME #EXTRAVARIABLES}'
loginString = '{"connectionType":"newuser", "clientname":"testsocketname"}'
loggedIn = False

def Login(username, password):
    sendmessage = dataString.replace('#CONNECTIONTYPE','"login"')
    sendmessage = sendmessage.replace('#CLIENTNAME', '"'+username+'"')
    sendmessage = sendmessage.replace('#EXTRAVARIABLES', ',"password":'+'"'+password+'"')
    return sendmessage

def Register(username, password):
    sendmessage = dataString.replace('#CONNECTIONTYPE','"register"')
    sendmessage = sendmessage.replace('#CLIENTNAME', '"'+username+'"')
    sendmessage = sendmessage.replace('#EXTRAVARIABLES', ',"password":'+'"'+password+'"')
    return sendmessage

def CreateRoom(username, loggedIn):
    if loggedIn:
        Print("Creating room")

def SocketConnector():
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect((HOST, PORT))
    sendmessage = Login("user","pass") + '<EOF>'
    s.send(str.encode(sendmessage))
    connected = True
    received_message = False
    while connected:
        data = s.recv(1024)
        
        if '<BOF>' in repr(data):
            recieved_message = False
            message = ''
            
        while not recieved_message:
            message += repr(data)
            if '<EOF>' in message:
                print('Received: ', message)
                recieved_message = True

SocketConnector()
