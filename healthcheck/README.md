## TL;DR

gRPC C# Implementation with Greet and Healthcheck.

You can try health check with service nanme `Check`.

Run Server

```
docker run --rm -it -p 50051:50051 guitarrapc/grpc-example-server-csharp:latest
```

Run Client

```
docker run --rm -it guitarrapc/grpc-example-client-csharp:latest
```

You can use docker-compose to run both server/client.

```
version: '3'
services:
  server:
    image: guitarrapc/grpc-example-server-csharp:latest
    expose:
      - 50051
    ports:
      - 50051:50051
  client:
    image: guitarrapc/grpc-example-client-csharp:latest
    environment:
      GRPC_HOST: server
      GRPC_PORT : 50051
    links:
      - server
```


## Run

### DockerHub

> [guitarrapc/grpc-example-server-csharp](https://cloud.docker.com/u/guitarrapc/repository/docker/guitarrapc/grpc-example-server-csharp)
>
> [guitarrapc/grpc-example-client-csharp](https://cloud.docker.com/u/guitarrapc/repository/docker/guitarrapc/grpc-example-client-csharp)

```
pushd src/Greeter
docker build -t grpc-example-server-csharp:v0.0.3 -f GreeterServer/Dockerfile .
docker tag grpc-example-server-csharp:v0.0.3 guitarrapc/grpc-example-server-csharp:v0.0.3
docker tag grpc-example-server-csharp:v0.0.3 guitarrapc/grpc-example-server-csharp:latest
docker push guitarrapc/grpc-example-server-csharp:v0.0.3
docker push guitarrapc/grpc-example-server-csharp:latest
popd
```

```
pushd src/Greeter
docker build -t grpc-example-client-csharp:v0.0.3 -f GreeterClient/Dockerfile .
docker tag grpc-example-client-csharp:v0.0.3 guitarrapc/grpc-example-client-csharp:v0.0.3
docker tag grpc-example-client-csharp:v0.0.3 guitarrapc/grpc-example-client-csharp:latest
docker push guitarrapc/grpc-example-client-csharp:v0.0.3
docker push guitarrapc/grpc-example-client-csharp:latest
popd
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
docker run -it --rm -p 50051:50051 grpc-example-server-csharp:v0.0.1
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