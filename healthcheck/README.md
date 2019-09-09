docker

```
# build
pushd src/Greeter
docker build -t greet-server:v0.0.1 -f GreeterServer/Dockerfile .
popd

# run
docker run -it --rm -p 50051:50051 greet-server:v0.0.1

# docker-compose
docker-compose up
```

k8s server only deployment

```
kubectl kustomize k8s/simple/base | kubectl apply -f -
kubectl get pod
kubectl get deploy
kubectl get svc
kubectl kustomize k8s/simple/base | kubectl delete -f -
```

k8s with envoy deployment

```
kubectl kustomize k8s/envoy/base | kubectl apply -f -
kubectl get pod
kubectl get deploy
kubectl get svc
kubectl kustomize k8s/envoy/base | kubectl delete -f -
```

