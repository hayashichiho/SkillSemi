# -*- coding: utf-8 -*-
import zmq
import struct
import numpy as np
import cv2
import atexit
import openface
import os

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
        exit_sock.bind(exit_str)  # Bind socket for receiving exit command from C#
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
    camera_connected = struct.unpack('?', parts[0])[0]
    command = struct.unpack('i', parts[1])[0]
    byte_rows = parts[2]
    byte_cols = parts[3]
    data = parts[4]

    # Convert byte array to numbers
    rows = struct.unpack('i', byte_rows)[0]
    cols = struct.unpack('i', byte_cols)[0]

    # Reconstruct image data
    image = np.frombuffer(data, dtype=np.uint8).reshape((rows, cols, 3))
    image = image.copy()  # Make the image writable by creating a copy

    return camera_connected, command, image

def send_image(send_sock, image, camera_connected):
    # Encode image data in JPEG format
    _, buffer = cv2.imencode('.jpg', image)
    jpeg_data = buffer.tobytes()

    # Send camera_connected state and image data to C#
    send_sock.send_multipart([struct.pack('?', camera_connected), jpeg_data])
    print("Sent image data and camera_connected state")

def receive_flag(exit_sock):
    try:
        command = exit_sock.recv_string(flags=zmq.NOBLOCK)
        if command == "exit":
            return False
    except zmq.Again:
        pass
    return True

def detect_faces(face_aligner, image):
    # Convert image to RGB
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # Detect faces
    detected_faces = face_aligner.getAllFaceBoundingBoxes(image_rgb)

    # Draw bounding boxes on the image
    for rect in detected_faces:
        x, y, w, h = rect.left(), rect.top(), rect.width(), rect.height()
        cv2.rectangle(image, (x, y), (x + w, y + h), (0, 255, 0), 2)

    return image

def cleanup(ctx, recv_sock, send_sock, exit_sock):
    # Release resources
    print("Closing sockets")
    recv_sock.close()
    send_sock.close()
    exit_sock.close()
    ctx.term()
    print("Sockets closed")

def main():
    # Connection addresses
    conn_str = "tcp://*:5555"
    send_str = "tcp://localhost:5556"
    exit_str = "tcp://*:5558"

    ctx, recv_sock, send_sock, exit_sock = setup_sockets(conn_str, send_str, exit_str)
    print("Data receiving setup complete")

    # Register cleanup function to be called on exit
    atexit.register(cleanup, ctx, recv_sock, send_sock, exit_sock)

    # Get the absolute path of the shape predictor file
    predictor_path = os.path.join(os.path.dirname(__file__), "shape_predictor_68_face_landmarks.dat")

    # Initialize OpenFace face aligner
    face_aligner = openface.AlignDlib(predictor_path)

    is_running = True
    is_sending = False
    
    while is_running:
        try:
            # Receive data
            camera_connected, command, image = receive_data(recv_sock)
            
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

            # Detect faces
            if is_sending:
                image = detect_faces(face_aligner, image)
                send_image(send_sock, image, camera_connected)

        except Exception as e:
            print(f"Error: {e}")
            recv_sock.send(b"ERROR")

    print("Program finished")

if __name__ == "__main__":
    main()
