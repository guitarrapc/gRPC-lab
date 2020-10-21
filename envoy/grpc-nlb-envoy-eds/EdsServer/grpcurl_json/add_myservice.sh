cat grpcurl_json/add_myservice.json | grpcurl -plaintext -d @ localhost:8080 envoy.ClusterRegisterService.Add
cat grpcurl_json/add_myservice_1.json | grpcurl -plaintext -d @ localhost:8080 envoy.ClusterRegisterService.Add
cat grpcurl_json/add_myservice_2.json | grpcurl -plaintext -d @ localhost:8080 envoy.ClusterRegisterService.Add
cat grpcurl_json/add_myservice_3.json | grpcurl -plaintext -d @ localhost:8080 envoy.ClusterRegisterService.Add
