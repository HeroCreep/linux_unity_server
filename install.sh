#!/bin/bash

# Check if script is run as root
if [ "$(id -u)" -ne 0 ]; then
  echo "Please run as root"
  exit 1
fi

# Prompt user for server password
read -p "Enter the server password: " server_password

# Prompt user for server port
read -p "Enter the server port [default: 12345]: " server_port
server_port=${server_port:-12345}

# Detect machine's IP address
ip_address=$(hostname -I | cut -f1 -d' ')

# Detect machine's hostname
hostname=$(hostname)

# Install necessary dependencies
apt-get update
apt-get install -y python3 python3-pip

# Install required Python packages
pip3 install --upgrade pip
pip3 install pickle5  # Used for Python 3.7 compatibility

# Create a directory for the server
mkdir /opt/player_server
cd /opt/player_server

# Create the Python server script
cat <<EOL > server.py
import socket
import pickle
import os

# Detect the machine's IP address and hostname
ip_address = socket.gethostbyname(socket.gethostname())
hostname = socket.gethostname()

# Define the server address and port
server_address = (ip_address, $server_port)

# Password for server access
server_password = "$server_password"

# Create a folder to store data files
data_folder = "player_data"
os.makedirs(data_folder, exist_ok=True)

def handle_data(client_socket):
    while True:
        data = client_socket.recv(1024)
        if not data:
            break

        received_data = pickle.loads(data)
        process_data(received_data)

def process_data(data):
    if "password" in data and data["password"] == server_password:
        command = data["command"]

        if command == "store":
            name = data["name"]
            value = data["value"]
            store_data(name, value)

        elif command == "read":
            name = data["name"]
            send_data(read_data(name))

        elif command == "remove":
            name = data["name"]
            remove_data(name)

        elif command == "update":
            name = data["name"]
            new_value = data["new_value"]
            update_data(name, new_value)

def store_data(name, value):
    with open(f"{data_folder}/{name}.pickle", "wb") as file:
        pickle.dump(value, file)

def read_data(name):
    try:
        with open(f"{data_folder}/{name}.pickle", "rb") as file:
            return pickle.load(file)
    except FileNotFoundError:
        return None

def remove_data(name):
    file_path = f"{data_folder}/{name}.pickle"
    if os.path.exists(file_path):
        os.remove(file_path)

def update_data(name, new_value):
    if read_data(name) is not None:
        store_data(name, new_value)

def send_data(data):
    client_socket.send(pickle.dumps(data))


def main():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(server_address)
    server_socket.listen(1)
    print(f"Server listening on {server_address}")

    client_socket, client_address = server_socket.accept()
    print(f"Connection from {client_address}")

    handle_data(client_socket)

    client_socket.close()
    server_socket.close()


if __name__ == "__main__":
    main()
EOL

# Create a systemd service file
cat <<EOL > /etc/systemd/system/player_server.service
[Unit]
Description=Player Server

[Service]
ExecStart=/usr/bin/python3 /opt/player_server/server.py
Restart=always
User=nobody
Group=nogroup
Environment=PYTHONUNBUFFERED=1

[Install]
WantedBy=multi-user.target
EOL

# Enable and start the service
systemctl enable player_server.service
systemctl start player_server.service

echo "Server installation completed successfully"
echo "You can connect to the server using IP address: $ip_address or hostname: $hostname"
