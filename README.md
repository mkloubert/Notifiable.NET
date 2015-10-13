# Notifiable.NET (C# 4.0)

Set of classes for easy and powerful implementation of [INotifyPropertyChanged](https://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged%28v=vs.110%29.aspx) based objects.

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
