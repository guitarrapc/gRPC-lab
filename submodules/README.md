submodules to build envoy gRPC.

## Copy protos to project directory

run shell on repository root.

```shell
RPATH="envoy/grpc-nlb-envoy-eds/EdsServer/GrpcEdsService/"
pushd submodules/client_model
    find . -name "*.proto" | cpio -pd ../../${RPATH}/Protos/client_model/
popd
pushd submodules/envoy/api/envoy
    find . -name "*.proto" | cpio -pd ../../../../${RPATH}/Protos/envoy/
popd
pushd submodules/googleapis/google
    find . -name "*.proto" | cpio -pd ../../../${RPATH}/Protos/google/
    rm -rf ../../../${RPATH}/Protos/google/ads
    rm -rf ../../../${RPATH}/Protos/google/cloud
    rm -rf ../../../${RPATH}/Protos/google/devtools
popd
pushd submodules/opencensus-proto/src/opencensus
    find . -name "*.proto" | cpio -pd ../../../../${RPATH}/Protos/opencensus/
popd
pushd submodules/protoc-gen-validate/validate
    find . -name "*.proto" | cpio -pd ../../../${RPATH}/Protos/validate/
popd
pushd submodules/udpa/udpa
    find . -name "*.proto" | cpio -pd ../../../${RPATH}/Protos/udpa/
popd
```

