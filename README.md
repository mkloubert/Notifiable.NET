# Notifiable.NET

Set of classes for easy and powerful implementation / use of [INotifyPropertyChanged](https://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged%28v=vs.110%29.aspx) and [INotifyCollectionChanged](https://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged%28v=vs.110%29.aspx) based objects.

## Branches

| Name  | Targets on  |
| ----- | ----------- |
| master (current)  | C# 4.0  |
| [CSharp5](https://github.com/mkloubert/Notifiable.NET/tree/CSharp5)  | C# 5.0  |
| [Portable8](https://github.com/mkloubert/Notifiable.NET/tree/Portable8)  | C# 4.0, .NET 4.5, Silverlight 5, Windows 8, Windows Phone 8.1 + 8 (Silverlight)  |

## Install

### NuGet

Visit [NuGet site](https://www.nuget.org/packages/MarcelJoachimKloubert.Notifiable.dll) or enter the following command:

```powershell
Install-Package MarcelJoachimKloubert.Notifiable.dll
```

## Notifiable objects

The following example demonstrates how to implement a simple ViewModel object:

### Basic usage

```csharp
using System;
using MarcelJoachimKloubert.ComponentModel;

class MyViewModel : NotifiableBase {
    public object Value1 {
        get { return this.Get(() => this.Value1); }
        
        set { this.Set(value, () => this.Value1); }
    }
}

class Program {
    static void Main() {
       var vm = new MyViewModel();
       vm.PropertyChanged += (sender, e) =>
           {
               Console.WriteLine("Property '{0}' has been changed.",
                                 e.PropertyName);
           };
           
       // this will invoke the 'PropertyChanged' event
       vm.Value1 = "Hello, world!";
       
       // this will NOT invoke the 'PropertyChanged' event
       // because the value has not been changed
       vm.Value1 = "Hello, world!";
    }
}
```

### Auto notification

You can use the `ReceiveNotificationFrom` attribute if you want raise the `PropertyChanged` event for a property if the value of another property has been changed:

```csharp
class MyViewModel : NotifiableBase {
    public string Name {
        get { return this.Get(() => this.Name); }
        
        set { this.Set(value, () => this.Name); }
    }
    
    [ReceiveNotificationFrom("Name")]
    public string UpperName {
        get {
            if (this.Name == null) {
                return null;
            }
        
            return this.Name.ToUpper();
        }
    }
}
```

If you change the value of `Name` property, the object will also raise the property changed event for the `UpperName` property.

Another way to do this is to use the `ReceiveValueFrom` attribute.

This makes it possible to invoke a method, field or property when a values has been changed:

```csharp
class MyViewModel : NotifiableBase {
    private string _upperName;

    public string Name {
        get { return this.Get(() => this.Name); }
        
        set { this.Set(value, () => this.Name); }
    }
    
    [ReceiveNotificationFrom("Name")]
    public string UpperName {
        get { return this._upperName; }
    }
    
    [ReceiveValueFrom("Name")]
    protected void OnNameChanged(IReceiveValueFromArgs args) {
        var newUpperName = (string)args.NewValue;
        if (newUpperName != null) {
            newUpperName = newUpperName.ToUpper();
        }
        
        this._upperName = newUpperName;
    }
}
```

In that example the `OnNameChanged()` method is invoked when the value of `Name` has been changed.

After that a property changed event is raised for the `UpperName` property.

### Work thread safe

The `NotifiableBase` class uses an [IDictionary&lt;string, object&gt;](https://msdn.microsoft.com/en-us/library/s4ys34ea(v=vs.110).aspx) instance to store the properties and theirs values.

By default a [Dictionary&lt;string, object&gt;](https://msdn.microsoft.com/en-us/library/xfhwa508%28v=vs.110%29.aspx) instance is created.

The problem, that this class is NOT thread safe!

Now you have two options:

The first (and not recommed) one is to use the `InvokeThreadSafe()` method:

```csharp
class MyViewModel : NotifiableBase {
    public string Name {
        get {
            return this.InvokeThreadSafe(func: (obj) => this.Get(() => this.Name));
        }
        
        set {
            this.InvokeThreadSafe(action: (obj, state) => this.Set(state, () => this.Name),
                                  actionState: value);
        }
    }
}
```

The method uses the object from `SyncRoot` property for the internal used [lock](https://msdn.microsoft.com/en-us/library/vstudio/c5kehkcz%28v=vs.100%29.aspx) statement.

The problem is that you have to do this for all notifiable properties!

The seond and BETTER WAY is to initialize an own instance of the property storage.

Overwrite the `CreatePropertyStorage()` method and return an instance of a thread safe dictionary, like [ConcurrentDictionary](https://msdn.microsoft.com/en-us/library/dd287191%28v=vs.110%29.aspx) (or an own one):

```csharp
using System.Collections.Concurrent;
using System.Collections.Generic;
using MarcelJoachimKloubert.ComponentModel;

class MyViewModel : NotifiableBase {
    protected override IDictionary<string, object> CreatePropertyStorage() {
        return new ConcurrentDictionary<string, object>();
    }
    
    
    public string Name {
        get { return this.Get(() => this.Name); }
        
        set { this.Set(value, () => this.Name); }
    }
}
```

And this is something that is done once in the constructor of the `NotifiableBase` class.
