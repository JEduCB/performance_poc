// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;  
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Extensions;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using MicroBenchmarks;

namespace System.Net.Security.Tests
{
    [Config(typeof(Config))]
    public partial class SslStreamTests
    {
        private class Config : ManualConfig
        {
            public Config() => Orderer = new AlternateJobsExecutionOrder();

            private class AlternateJobsExecutionOrder : IOrderer
            {
                public IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarksCase, IEnumerable<BenchmarkLogicalGroupRule> order = null) =>
                    from benchmark in benchmarksCase
                    orderby benchmark.Descriptor.WorkloadMethodDisplayInfo ascending, benchmark.Parameters["protocol"], benchmark.Job.Id
                    select benchmark;

                public IEnumerable<BenchmarkCase> GetSummaryOrder(ImmutableArray<BenchmarkCase> benchmarksCase, Summary summary) =>
                    from benchmark in benchmarksCase
                    orderby benchmark.Descriptor.WorkloadMethodDisplayInfo ascending, benchmark.Parameters["protocol"], benchmark.Job.Id
                    select benchmark;

                public string GetHighlightGroupKey(BenchmarkCase benchmarkCase) => null;

                public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarksCases, BenchmarkCase benchmarkCase) =>
                    benchmarkCase.Descriptor.WorkloadMethodDisplayInfo + "_" + benchmarkCase.Parameters["protocol"];// + "_" + benchmarkCase.Job.Id;

                public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups,
                    IEnumerable<BenchmarkLogicalGroupRule> order = null) =>
                        logicalGroups.OrderBy(it => it.Key);

