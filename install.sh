#!/bin/bash

# Function to generate a random token
generate_token() {
    head -c 32 /dev/urandom | base64 | tr -d '+/=' | cut -c1-32
}

# Install Node.js if not already installed
if ! command -v node &> /dev/null; then
    echo "Node.js not found. Installing..."
    sudo apt-get update
    sudo apt-get install -y nodejs
fi

# Install npm if not already installed
if ! command -v npm &> /dev/null; then
    echo "npm not found. Installing..."
    sudo apt-get install -y npm
fi

# Ask for the port number
read -p "Enter the port number for the server (default is 3000): " port
port=${port:-3000}

# Generate a random token
token=$(generate_token)

# Create the server script
echo "const express = require('express');
const bodyParser = require('body-parser');
const app = express();
const port = $port;

app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

let sessions = {};

app.get('/', (req, res) => {
    const token = req.header('Authorization').replace('Bearer ', '');
    const sessionName = req.header('Session-Name');
    const password = req.header('Password');

    if (validateToken(token) && validateSession(sessionName, password)) {
        res.send('Connection successful!');
    } else {
        res.status(401).send('Unauthorized');
    }
});

app.post('/create-session', (req, res) => {
    const token = req.header('Authorization').replace('Bearer ', '');
    const sessionName = req.header('Session-Name');
    const password = req.header('Password');

    if (validateToken(token) && !sessions[sessionName]) {
        sessions[sessionName] = { password, data: {} };
        res.send('Session created successfully!');
    } else {
        res.status(401).send('Unauthorized or Session already exists');
    }
});

app.post('/store-data', (req, res) => {
    const token = req.header('Authorization').replace('Bearer ', '');
    const sessionName = req.header('Session-Name');
    const key = req.header('Key');
    const value = req.header('Value');

    if (validateToken(token) && validateSession(sessionName) && key && value) {
        sessions[sessionName].data[key] = value;
        res.send('Data stored successfully!');
    } else {
        res.status(401).send('Unauthorized or Invalid data');
    }
});

app.get('/read-data', (req, res) => {
    const token = req.header('Authorization').replace('Bearer ', '');
    const sessionName = req.header('Session-Name');
    const key = req.query.key;

    if (validateToken(token) && validateSession(sessionName) && key) {
        const data = sessions[sessionName].data[key];
        res.send(data || 'Data not found');
    } else {
        res.status(401).send('Unauthorized or Invalid data');
    }
});

app.listen(port, () => {
    console.log(\`Server is running at http://127.0.0.1:\${port}\`);
});

function validateToken(token) {
    return token === '$token';
}

function validateSession(sessionName, password) {
    return sessions[sessionName] && sessions[sessionName].password === password;
}" > server.js

echo "Node.js server script created successfully!"

# Install required npm packages
npm install express body-parser

echo "Installation complete! Run the server using: node server.js"
