import {
  CommandNode,
  DecisionNode,
  EngineHandlers,
  ProcessDefinition,
  ProcessNode,
  ProcessResult,
  QueryNode,
  VariableContext,
} from './types';
import { resolveParameters } from './variable-resolver';
import { evaluateCondition } from './condition-evaluator';

export class ProcessEngine {
  private handlers: EngineHandlers;

  constructor(handlers: EngineHandlers) {
    this.handlers = handlers;
  }

  /**
   * Execute a process definition with an initial variable context.
   *
   * @param definition   The process definition to run
   * @param initialVars  Initial variable values (merged with declared defaults)
   */
  async run(
    definition: ProcessDefinition,
    initialVars: VariableContext = {},
  ): Promise<ProcessResult> {
    // Build variable context: start with declared defaults, then apply initial values
    const variables: VariableContext = {};
    for (const varDef of definition.variables ?? []) {
      if (varDef.default !== undefined) {
        variables[varDef.name] = varDef.default;
      }
    }
    Object.assign(variables, initialVars);

    const nodeMap = new Map<string, ProcessNode>(
      definition.nodes.map((n) => [n.id, n]),
    );

    const executedNodes: string[] = [];
    let currentNodeId = this.findStartNode(definition);

    while (currentNodeId !== null) {
      const node = nodeMap.get(currentNodeId);
      if (!node) {
        throw new Error(`Node "${currentNodeId}" not found in process "${definition.id}"`);
      }

      executedNodes.push(node.id);

      switch (node.type) {
        case 'start':
          currentNodeId = node.next;
          break;

        case 'end':
          currentNodeId = null;
          break;

        case 'command':
          currentNodeId = await this.executeCommand(node, variables);
          break;

        case 'query':
          currentNodeId = await this.executeQuery(node, variables);
          break;

        case 'decision':
          currentNodeId = this.executeDecision(node, variables);
          break;

        default:
          throw new Error(`Unknown node type: ${(node as ProcessNode).type}`);
      }
    }

    return {
      processId: definition.id,
      completed: true,
      variables,
      executedNodes,
    };
  }

  // ─── Private helpers ────────────────────────────────────────────────────────

  private findStartNode(definition: ProcessDefinition): string {
    const start = definition.nodes.find((n) => n.type === 'start');
    if (!start) {
      throw new Error(`Process "${definition.id}" has no start node`);
    }
    return start.id;
  }

  private async executeCommand(
    node: CommandNode,
    variables: VariableContext,
  ): Promise<string> {
    const handler = this.handlers.commands[node.command];
    if (!handler) {
      throw new Error(`No handler registered for command "${node.command}"`);
    }

    // Resolve parameters: static values stay as-is, { $var } references are
    // replaced with the current variable value
    const resolvedParams = resolveParameters(node.parameters, variables);

    const result = await handler.execute(resolvedParams, variables);

    if (node.resultVar !== undefined) {
      variables[node.resultVar] = result;
    }

    return node.next;
  }

  private async executeQuery(
    node: QueryNode,
    variables: VariableContext,
  ): Promise<string> {
    const handler = this.handlers.queries[node.query];
    if (!handler) {
      throw new Error(`No handler registered for query "${node.query}"`);
    }

    // Resolve parameters: static values stay as-is, { $var } references are
    // replaced with the current variable value
    const resolvedParams = resolveParameters(node.parameters, variables);

    const result = await handler.execute(resolvedParams, variables);

    if (node.resultVar !== undefined) {
      variables[node.resultVar] = result;
    }

    return node.next;
  }

  private executeDecision(
    node: DecisionNode,
    variables: VariableContext,
  ): string {
    // Resolve parameters so they can be used inside condition expressions
    // via the `params` object (e.g. "params.threshold > vars.balance")
    const resolvedParams = resolveParameters(node.parameters, variables);

    for (const branch of node.branches) {
      if (evaluateCondition(branch.condition, variables, resolvedParams)) {
        return branch.next;
      }
    }

    return node.default;
  }
}
