#!/bin/bash

# API URL
api_url="http://localhost:5000" # Replace with your API URL
timestamp=$(date +%s)
random_str=$(head /dev/urandom | tr -dc A-Za-z0-9 | head -c 13 ; echo '')

# User Registration
username="testuser_${timestamp}_${random_str}"
password="Password123!"
echo "Registering a new user: $username"
register_response=$(curl -s -X POST "$api_url/api/user" \
    -H "Content-Type: application/json" \
    -d '{"username": "'"$username"'", "password": "'"$password"'"}')

# User Login
echo "Logging in the user: $username"
login_response=$(curl -s -X POST "$api_url/api/user/login" \
    -H "Content-Type: application/json" \
    -d '{"username": "'"$username"'", "password": "'"$password"'"}')
token=$(echo $login_response | jq -r '.token')

# Create Club
club_name="New Club_${timestamp}_${random_str}"
echo "Creating a new club: $club_name"
create_club_response=$(curl -s -X POST "$api_url/api/club" \
    -H "Authorization: Bearer $token" \
    -H "Content-Type: application/json" \
    -d '{"name": "'"$club_name"'", "httpLink": "http://newclub.com"}')

echo "User registration, login, and club creation responses:"
echo "Register Response: $register_response"
echo "Login Response: $login_response"
echo "Create Club Response: $create_club_response"
