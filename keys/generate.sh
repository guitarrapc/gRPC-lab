#!/bin/bash
openssl rand -base64 12 | fold -w 10 | head -1 > passphrase.txt

# Generate CA key:
openssl genrsa -aes256 -out ca.key -passout file:passphrase.txt 4096

# Generate CA certificate:
openssl req -new -x509 -days 365 -key ca.key -passin file:passphrase.txt -out ca.crt -subj "/C=JP/ST=Example/L=Tokyo/O=Example/OU=Example/CN=*.example.com"

# Generate server key:
openssl genrsa -aes256 -out server.key -passout file:passphrase.txt 4096

# Generate server signing request:
openssl req -new -key server.key -out server.csr -passin file:passphrase.txt -subj "/C=JP/ST=Example/L=Tokyo/O=Example/OU=Example/CN=dummy.example.com"

# Self-sign server certificate:
openssl x509 -req -days 365 -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out server.crt -passin file:passphrase.txt

# Remove passphrase from the server key:
openssl rsa -in server.key -out server.key -passin file:passphrase.txt

# Generate client key:
openssl genrsa -aes256 -out client.key -passout file:passphrase.txt 4096

# Generate client signing request:
openssl req -new -key client.key -out client.csr -passin file:passphrase.txt -subj "/C=JP/ST=Example/L=Tokyo/O=Example/OU=Example/CN=dummy.example.com"

# Self-sign client certificate:
openssl x509 -req -days 365 -in client.csr -CA ca.crt -CAkey ca.key -set_serial 01 -out client.crt -passin file:passphrase.txt

# Remove passphrase from the client key:
openssl rsa -in client.key -out client.key -passin file:passphrase.txt
