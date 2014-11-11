Outsorcery
==========

Introduction
------------
Outsorcery is an open-source library for adding [distributed computing](http://en.wikipedia.org/wiki/Distributed_computing) capabilities to your software.  It is scalable, customisable and completely asynchronous.  While many distributed computing solutions expose the consuming code to the details of work distribution and messaging, Outsorcery has been built from the ground up to be used in the exact same way as a local async method call.

Getting Started
---------------
The first thing to do when getting started with Outsorcery is define your first item of distributable work. The only requirements are that it implements IWorkItem< TResult > and that it is Serializable.  You should put all your work items into a class library so that both the server and client projects can reference them.

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

Performing the Work Locally
---------------------------
Local workers can be useful for testing or as a failsafe when the remote servers are unavailable.

```csharp
// Setup
var worker = new LocalWorker();

// Work
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```

Performing the Work Remotely
----------------------------
Getting started with distributed computing using Outsorcery has been designed to be as easy as possible. The projects [ExampleServer](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleServer) and [ExampleClient](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleClient) are examples of a simple implementation.

To be able to perform your work remotely, you'll need a server application.  Adding the below code to a new console application's Main() and a reference to your work item class library is all it takes.

```csharp
// *** REMINDER - Add a reference to your work item library in the 
//                server project or it won't know what it's receiving! ***
var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
new TcpWorkServer(localEndPoint).Run(cancellationToken).Wait();
```

And on your client application you'll add a connection provider so the outsourced worker knows where to go when distributing the work and swap LocalWorker for OutsourcedWorker.

```csharp
// Setup
var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
var provider = new SingleTcpWorkerConnectionProvider(localEndPoint);
var worker = new OutsourcedWorker(provider);

// Work
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```


