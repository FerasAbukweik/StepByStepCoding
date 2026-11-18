export interface UpdateExamDTO {
  version: string;
  title: string;
  instructions: string | null;
  startsAt: string;
  deadline: string;
  durationMinutes: number;
}
