import { ProgrammingLanguage } from '../../enums/programming-language';

export interface SubmissionDetailDTO {
  id: string;
  version: string;
  attemptId: string;
  questionId: string;
  submittedCode: string;
  language: ProgrammingLanguage;
  submittedAt: string;
  score: number | null;
  feedback: string | null;
  reviewedBy: string | null;
  reviewedAt: string | null;
}
