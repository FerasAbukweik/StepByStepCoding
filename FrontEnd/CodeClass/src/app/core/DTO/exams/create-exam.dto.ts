export interface CreateExamDTO {
  sectionId: string;
  title: string;
  instructions: string | null;
  startsAt: string;
  deadline: string;
  durationMinutes: number;
}
