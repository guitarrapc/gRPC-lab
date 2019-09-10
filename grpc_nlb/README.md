## Run

docker

```
# build
pushd ./src/Greeter
docker build -t greet-server:v0.0.1 -f GreeterServer/Dockerfile .
popd

# run
docker run -it --rm -p 50051:50051 greet-server:v0.0.1
```

docker-compose

```
cd ./src
docker-compose build
docker-compose up
```

k8s. server only deployment, without envoy.

```
kubectl kustomize k8s/simple/base | kubectl apply -f -
kubectl get svc
kubectl get pod
kubectl get deploy
kubectl kustomize k8s/simple/base | kubectl delete -f -
```

k8s. envoy deployment. (namespace: `grpc-lab-nlb`)

> better grpc loadbalancing handle.

```
kubectl kustomize k8s/envoy/base | kubectl apply -f -
kubectl get svc -n grpc-lab-nlb
kubectl get pod -n grpc-lab-nlb
kubectl get deploy -n grpc-lab-nlb
kubectl kustomize k8s/envoy/base | kubectl delete -f -
```

## REF

> [Amazon EKSでgRPCサーバを運用する \- 一休\.com Developers Blog](https://user-first.ikyu.co.jp/entry/2019/08/27/093858)
>
> [Using Envoy Proxy to load\-balance gRPC services on GKE  \|  Solutions  \|  Google Cloud](https://cloud.google.com/solutions/exposing-grpc-services-on-gke-using-envoy-proxy)
>
> [GoogleCloudPlatform/grpc\-gke\-nlb\-tutorial: gRPC load\-balancing on GKE using Envoy](https://github.com/GoogleCloudPlatform/grpc-gke-nlb-tutorial)
> 
> [grpc\-ecosystem/grpc\-health\-probe: A command\-line tool to perform health\-checks for gRPC applications in Kubernetes etc\.](https://github.com/grpc-ecosystem/grpc-health-probe/)
>
> [Health checking gRPC servers on Kubernetes \- Kubernetes](https://kubernetes.io/blog/2018/10/01/health-checking-grpc-servers-on-kubernetes/)