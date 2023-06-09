# Sample project to play with NServiceBus.Router

## Components of the system

| Endpoint                        | Notes                                      |
|---------------------------------|--------------------------------------------|
| `NsbBridgePlayground.Sender`    |                                            |
| `NsbBridgePlayground.Receiver`  |                                            |
| `NsbBridgePlayground.Notifier`  |                                            |
| `NsbBridgePlayground.Router`    |                                            |
| `NsbBridgePlayground.Common`    | Messages and infrastructure components     |
| `NsbBridgePlayground.Bootstrap` | Component to configure and start endpoints |

## Behavior

### Without the routrer

```puml
@startuml

Sender: sends ""CreateVendor"" command
Sender: processes ""CreateVendorResponse"" message

Receiver: processes ""CreateVendor"" command
Receiver: publishes ""VendorCreated"" event
Receiver: replies ""CreateVendorResponse"" message

Notifier: processes ""VendorCreated"" event

Sender --> Receiver : ""CreateVendor""
Receiver --> Sender : ""CreateVendorResponse""    

Receiver -[dotted]> Notifier : ""VendorCreated""
```

### Using the router

```puml
@startuml

legend top right
| Pattern  | |
| ""----"" | //Original// message        |
| ""...."" | Message //moved// by Router |
endlegend

Sender: sends ""CreateVendor"" command
Sender: processes ""CreateVendorResponse"" message

Router: forwards ""CreateVendor"" command to ""Receiver""
Router: forwards ""CreateVendorResponse"" message to ""Sender""
Router: forwards ""VendorCreated"" event to ""Notifier""

Sender -[#red,dashed]right-> Router : **(1)** ""CreateVendor""

Router -[#red,dotted]right-> Receiver : **(2)** ""CreateVendor""  

Receiver: process ""CreateVendor"" command
Receiver: publish ""VendorCreated"" event
Receiver: reply ""CreateVendorResponse"" message

Notifier: process ""VendorCreated"" events
 
Receiver -[#green,dashed]> Router : **(3)** ""VendorCreated""
Receiver -[#magenta,dashed]> Router : **(3)** ""CreateVendorResponse""
Router -[#green,dotted]-> Notifier : **(4)** ""VendorCreated""
Router -[#magenta,dotted]-> Sender : **(4)** ""CreateVendorResponse""
```

### How it works

These are relevant table in each database

| Database                       | Table                  | Used for                                                                                                     |
|--------------------------------|------------------------|--------------------------------------------------------------------------------------------------------------|
| `NsbRouterPlayground.Sender`   | `Sender`               | Input queue for `Sender`; receives messages for `Sender`, i.e. replies, subscription requests, ...           |
|                                | `SubscriptionRouting`  | Subscription data when native pub/sub is supported (`NserviceBus.SqlServer` v5.0 or later)                   |
|                                | `Router`               | Input queue for router; stores messages to be forwarded to other parts of the system                         |
| `NsbRouterPlayground.Receiver` | `Receiver`             | Input queue for `Receiver`; receives messages (forwarded by router), e.g. commands `Receiver` should process |
|                                | `SubscriptionRouting`  | Subscription data when native pub/sub is supported (`NserviceBus.SqlServer` v5.0 or later)                   |
|                                | `Router`               | Input queue for router; stores messages to be forwarded to other parts of the system                         |
| `NsbRouterPlayground.Notifier` | `Notifier`             | Input queue for `Notifier`; receives messages (forwarded by router) for `Notifier`, e.g. events              |
|                                | `SubscriptionRouting`  | Subscription data when native pub/sub is supported (`NserviceBus.SqlServer` v5.0 or later)                   |
|                                | `Router`               | Input queue for router; stores messages to be forwarded to other parts of the system                         |

> Assuming `NserviceBus.SqlServer` v5 or later is used

> `Router` is added by the router, i.e. it is not created as part of the endpoint initialization, as it happens for other tables for the endpoint (assuming `EndpointConfiguration.EnableInstallers` is called, of course). 
