# -*- coding: utf-8 -*-
import zmq
import struct
import numpy as np
import cv2
import atexit

def setup_sockets(conn_str, send_str, exit_str):
    # Create sockets
    ctx = zmq.Context()
    recv_sock = ctx.socket(zmq.REP)
    try:
        recv_sock.bind(conn_str)  # Bind socket for receiving from C++
    except zmq.ZMQError as e:
        print(f"Failed to bind port {conn_str}: {e}")
        exit(1)

    exit_sock = ctx.socket(zmq.REP)
    try:
        exit_sock.bind(exit_str)  # Bind socket for receiving from C#
    except zmq.ZMQError as e:
        print(f"Failed to bind port {exit_str}: {e}")
        exit(1)

    send_sock = ctx.socket(zmq.PUSH)
    send_sock.connect(send_str)  # Connect socket for sending

    print("Connected to ports.")

    return ctx, recv_sock, send_sock, exit_sock

def receive_data(recv_sock):
    # Receive data as binary
    parts = recv_sock.recv_multipart()
    command = struct.unpack('i', parts[0])[0]
    byte_rows = parts[1]
    byte_cols = parts[2]
    data = parts[3]

    # Convert byte array to numbers
    rows = struct.unpack('i', byte_rows)[0]
    cols = struct.unpack('i', byte_cols)[0]

    # Reconstruct image data
    image = np.frombuffer(data, dtype=np.uint8).reshape((rows, cols, 3))

    return command, image

def send_image(send_sock, image):
    # Encode image data in JPEG format
    _, buffer = cv2.imencode('.jpg', image)
    jpeg_data = buffer.tobytes()

    # Send image data to C#
    send_sock.send_multipart([jpeg_data])

def receive_flag(exit_sock):
    try:
        command = exit_sock.recv_string(flags=zmq.NOBLOCK)
        if command == "exit":
            return False
    except zmq.Again:
        pass
    return True

def cleanup(ctx, recv_sock, send_sock, exit_sock):
    # Release resources
    print("closing socket")
    recv_sock.close()
    send_sock.close()
    exit_sock.close()
    ctx.term()
    print("soket closed")

def main():
    # Connection addresses
    conn_str = "tcp://*:5555"
    send_str = "tcp://localhost:5556"
    exit_str = "tcp://*:5558"

    ctx, recv_sock, send_sock, exit_sock = setup_sockets(conn_str, send_str, exit_str)
    print("Data receiving setup complete")

    # Register cleanup function to be called on exit
    atexit.register(cleanup, ctx, recv_sock, send_sock, exit_sock)

    is_running = True
    is_sending = False
    
    while is_running:
        try:
            # Receive
            command, image = receive_data(recv_sock)
            
            # Send response to receiving socket
            recv_sock.send(b"OK")

            # Process command
            if command == -1: # exit
                is_running = False
                break

            # Process command
            if command == 1: # start
                is_sending = True
            elif command == 0: # stop
                is_sending = False

            # Send
            if is_sending:
                send_image(send_sock, image)

        except Exception as e:
            print(f"Error: {e}")
            recv_sock.send(b"ERROR")

    print("finish program")

if __name__ == "__main__":
    main()
