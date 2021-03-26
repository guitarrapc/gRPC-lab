docker build -t guitarrapc/grpc-lab-dotnet-service -f GrpcService/Dockerfile .
docker tag guitarrapc/grpc-lab-dotnet-service:latest guitarrapc/grpc-lab-dotnet-service:2.36.0
docker push guitarrapc/grpc-lab-dotnet-service:latest
docker push guitarrapc/grpc-lab-dotnet-service:2.36.0
