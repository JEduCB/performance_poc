#! /bin/sh

cmd="dotnet run -c Release -f net8.0 --statisticalTest 5% --filter System.Net.Security.Tests.SslStreamTests.*Handshake* \
          --envVars DOTNET_PROCESSOR_COUNT:$core_count OPENSSL_ASYNC_ENABLED:1 QAT_ENGINE:QAT_HW
          --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
          --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun" \
                    "/home/user/git/innersource/dotnet.runtime.poc1/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun" \
          --apples --iterationCount 5"

for core_count in "$@"
do   
#     echo "DOTNET_EnableAVX512F=1 DOTNET_PROCESSOR_COUNT=$core_count OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW ENGINE_METHOD=RSA_EC"
     echo "DOTNET_PROCESSOR_COUNT=$core_count OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW"
     #sudo DOTNET_EnableAVX512F=1 LD_PRELOAD=/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so \
     sudo $cmd >SPR_RESULTS/core_count_$core_count.txt 2>SPR_RESULTS/stderr.txt
done
