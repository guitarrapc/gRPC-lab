## Run

### DockerHub

> [guitarrapc/grpc-example-server-csharp](https://cloud.docker.com/u/guitarrapc/repository/docker/guitarrapc/grpc-example-server-csharp)
>
> [guitarrapc/grpc-example-client-csharp](https://cloud.docker.com/u/guitarrapc/repository/docker/guitarrapc/grpc-example-client-csharp)

```
docker build -t grpc-example-server-csharp:v0.0.1 -f GreeterServer/Dockerfile .
docker tag grpc-example-server-csharp:v0.0.1 guitarrapc/grpc-example-server-csharp:v0.0.1
docker tag grpc-example-server-csharp:v0.0.1 guitarrapc/grpc-example-server-csharp:latest
docker push guitarrapc/grpc-example-server-csharp:v0.0.1
docker push guitarrapc/grpc-example-server-csharp:latest

docker build -t grpc-example-client-csharp:v0.0.1 -f GreeterClient/Dockerfile .
docker tag grpc-example-client-csharp:v0.0.1 guitarrapc/grpc-example-client-csharp:v0.0.1
docker tag grpc-example-client-csharp:v0.0.1 guitarrapc/grpc-example-client-csharp:latest
docker push guitarrapc/grpc-example-client-csharp:v0.0.1
docker push guitarrapc/grpc-example-client-csharp:latest
```

### Server & Client

docker-compose

```
cd ./src
docker-compose build
docker-compose up
```

Server

```
# build
pushd ./src/Greeter
docker build -t grpc-example-server-csharp:v0.0.1 -f GreeterServer/Dockerfile .
popd

# run
docker run -it --rm -p 50051:50051 greet-server:v0.0.1
```

Client

```
# build
pushd ./src/Greeter
docker build -t grpc-example-client-csharp:v0.0.1 -f GreeterClient/Dockerfile .
popd

# run
docker run -it --rm grpc-example-client-csharp:v0.0.1
```

## REF

> [grpc\-ecosystem/grpc\-health\-probe: A command\-line tool to perform health\-checks for gRPC applications in Kubernetes etc\.](https://github.com/grpc-ecosystem/grpc-health-probe/)
>
> [Health checking gRPC servers on Kubernetes \- Kubernetes](https://kubernetes.io/blog/2018/10/01/health-checking-grpc-servers-on-kubernetes/)

