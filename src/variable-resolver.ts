import {
  isVariableReference,
  ParameterValue,
  Parameters,
  ResolvedParameters,
  VariableContext,
} from './types';

/**
 * Resolves a single parameter value against the current variable context.
 *
 * Rules:
 *   - `{ $var: "name" }` → looks up `name` in `variables`
 *   - Plain string/number/boolean/null → returned as-is
 *   - Object → each property is resolved recursively
 *   - Array → each element is resolved recursively
 *
 * @throws {Error} when a referenced variable does not exist in the context
 */
export function resolveValue(value: ParameterValue, variables: VariableContext): unknown {
  // Variable reference: { $var: "variableName" }
  if (isVariableReference(value)) {
    const varName = value.$var;
    if (!(varName in variables)) {
      throw new Error(`Variable "${varName}" is not defined in the current context`);
    }
    return variables[varName];
  }

  // Array: resolve each element
  if (Array.isArray(value)) {
    return value.map((item) => resolveValue(item, variables));
  }

  // Plain object: resolve each property value
  if (typeof value === 'object' && value !== null) {
    const resolved: Record<string, unknown> = {};
    for (const [k, v] of Object.entries(value)) {
      resolved[k] = resolveValue(v as ParameterValue, variables);
    }
    return resolved;
  }

  // Primitive (string, number, boolean, null)
  return value;
}

/**
 * Resolves all parameters in a parameter map against the current variable context.
 *
 * Example:
 * ```
 * const params = {
 *   userId:  { $var: "currentUserId" },   // resolved from variables
 *   source:  "database",                  // static string
 *   limit:   50,                          // static number
 *   filters: { active: { $var: "isActive" } },  // nested variable reference
 * };
 * const resolved = resolveParameters(params, { currentUserId: "u-42", isActive: true });
 * // → { userId: "u-42", source: "database", limit: 50, filters: { active: true } }
 * ```
 */
export function resolveParameters(
  parameters: Parameters | undefined,
  variables: VariableContext,
): ResolvedParameters {
  if (!parameters) return {};

  const resolved: ResolvedParameters = {};
  for (const [key, value] of Object.entries(parameters)) {
    resolved[key] = resolveValue(value, variables);
  }
  return resolved;
}
