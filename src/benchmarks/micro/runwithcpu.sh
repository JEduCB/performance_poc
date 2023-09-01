#! /bin/sh

for core_count in "$@"
do   
     echo "DOTNET_EnableAVX512F=1 DOTNET_PROCESSOR_COUNT=$core_count OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW ENGINE_METHOD=RSA_EC"

     sudo DOTNET_EnableAVX512F=1 LD_PRELOAD=/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so \
          OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW ENGINE_METHOD=RSA_EC \
          dotnet run -c Release -f net8.0 --statisticalTest 1% --filter System.Net.Security.Tests.SslStreamTests.*Handshake*_* \
               --envVars DOTNET_PROCESSOR_COUNT:$core_count \
               --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
               --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \
                         "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \
               --apples --iterationCount 5 >./SPR_RESULTS/core_count_$core_count.txt 2>./SPR_RESULTS/stderr.txt
done

# echo "DOTNET_EnableAVX512F=1 OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW ENGINE_METHOD=RSA_EC"

# sudo DOTNET_EnableAVX512F=1 LD_PRELOAD=/usr/lib/x86_64-linux-gnu/engines-1.1/libplock.so \
#      OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW ENGINE_METHOD=RSA_EC \
#      dotnet run -c Release -f net8.0 --statisticalTest 1% --filter System.Net.Security.Tests.SslStreamTests.*Handshake*Async \
#           --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
#           --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \
#                     "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net8.0-linux-Release-x64/shared/Microsoft.NETCore.App/8.0.0/corerun" \
#           --apples --iterationCount 5 >./SPR_RESULTS/core_count_224.txt 2>./SPR_RESULTS/stderr.txt
