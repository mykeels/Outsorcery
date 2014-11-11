Outsorcery
==========

Introduction
------------
Outsorcery is a library for adding [distributed computing](http://en.wikipedia.org/wiki/Distributed_computing) capabilities to your software.  It is scaleable, customisable and completely asynchronous.  While many distributed computing solutions expose the consuming code to the details of work distribution and messaging, Outsorcery has been built from the ground up to be used in the exact same way as a local async operation.

Getting Started
---------------
The first thing you will need when getting started with Outsorcery is to define your first item of distributable work. The only requirements are that it implements IWorkItem<TResult> and that it is Serializable.  You'll need to put work items in their own class library so that both the server and client projects can reference them.

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
To test your first work item locally use a LocalWorker.  LocalWorkers can be useful for testing or for falling back on when the remote servers are unavailable.

```csharp
// Setup
var worker = new LocalWorker();

// Work
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```

Performing the Work Remotely
----------------------------
Getting started in distributed computing using Outsorcery has been designed to be as easy as possible. The projects [ExampleServer](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleServer) and [ExampleClient](https://github.com/SteveLillis/Outsorcery/tree/master/Outsorcery.ExampleClient) are examples of a simple implementation.

Programming for distributed work is almost identical to programming for local work with just a couple of additions.  First of all you'll need a separate application to run as the server. Adding the below code and a reference to your work item library to a new console application is all it takes.

```csharp
// *** REMINDER - Add a reference to your work item library in the server project or it won't know what it's receiving! ***
var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
new TcpWorkServer(localEndPoint).Run(cancellationToken).Wait();
```

And on your client application you'll need to swap LocalWorker for OutsourcedWorker and add a connection provider so the outsourced worker knows where to go when distributing the work.

```csharp
// Setup
var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
var provider = new SingleTcpWorkerConnectionProvider(localEndPoint);
var worker = new OutsourcedWorker(provider);

// Work
var workItem = new MyFirstWorkItem { TestValue = 11 };
var result = await worker.DoWorkAsync(workItem, new CancellationToken());
```


