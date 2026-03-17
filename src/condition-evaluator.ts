import { ResolvedParameters, VariableContext } from './types';

/**
 * Evaluates a condition string in the context of the current variables and
 * the resolved parameters of the decision node.
 *
 * The condition is a JavaScript expression that has access to two objects:
 *   - `vars`   – the current process variable context
 *   - `params` – the resolved parameters of the decision node
 *
 * Examples:
 *   "vars.orderAmount > 100"
 *   "vars.status === 'approved'"
 *   "params.threshold > 0 && vars.balance >= params.threshold"
 *
 * @throws {Error} if the expression throws at runtime
 */
export function evaluateCondition(
  condition: string,
  variables: VariableContext,
  params: ResolvedParameters,
): boolean {
  // Build a safe evaluation function with `vars` and `params` in scope.
  // We intentionally avoid `eval` on arbitrary user code in production; for a
  // real system you would use a sandboxed expression parser (e.g. `expr-eval`,
  // `filtrex`, or `jexl`). This implementation uses the Function constructor
  // which is acceptable for trusted process definitions.
  try {
    // eslint-disable-next-line no-new-func
    const fn = new Function('vars', 'params', `"use strict"; return !!(${condition});`);
    return fn(variables, params) as boolean;
  } catch (err) {
    throw new Error(
      `Failed to evaluate condition "${condition}": ${(err as Error).message}`,
    );
  }
}
