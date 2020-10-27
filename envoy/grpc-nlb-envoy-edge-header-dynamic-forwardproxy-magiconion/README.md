## Local

```shell
getenvoy run standard:1.16.0 -- -c ./envoy_config_dynamic.yaml
```

## Kubernetes

### Personnal image

* echo-grpc/go.mod
* reverse-grpc/go.mod

```go
- google.golang.org/grpc v1.21.0
+ google.golang.org/grpc v1.29.1
```

### Local (could not work.....)

add nginx ingress to distribute EXTERANL host to svc.

```shell
  externalIPs:
  - $(hostname -I | cut -d' ' -f1)
```

### AWS

add nlb annotations to k8s/envoy-service.yaml

```yaml
metadata:
  annotations:
    service.beta.kubernetes.io/aws-load-balancer-type: "nlb"
spec:
  externalTrafficPolicy: Local
```

## Build Image

> https://cloud.google.com/solutions/exposing-grpc-services-on-gke-using-envoy-proxy?hl=ja

```shell
DOCKER_BUILDKIT=1
docker build -t guitarrapc/echo-magiconion EchoGrpcMagicOnion -f EchoGrpcMagicOnion/EchoGrpcMagicOnion/Dockerfile
docker push guitarrapc/echo-magiconion
```
## Deploy Kubernetes

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-magiconion
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./k8s |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" | 
    sed -e "s|<namespace>|$NAMESPACE|g" | 
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" | 
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-grpc -o yaml | grep podIP
```


## test grpc response

unary
```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Echo -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-0-210' -message "echo"
```

streaming hub

```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Stream -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-0-210' -roomName "A" -userName "hoge"
```

