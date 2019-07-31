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

## SSL connection

Let's think about using Let's encrypt, you have 2 choice to verify SSL/TLS.

1. use server certificate: you have to send renewed server certificate `cert.pem` every 3 months.
1. use root certificate to verify Let't encrypt cert.

### create CA,Server,Client certificate

generate ca, server, client certificates with keys/generate.sh. passphrase for ca.key will be generated in keys/passphrase.txt.

```shell
$ cd keys
$ . ./generate.sh
$ cd ..
```

in case you want check serial is correct, use follows.

```shell
openssl x509 -in keys/server.crt -noout -text
```

Server side C# project will load generated keys on build automatically.

Client side Unity project required to copy.

```shell
mkdir src/grpc_HelloworldUnity/Assets/StreamingAssets
cp keys/ca.crt src/grpc_HelloworldUnity/Assets/StreamingAssets/.
cp keys/client.crt src/grpc_HelloworldUnity/Assets/StreamingAssets/.
cp keys/client.key src/grpc_HelloworldUnity/Assets/StreamingAssets/.
```


### use Let's Encrypt certificate and verify with gRPC roots.pem

* Root Certificate Authority: DST Root CA X3
* Certificate Chain: Let's Encrypt Authority X3

gRPC certificate, roots.pem, is generated from [Mozzila/certdata.txt](https://hg.mozilla.org/mozilla-central/file/tip/security/nss/lib/ckfw/builtins/certdata.txt) via https://github.com/agl/extract-nss-root-certs. certdata.txt contains certificate for `DST Root CA X3` therefore Let's encrypt certificate can verify with gRPC's roots.pem.

download grpc roots.pem

```shell
$ curl https://raw.githubusercontent.com/grpc/grpc/master/etc/roots.pem -o keys/roots.pem -J -L
```

check roots.pem information

```shell
$ openssl x509 -text -fingerprint -noout -in keys/roots.pem
```

copy to project assets.

```shell
$ cp keys/roots.pem ./src/csharp-Helloworld/GreeterServer/.
$ mkdir -p ./src/grpc_HelloworldUnity/Assets/StreamingAssets
$ cp keys/roots.pem ./src/grpc_HelloworldUnity/Assets/StreamingAssets/.
```

check your domain's certificate chain & root certificate authority is using let's encrypt.

```shell
$ openssl s_client -showcerts -verify 5 -connect YOUR.DOMAIN.com:443 < /dev/null
```



## ref:

ssl/tls

* https://gist.github.com/guitarrapc/f5e8aadc6d67d5ad8484c70bfff540be
* https://kazuhira-r.hatenablog.com/entry/20180803/1533302929

grpc

* https://github.com/guitarrapc/gRPC-lab/tree/master/src/grpc_HelloworldUnity
* https://github.com/grpc/grpc/blob/master/etc/roots.pem
* https://qiita.com/mxProject/items/1cd0f6e5f53d5fe638ac
* https://hg.mozilla.org/mozilla-central/file/tip/security/nss/lib/ckfw/builtins/certdata.txt#l4845
* https://nekonenene.hatenablog.com/entry/2019/02/17/073806
* https://nekonenene.hatenablog.com/entry/grpc-nginx-proxy
