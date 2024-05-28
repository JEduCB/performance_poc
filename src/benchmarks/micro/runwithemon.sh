#! /bin/sh

# run_test()
# {
#      emon_stop="emon -stop"

#      for core_count in $1
#      do   
#           echo "$4$2 - Core count=$core_count"       

#           cmd="$3 2>/dev/null >SPR_RESULTS/$4$2.$core_count.txt"
#           emon_start="emon -collect-edp -f SPR_RESULTS/$4$2.$core_count.dat &"
#           core=$((core_count - 1))
#           task_set="taskset -c 0-$core $cmd"

#           eval "$emon_start"
#           eval "$task_set"
#           eval "$emon_stop"
#      done
# }

run_test()
{
     emon_stop="emon -stop"

     for core_count in $1
     do   
          echo "$4$2 - Core count=$core_count"       
          core_mask=$(((1 << core_count) - 1))
          cmd="$3 --affinity $core_mask -p EP 2>/dev/null >SPR_RESULTS/$4$2.$core_count.txt"
          # echo $cmd
          # continue
          emon_start="emon -collect-edp -f SPR_RESULTS/$4$2.$core_count.dat &"

          eval "$emon_start"
          eval "$cmd"
          eval "$emon_stop"
     done
}

base="*Handshake*RSA*Async"
mod_filename="base."

for it in $(seq 1 2)
do
     cmd1="../../../artifacts/bin/MicroBenchmarks/Release/net9.0/MicroBenchmarks \
          --filter System.Net.Security.Tests.SslStreamTests.$base --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
          --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun""

     run_test "$1" "no_qat" "$cmd1" "$mod_filename"

     cmd2="../../../artifacts/bin/MicroBenchmarks/Release/net9.0/MicroBenchmarks \
          --envVars OPENSSL_ASYNC_ENABLED:1 QAT_ENGINE:QAT_HW \
          --filter System.Net.Security.Tests.SslStreamTests.$base --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
          --coreRun "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun""

     run_test "$1" "qat" "$cmd2" "$mod_filename"

     cmd3="../../../artifacts/bin/MicroBenchmarks/Release/net9.0/MicroBenchmarks \
          --envVars OPENSSL_ASYNC_ENABLED:1 QAT_ENGINE:QAT_HW \
          --statisticalTest $2 --filter System.Net.Security.Tests.SslStreamTests.$base --hide Job Toolchain InvocationCount UnrollFactor Gen0 Gen1 Gen2 \
          --coreRun "/home/user/git/innersource/dotnet.runtime.main/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun" \
                    "/home/user/git/innersource/dotnet.runtime.poc/artifacts/bin/testhost/net9.0-linux-Release-x64/shared/Microsoft.NETCore.App/9.0.0/corerun""

     run_test "$1" "compare_both" "$cmd3" "$mod_filename"

     base="$base""_*"
     mod_filename="mt."
done

echo ""