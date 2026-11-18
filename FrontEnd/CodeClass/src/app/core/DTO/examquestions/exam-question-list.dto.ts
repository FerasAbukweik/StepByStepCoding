export interface ExamQuestionListDTO {
  id: string;
  version: string;
  examId: string;
  title: string;
  imagePath: string | null;
  maxScore: number;
  displayOrder: number;
}
