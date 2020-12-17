#!/bin/bash
ACM=abcdefg
DOMAIN=example.com
kubectl kustomize ./k8s | sed -e "s|<acm>|${ACM}|g" -e "s|<domain>|${DOMAIN}|g"