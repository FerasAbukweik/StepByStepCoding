export interface CreateExamQuestionDTO {
  examId: string;
  title: string;
  description: string;
  imagePath: string | null;
  maxScore: number;
  displayOrder: number;
}
