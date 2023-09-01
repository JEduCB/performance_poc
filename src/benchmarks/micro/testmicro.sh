#! /bin/sh

#sudo service qat_service restart

#DOTNET_SYSTEM_NET_SECURITY_DISABLETLSRESUME=1
#LD_PRELOAD=/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so

sudo DOTNET_EnableAVX512F=1 LD_PRELOAD=/lib/x86_64-linux-gnu/libssl.so.1.1:/lib/x86_64-linux-gnu/libcrypto.so.1.1:/usr/lib/x86_64-linux-gnu/engines-1.1/qatengine.so:/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so \
     DOTNET_PROCESSOR_COUNT=$1 OPENSSL_ASYNC_ENABLED=$2 QAT_ENGINE=QAT_$3 ENGINE_METHOD=$4 \
     dotnet run -c Release -f net8.0 --statisticalTest $5 --filter System.Net.Security.Tests.SslStreamTests.$6 \
     --coreRun "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \
     --hide Job Toolchain --iterationCount $7

# sudo LD_PRELOAD=/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so OPENSSL_ASYNC_ENABLED=$1 QAT_ENGINE=QAT_$2 \
#      dotnet run -c Release -f net8.0 --statisticalTest $3 --filter System.Net.Security.Tests.SslStreamTests.$4 \
#      --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \
#      "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" --hide Job Toolchain --apples --iterationCount 5
