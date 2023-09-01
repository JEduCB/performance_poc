#! /bin/sh

#sudo service qat_service restart

#DOTNET_SYSTEM_NET_SECURITY_DISABLETLSRESUME=1
#LD_PRELOAD=/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so
#DOTNET_PROCESSOR_COUNT=$1

sudo DOTNET_EnableAVX512F=1 LD_PRELOAD=/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so \
     OPENSSL_ASYNC_ENABLED=$2 QAT_ENGINE=QAT_$3 ENGINE_METHOD=$4 \
     dotnet run -c Release -f net8.0 --statisticalTest $5 --filter System.Net.Security.Tests.SslStreamTests.$6 --apples --iterationCount $7 \
     --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
     --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \
               "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \

