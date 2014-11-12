Outsorcery
==========

Introduction
------------
Outsorcery is an open-source library for adding [distributed computing](http://en.wikipedia.org/wiki/Distributed_computing) capabilities to your software.  It is scalable, customisable and asynchronous.  While many distributed computing solutions expose the consuming code to the details of work distribution and messaging, Outsorcery has been built from the ground up to be used in the exact same way as a local async method.

Getting Started
---------------
The first thing to do when getting started with Outsorcery is define a unit of distributable work. The only requirements are that it implements IWorkItem< TResult > and that it is Serializable.  You should put all your work items into a class library so that both the server and client projects can reference them.

```csharp
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

Do Work Locally
---------------
Local workers can be useful for testing or as a failsafe when the work servers are unavailable.

```csharp
// CLIENT APPLICATION
var worker = new LocalWorker();
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```

Do Work Remotely
----------------
Getting started with distributed computing using Outsorcery has been designed to be as easy as possible. The projects [ExampleServer](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleServer) and [ExampleClient](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleClient) are a working example of basic usage.

```csharp
// SERVER APPLICATION
// *** REMINDER - Add a reference to your work item library in the 
//                server project or it won't know what it's receiving! ***
var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
new TcpWorkServer(localEndPoint).Run(cancellationToken).Wait();

// CLIENT APPLICATION
// Setup
var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444); 
var provider = new SingleTcpWorkerConnectionProvider(remoteEndPoint);
var worker = new OutsourcedWorker(provider);

// Work
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```

Load Balancing
--------------
[Round Robin](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/RoundRobinTcpWorkerConnectionProvider.cs)
Distributes work items evenly across all end points.

```csharp
// CLIENT
var endPoints = new List<IPEndPoint> { remoteEndPoint1, remoteEndPoint2, remoteEndPoint3, ... };
var provider = new RoundRobinTcpWorkerConnectionProvider(endPoints);
```

[Load Balanced](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/LoadBalancedTcpWorkerConnectionProvider.cs)
Distributes work items to the server that reports having the lowest workload. You can customise the server's response by providing a custom implementation of IWorkloadBenchmark to the server constructor.

```csharp
// CLIENT
var endPoints = new List<IPEndPoint> { remoteEndPoint1, remoteEndPoint2, remoteEndPoint3, ... };
var provider = new LoadBalancedTcpWorkerConnectionProvider(endPoints);

// SERVER
new TcpWorkServer(localEndPoint, new MyCustomWorkloadBenchmark()).Run(cancellationToken).Wait();
```

Error Handling
--------------
Documentation coming soon!
