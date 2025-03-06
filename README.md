# DevEn.Xrm.Observables

DevEn.Xrm.Observables is a library that provides an observable entity class for tracking changes to attributes and allowing subscriptions to attribute changes. This library is built on top of Microsoft.Xrm.Sdk and targets .NET Framework 4.8.

## Features

- Track changes to entity attributes.
- Subscribe to attribute changes with delegates.
- Implicit conversions between `ObservableEntity` and `Entity`.

## Installation

To install DevEn.Xrm.Observables, add the following package to your project:
## Usage

### Creating an Observable Entity

You can create an `ObservableEntity` from an existing entity or by specifying a logical name.
### Accessing and Modifying Attributes

You can access and modify attributes using the indexer or the `GetValue` and `SetValue` methods.
### Subscribing to Attribute Changes

You can subscribe to attribute changes using the `AddOnChange` method and unsubscribe using the `RemoveOnChange` method.
### Invoking Change Delegates

You can manually invoke all change delegates or specific delegates for a given attribute.
### Implicit Conversions

`ObservableEntity` supports implicit conversions to and from `Entity` and logical names.
## Unit Tests

The library includes unit tests to ensure the functionality of the `ObservableEntity` class. Below are some examples of the unit tests.

### Indexer Get and Set
### GetValue and SetValue
### AddOnChange and RemoveOnChange
## Integration Tests

The library also includes integration tests to ensure the overall functionality of the `ObservableEntity` class.

### Implicit Conversions
## License

This project is licensed under the MIT License