                public bool SeparateLogicalGroups => true;
            }
        }

        //Total handshakes will be Iterations Count * Concurrent Tasks
        private const int IterationsCount = 100;
        private const int ConcurrentTasks = 200;//250;//500;
        private const int ConcurrentIpTasks = 200;
        private const int ConcurrentContextTasks = 200;//250;

        private SslStreamCertificateContext _certContext = SslStreamCertificateContext.Create(Test.Common.Configuration.Certificates.GetServerCertificate(), null);

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        public Task DefaultHandshakeContextIPv4Async_Concurrent() => Spawn(IterationsCount, ConcurrentContextTasks, async () =>
        {
            await Task.Yield();
            (NetworkStream client, NetworkStream server) = ConcurrentObjectProvider.CreateIPv4Pair();
            await ConcurrentDefaultContextHandshake(client, server);
            client.Dispose();
            server.Dispose();
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        public Task DefaultHandshakeContextIPv6Async_Concurrent() => Spawn(IterationsCount, ConcurrentContextTasks, async () =>
        {
            await Task.Yield();
            (NetworkStream client, NetworkStream server) = ConcurrentObjectProvider.CreateIPv6Pair();
            await ConcurrentDefaultContextHandshake(client, server);
            client.Dispose();
            server.Dispose();
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        public Task DefaultHandshakeIPv4Async_Concurrent() => Spawn(IterationsCount, ConcurrentIpTasks, async () =>
        {
            await Task.Yield();
            (NetworkStream client, NetworkStream server) = ConcurrentObjectProvider.CreateIPv4Pair();
            await ConcurrentDefaultHandshake(client, server);
            client.Dispose();
            server.Dispose();     
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        public Task DefaultHandshakeIPv6Async_Concurrent() => Spawn(IterationsCount, ConcurrentIpTasks, async () =>
        {
            await Task.Yield();
            (NetworkStream client, NetworkStream server) = ConcurrentObjectProvider.CreateIPv6Pair();
            await ConcurrentDefaultHandshake(client, server);
            client.Dispose();
            server.Dispose();
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        [OperatingSystemsFilter(allowed: true, platforms: OS.Linux)]    // Not supported on Windows at the moment.
        public Task DefaultHandshakePipeAsync_Concurrent() => Spawn(IterationsCount, ConcurrentIpTasks, async () =>
        {
            await Task.Yield();
            (NamedPipeClientStream client, NamedPipeServerStream server) = ConcurrentObjectProvider.CreatePipePair();
            await Task.WhenAll(server.WaitForConnectionAsync(), client.ConnectAsync());
            await ConcurrentDefaultHandshake(client, server);
            client.Dispose();
            server.Dispose();
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        public Task DefaultMutualHandshakeIPv4Async_Concurrent() => Spawn(IterationsCount, ConcurrentIpTasks / 2, async () =>
        {
            await Task.Yield();
            (NetworkStream client, NetworkStream server) = ConcurrentObjectProvider.CreateIPv4Pair();
            await ConcurrentDefaultHandshake(client, server, requireClientCert: true);
            client.Dispose();
            server.Dispose();
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        public Task DefaultMutualHandshakeIPv6Async_Concurrent() => Spawn(IterationsCount, ConcurrentIpTasks / 2, async () =>
        {
            await Task.Yield();
            (NetworkStream client, NetworkStream server) = ConcurrentObjectProvider.CreateIPv6Pair();
            await ConcurrentDefaultHandshake(client, server, requireClientCert: true);
            client.Dispose();
            server.Dispose();
        });
            
        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        [ArgumentsSource(nameof(TlsProtocols))]
        public Task HandshakeContosoAsync_Concurrent(SslProtocols protocol) => Spawn(IterationsCount, ConcurrentTasks, async () =>
        {
            await Task.Yield();
            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839
            //it should be ok to reuse the certificate in multiple threads.
            await HandshakeAsync(_cert, protocol);
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        [ArgumentsSource(nameof(TlsProtocols))]
        public Task HandshakeECDSA256CertAsync_Concurrent(SslProtocols protocol) => Spawn(IterationsCount, ConcurrentTasks, async () =>
        {
            await Task.Yield();
            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839
            //it should be ok to reuse the certificate in multiple threads.
            await HandshakeAsync(_ec256Cert, protocol);
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        [ArgumentsSource(nameof(TlsProtocols))]
        [OperatingSystemsFilter(allowed: true, platforms: OS.Linux)]    // Not supported on Windows at the moment.
        public Task HandshakeECDSA512CertAsync_Concurrent(SslProtocols protocol) => Spawn(IterationsCount, ConcurrentTasks, async () =>
        {
            await Task.Yield();
            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839
            //it should be ok to reuse the certificate in multiple threads.
            await HandshakeAsync(_ec512Cert, protocol);
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        [ArgumentsSource(nameof(TlsProtocols))]
        public Task HandshakeRSA2048CertAsync_Concurrent(SslProtocols protocol) => Spawn(IterationsCount, ConcurrentTasks, async () =>
        {
            await Task.Yield();
            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839
            //it should be ok to reuse the certificate in multiple threads.
            await HandshakeAsync(_rsa2048Cert, protocol);
        });

        [Benchmark]
        [BenchmarkCategory(Categories.ThirdParty)]
        [ArgumentsSource(nameof(TlsProtocols))]
        public Task HandshakeRSA4096CertAsync_Concurrent(SslProtocols protocol) => Spawn(IterationsCount, ConcurrentTasks, async () =>
        {
            await Task.Yield();
            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839
            //it should be ok to reuse the certificate in multiple threads.
            await HandshakeAsync(_rsa4096Cert, protocol);
        });

        private static async Task Spawn(int numRequests, int concurrentTasks, Func<Task> method)
        {
            var _tasks = new Collections.Generic.List<Task>(concurrentTasks);

            for(int j = 0; j < numRequests; ++j)
            {
                for(int i = 0; i < concurrentTasks; ++i)
                {
                    _tasks.Add(Task.Run(method));
                }

                await Task.WhenAll(_tasks);
                _tasks.Clear();
            }
        }

        private async Task ConcurrentDefaultContextHandshake(Stream client, Stream server)
        {          
            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839
            //it should be ok to reuse the certificate in multiple threads.
            SslServerAuthenticationOptions serverOptions = new SslServerAuthenticationOptions
            {
                AllowRenegotiation = false,
                EnabledSslProtocols = SslProtocols.None,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                ServerCertificateContext  = _certContext,
            };

            using (var sslClient = new SslStream(client, leaveInnerStreamOpen: true, delegate { return true; }))
            using (var sslServer = new SslStream(server, leaveInnerStreamOpen: true, delegate { return true; }))
            {
                await Task.WhenAll(
                    sslClient.AuthenticateAsClientAsync("localhost", null, SslProtocols.None, checkCertificateRevocation: false),
                    sslServer.AuthenticateAsServerAsync(serverOptions, default));

                // In Tls1.3 part of handshake happens with data exchange.
                // To be consistent we do this extra step for all protocol versions
                byte[] clientBuffer = new byte[1], serverBuffer = new byte[1];

                await sslClient.WriteAsync(clientBuffer, default);
                await sslServer.ReadAsync(serverBuffer, default);
                await sslServer.WriteAsync(serverBuffer, default);
                await sslClient.ReadAsync(clientBuffer, default);
            }
        }      

        private async Task ConcurrentDefaultHandshake(Stream client, Stream server, bool requireClientCert = false)
        {
            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839
            //it should be ok to reuse the certificate in multiple threads.
            SslClientAuthenticationOptions clientOptions = new SslClientAuthenticationOptions
            {
                AllowRenegotiation = false,
                EnabledSslProtocols = SslProtocols.None,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                TargetHost = "loopback",
                ClientCertificates = requireClientCert ? new X509CertificateCollection() { _clientCert } : null,
            };

            //Based on this comment https://github.com/dotnet/runtime/issues/87085#issuecomment-1575088839 
            //it should be ok to reuse the certificate in multiple threads.
            SslServerAuthenticationOptions serverOptions = new SslServerAuthenticationOptions
            {
                AllowRenegotiation = false,
                EnabledSslProtocols = SslProtocols.None,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                ServerCertificate = _cert,
                ClientCertificateRequired = requireClientCert,
            };

            using (var sslClient = new SslStream(client, leaveInnerStreamOpen: true, delegate { return true; }))
            using (var sslServer = new SslStream(server, leaveInnerStreamOpen: true, delegate { return true; }))
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(10));

                await Task.WhenAll(
                    sslClient.AuthenticateAsClientAsync(clientOptions, cts.Token),
                    sslServer.AuthenticateAsServerAsync(serverOptions, cts.Token));

                if ((int)sslClient.SslProtocol > (int)SslProtocols.Tls12)
                {
                    // In Tls1.3 part of handshake happens with data exchange.
                    byte[] clientBuffer = new byte[1], serverBuffer = new byte[1];

                    await sslClient.WriteAsync(clientBuffer, cts.Token);
                    await sslServer.ReadAsync(serverBuffer, cts.Token);
                    await sslServer.WriteAsync(serverBuffer, cts.Token);
                    await sslClient.ReadAsync(clientBuffer, cts.Token);
                }
            }
        } 
    }

    internal static class ConcurrentObjectProvider
    {
        private static Socket _listenerIPv4 = null;
        private static Socket _listenerIPv6 = null;

        private const string _pipeName = "ConcurrentTlsHandshakePipe";

        private static int pipeCount = 0;

        static ConcurrentObjectProvider()
        {
            _listenerIPv4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerIPv4.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            _listenerIPv4.Listen((int)SocketOptionName.MaxConnections);

            _listenerIPv6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            _listenerIPv6.Bind(new IPEndPoint(IPAddress.IPv6Loopback, 0));
            _listenerIPv6.Listen((int)SocketOptionName.MaxConnections);
        }

        public static Tuple<NetworkStream, NetworkStream> CreateIPv4Pair()
        {
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Based on MSDN https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?redirectedfrom=MSDN&view=net-7.0#:~:text=Thread%20Safety,are%20thread%20safe.
            //_listenerIPv4 is thread-safe.
            client.Connect(_listenerIPv4.LocalEndPoint);
            var server = _listenerIPv4.Accept();

            var clientIPv4 = new NetworkStream(client, ownsSocket: true);
            var serverIPv4 = new NetworkStream(server, ownsSocket: true);
            
            return new Tuple<NetworkStream, NetworkStream>(clientIPv4, serverIPv4);
        }

        public static Tuple<NetworkStream, NetworkStream> CreateIPv6Pair()
        {
            var client = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            //Based on MSDN https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?redirectedfrom=MSDN&view=net-7.0#:~:text=Thread%20Safety,are%20thread%20safe.
            //_listenerIPv6 is thread-safe.
            client.Connect(_listenerIPv6.LocalEndPoint);
            Socket server = _listenerIPv6.Accept();

            var clientIPv6 = new NetworkStream(client, ownsSocket: true);
            var serverIPv6 = new NetworkStream(server, ownsSocket: true);

            return new Tuple<NetworkStream, NetworkStream>(clientIPv6, serverIPv6);
        }

        public static Tuple<PipeStream, PipeStream> CreatePipePair()
        {
            var pipe = _pipeName + pipeCount++;

            var pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            
            var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

            Task.WaitAll(pipeServer.WaitForConnectionAsync(), pipeClient.ConnectAsync());

            return new Tuple<PipeStream, PipeStream>(pipeClient, pipeServer);
        }

        public static void Cleanup()
        {
            _listenerIPv4.Dispose();
            _listenerIPv6.Dispose();
        }        
    }
}
