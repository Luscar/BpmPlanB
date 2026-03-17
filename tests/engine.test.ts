import { ProcessEngine } from '../src/engine';
import {
  CommandHandler,
  EngineHandlers,
  ProcessDefinition,
  QueryHandler,
  ResolvedParameters,
  VariableContext,
} from '../src/types';

// ─── Helpers ──────────────────────────────────────────────────────────────────

function makeHandlers(
  commands: Record<string, (p: ResolvedParameters) => Promise<unknown>> = {},
  queries: Record<string, (p: ResolvedParameters) => Promise<unknown>> = {},
): EngineHandlers {
  const wrap = (
    fns: Record<string, (p: ResolvedParameters) => Promise<unknown>>,
  ): Record<string, CommandHandler | QueryHandler> =>
    Object.fromEntries(
      Object.entries(fns).map(([k, fn]) => [
        k,
        { execute: (p: ResolvedParameters, _ctx: VariableContext) => fn(p) },
      ]),
    );

  return {
    commands: wrap(commands) as Record<string, CommandHandler>,
    queries: wrap(queries) as Record<string, QueryHandler>,
  };
}

// ─── Tests: Command node ──────────────────────────────────────────────────────

describe('Command node', () => {
  test('passes static parameters to handler', async () => {
    let received: ResolvedParameters = {};
    const handlers = makeHandlers({
      SendEmail: async (p) => { received = p; },
    });
    const process: ProcessDefinition = {
      id: 'p1',
      name: 'Test',
      nodes: [
        { id: 'start', type: 'start', next: 'sendEmail' },
        {
          id: 'sendEmail',
          type: 'command',
          command: 'SendEmail',
          parameters: { to: 'alice@example.com', subject: 'Hello' },
          next: 'end',
        },
        { id: 'end', type: 'end' },
      ],
    };
    await new ProcessEngine(handlers).run(process);
    expect(received).toEqual({ to: 'alice@example.com', subject: 'Hello' });
  });

  test('resolves variable references in parameters', async () => {
    let received: ResolvedParameters = {};
    const handlers = makeHandlers({
      SendEmail: async (p) => { received = p; },
    });
    const process: ProcessDefinition = {
      id: 'p2',
      name: 'Test',
      variables: [{ name: 'recipientEmail', type: 'string' }],
      nodes: [
        { id: 'start', type: 'start', next: 'sendEmail' },
        {
          id: 'sendEmail',
          type: 'command',
          command: 'SendEmail',
          parameters: {
            to: { $var: 'recipientEmail' },
            subject: 'Hello',
          },
          next: 'end',
        },
        { id: 'end', type: 'end' },
      ],
    };
    await new ProcessEngine(handlers).run(process, { recipientEmail: 'bob@example.com' });
    expect(received).toEqual({ to: 'bob@example.com', subject: 'Hello' });
  });

  test('stores command result in resultVar', async () => {
    const handlers = makeHandlers({
      CreateOrder: async () => ({ orderId: 'ord-99' }),
    });
    const process: ProcessDefinition = {
      id: 'p3',
      name: 'Test',
      nodes: [
        { id: 'start', type: 'start', next: 'createOrder' },
        {
          id: 'createOrder',
          type: 'command',
          command: 'CreateOrder',
          parameters: { product: 'widget' },
          resultVar: 'order',
          next: 'end',
        },
        { id: 'end', type: 'end' },
      ],
    };
    const result = await new ProcessEngine(handlers).run(process);
    expect(result.variables['order']).toEqual({ orderId: 'ord-99' });
  });
});

// ─── Tests: Query node ────────────────────────────────────────────────────────

describe('Query node', () => {
  test('resolves variable references in query parameters', async () => {
    let received: ResolvedParameters = {};
    const handlers = makeHandlers(
      {},
      { GetUser: async (p) => { received = p; return { name: 'Alice' }; } },
    );
    const process: ProcessDefinition = {
      id: 'p4',
      name: 'Test',
      variables: [{ name: 'userId', type: 'string' }],
      nodes: [
        { id: 'start', type: 'start', next: 'getUser' },
        {
          id: 'getUser',
          type: 'query',
          query: 'GetUser',
          parameters: {
            id: { $var: 'userId' },
            fields: ['name', 'email'],
          },
          resultVar: 'user',
          next: 'end',
        },
        { id: 'end', type: 'end' },
      ],
    };
    const result = await new ProcessEngine(handlers).run(process, { userId: 'u-42' });
    expect(received).toEqual({ id: 'u-42', fields: ['name', 'email'] });
    expect(result.variables['user']).toEqual({ name: 'Alice' });
  });
});

// ─── Tests: Decision node ─────────────────────────────────────────────────────

