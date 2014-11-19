Outsorcery
==========
[Available on NuGet](https://www.nuget.org/packages/Outsorcery/)    -    [Request New Features Here](https://github.com/SteveLillis/Outsorcery/issues)

Overview
--------
Outsorcery is an open-source library for adding [distributed computing](http://en.wikipedia.org/wiki/Distributed_computing) capabilities to your software.  It is scalable, customisable and asynchronous.  While many distributed computing solutions expose the consuming code to the details of work distribution to some degree, Outsorcery has been built from the ground up to be used in the same way as any local async method.

Getting Started
---------------
The first thing to do when getting started with Outsorcery is define a unit of distributable work.  All it takes is to implement [IWorkItem](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/IWorkItem.cs) and to make your class serializable so it can be sent to the work server.  Your result Type needs to be serializable too so the server can return it.  Don't worry though, when you get the result back it'll be the Type you expect!

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
[Local Workers](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/LocalWorker.cs) can be useful for testing or as a failsafe when the work servers are unavailable.

```csharp
// CLIENT APPLICATION
var worker = new LocalWorker();
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```

Do Work Remotely
----------------
[Outsourced Workers](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/OutsourcedWorker.cs) distribute the work to one or more servers for completion. Distributed computing using Outsourced Workers has been designed to be as easy as possible. The projects [ExampleServer](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleServer) and [ExampleClient](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleClient) are a fully functioning example.

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

[Load Balanced](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/LoadBalancedTcpWorkerConnectionProvider.cs) distributes a work item to the server that reports having the lowest workload. You can customise how the server estimates its workload by providing a custom implementation of IWorkloadBenchmark to the server constructor.

```csharp
// CLIENT APPLICATION
var endPoints = new List<IPEndPoint> { remoteEndPoint1, remoteEndPoint2, ... };
var provider = new LoadBalancedTcpWorkerConnectionProvider(endPoints);

// SERVER APPLICATION
var customBenchmark = new MyCustomWorkloadBenchmark();
new TcpWorkServer(localEndPoint, customBenchmark).Run(cancellationToken).Wait();
```

Exception Handling and Retries
------------------------------
To receive notification when a Worker encounters an exception, subscribe to the WorkException event.  This event occurs even if an exception is suppressed by a Retry so can be useful for keeping track of your application's silent failures.

```csharp
// CLIENT APPLICATION
var worker = new OutsourcedWorker(provider);
worker.WorkException += MyWorkerOnWorkExceptionHandler;
```

You can suppress the first X exceptions that a Worker encounters and automatically attempt the work again using a [Retry Worker](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/RetryWorker.cs). Retry Workers can be created manually or by using the [fluent extensions](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/FluentWorkerExtensions.cs) provided.  You can limit which exceptions cause retries by supplying an optional predicate to the Retry Worker.

```csharp
// CLIENT APPLICATION
var worker = new OutsourcedWorker(provider);

// Manual creation
var result = await new RetryWorker(worker, 2)
                            .DoWorkAsync(myWorkItem, cancellationToken);

// Extension method
var result = await worker.WithRetries(2)
                            .DoWorkAsync(myWorkItem, cancellationToken);

// Manual creation with predicate
var result = 
    await new RetryWorker(worker, 2, e => e.InnerException is CommunicationException)
                .DoWorkAsync(myWorkItem, cancellationToken);

// Extension method with predicate
var result = 
    await worker.WithRetries(2, e => e.InnerException is CommunicationException)
                .DoWorkAsync(myWorkItem, cancellationToken);
```

Work Servers suppress all exceptions that they encounter while processing a client connection and its associated work. To receive notification when client related exceptions occur, subscribe to the RemoteWorkException event.

```csharp
// SERVER APPLICATION
var server = new TcpWorkServer(localEndPoint);
server.RemoteWorkException += MyServerOnRemoteWorkExceptionHandler;
```

When an exception occurs during DoWorkAsync on the Work Server, that exception is returned to the client before closing the connection.  The Outsourced Worker then re-throws the exception on the client.  This allows you to react to work related exceptions locally. In the below example, the work is only retried by the client if the exception that occurred is due to "Bad luck".

```csharp
// WORK ITEM
[Serializable]
public class WorkItemWithExceptions : IWorkItem<int>
{
    public int WorkCategoryId { get { return 0; } }
    
    public Task<int> DoWorkAsync(CancellationToken cancellationToken)
    {
        if (new Random().Next(100) == 0)
            throw new InvalidOperationException("Bad luck");
        
        return Task.FromResult(10);
    }
}     

// CLIENT APPLICATION
var worker = new OutsourcedWorker(provider);
var myWorkItem = new WorkItemWithExceptions();

var result = await worker
                    .WithRetries(2, e => e.InnerException.Message == "Bad luck")
                    .DoWorkAsync(myWorkItem, cancellationToken);
```

Timing Out
----------
All workers will wait as long as it takes to finish doing the work before returning.  You can set a timeframe in which they must complete the work or be automatically cancelled by using a [Timeout Worker](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/TimeoutWorker.cs).  Timeout Workers can be created manually or by using the [fluent extensions](https://github.com/SteveLillis/Outsorcery/blob/master/Outsorcery/FluentWorkerExtensions.cs) provided.

```csharp
// CLIENT APPLICATION
var worker = new OutsourcedWorker(provider);

// Manual creation
var result = await new TimeoutWorker(worker, TimeSpan.FromSeconds(5))
                        .DoWorkAsync(myWorkItem, cancellationToken);

// Extension method
var result = await worker.WithTimeout(TimeSpan.FromSeconds(5))
                        .DoWorkAsync(myWorkItem, cancellationToken);
```
