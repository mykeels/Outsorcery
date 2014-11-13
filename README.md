Outsorcery
==========
[Now available on NuGet](https://www.nuget.org/packages/Outsorcery/)

Overview
--------
Outsorcery is an open-source library for adding [distributed computing](http://en.wikipedia.org/wiki/Distributed_computing) capabilities to your software.  It is scalable, customisable and asynchronous.  While many distributed computing solutions expose the consuming code to the details of work distribution to some degree, Outsorcery has been built from the ground up to be used in the same way as any local async method.

Getting Started
---------------
The first thing to do when getting started with Outsorcery is define a unit of distributable work.  All it takes is to implement IWorkItem< TResult > and to make your class serializable so Outsorcery can send it to the work server.  Your result Type needs to be serializable too, for the same reason.

```csharp
// WORK ITEM CLASS LIBRARY
[Serializable]
public class MyFirstWorkItem : IWorkItem<int>
{
    // Don't worry about WorkCategoryId for now, just return zero
    public int WorkCategoryId { get { return 0; } }
    
    public int TestValue { get; set; }

    public Task<int> DoWorkAsync(CancellationToken cancellationToken)
    {
      return Task.FromResult(TestValue * 5);
    }
}            
```

You should put all your work items into a class library so that the Server and Client applications both have access.

Do Work Locally
---------------
Local Workers can be useful for testing or as a failsafe when the work servers are unavailable.

```csharp
// CLIENT APPLICATION
var worker = new LocalWorker();
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```

Do Work Remotely
----------------
Remote Workers distribute the work to one or more servers for completion. Distributed computing using Outsorcery's Remote Workers has been designed to be as easy as possible. The projects [ExampleServer](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleServer) and [ExampleClient](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleClient) are a fully functioning example.

```csharp
// CLIENT APPLICATION
// Setup
var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444); 
var provider = new SingleTcpWorkerConnectionProvider(remoteEndPoint);
var worker = new OutsourcedWorker(provider);

// Work
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());

// SERVER APPLICATION
// *** REMINDER - Add a reference to your work item library in the 
//                server project or it won't know what it's receiving! ***
var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
new TcpWorkServer(localEndPoint).Run(cancellationToken).Wait();
```

Load Balancing
--------------
[Round Robin](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/RoundRobinTcpWorkerConnectionProvider.cs) distributes each work item to a different server.

```csharp
// CLIENT APPLICATION
var endPoints = new List<IPEndPoint> { remoteEndPoint1, remoteEndPoint2, ... };
var provider = new RoundRobinTcpWorkerConnectionProvider(endPoints);
```

[Load Balanced](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/LoadBalancedTcpWorkerConnectionProvider.cs) distributes a work item to the server that reports having the lowest workload. You can customise the server's response by providing a custom implementation of IWorkloadBenchmark to the server constructor.

```csharp
// CLIENT APPLICATION
var endPoints = new List<IPEndPoint> { remoteEndPoint1, remoteEndPoint2, ... };
var provider = new LoadBalancedTcpWorkerConnectionProvider(endPoints);

// SERVER APPLICATION
var customBenchmark = new MyCustomWorkloadBenchmark();
new TcpWorkServer(localEndPoint, customBenchmark).Run(cancellationToken).Wait();
```

Exception Handling
------------------
Documentation coming soon!

Retries and Timeouts
--------------------
Documentation coming soon!
