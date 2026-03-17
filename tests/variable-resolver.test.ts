import { resolveValue, resolveParameters } from '../src/variable-resolver';
import { VariableContext } from '../src/types';

describe('resolveValue', () => {
  const ctx: VariableContext = {
    userId: 'u-42',
    amount: 150,
    active: true,
  };

  test('returns static string as-is', () => {
    expect(resolveValue('hello', ctx)).toBe('hello');
  });

  test('returns static number as-is', () => {
    expect(resolveValue(99, ctx)).toBe(99);
  });

  test('returns static boolean as-is', () => {
    expect(resolveValue(false, ctx)).toBe(false);
  });

  test('returns null as-is', () => {
    expect(resolveValue(null, ctx)).toBeNull();
  });

  test('resolves variable reference', () => {
    expect(resolveValue({ $var: 'userId' }, ctx)).toBe('u-42');
  });

  test('resolves numeric variable reference', () => {
    expect(resolveValue({ $var: 'amount' }, ctx)).toBe(150);
  });

  test('resolves boolean variable reference', () => {
    expect(resolveValue({ $var: 'active' }, ctx)).toBe(true);
  });

  test('throws when variable is not defined', () => {
    expect(() => resolveValue({ $var: 'missing' }, ctx)).toThrow(
      'Variable "missing" is not defined',
    );
  });

  test('resolves nested object with variable reference', () => {
    const result = resolveValue({ id: { $var: 'userId' }, source: 'db' }, ctx);
    expect(result).toEqual({ id: 'u-42', source: 'db' });
  });

  test('resolves array with mixed static and variable values', () => {
    const result = resolveValue(['static', { $var: 'userId' }, 42], ctx);
    expect(result).toEqual(['static', 'u-42', 42]);
  });
});

describe('resolveParameters', () => {
  const ctx: VariableContext = { currentUserId: 'u-7', limit: 10 };

  test('returns empty object for undefined parameters', () => {
    expect(resolveParameters(undefined, ctx)).toEqual({});
  });

  test('resolves mixed static and variable parameters', () => {
    const result = resolveParameters(
      {
        userId: { $var: 'currentUserId' },
        source: 'database',
        pageSize: { $var: 'limit' },
      },
      ctx,
    );
    expect(result).toEqual({ userId: 'u-7', source: 'database', pageSize: 10 });
  });
});
