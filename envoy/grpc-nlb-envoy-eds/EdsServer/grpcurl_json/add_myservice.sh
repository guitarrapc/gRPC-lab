cat grpcurl_json/add_myservice.json | grpcurl -plaintext -d @ localhost:8080 envoy.ClusterRegisterService.Add