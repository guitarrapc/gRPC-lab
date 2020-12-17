:: grpc_unity_package.2.35.0-dev202012021242
:: grpc.tools 2.27.0
:: google.protobuf 3.14.0
protoc.exe --csharp_out Grpc.Unity/Assets/Scripts/Grpc --grpc_out Grpc.Unity/Assets/Scripts/Grpc --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe GrpcClient/Protos/greet.proto
