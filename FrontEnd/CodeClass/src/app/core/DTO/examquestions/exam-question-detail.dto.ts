export interface ExamQuestionDetailDTO {
  id: string;
  version: string;
  examId: string;
  title: string;
  description: string;
  imagePath: string | null;
  maxScore: number;
  displayOrder: number;
}
