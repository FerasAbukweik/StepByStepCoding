export interface ExecutionStageDTO {
  stdout: string;
  stderr: string;
  exitCode: number | null;
  signal: string | null;
  status: string | null;
  message: string | null;
  wallTimeMilliseconds: number | null;
}

export interface CodeExecutionResultDTO {
  language: string;
  version: string;
  compile: ExecutionStageDTO | null;
  run: ExecutionStageDTO | null;
}
