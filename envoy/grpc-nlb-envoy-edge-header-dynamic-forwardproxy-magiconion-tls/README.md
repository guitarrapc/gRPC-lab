## Local

```shell
getenvoy run standard:1.16.0 -- -c ./envoy_config_dynamic.yaml
```

## Kubernetes

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-magiconion
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


## Gen self-signed cert

```shell
openssl genrsa 2048 > server.key
openssl req -new -sha256 -key server.key -out server.csr -subj "/C=JP/ST=Tokyo/L=Tokyo/O=MagicOnion Demo/OU=Dev/CN=$MY_DOMAIN"
openssl x509 -req -in server.csr -signkey server.key -out server.crt -days 7300 -extensions server
cp ./server.crt  EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/.
```

## Build Image

> https://cloud.google.com/solutions/exposing-grpc-services-on-gke-using-envoy-proxy?hl=ja

```shell
DOCKER_BUILDKIT=1
docker build -t guitarrapc/echo-magiconion-tls EchoGrpcMagicOnion -f EchoGrpcMagicOnion/EchoGrpcMagicOnion/Dockerfile
docker push guitarrapc/echo-magiconion-tls
```
## Deploy Kubernetes

deploy app and service.


prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

add hosts entry to `/etc/hosts`

```
127.0.0.1 dummy.example.com
```

deploy app

```shell
kubectl create secret tls envoy-certs --key server.key --cert server.crt --dry-run -o yaml | kubectl apply -f -
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
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Echo -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-1-38' -message "echo"
```

streaming hub

```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Stream -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-1-38' -roomName "A" -userName "hoge"
```

