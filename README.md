# gRPC-lab

## Getttings started

run following on repository root to restore gRPC Unity Plugins.

```shell
# macOS/Linux/WSL
$ curl https://packages.grpc.io/archive/2019/07/b8b6df08ae6d9f60e1b282a659d26b8c340de5c9-1785a3f7-80cd-4809-bd74-e8a0871cdff2/csharp/grpc_unity_package.1.23.0-dev.zip -o grpc_unity_package.zip -J -L
$ unzip grpc_unity_package.zip -d ./src/grpc_HelloworldUnity/Assets
```

## Update Proto and pass to unity

sync proto generated .cs and unity client

```shell
find ./src/csharp-Helloworld/Greeter/obj/Debug/netstandard2.0/ -maxdepth 1 -path "*.cs" ! -path "*AssemblyInfo.cs" | xargs -i cp -p {} ./src/grpc_HelloworldUnity/Assets/Scripts
```