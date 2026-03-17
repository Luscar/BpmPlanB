/**
 * BpmPlanB - Business Process Management Engine
 * Type definitions
 */

// ─── Variable definitions ────────────────────────────────────────────────────

export type VariableType = 'string' | 'number' | 'boolean' | 'object' | 'array';

export interface VariableDefinition {
  name: string;
  type: VariableType;
  default?: unknown;
}

// ─── Parameter values ────────────────────────────────────────────────────────

/**
 * A parameter value can be:
 *   - A static literal value (string, number, boolean, null)
 *   - A variable reference: { $var: "variableName" }
 *   - An object or array composed of the above
 */
export type StaticValue = string | number | boolean | null;

export interface VariableReference {
  $var: string;
}

export type ParameterValue =
  | StaticValue
  | VariableReference
  | { [key: string]: ParameterValue }
  | ParameterValue[];

export function isVariableReference(v: unknown): v is VariableReference {
  return typeof v === 'object' && v !== null && '$var' in v && typeof (v as VariableReference).$var === 'string';
}

/** Map of parameter name → value (static or variable reference) */
export type Parameters = Record<string, ParameterValue>;

// ─── Node types ──────────────────────────────────────────────────────────────

export interface StartNode {
  id: string;
  type: 'start';
  next: string;
}

export interface EndNode {
  id: string;
  type: 'end';
}

export interface CommandNode {
  id: string;
  type: 'command';
  command: string;
  /** Parameters passed to the command. Values may be static or variable references. */
  parameters?: Parameters;
  /** Variable to store the command result */
  resultVar?: string;
  next: string;
}

export interface QueryNode {
  id: string;
  type: 'query';
  query: string;
  /** Parameters passed to the query. Values may be static or variable references. */
  parameters?: Parameters;
  /** Variable to store the query result */
  resultVar?: string;
  next: string;
}

export interface DecisionBranch {
  condition: string;
  next: string;
}

export interface DecisionNode {
  id: string;
  type: 'decision';
  /**
   * Parameters available to condition expressions via the `params` object.
   * Values may be static or variable references.
   */
  parameters?: Parameters;
  branches: DecisionBranch[];
  /** Default branch when no condition matches */
  default: string;
}

export type ProcessNode = StartNode | EndNode | CommandNode | QueryNode | DecisionNode;

// ─── Process definition ──────────────────────────────────────────────────────

export interface ProcessDefinition {
  id: string;
  name: string;
  variables?: VariableDefinition[];
  nodes: ProcessNode[];
}

// ─── Runtime context ─────────────────────────────────────────────────────────

/** Variables live here during process execution */
export type VariableContext = Record<string, unknown>;

// ─── Handler interfaces ──────────────────────────────────────────────────────

export interface ResolvedParameters {
  [key: string]: unknown;
}

export interface CommandHandler {
  execute(parameters: ResolvedParameters, context: VariableContext): Promise<unknown>;
}

export interface QueryHandler {
  execute(parameters: ResolvedParameters, context: VariableContext): Promise<unknown>;
}

export interface EngineHandlers {
  commands: Record<string, CommandHandler>;
  queries: Record<string, QueryHandler>;
}

// ─── Execution result ────────────────────────────────────────────────────────

export interface ProcessResult {
  processId: string;
  completed: boolean;
  variables: VariableContext;
  executedNodes: string[];
}
