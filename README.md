# Notifiable.NET

Set of classes for easy and powerful implementation / use of [INotifyPropertyChanged](https://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged%28v=vs.110%29.aspx) and [INotifyCollectionChanged](https://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged%28v=vs.110%29.aspx) based objects.

## Branches

| Name  | Targets on  |
| ----- | ----------- |
| master (current)  | C# 4.0  |
| [CSharp5](https://github.com/mkloubert/Notifiable.NET/tree/CSharp5)  | C# 5.0  |

### nuget

Visit [nuget site](https://www.nuget.org/packages/MarcelJoachimKloubert.Notifiable.dll) or enter the following command:

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

