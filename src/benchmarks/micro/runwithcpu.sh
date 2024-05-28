#! /bin/sh

cmd="dotnet run -c Release -f net9.0 --statisticalTest 1% --filter System.Net.Security.Tests.SslStreamTests.*Handshake*Async \
          --envVars DOTNET_PROCESSOR_COUNT:$core_count OPENSSL_ASYNC_ENABLED:1 QAT_ENGINE:QAT_HW
          --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
          --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun" \
                    "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun" \
          --apples --iterationCount 10"

for core_count in "$@"
do   
     echo "DOTNET_PROCESSOR_COUNT=$core_count OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW"
     $cmd >SPR_RESULTS/core_count_$core_count.both.txt 2>SPR_RESULTS/stderr.txt
done

# cmd="dotnet run -c Release -f net9.0 --filter System.Net.Security.Tests.SslStreamTests.*Handshake*Async \
#           --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
#           --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun""

# for core_count in "$@"
# do   
#      echo "DOTNET_PROCESSOR_COUNT=$core_count"
#      $cmd >SPR_RESULTS/core_count_$core_count.noQAT.txt 2>SPR_RESULTS/stderr.txt
# done

# cmd="dotnet run -c Release -f net9.0 --filter System.Net.Security.Tests.SslStreamTests.*Handshake*Async \
#           --envVars DOTNET_PROCESSOR_COUNT:$core_count OPENSSL_ASYNC_ENABLED:1 QAT_ENGINE:QAT_HW
#           --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
#           --coreRun "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun""

# for core_count in "$@"
# do   
#      echo "DOTNET_PROCESSOR_COUNT=$core_count OPENSSL_ASYNC_ENABLED=1 QAT_ENGINE=QAT_HW"
#      $cmd >SPR_RESULTS/core_count_$core_count.QAT.txt 2>SPR_RESULTS/stderr.txt
# done
