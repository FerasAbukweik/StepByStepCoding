import { ProgrammingLanguage } from '../../enums/programming-language';

export interface SubmissionListDTO {
  id: string;
  version: string;
  attemptId: string;
  questionId: string;
  language: ProgrammingLanguage;
  submittedAt: string;
  score: number | null;
  reviewedBy: string | null;
  reviewedAt: string | null;
}
