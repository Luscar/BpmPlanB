# BpmPlanB

A lightweight Business Process Management (BPM) engine written in TypeScript.

## Table of Contents

- [Concepts](#concepts)
- [Process Definition](#process-definition)
- [Variables](#variables)
- [Parameter Values](#parameter-values)
  - [Static values](#static-values)
  - [Variable references](#variable-references)
- [Node types](#node-types)
  - [Start node](#start-node)
  - [End node](#end-node)
  - [Command node](#command-node)
  - [Query node](#query-node)
  - [Decision node](#decision-node)
- [Running a process](#running-a-process)
- [Complete example](#complete-example)

---

## Concepts

| Term | Description |
|------|-------------|
| **Process** | A graph of nodes that describes a business workflow |
| **Variable** | Named value that lives in the process context and can change during execution |
| **Command** | A write-side action (create, update, delete) handled by a `CommandHandler` |
| **Query** | A read-side operation handled by a `QueryHandler` |
| **Decision node** | A routing node that evaluates conditions and forwards to the matching branch |
| **Parameter** | A named input passed to a command, query, or decision node |

---

## Process Definition

```ts
const definition: ProcessDefinition = {
  id: 'order-process',
  name: 'Order Processing',
  variables: [...],
  nodes: [...],
};
```

---

## Variables

Declare variables at the process level. They can have a default value and are
available throughout the process.

```ts
variables: [
  { name: 'userId',      type: 'string' },
  { name: 'orderAmount', type: 'number', default: 0 },
  { name: 'isVip',       type: 'boolean', default: false },
]
```

Pass initial values at runtime:

```ts
engine.run(definition, {
  userId: 'u-42',
  orderAmount: 250,
});
```

Initial values **override** declared defaults.

---

## Parameter Values

Every node that accepts a `parameters` map can use either a **static value** or
a **variable reference** for each parameter.

### Static values

A literal string, number, boolean, or `null`.

```ts
parameters: {
  source:   'database',
  pageSize: 20,
  active:   true,
  filter:   null,
}
```

### Variable references

Use `{ $var: "variableName" }` to inject the current value of a variable into
any parameter — for commands, queries, **and decision nodes**.

```ts
parameters: {
  userId:   { $var: 'currentUserId' },   // resolved at runtime
  pageSize: 20,                           // static
}
```

Variable references are resolved **deeply**: you can embed them inside nested
objects or arrays.

```ts
parameters: {
  // Nested object
  filter: {
    owner:  { $var: 'currentUserId' },
    status: 'active',
  },
  // Array with a variable element
  ids: ['default-id', { $var: 'selectedId' }],
}
```

The engine throws a descriptive error at execution time if a referenced
variable does not exist in the context.

---

## Node types

### Start node

Entry point of the process. There must be exactly one start node.

```ts
{
  id:   'start',
  type: 'start',
  next: 'fetchUser',   // id of the next node
}
```

### End node

Terminates process execution.

```ts
{
  id:   'end',
  type: 'end',
}
```

### Command node

Executes a write-side action.

```ts
{
  id:        'createOrder',
  type:      'command',
  command:   'CreateOrder',           // name of the registered CommandHandler
  parameters: {
    userId:  { $var: 'currentUserId' },  // variable reference
    product: 'widget',                    // static value
    qty:     { $var: 'requestedQty' },   // variable reference
  },
  resultVar: 'newOrder',              // (optional) stores the return value
  next:      'notifyUser',
}
```

The `resultVar` stores whatever the handler returns, making the result
available to subsequent nodes as a variable.

### Query node

Executes a read-side operation.

```ts
{
  id:        'fetchUser',
  type:      'query',
  query:     'GetUser',               // name of the registered QueryHandler
  parameters: {
    id:      { $var: 'userId' },      // variable reference
    fields:  ['name', 'email'],       // static array
  },
  resultVar: 'user',                  // (optional) stores the return value
  next:      'decideRoute',
}
```

### Decision node

Routes execution to one of several branches based on JavaScript conditions.
Branches are evaluated **in order**; the first matching condition wins.
If no branch matches, the `default` node is used.

```ts
{
  id:   'decideRoute',
  type: 'decision',

  // Parameters are resolved (including variable references) before conditions
  // are evaluated. Use them in conditions via the `params` object.
  parameters: {
    minOrderAmount: { $var: 'vipThreshold' },   // variable reference
    currency:       'EUR',                        // static value
  },

  branches: [
    // `vars`   → current process variable context
    // `params` → resolved parameters of this decision node
    { condition: 'vars.isVip && vars.orderAmount >= params.minOrderAmount', next: 'vipPath' },
    { condition: 'vars.orderAmount > 0',                                    next: 'normalPath' },
  ],

  default: 'emptyOrderPath',
}
```

#### Condition expressions

Conditions are JavaScript expressions that have access to two objects:

| Object | Contents |
|--------|----------|
| `vars` | All current process variables |
| `params` | Resolved parameters of the decision node |

Examples:

```js
// Simple variable check
"vars.status === 'approved'"

// Numeric comparison
"vars.balance > 0"

// Using a resolved parameter
"vars.amount >= params.threshold"

// Combined
"vars.isVip && vars.orderAmount >= params.minOrderAmount"
```

> **Tip:** Use `parameters` in the decision node to avoid hardcoding thresholds.
> Instead of `"vars.amount > 100"`, declare
> `parameters: { threshold: { $var: 'dynamicThreshold' } }`
> and write `"vars.amount > params.threshold"`.
> This keeps the routing logic data-driven.

---

## Running a process

```ts
import { ProcessEngine, EngineHandlers } from 'bpmplanb';

const handlers: EngineHandlers = {
  commands: {
    CreateOrder: {
      async execute(params, vars) {
        // params are already resolved — no $var objects remain
        const order = await db.orders.create(params);
        return order;
      },
    },
  },
  queries: {
    GetUser: {
      async execute(params, vars) {
        return db.users.findById(params.id);
      },
    },
  },
};

const engine = new ProcessEngine(handlers);

const result = await engine.run(definition, {
  userId: 'u-42',
  orderAmount: 250,
});

console.log(result.variables);      // final variable state
console.log(result.executedNodes);  // ordered list of executed node ids
```

---

## Complete example

```ts
import { ProcessEngine, ProcessDefinition } from 'bpmplanb';

const definition: ProcessDefinition = {
  id: 'checkout',
  name: 'Checkout Process',
  variables: [
    { name: 'userId',       type: 'string' },
    { name: 'orderAmount',  type: 'number' },
    { name: 'vipThreshold', type: 'number', default: 200 },
    { name: 'discountRate', type: 'number', default: 0 },
  ],
  nodes: [
    { id: 'start', type: 'start', next: 'getUser' },

    // Query: fetch user profile — userId injected from variable
    {
      id:        'getUser',
      type:      'query',
      query:     'GetUser',
      parameters: { id: { $var: 'userId' } },
      resultVar: 'userProfile',
      next:      'checkVip',
    },

    // Decision: route based on variables and a variable-backed threshold
    {
      id:   'checkVip',
      type: 'decision',
      parameters: {
        threshold: { $var: 'vipThreshold' },  // threshold comes from a variable
      },
      branches: [
        {
          // vars.userProfile was populated by the previous query
          // params.threshold was resolved from the vipThreshold variable
          condition: 'vars.userProfile.isVip && vars.orderAmount >= params.threshold',
          next: 'applyVipDiscount',
        },
        {
          condition: 'vars.orderAmount > 0',
          next: 'processPayment',
        },
      ],
      default: 'cancelOrder',
    },

    // Command: apply discount — rate injected from variable
    {
      id:      'applyVipDiscount',
      type:    'command',
      command: 'ApplyDiscount',
      parameters: {
        userId: { $var: 'userId' },
        rate:   { $var: 'discountRate' },
      },
      next: 'processPayment',
    },

    // Command: process payment — both params from variables
    {
      id:      'processPayment',
      type:    'command',
      command: 'ProcessPayment',
      parameters: {
        userId: { $var: 'userId' },
        amount: { $var: 'orderAmount' },
      },
      resultVar: 'paymentResult',
      next:      'end',
    },

    {
      id:      'cancelOrder',
      type:    'command',
      command: 'CancelOrder',
      parameters: { userId: { $var: 'userId' } },
      next:    'end',
    },

    { id: 'end', type: 'end' },
  ],
};

const engine = new ProcessEngine(handlers);
const result = await engine.run(definition, {
  userId:       'u-42',
  orderAmount:  300,
  discountRate: 0.15,
});
```