describe('Decision node', () => {
  test('routes to correct branch using static condition on vars', async () => {
    const handlers = makeHandlers();
    const process: ProcessDefinition = {
      id: 'p5',
      name: 'Test',
      variables: [{ name: 'amount', type: 'number' }],
      nodes: [
        { id: 'start', type: 'start', next: 'decide' },
        {
          id: 'decide',
          type: 'decision',
          branches: [
            { condition: 'vars.amount > 100', next: 'highValue' },
            { condition: 'vars.amount > 0', next: 'lowValue' },
          ],
          default: 'noValue',
        },
        { id: 'highValue', type: 'end' },
        { id: 'lowValue', type: 'end' },
        { id: 'noValue', type: 'end' },
      ],
    };

    const r1 = await new ProcessEngine(handlers).run(process, { amount: 200 });
    expect(r1.executedNodes).toContain('highValue');

    const r2 = await new ProcessEngine(handlers).run(process, { amount: 50 });
    expect(r2.executedNodes).toContain('lowValue');

    const r3 = await new ProcessEngine(handlers).run(process, { amount: 0 });
    expect(r3.executedNodes).toContain('noValue');
  });

  test('decision node resolves variable references in parameters', async () => {
    const handlers = makeHandlers();
    const process: ProcessDefinition = {
      id: 'p6',
      name: 'Test',
      variables: [
        { name: 'balance', type: 'number' },
        { name: 'threshold', type: 'number' },
      ],
      nodes: [
        { id: 'start', type: 'start', next: 'decide' },
        {
          id: 'decide',
          type: 'decision',
          // threshold is taken from a variable, not hardcoded
          parameters: { minAmount: { $var: 'threshold' } },
          branches: [
            { condition: 'vars.balance >= params.minAmount', next: 'approved' },
          ],
          default: 'rejected',
        },
        { id: 'approved', type: 'end' },
        { id: 'rejected', type: 'end' },
      ],
    };

    const r1 = await new ProcessEngine(handlers).run(process, { balance: 500, threshold: 200 });
    expect(r1.executedNodes).toContain('approved');

    const r2 = await new ProcessEngine(handlers).run(process, { balance: 100, threshold: 200 });
    expect(r2.executedNodes).toContain('rejected');
  });

  test('decision node uses default when no branch matches', async () => {
    const handlers = makeHandlers();
    const process: ProcessDefinition = {
      id: 'p7',
      name: 'Test',
      nodes: [
        { id: 'start', type: 'start', next: 'decide' },
        {
          id: 'decide',
          type: 'decision',
          branches: [{ condition: 'false', next: 'never' }],
          default: 'fallback',
        },
        { id: 'never', type: 'end' },
        { id: 'fallback', type: 'end' },
      ],
    };
    const result = await new ProcessEngine(handlers).run(process);
    expect(result.executedNodes).toContain('fallback');
    expect(result.executedNodes).not.toContain('never');
  });
});

// ─── Tests: Variable defaults & chaining ─────────────────────────────────────

describe('Variable defaults and chaining', () => {
  test('applies declared variable defaults', async () => {
    const handlers = makeHandlers();
    const process: ProcessDefinition = {
      id: 'p8',
      name: 'Test',
      variables: [{ name: 'pageSize', type: 'number', default: 20 }],
      nodes: [
        { id: 'start', type: 'start', next: 'end' },
        { id: 'end', type: 'end' },
      ],
    };
    const result = await new ProcessEngine(handlers).run(process);
    expect(result.variables['pageSize']).toBe(20);
  });

  test('initial vars override variable defaults', async () => {
    const handlers = makeHandlers();
    const process: ProcessDefinition = {
      id: 'p9',
      name: 'Test',
      variables: [{ name: 'pageSize', type: 'number', default: 20 }],
      nodes: [
        { id: 'start', type: 'start', next: 'end' },
        { id: 'end', type: 'end' },
      ],
    };
    const result = await new ProcessEngine(handlers).run(process, { pageSize: 50 });
    expect(result.variables['pageSize']).toBe(50);
  });

  test('result of query feeds into command via variable', async () => {
    const handlers = makeHandlers(
      { ActivateUser: async (p) => `activated:${p.id}` },
      { GetUserId: async () => 'u-99' },
    );
    const process: ProcessDefinition = {
      id: 'p10',
      name: 'Test',
      nodes: [
        { id: 'start', type: 'start', next: 'getUser' },
        {
          id: 'getUser',
          type: 'query',
          query: 'GetUserId',
          resultVar: 'fetchedId',
          next: 'activate',
        },
        {
          id: 'activate',
          type: 'command',
          command: 'ActivateUser',
          parameters: { id: { $var: 'fetchedId' } },
          resultVar: 'activationResult',
          next: 'end',
        },
        { id: 'end', type: 'end' },
      ],
    };
    const result = await new ProcessEngine(handlers).run(process);
    expect(result.variables['activationResult']).toBe('activated:u-99');
  });
});
